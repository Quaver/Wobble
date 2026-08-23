using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Wobble.Configuration;

namespace Wobble.Tests.Screens.Tests.YamlConfiguration
{
    public sealed class DemoSkinConfig
    {
        [ConfigRequired]
        [Range(2, 2)]
        public int FormatVersion { get; set; } = 2;

        public string SkinId { get; set; } = "wobble-default";

        public DemoSkinIdentity Identity { get; set; } = new DemoSkinIdentity();

        [ConfigEditable]
        public DemoSkinPanel Panel { get; set; } = new DemoSkinPanel();

        [ConfigEditable]
        public List<string> Fonts { get; set; } = new List<string> { "Inter", "Noto" };
    }

    public sealed class DemoSkinIdentity
    {
        public string Version { get; set; } = "1.0";
    }

    public sealed class DemoSkinPanel
    {
        [RegularExpression("^#[0-9A-Fa-f]{6}$")]
        public string AccentColor { get; set; } = "#0FBAE5";

        [Range(0d, 1d)]
        public double Opacity { get; set; } = 0.85;
    }

    internal sealed class YamlConfigurationCheck
    {
        public string Name { get; }
        public bool Passed { get; }
        public string Detail { get; }

        public YamlConfigurationCheck(string name, bool passed, string detail = null)
        {
            Name = name;
            Passed = passed;
            Detail = detail;
        }
    }

    internal sealed class YamlConfigurationTestContext
    {
        private const string MainAccent = "#0FBAE5";
        private const double MainOpacity = 0.85;

        public string DirectoryPath { get; }
        public string MainPath { get; }
        public string PlayerPath { get; }
        public YamlConfig<DemoSkinConfig> Config { get; }
        public List<YamlConfigurationCheck> Checks { get; } = new List<YamlConfigurationCheck>();
        public string LastAction { get; private set; } = "Self-checks completed";

        public YamlConfigurationTestContext()
        {
            DirectoryPath = Path.Combine(AppContext.BaseDirectory, "YamlConfigurationTest");
            MainPath = Path.Combine(DirectoryPath, "skin-main.yml");
            PlayerPath = Path.Combine(DirectoryPath, "skin-player.yml");

            ResetTestFiles();

            Config = YamlConfig<DemoSkinConfig>.LoadOrCreate(MainPath, PlayerPath);
            RunChecks();
            PrepareInteractiveState();
        }

        public void SetAccent()
        {
            var edited = Config.GetSnapshot();
            edited.Panel.AccentColor = "#755CDE";
            Config.SaveOverrides(edited);
            LastAction = "Saved accent override";
        }

        public void ResetAccent()
        {
            var edited = Config.GetSnapshot();
            edited.Panel.AccentColor = Config.GetMainSnapshot().Panel.AccentColor;
            Config.SaveOverrides(edited);
            LastAction = "Reset accent override";
        }

        public void ResetPanel()
        {
            var edited = Config.GetSnapshot();
            edited.Panel = Config.GetMainSnapshot().Panel;
            Config.SaveOverrides(edited);
            LastAction = "Reset panel overrides";
        }

        public void ResetAll()
        {
            Config.ResetOverrides();
            LastAction = "Reset all overrides";
        }

        public void AttemptNonEditableSet()
        {
            var edited = Config.GetSnapshot();
            edited.SkinId = "player-hacked";
            Config.SaveOverrides(edited);
            LastAction = Config.GetSnapshot().SkinId == "player-hacked"
                ? "ERROR: changed non-editable SkinId"
                : "Ignored non-editable SkinId edit";
        }

        public void Reload()
        {
            var success = Config.Reload();
            LastAction = $"Reload: {(success ? "success" : "failed")} ({Config.Warnings.Count} warnings)";
        }

        public string ReadPlayerYaml() => File.Exists(PlayerPath)
            ? File.ReadAllText(PlayerPath).Trim()
            : "<no player override file>";

