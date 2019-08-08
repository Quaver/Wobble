using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Wobble.Managers
{
    public static class LocalizationManager
    {
        /// <summary>
        ///     The default/fallback language for localization
        /// </summary>
        public static Dictionary<string, string> DefaultLanguage { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        ///     The current language to use for strings
        /// </summary>
        public static Dictionary<string, string> CurrentLanguage { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        ///     Sets the default/fallback language. You'd typically only want to do this once at the start
        ///     of the game
        /// </summary>
        public static void SetDefaultLanguageFile(string resource)
        {
            if (DefaultLanguage.Keys.Count > 0)
                throw new InvalidOperationException("Default language file was already specified!");

            DefaultLanguage = ParseLanguageFile(GameBase.Game.Resources.Get(resource));
        }

        /// <summary>
        ///     Sets the current language resource to use for strings
        /// </summary>
        /// <param name="resource"></param>
        public static void SetCurrentLanguage(string resource) => CurrentLanguage = ParseLanguageFile(GameBase.Game.Resources.Get(resource));

        /// <summary>
        ///     Gets a localized string. Checks <see cref="CurrentLanguage"/>  for the string first. Then it will
        ///     check <see cref="DefaultLanguage"/> as a fallback. If it cannot find either, then it will throw
        ///     an exception
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            if (CurrentLanguage.ContainsKey(key))
                return CurrentLanguage[key];

            if (DefaultLanguage.ContainsKey(key))
                return DefaultLanguage[key];

            throw new ArgumentException($"Cannot find localized string for key: {key}");
        }

        /// <summary>
        ///     Gets a localized string from a generic enum value.
        ///     Takes the stringified version of enum value and uses that as the key.
        ///     Useful if you want to define all of your possible keys in an enum rather than using strings everywhere
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static string Get<TEnum>(TEnum value) => Get(value.ToString());

        /// <summary>
        ///     Language files should be setup similar to ini.
        ///
        ///     Key=Value
        ///     Example: Hello!=¡Hola!
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> ParseLanguageFile(byte[] file)
        {
            var strings = new Dictionary<string, string>();

            foreach (var line in Encoding.UTF8.GetString(file).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                var matches = Regex.Matches(line, @"^(\w+)=(\w+)$");

                if (matches.Count == 0)
                    throw new ArgumentException($"Input was not in the correct format on line: {line}");

                foreach (Match match in matches)
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[2].Value;

                    if (!strings.ContainsKey(key))
                        strings.Add(key, value);
                }
            }

            return strings;
        }
    }
}