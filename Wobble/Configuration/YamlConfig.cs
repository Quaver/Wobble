using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Wobble.Configuration
{
    /// <summary>
    ///     Loads a trusted main YAML document and applies a sparse player override. Properties are immutable by
    ///     default and must opt into player editing with <see cref="ConfigEditableAttribute"/>.
    /// </summary>
    public sealed class YamlConfig<T> where T : class, new()
    {
        private static readonly INamingConvention NamingConvention = new CamelCaseNamingConvention();

        private readonly string mainPath;
        private readonly Func<Stream> mainSource;
        private readonly string overridePath;
        private readonly ISerializer serializer;
        private readonly IDeserializer deserializer;

        private T mainValue;
        private T currentValue;

        /// <summary>
        ///     Warnings produced by the latest load. Invalid player values are ignored.
        /// </summary>
        public IReadOnlyList<string> Warnings { get; private set; } = Array.Empty<string>();

        private YamlConfig(string mainPath, Func<Stream> mainSource, string overridePath)
        {
            if (string.IsNullOrWhiteSpace(overridePath))
                throw new ArgumentException("A player override path is required.", nameof(overridePath));

            this.mainPath = mainPath;
            this.mainSource = mainSource;
            this.overridePath = overridePath;
            serializer = new SerializerBuilder().WithNamingConvention(NamingConvention).Build();
            deserializer = new DeserializerBuilder().WithNamingConvention(NamingConvention).Build();

            if (!Reload())
                throw new InvalidDataException(Warnings[0]);
        }

        /// <summary>
        ///     Loads a filesystem-backed config, creating the main file from model defaults when missing.
        /// </summary>
        public static YamlConfig<T> LoadOrCreate(string mainPath, string overridePath)
        {
            if (string.IsNullOrWhiteSpace(mainPath))
                throw new ArgumentException("A main configuration path is required.", nameof(mainPath));

            if (string.Equals(Path.GetFullPath(mainPath), Path.GetFullPath(overridePath),
                    StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The main and player configuration paths must be different.",
                    nameof(overridePath));

            return new YamlConfig<T>(mainPath, null, overridePath);
        }

        /// <summary>
        ///     Loads a config whose trusted main YAML is supplied by a fresh stream on every reload.
        /// </summary>
        public static YamlConfig<T> Load(Func<Stream> mainSource, string overridePath)
        {
            if (mainSource == null)
                throw new ArgumentNullException(nameof(mainSource));

            return new YamlConfig<T>(null, mainSource, overridePath);
        }

        /// <summary>
        ///     Returns a detached copy of the effective configuration for reading or editing.
        /// </summary>
        public T GetSnapshot() => Clone(currentValue);

        /// <summary>
        ///     Returns a detached copy of the trusted main configuration.
        /// </summary>
        public T GetMainSnapshot() => Clone(mainValue);

        /// <summary>
        ///     Saves differences for editable properties between an edited model and the main configuration.
        /// </summary>
        public void SaveOverrides(T editedValue)
        {
            if (editedValue == null)
                throw new ArgumentNullException(nameof(editedValue));

            var overrides = BuildDifferences(typeof(T), mainValue, editedValue, false);
            if (overrides.Children.Count == 0)
            {
                if (File.Exists(overridePath))
                    File.Delete(overridePath);
            }
            else
            {
                using (var writer = new StringWriter())
                {
                    new YamlStream(new YamlDocument(overrides)).Save(writer, false);
                    WriteTextAtomically(overridePath, writer.ToString());
                }
            }

            currentValue = Clone(mainValue);
            ApplyMapping(currentValue, overrides, typeof(T));
        }

        /// <summary>
        ///     Deletes all player overrides and restores the main configuration.
        /// </summary>
        public void ResetOverrides()
        {
            if (File.Exists(overridePath))
                File.Delete(overridePath);

            currentValue = Clone(mainValue);
        }

        /// <summary>
        ///     Reloads both YAML layers. A bad main document leaves the last valid state in place.
        /// </summary>
        public bool Reload()
        {
            var warnings = new List<string>();
            T loadedMain;

            try
            {
                using (var reader = new StringReader(ReadMainYaml()))
                    loadedMain = deserializer.Deserialize<T>(reader);

                if (loadedMain == null)
                    throw new InvalidDataException("The main YAML document produced a null model.");
            }
            catch (Exception e)
            {
                warnings.Add("The main YAML configuration could not be loaded: " + e.Message);
                Warnings = warnings.AsReadOnly();
                return false;
            }

            var overrides = LoadPlayerOverrides(warnings);
            var loadedCurrent = Clone(loadedMain);
            ApplyMapping(loadedCurrent, overrides, typeof(T));

            mainValue = loadedMain;
            currentValue = loadedCurrent;
            Warnings = warnings.AsReadOnly();
            return true;
        }

        private string ReadMainYaml()
        {
            if (mainSource != null)
            {
                using (var stream = mainSource())
                {
                    if (stream == null)
                        throw new InvalidOperationException("The main YAML stream factory returned null.");

                    using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, false))
                        return reader.ReadToEnd();
                }
            }

            if (!File.Exists(mainPath))
                WriteTextAtomically(mainPath, serializer.Serialize(new T()));

            return File.ReadAllText(mainPath, Encoding.UTF8);
        }

        private YamlMappingNode LoadPlayerOverrides(ICollection<string> warnings)
        {
            if (!File.Exists(overridePath))
                return new YamlMappingNode();

            try
            {
                using (var reader = File.OpenText(overridePath))
                {
                    var yaml = new YamlStream();
                    yaml.Load(reader);
                    if (yaml.Documents.Count != 1 || !(yaml.Documents[0].RootNode is YamlMappingNode root))
                        throw new InvalidDataException("The document must contain one mapping.");

                    return SanitizeMapping(root, typeof(T), string.Empty, warnings, false);
                }
            }
            catch (Exception e)
            {
                warnings.Add("The player override was ignored: " + e.Message);
                return new YamlMappingNode();
            }
        }

        private YamlMappingNode SanitizeMapping(YamlMappingNode source, Type modelType, string parentPath,
            ICollection<string> warnings, bool parentIsEditable)
        {
            var result = new YamlMappingNode();
            var properties = GetPropertyMap(modelType);

            foreach (var pair in source.Children)
            {
                if (!(pair.Key is YamlScalarNode key) || string.IsNullOrEmpty(key.Value))
                {
                    warnings.Add("Player configuration keys must be non-empty scalar values.");
                    continue;
                }

                var path = CombinePath(parentPath, key.Value);
                if (!properties.TryGetValue(key.Value, out var property))
                {
                    warnings.Add($"Unknown player configuration value '{path}' was ignored.");
                    continue;
                }

                var isEditable = parentIsEditable ||
                                 property.GetCustomAttribute<ConfigEditableAttribute>() != null;

                if (pair.Value is YamlMappingNode nested && IsNestedObjectType(property.PropertyType))
                {
                    var sanitized = SanitizeMapping(nested, property.PropertyType, path, warnings, isEditable);
                    if (sanitized.Children.Count > 0)
                        result.Add(new YamlScalarNode(GetYamlName(property)), sanitized);
                    continue;
                }

                if (!isEditable)
                {
                    warnings.Add($"Non-editable player configuration value '{path}' was ignored.");
                    continue;
                }

                try
                {
                    var value = DeserializeNode(pair.Value, property.PropertyType);
                    if (value == null && IsNonNullableValueType(property.PropertyType))
                        throw new InvalidDataException("A non-nullable value cannot be null.");

                    result.Add(new YamlScalarNode(GetYamlName(property)), pair.Value);
                }
                catch (Exception e)
                {
                    warnings.Add($"Invalid player configuration value '{path}' was ignored: {e.Message}");
                }
            }

            return result;
        }

        private void ApplyMapping(object target, YamlMappingNode mapping, Type modelType)
        {
            var properties = GetPropertyMap(modelType);
            foreach (var pair in mapping.Children)
            {
                var property = properties[((YamlScalarNode) pair.Key).Value];
                if (pair.Value is YamlMappingNode nested && IsNestedObjectType(property.PropertyType))
                {
                    var nestedTarget = property.GetValue(target) ?? Activator.CreateInstance(property.PropertyType);
                    property.SetValue(target, nestedTarget);
                    ApplyMapping(nestedTarget, nested, property.PropertyType);
                }
                else
                    property.SetValue(target, DeserializeNode(pair.Value, property.PropertyType));
            }
        }

        private YamlMappingNode BuildDifferences(Type modelType, object main, object edited,
            bool parentIsEditable)
        {
            var result = new YamlMappingNode();
            foreach (var pair in GetPropertyMap(modelType))
            {
                var property = pair.Value;
                var isEditable = parentIsEditable ||
                                 property.GetCustomAttribute<ConfigEditableAttribute>() != null;

                var mainProperty = main == null ? null : property.GetValue(main);
                var editedProperty = edited == null ? null : property.GetValue(edited);

                if (IsNestedObjectType(property.PropertyType) && mainProperty != null && editedProperty != null)
                {
                    var nested = BuildDifferences(property.PropertyType, mainProperty, editedProperty, isEditable);
                    if (nested.Children.Count > 0)
                        result.Add(new YamlScalarNode(pair.Key), nested);
                }
                else if (isEditable && !ValuesEqual(mainProperty, editedProperty))
                    result.Add(new YamlScalarNode(pair.Key), SerializeToNode(editedProperty));
            }

            return result;
        }

        private Dictionary<string, PropertyInfo> GetPropertyMap(Type type)
        {
            var result = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetMethod == null || !property.GetMethod.IsPublic || property.SetMethod == null ||
                    !property.SetMethod.IsPublic || property.GetIndexParameters().Length != 0 ||
                    property.GetCustomAttribute<YamlIgnoreAttribute>() != null)
                    continue;

                result.Add(GetYamlName(property), property);
            }

            return result;
        }

        private static string GetYamlName(PropertyInfo property)
        {
            var member = property.GetCustomAttribute<YamlMemberAttribute>();
            return member != null && !string.IsNullOrEmpty(member.Alias)
                ? member.Alias
                : NamingConvention.Apply(property.Name);
        }

        private static bool IsNestedObjectType(Type type) =>
            type.IsClass && type != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(type);

        private static bool IsNonNullableValueType(Type type) =>
            type.IsValueType && Nullable.GetUnderlyingType(type) == null;

        private static string CombinePath(string parent, string name) =>
            string.IsNullOrEmpty(parent) ? name : parent + "." + name;

        private object DeserializeNode(YamlNode node, Type type)
        {
            using (var writer = new StringWriter())
            {
                new YamlStream(new YamlDocument(node)).Save(writer, false);
                using (var reader = new StringReader(writer.ToString()))
                    return deserializer.Deserialize(reader, type);
            }
        }

        private YamlNode SerializeToNode(object value)
        {
            using (var reader = new StringReader(serializer.Serialize(value)))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);
                return yaml.Documents[0].RootNode;
            }
        }

        private T Clone(T value)
        {
            using (var reader = new StringReader(serializer.Serialize(value)))
                return deserializer.Deserialize<T>(reader);
        }

        private bool ValuesEqual(object left, object right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left == null || right == null)
                return false;
            if (left.GetType().IsValueType || left is string)
                return left.Equals(right);

            return serializer.Serialize(left) == serializer.Serialize(right);
        }

        private static void WriteTextAtomically(string path, string contents)
        {
            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var temporaryPath = fullPath + ".tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(temporaryPath, contents, Encoding.UTF8);
                File.Move(temporaryPath, fullPath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }
    }
}