        private void RunChecks()
        {
            Check("GENERATES MAIN YAML", () => File.Exists(MainPath));

            File.WriteAllText(PlayerPath,
                "SkinId: player-hacked\n" +
                "Identity:\n  Version: 9.9\n" +
                "Panel:\n  AccentColor: '#FF3366'\n  Opacity: invalid-number\n" +
                "Fonts:\n  - PlayerFont\n" +
                "UnknownKey: true\n");

            Config.Reload();
            Check("APPLIES VALID NESTED VALUE", () => Config.GetSnapshot().Panel.AccentColor == "#FF3366");
            Check("IGNORES INVALID VALUE", () => Math.Abs(Config.GetSnapshot().Panel.Opacity - MainOpacity) < 0.001);
            Check("REPLACES COLLECTION", () => Config.GetSnapshot().Fonts.SequenceEqual(new[] { "PlayerFont" }));
            Check("REPORTS NON-EDITABLE + UNKNOWN", () =>
                Config.Warnings.Any(x => x.Contains("Non-editable")) &&
                Config.Warnings.Any(x => x.Contains("Unknown")) &&
                Config.Warnings.Any(x => x.Contains("Invalid")));

            var nonEditableEdit = Config.GetSnapshot();
            nonEditableEdit.SkinId = "player-hacked";
            Config.SaveOverrides(nonEditableEdit);
            Check("REJECTS NON-EDITABLE EDIT", () => Config.GetSnapshot().SkinId == "wobble-default");

            var detached = Config.GetSnapshot();
            detached.SkinId = "snapshot-change";
            detached.Panel.AccentColor = "#000000";
            Check("DEFENSIVE SNAPSHOT", () => Config.GetSnapshot().SkinId == "wobble-default" &&
                                                Config.GetSnapshot().Panel.AccentColor == "#FF3366");

            var edited = Config.GetSnapshot();
            edited.Panel.AccentColor = MainAccent;
            Config.SaveOverrides(edited);
            Check("RESETS SPECIFIC VALUE", () => Config.GetSnapshot().Panel.AccentColor == MainAccent &&
                                                  !File.ReadAllText(PlayerPath).Contains("AccentColor"));

            File.WriteAllText(PlayerPath,
                "Panel:\n  AccentColor: '#334455'\n  Opacity: 0.25\nFonts:\n  - Compact\n");
            Config.Reload();
            edited = Config.GetSnapshot();
            edited.Panel = Config.GetMainSnapshot().Panel;
            Config.SaveOverrides(edited);
            Check("RESETS NESTED VALUES", () =>
                Config.GetSnapshot().Panel.AccentColor == MainAccent &&
                Math.Abs(Config.GetSnapshot().Panel.Opacity - MainOpacity) < 0.001 &&
                Config.GetSnapshot().Fonts.SequenceEqual(new[] { "Compact" }));

            Config.ResetOverrides();
            edited = Config.GetSnapshot();
            edited.Panel.AccentColor = "#112233";
            Config.SaveOverrides(edited);
            var sparseYaml = File.ReadAllText(PlayerPath);
            Check("SAVES SPARSE OVERRIDES", () => sparseYaml.Contains("AccentColor") &&
                                                        !sparseYaml.Contains("SkinId") &&
                                                        !sparseYaml.Contains("Opacity"));

            Check("RELOAD ROUND-TRIP", () => Config.Reload() &&
                                                Config.GetSnapshot().Panel.AccentColor == "#112233");

            Config.ResetOverrides();
            Check("DELETES EMPTY OVERRIDE", () => !File.Exists(PlayerPath));

            var optionalMain = Path.Combine(DirectoryPath, "optional-main.yml");
            var optional = YamlConfig<DemoSkinConfig>.LoadOptional(optionalMain);
            Check("OPTIONAL MAIN USES DEFAULTS", () => !File.Exists(optionalMain) &&
                                                          optional.GetSnapshot().FormatVersion == 2);

            File.WriteAllText(optionalMain,
                "FormatVersion: 2\n" +
                "SkinId: author-hacked\n" +
                "Identity:\n  Version: 9.9\n" +
                "Panel:\n  AccentColor: '#334455'\n  Opacity: 4\n" +
                "UnknownMain: true\n");
            Check("MAIN RESTRICTION + FIELD VALIDATION", () => optional.Reload() &&
                                                  optional.GetSnapshot().SkinId == "wobble-default" &&
                                                  optional.GetSnapshot().Identity.Version == "1.0" &&
                                                  optional.GetSnapshot().Panel.AccentColor == "#334455" &&
                                                  Math.Abs(optional.GetSnapshot().Panel.Opacity - MainOpacity) < 0.001 &&
                                                  optional.Warnings.Any(x => x.Contains("Opacity")) &&
                                                  optional.Warnings.Any(x => x.Contains("SkinId")) &&
                                                  optional.Warnings.Any(x => x.Contains("Identity.Version")) &&
                                                  optional.Warnings.Any(x => x.Contains("UnknownMain")));

            var invalidSave = optional.GetMainSnapshot();
            invalidSave.Panel.Opacity = 2;
            Check("REJECTS INVALID MAIN SAVE", () =>
                !optional.TrySaveMain(invalidSave, out var errors) && errors.Count > 0);

            var validSave = optional.GetMainSnapshot();
            validSave.Panel.Opacity = 0.5;
            Check("ATOMIC MAIN SAVE ROUND-TRIP", () =>
                optional.TrySaveMain(validSave, out var errors) && errors.Count == 0 &&
                optional.GetSnapshot().Panel.Opacity == 0.5 &&
                File.ReadAllText(optionalMain).Contains("FormatVersion") &&
                File.ReadAllText(optionalMain).Contains("Panel") &&
                !File.ReadAllText(optionalMain).Contains("SkinId") &&
                !File.ReadAllText(optionalMain).Contains("Identity"));

            File.WriteAllText(optionalMain, "Panel:\n  Opacity: 0.25\n");
            Check("REQUIRES EXPLICIT MAIN VALUE", () => !optional.Reload() &&
                                                             optional.GetSnapshot().Panel.Opacity == 0.5 &&
                                                             optional.Warnings.Any(x => x.Contains("FormatVersion")));
            File.Delete(optionalMain);

            var secondMain = Path.Combine(DirectoryPath, "second-main.yml");
            var secondPlayer = Path.Combine(DirectoryPath, "second-player.yml");
            var second = YamlConfig<DemoSkinConfig>.LoadOrCreate(secondMain, secondPlayer);
            var secondEdit = second.GetSnapshot();
            secondEdit.Panel.AccentColor = "#ABCDEF";
            second.SaveOverrides(secondEdit);
            Check("ISOLATES MULTIPLE CONFIGS", () => second.GetSnapshot().Panel.AccentColor == "#ABCDEF" &&
                                                       Config.GetSnapshot().Panel.AccentColor == MainAccent);
            File.Delete(secondMain);
            if (File.Exists(secondPlayer))
                File.Delete(secondPlayer);

            File.WriteAllText(PlayerPath, "Panel: [unterminated\n");
            var invalidPlayer = Config.Reload();
            Check("SURVIVES MALFORMED PLAYER YAML", () => invalidPlayer &&
                                                               Config.GetSnapshot().Panel.AccentColor == MainAccent &&
                                                               Config.Warnings.Count > 0);

            var validMain = File.ReadAllText(MainPath);
            File.WriteAllText(MainPath, validMain.Replace("wobble-default", "main-file-value"));
            var changedMain = Config.Reload();
            Check("NON-EDITABLE VALUE FOLLOWS MAIN", () => changedMain &&
                                                        Config.GetSnapshot().SkinId == "main-file-value" &&
                                                        Config.GetMainSnapshot().SkinId == "main-file-value");
            File.WriteAllText(MainPath, validMain);
            Config.Reload();

            File.WriteAllText(MainPath, "Panel: [unterminated\n");
            var invalidMain = Config.Reload();
            Check("RETAINS LAST VALID ON MAIN ERROR", () => !invalidMain &&
                                                               Config.GetSnapshot().Panel.AccentColor == MainAccent);
            File.WriteAllText(MainPath, validMain);
            Config.Reload();
        }

        private void PrepareInteractiveState()
        {
            Config.ResetOverrides();
            Config.Reload();
        }

        private void ResetTestFiles()
        {
            Directory.CreateDirectory(DirectoryPath);

            foreach (var name in new[]
                     {
                         "skin-main.yml", "skin-player.yml", "second-main.yml", "second-player.yml",
                         "optional-main.yml"
                     })
            {
                var path = Path.Combine(DirectoryPath, name);
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private void Check(string name, Func<bool> assertion)
        {
            try
            {
                var passed = assertion();
                Checks.Add(new YamlConfigurationCheck(name, passed, passed ? null : "Assertion returned false"));
            }
            catch (Exception e)
            {
                Checks.Add(new YamlConfigurationCheck(name, false, e.GetType().Name + ": " + e.Message));
            }
        }
    }
}
