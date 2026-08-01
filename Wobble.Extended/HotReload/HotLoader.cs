using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Wobble.Logging;

namespace Wobble.Extended.HotReload
{
    public class HotLoader : IDisposable, IHotLoaderUpdate
    {
        /// <summary>
        ///     The directory that will be watched for changes
        /// </summary>
        protected string ProjectDirectory { get; }

        /// <summary>
        ///     The project file that will be built and hotloaded.
        /// </summary>
        private string ProjectFilePath => Path.Combine(ProjectDirectory, $"{ProjectName}.csproj");

        /// <summary>
        ///     Watches the directory for changes
        /// </summary>
        public FileSystemWatcher Watcher { get; }

        /// <summary>
        ///     The compiling process
        /// </summary>
        protected ProcessStartInfo Compiler { get; set; }

        /// <summary>
        ///     The currently loaded assembly
        /// </summary>
        public Assembly Asm { get; private set; }

        /// <summary>
        ///     Fetches the name of the project from the directory
        /// </summary>
        protected string ProjectName => new DirectoryInfo(ProjectDirectory).Name;

        /// <summary>
        ///     The subscreen/test screen that will be drawn
        /// </summary>
        public dynamic Screen { get; set; }

        /// <summary>
        ///     If the previous compilation has failed
        /// </summary>
        public bool CompilationFailed { get; private set; }

        /// <summary>
        ///     If the hotloader is currently compiling
        /// </summary>
        public bool IsCompiling => Compiler != null;

        /// <summary>
        ///     Action to be called after recompiling
        /// </summary>
        public Action AfterCompiling { get; set; }

        /// <summary>
        ///     Initializes resources owned by a newly loaded assembly before its screen is constructed.
        /// </summary>
        public Action<Assembly> InitializeAssembly { get; set; }

        /// <summary>
        ///     Disposes resources owned by an assembly after its screen has been replaced.
        /// </summary>
        public Action<Assembly> DisposeAssembly { get; set; }

        private readonly object CompilationLock = new object();

        private int ReloadRequested;

        private Assembly ScreenAssembly { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="projectDirectory"></param>
        /// <param name="afterCompiling"></param>
        /// <param name="filter"></param>
        public HotLoader(string projectDirectory, Action afterCompiling = null, string filter = "*.cs")
        {
            ProjectDirectory = Path.GetFullPath(projectDirectory);
            AfterCompiling = afterCompiling;

            Watcher = new FileSystemWatcher
            {
                Path = ProjectDirectory,
                NotifyFilter = NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.Size,
                Filter = filter,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            Watcher.Changed += OnChanged;
            Watcher.Renamed += OnChanged;
        }

        /// <summary>
        ///     Loads in the new dll
        /// </summary>
        public void LoadDll()
        {
            Assembly newAssembly = null;

            try
            {
                var path = GetCompiledAssemblyPath();
                newAssembly = Assembly.Load(File.ReadAllBytes(path));
                InitializeAssembly?.Invoke(newAssembly);
                Asm = newAssembly;

                if (Screen == null)
                    return;

                foreach (var type in Asm.GetExportedTypes())
                {
                    if (type.FullName != Screen.GetType().ToString())
                        continue;

                    TryChangeScreen(type);
                    break;
                }
            }
            catch (Exception e)
            {
                if (newAssembly != null && newAssembly != Asm)
                    TryDisposeAssembly(newAssembly);

                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Recompiles the project when changes are made to filtered files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnChanged(object sender, FileSystemEventArgs e)
        {
            CompileProject();

            if (!CompilationFailed)
                Interlocked.Exchange(ref ReloadRequested, 1);
        }

        /// <summary>
        ///     Recompiles the project
        /// </summary>
        /// <returns></returns>
        public void CompileProject()
        {
            lock (CompilationLock)
            {
                CompileProjectInternal();
            }
        }

        private void CompileProjectInternal()
        {
            Watcher.EnableRaisingEvents = false;

            Logger.Debug($"Initializing Compilation for project: {ProjectName}", LogType.Runtime);

            if (Compiler != null)
            {
                Watcher.EnableRaisingEvents = true;
                return;
            }

            const string command = "dotnet";
            var args = $"build \"{ProjectFilePath}\"";

            Compiler = new ProcessStartInfo(command, args)
            {
                WorkingDirectory = Environment.CurrentDirectory,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var p = Process.Start(Compiler);

            if (p == null)
            {
                Compiler = null;
                Watcher.EnableRaisingEvents = true;
                CompilationFailed = true;
                return;
            }

            var output = p.StandardOutput.ReadToEnd();
            output += p.StandardError.ReadToEnd();

            p.WaitForExit();

            if (p.ExitCode == 0)
            {
                Compiler = null;
                Logger.Debug("Compilation Success", LogType.Runtime);
                CompilationFailed = false;
                Watcher.EnableRaisingEvents = true;
                return;
            }

            CompilationFailed = true;
            Compiler = null;
            Watcher.EnableRaisingEvents = true;
            Logger.Debug(output, LogType.Runtime);
        }

        private string GetCompiledAssemblyPath()
        {
            var assemblyName = GetProjectProperty("AssemblyName") ?? ProjectName;
            var targetFramework = GetProjectProperty("TargetFramework") ?? GetProjectProperty("TargetFrameworks")?.Split(';').FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(targetFramework))
            {
                var path = Path.Combine(ProjectDirectory, "bin", "Debug", targetFramework, $"{assemblyName}.dll");

                if (File.Exists(path))
                    return path;
            }

            var debugOutputDirectory = Path.Combine(ProjectDirectory, "bin", "Debug");
            var fallbackPath = Directory.Exists(debugOutputDirectory)
                ? Directory
                    .EnumerateFiles(debugOutputDirectory, $"{assemblyName}.dll", SearchOption.AllDirectories)
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .FirstOrDefault()
                : null;

            if (fallbackPath != null)
                return fallbackPath;

            throw new FileNotFoundException($"Could not find compiled assembly for project '{ProjectName}'.", $"{assemblyName}.dll");
        }

        private string GetProjectProperty(string propertyName)
        {
            if (!File.Exists(ProjectFilePath))
                return null;

            return XDocument
                .Load(ProjectFilePath)
                .Descendants(propertyName)
                .Select(x => x.Value.Trim())
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (!IsCompiling && !CompilationFailed &&
                Interlocked.CompareExchange(ref ReloadRequested, 0, 1) == 1)
            {
                try
                {
                    AfterCompiling?.Invoke();
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }

                LoadDll();
            }

            Screen?.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime) => Screen?.Draw(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool TryChangeScreen(Type type)
        {
            try
            {
                var nextScreen = Activator.CreateInstance(type);
                var oldScreen = Screen;
                var oldAssembly = ScreenAssembly;

                Screen = nextScreen;
                ScreenAssembly = Asm;

                try
                {
                    oldScreen?.Destroy();
                }
                finally
                {
                    if (oldAssembly != null && oldAssembly != ScreenAssembly)
                        TryDisposeAssembly(oldAssembly);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return false;
            }
        }

        public void Dispose() => Watcher.Dispose();

        private void TryDisposeAssembly(Assembly assembly)
        {
            try
            {
                DisposeAssembly?.Invoke(assembly);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}
