using System;
using Wobble.IO;
using Wobble.Managers;
using Xunit;

namespace Wobble.Tests.Unit.Localization
{
    public class TestLocalization
    {
        private const string FolderPath = "Wobble.Tests.Resources/Localization";

        [Fact]
        public void TestGet()
        {
            Setup();

            LocalizationManager.SetCurrentLanguage($"{FolderPath}/es.txt");
            Assert.True(LocalizationManager.Get("Greeting") == "Hola");
        }

        [Fact]
        public void TestGetEnum()
        {
            Setup();

            LocalizationManager.SetCurrentLanguage($"{FolderPath}/es.txt");
            Assert.True(LocalizationManager.Get(LocalizationStrings.Greeting) == "Hola");
        }

        [Fact]
        public void TestOtherLanguage()
        {
            Setup();

            LocalizationManager.SetCurrentLanguage($"{FolderPath}/fr.txt");
            Assert.True(LocalizationManager.Get(LocalizationStrings.Greeting) == "Bonjour");
        }

        [Fact]
        public void TestJapanese()
        {
            Setup();

            LocalizationManager.SetCurrentLanguage($"{FolderPath}/jp.txt");
            Assert.True(LocalizationManager.Get(LocalizationStrings.Greeting) == "こんにちは こんにちは こんにちは");
        }

        [Fact]
        public void TestArabic()
        {
            LocalizationManager.SetCurrentLanguage($"{FolderPath}/ar.txt");
            Assert.True(LocalizationManager.Get(LocalizationStrings.Greeting) == "مرحبا كيف حالك");
        }

        [Fact]
        public void TestFallback()
        {
            Setup();

            LocalizationManager.SetCurrentLanguage($"{FolderPath}/es.txt");

            // Should fallback to English in this case
            Assert.True(LocalizationManager.Get(LocalizationStrings.Bye) == "Goodbye");
        }

        [Fact]
        public void TestNonExistent()
        {
            Setup();

            LocalizationManager.SetCurrentLanguage($"{FolderPath}/es.txt");

            try
            {
                var str = LocalizationManager.Get("Unknown_Key");
            }
            catch (Exception)
            {
                // All good. We want there to be an exception if the key was never found
            }
        }

        [Fact]
        public void TestInterpolation()
        {
            Setup();
            
            LocalizationManager.SetCurrentLanguage($"{FolderPath}/en.txt");
            Assert.True(LocalizationManager.Get(LocalizationStrings.Results_Found, 3) == "3 Results Found");
        }

        private void Setup()
        {
            if (GameBase.Game != null)
                return;

            var game = new WobbleTestsGame();
            game.Resources.AddStore(new DllResourceStore("Wobble.Tests.Resources.dll"));

            LocalizationManager.SetDefaultLanguageFile($"{FolderPath}/en.txt");
        }
    }

    public enum LocalizationStrings
    {
        Greeting,
        Bye,
        Results_Found
    }
}