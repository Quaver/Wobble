using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Wobble.Configuration
{
    /// <summary>
    ///     Loads a main YAML document and optionally applies a sparse override. Restricted main documents and
    ///     overrides are immutable by default and opt into external editing with <see cref="ConfigEditableAttribute"/>.
    /// </summary>
    public sealed class YamlConfig<T> where T : class, new()
    {
        private static readonly INamingConvention NamingConvention = new PascalCaseNamingConvention();

        private readonly string mainPath;
        private readonly Func<Stream> mainSource;
        private readonly string overridePath;
        private readonly bool optionalMain;
        private readonly bool restrictMainToEditable;
        private readonly ISerializer serializer;
        private readonly IDeserializer deserializer;

        private T mainValue;
        private T currentValue;

        /// <summary>
        ///     Warnings produced by the latest load. Invalid player values are ignored.
        /// </summary>
        public IReadOnlyList<string> Warnings { get; private set; } = Array.Empty<string>();

        private YamlConfig(string mainPath, Func<Stream> mainSource, string overridePath, bool optionalMain = false,
            bool restrictMainToEditable = false)
        {
            if (!optionalMain && string.IsNullOrWhiteSpace(overridePath))
                throw new ArgumentException("A player override path is required.", nameof(overridePath));

            this.mainPath = mainPath;
            this.mainSource = mainSource;
            this.overridePath = overridePath;
            this.optionalMain = optionalMain;
            this.restrictMainToEditable = restrictMainToEditable;
            serializer = new SerializerBuilder().WithNamingConvention(NamingConvention).Build();
            deserializer = new DeserializerBuilder().WithNamingConvention(NamingConvention).Build();
            mainValue = new T();
            currentValue = Clone(mainValue);

            if (!Reload() && !optionalMain)
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
        ///     Loads an optional filesystem-backed author configuration without creating a missing file or loading
        ///     an override. Only properties marked <see cref="ConfigEditableAttribute"/> and required document
        ///     metadata are accepted.
        /// </summary>
        public static YamlConfig<T> LoadOptional(string mainPath)
        {
            if (string.IsNullOrWhiteSpace(mainPath))
                throw new ArgumentException("A main configuration path is required.", nameof(mainPath));

            return new YamlConfig<T>(mainPath, null, null, true, true);
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
            if (string.IsNullOrWhiteSpace(overridePath))
                throw new InvalidOperationException("This configuration does not have a player override file.");

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
            if (string.IsNullOrWhiteSpace(overridePath))
                throw new InvalidOperationException("This configuration does not have a player override file.");

            if (File.Exists(overridePath))
                File.Delete(overridePath);

            currentValue = Clone(mainValue);
        }

        /// <summary>
        ///     Validates and atomically saves a filesystem-backed main configuration.
        /// </summary>
        public bool TrySaveMain(T editedValue, out IReadOnlyList<string> validationErrors)
        {
            var errors = new List<string>();
            if (editedValue == null)
                errors.Add("The main configuration cannot be null.");
            else if (string.IsNullOrWhiteSpace(mainPath) || mainSource != null)
                errors.Add("The main configuration is not filesystem-backed and cannot be saved.");
            else
                ValidateObjectGraph(editedValue, typeof(T), string.Empty, errors);

            if (errors.Count > 0)
            {
                validationErrors = errors.AsReadOnly();
                return false;
            }

            try
            {
                if (restrictMainToEditable)
                {
                    var editable = BuildEditableMapping(typeof(T), editedValue, false);
                    using (var writer = new StringWriter())
                    {
                        new YamlStream(new YamlDocument(editable)).Save(writer, false);
                        WriteTextAtomically(mainPath, writer.ToString());
                    }
                }
                else
                    WriteTextAtomically(mainPath, serializer.Serialize(editedValue));

                if (!Reload())
                    errors.AddRange(Warnings);
            }
            catch (Exception e)
            {
                errors.Add("The main YAML configuration could not be saved: " + e.Message);
            }

            validationErrors = errors.AsReadOnly();
            return errors.Count == 0;
        }

        /// <summary>
        ///     Reloads both YAML layers. A bad main document leaves the last valid state in place.
        /// </summary>
        public bool Reload()
        {
            var warnings = new List<string>();
            var loadedMain = new T();

            try
            {
                var contents = ReadMainYaml();
                if (contents == null)
                {
                    mainValue = loadedMain;
                    currentValue = Clone(loadedMain);
                    Warnings = warnings.AsReadOnly();
                    return true;
                }

                using (var reader = new StringReader(contents))
                {
                    var yaml = new YamlStream();
                    yaml.Load(reader);
                    if (yaml.Documents.Count != 1 || !(yaml.Documents[0].RootNode is YamlMappingNode root))
                        throw new InvalidDataException("The document must contain one mapping.");

                    if (!ApplyMainMapping(loadedMain, root, typeof(T), string.Empty, warnings, false))
                    {
                        Warnings = warnings.AsReadOnly();
                        return false;
                    }
                }
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

            if (!File.Exists(mainPath) && optionalMain)
                return null;

            if (!File.Exists(mainPath))
                WriteTextAtomically(mainPath, serializer.Serialize(new T()));

            return File.ReadAllText(mainPath, Encoding.UTF8);
        }

        private YamlMappingNode LoadPlayerOverrides(ICollection<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(overridePath) || !File.Exists(overridePath))
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

        private bool ApplyMainMapping(object target, YamlMappingNode source, Type modelType, string parentPath,
            ICollection<string> warnings, bool parentIsEditable)
        {
            var properties = GetPropertyMap(modelType);
            var suppliedNames = new HashSet<string>(source.Children.Keys
                .OfType<YamlScalarNode>()
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Select(x => x.Value), StringComparer.Ordinal);
            var valid = true;

            foreach (var property in properties.Values.Where(x =>
                         x.GetCustomAttribute<ConfigRequiredAttribute>() != null))
            {
                var yamlName = GetYamlName(property);
                if (suppliedNames.Contains(yamlName))
                    continue;

                warnings.Add($"Required main configuration value '{CombinePath(parentPath, yamlName)}' is missing.");
                valid = false;
            }

            foreach (var pair in source.Children)
            {
                if (!(pair.Key is YamlScalarNode key) || string.IsNullOrEmpty(key.Value))
                {
                    warnings.Add("Main configuration keys must be non-empty scalar values.");
                    continue;
                }

                var path = CombinePath(parentPath, key.Value);
                if (!properties.TryGetValue(key.Value, out var property))
                {
                    warnings.Add($"Unknown main configuration value '{path}' was ignored.");
                    continue;
                }

                var required = property.GetCustomAttribute<ConfigRequiredAttribute>() != null;
                var explicitlyEditable = property.GetCustomAttribute<ConfigEditableAttribute>() != null;
                var isEditable = !restrictMainToEditable || parentIsEditable || explicitlyEditable || required;
                try
                {
                    if (pair.Value is YamlMappingNode nested && IsNestedObjectType(property.PropertyType))
                    {
                        var nestedTarget = property.GetValue(target) ?? Activator.CreateInstance(property.PropertyType);
                        if (!ApplyMainMapping(nestedTarget, nested, property.PropertyType, path, warnings,
                                parentIsEditable || explicitlyEditable))
                            valid = false;

                        if (!TryValidatePropertyValue(target, property, nestedTarget, out var validationMessage))
                            throw new ValidationException(validationMessage);

                        property.SetValue(target, nestedTarget);
                        continue;
                    }

                    if (!isEditable)
                    {
                        warnings.Add($"Non-editable main configuration value '{path}' was ignored.");
                        continue;
                    }

                    var value = DeserializeNode(pair.Value, property.PropertyType);
                    if (value == null && IsNonNullableValueType(property.PropertyType))
                        throw new InvalidDataException("A non-nullable value cannot be null.");
                    if (!TryValidatePropertyValue(target, property, value, out var error))
                        throw new ValidationException(error);

                    property.SetValue(target, value);
                }
                catch (Exception e)
                {
                    warnings.Add($"Invalid main configuration value '{path}' was ignored: {e.Message}");
                    if (required)
                        valid = false;
                }
            }

            return valid;
        }

        private YamlMappingNode BuildEditableMapping(Type modelType, object value, bool parentIsEditable)
        {
            var result = new YamlMappingNode();
            foreach (var property in GetPropertyMap(modelType).Values)
            {
                var explicitlyEditable = property.GetCustomAttribute<ConfigEditableAttribute>() != null;
                var required = property.GetCustomAttribute<ConfigRequiredAttribute>() != null;
                var isEditable = parentIsEditable || explicitlyEditable;
                var propertyValue = value == null ? null : property.GetValue(value);

                if (IsNestedObjectType(property.PropertyType) && propertyValue != null)
                {
                    var nested = BuildEditableMapping(property.PropertyType, propertyValue, isEditable);
                    if (nested.Children.Count > 0)
                        result.Add(new YamlScalarNode(GetYamlName(property)), nested);
                }
                else if (isEditable || required)
                    result.Add(new YamlScalarNode(GetYamlName(property)), SerializeToNode(propertyValue));
            }

            return result;
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
                    if (!TryValidatePropertyValue(new object(), property, value, out var validationMessage))
                        throw new ValidationException(validationMessage);

                    result.Add(new YamlScalarNode(GetYamlName(property)), pair.Value);
                }
                catch (Exception e)
                {
                    warnings.Add($"Invalid player configuration value '{path}' was ignored: {e.Message}");
                }
            }

            return result;
        }

        private static bool TryValidatePropertyValue(object target, PropertyInfo property, object value,
            out string message)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(target ?? new object()) { MemberName = property.Name };
            if (Validator.TryValidateValue(value, context, results,
                    property.GetCustomAttributes<ValidationAttribute>(true)))
            {
                message = null;
                return true;
            }

            message = string.Join("; ", results.Select(x => x.ErrorMessage));
            return false;
        }

        private void ValidateObjectGraph(object value, Type modelType, string parentPath,
            ICollection<string> errors)
        {
            if (value == null)
            {
                errors.Add($"Configuration value '{parentPath}' cannot be null.");
                return;
            }

            foreach (var property in GetPropertyMap(modelType).Values)
            {
                var yamlName = GetYamlName(property);
                var path = CombinePath(parentPath, yamlName);
                var propertyValue = property.GetValue(value);
                if (!TryValidatePropertyValue(value, property, propertyValue, out var message))
                    errors.Add($"Invalid configuration value '{path}': {message}");

                if (propertyValue != null && IsNestedObjectType(property.PropertyType))
                    ValidateObjectGraph(propertyValue, property.PropertyType, path, errors);
            }
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

        private string GetYamlName(PropertyInfo property)
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
