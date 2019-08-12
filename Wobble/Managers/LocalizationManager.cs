using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using IniFileParser.Model;

namespace Wobble.Managers
{
    public static class LocalizationManager
    {
        /// <summary>
        ///     The default/fallback language for localization
        /// </summary>
        public static IniData DefaultLanguage { get; private set; }

        /// <summary>
        ///     The current language to use for strings
        /// </summary>
        public static IniData CurrentLanguage { get; private set; }

        /// <summary>
        ///     Sets the default/fallback language. You'd typically only want to do this once at the start
        ///     of the game
        /// </summary>
        public static void SetDefaultLanguageFile(string resource)
        {
            if (DefaultLanguage != null)
                throw new InvalidOperationException("Default language file was already specified!");

            DefaultLanguage = ParseLanguageFile(GameBase.Game.Resources.GetStream(resource));
        }

        /// <summary>
        ///     Sets the current language resource to use for strings
        /// </summary>
        /// <param name="resource"></param>
        public static void SetCurrentLanguage(string resource) => CurrentLanguage = ParseLanguageFile(GameBase.Game.Resources.GetStream(resource));

        /// <summary>
        ///     Gets a localized string. Checks <see cref="CurrentLanguage"/>  for the string first. Then it will
        ///     check <see cref="DefaultLanguage"/> as a fallback. If it cannot find either, then it will throw
        ///     an exception
        /// </summary>
        /// <param name="key"></param>
        /// <param name="interpolated"></param>
        /// <returns></returns>
        public static string Get(string key, params object[] interpolated)
        {
            const string header = "Strings";

            if (CurrentLanguage[header].ContainsKey(key))
                return string.Format(CurrentLanguage[header][key], interpolated);

            if (DefaultLanguage[header].ContainsKey(key))
                return string.Format(DefaultLanguage[header][key], interpolated);

            throw new ArgumentException($"Cannot find localized string for key: {key}");
        }

        /// <summary>
        ///     Gets a localized string from a generic enum value.
        ///     Takes the stringified version of enum value and uses that as the key.
        ///     Useful if you want to define all of your possible keys in an enum rather than using strings everywhere
        /// </summary>
        /// <param name="value"></param>
        /// <param name="interpolated"></param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static string Get<TEnum>(TEnum value, params object[] interpolated) => Get(value.ToString(), interpolated);

        /// <summary>
        ///     Language files should be setup similar to ini.
        ///
        ///     Key=Value
        ///
        ///     Examples:
        ///         * (in en.txt) - Greeting=Hello
        ///         * (in es.txt) - Greeting=¡Hola!
        /// </summary>
        /// <returns></returns>
        private static IniData ParseLanguageFile(Stream file)
        {
            var parser = new IniFileParser.IniFileParser();
            return parser.ReadData(new StreamReader(file));
        }
    }
}