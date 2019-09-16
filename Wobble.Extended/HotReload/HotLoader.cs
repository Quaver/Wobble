using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Wobble.Extended.HotReload
{
    public class HotLoader : IDisposable, IUpdate
    {
        /// <summary>
        ///     The directory that will be watched for changes
        /// </summary>
        protected string ProjectDirectory { get; }

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
        /// </summary>
        /// <param name="projectDirectory"></param>
        /// <param name="afterCompiling"></param>
        /// <param name="filter"></param>
        public HotLoader(string projectDirectory, Action afterCompiling = null, string filter = "*.cs")
        {
            ProjectDirectory = projectDirectory;
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
            var path = $@"../../../../{ProjectName}/bin/Debug/netcoreapp2.1/{ProjectName}.dll";

            Asm = Assembly.Load(File.ReadAllBytes(path));

            foreach (var type in Asm.GetExportedTypes())
            {
                if (Screen == null)
                    break;

                if (type.FullName == Screen.GetType().ToString())
                {
                    // We found our gamelogic type, set our dynamic types logic, and state
                    var oldScreen = Screen;

                    Screen = Activator.CreateInstance(type);
                    oldScreen?.Destroy();
                    break;
                }
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
            AfterCompiling?.Invoke();
            LoadDll();
        }

        /// <summary>
        ///     Recompiles the project
        /// </summary>
        /// <returns></returns>
        public void CompileProject()
        {
            Watcher.EnableRaisingEvents = false;

            Console.WriteLine($"Initializing Compiliation for project: {ProjectName}");

            if (Compiler != null)
            {
                Watcher.EnableRaisingEvents = true;
                return;
            }

            const string command = "dotnet";
            var args = $"build {ProjectDirectory}/{new DirectoryInfo(ProjectDirectory).Name}.csproj";

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
                LoadDll();
                Watcher.EnableRaisingEvents = true;
                return;
            }

            var output = p.StandardOutput.ReadToEnd();
            output += p.StandardError.ReadToEnd();

            p.WaitForExit();

            if (p.ExitCode == 0)
            {
                Compiler = null;
                Console.WriteLine("Compilation Success!");
                CompilationFailed = false;
                Watcher.EnableRaisingEvents = true;

                AfterCompiling?.Invoke();
                return;
            }

            CompilationFailed = true;
            Compiler = null;
            Watcher.EnableRaisingEvents = true;
            Console.WriteLine(output);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime) => Screen?.Update(gameTime);

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime) => Screen?.Draw(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose() => Watcher.Dispose();
    }
}
