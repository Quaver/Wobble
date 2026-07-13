using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using SmartFormat;

namespace Wobble.Managers
{
    public static class LocalizationManager
    {
        private static readonly Dictionary<string, ResourceManager> EmbeddedCultureResourceManagers =
            new Dictionary<string, ResourceManager>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     The resource manager used for localized strings.
        /// </summary>
        public static ResourceManager ResourceManager { get; private set; }

        /// <summary>
        ///     The default/fallback culture for localization.
        /// </summary>
        public static CultureInfo DefaultCulture { get; private set; }

        /// <summary>
        ///     The current culture used for localized strings.
        /// </summary>
        public static CultureInfo CurrentCulture { get; private set; }

        /// <summary>
        ///     Configures localization resources and cultures.
        /// </summary>
        public static void Configure(ResourceManager resourceManager, CultureInfo defaultCulture, CultureInfo currentCulture = null,
            Assembly embeddedResourceAssembly = null)
        {
            ResourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            DefaultCulture = defaultCulture ?? throw new ArgumentNullException(nameof(defaultCulture));
            CurrentCulture = currentCulture ?? DefaultCulture;

            EmbeddedCultureResourceManagers.Clear();

            if (embeddedResourceAssembly != null)
                ConfigureEmbeddedCultureResources(embeddedResourceAssembly);
        }

        /// <summary>
        ///     Sets the current culture used for strings.
        /// </summary>
        public static void SetCurrentCulture(CultureInfo culture) => CurrentCulture = culture ?? throw new ArgumentNullException(nameof(culture));

        /// <summary>
        ///     Gets a localized string. Checks <see cref="CurrentCulture"/> first, then checks
        ///     <see cref="DefaultCulture"/> as a fallback.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Get(string key, params object[] args)
        {
            if (ResourceManager == null || DefaultCulture == null || CurrentCulture == null)
                throw new InvalidOperationException("LocalizationManager must be configured before strings can be retrieved.");

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Localization key cannot be empty.", nameof(key));

            var value = GetEmbeddedString(key, CurrentCulture) ?? ResourceManager.GetString(key, CurrentCulture);

            if (value == null && !Equals(CurrentCulture, DefaultCulture))
                value = GetEmbeddedString(key, DefaultCulture) ?? ResourceManager.GetString(key, DefaultCulture);

            if (value == null)
                throw new ArgumentException($"Cannot find localized string for key: {key}", nameof(key));

            return Smart.Format(CurrentCulture, value, args);
        }

        /// <summary>
        ///     Gets a localized string from a generic enum value.
        ///     Takes the stringified version of enum value and uses that as the key.
        ///     Useful if you want to define all of your possible keys in an enum rather than using strings everywhere
        /// </summary>
        /// <param name="value"></param>
        /// <param name="args"></param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static string Get<TEnum>(TEnum value, params object[] args) => Get(value.ToString(), args);

        private static void ConfigureEmbeddedCultureResources(Assembly assembly)
        {
            var resourcePrefix = ResourceManager.BaseName + ".";

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.StartsWith(resourcePrefix, StringComparison.Ordinal) ||
                    !resourceName.EndsWith(".resources", StringComparison.Ordinal) ||
                    resourceName.Length <= resourcePrefix.Length + ".resources".Length)
                    continue;

                var cultureName = resourceName.Substring(resourcePrefix.Length,
                    resourceName.Length - resourcePrefix.Length - ".resources".Length);

                try
                {
                    var culture = CultureInfo.GetCultureInfo(cultureName);
                    EmbeddedCultureResourceManagers[culture.Name] =
                        new ResourceManager(resourceName.Substring(0, resourceName.Length - ".resources".Length), assembly);
                }
                catch (CultureNotFoundException)
                {
                    // This is another resource sharing the same base-name prefix, not a culture resource.
                }
            }
        }

        private static string GetEmbeddedString(string key, CultureInfo culture)
        {
            for (var candidate = culture; candidate != null && !candidate.Equals(CultureInfo.InvariantCulture);
                 candidate = candidate.Parent)
            {
                if (EmbeddedCultureResourceManagers.TryGetValue(candidate.Name, out var resourceManager))
                    return resourceManager.GetString(key, CultureInfo.InvariantCulture);
            }

            return null;
        }
    }
}
