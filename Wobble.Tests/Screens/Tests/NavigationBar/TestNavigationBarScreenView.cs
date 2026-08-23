using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Assets;
using Wobble.Configuration;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Navigation;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Tests.Screens.Selection;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.NavigationBars
{
    public class TestNavigationBarScreenView : ScreenView
    {
        private const float BarHeight = 40;
        private const string WallpaperAsset = "Wallpaper";

        private static readonly Color PageBackground = new Color(10, 13, 18);
        private static readonly Color ButtonBackground = new Color(39, 48, 56);

        private static readonly Color[] SolidColors =
        {
            new Color(28, 49, 68),
            new Color(65, 35, 92),
            new Color(26, 83, 67)
        };

        private static readonly Color[][] GradientPalettes =
        {
            new[]
            {
                new Color(11, 132, 255),
                new Color(111, 66, 193),
                new Color(238, 74, 137)
            },
            new[]
            {
                new Color(255, 173, 51),
                new Color(239, 71, 111),
                new Color(131, 56, 236)
            },
            new[]
            {
                new Color(16, 185, 129),
                new Color(14, 165, 233),
                new Color(37, 99, 235)
            }
        };

        private static readonly NavigationBarBackgroundType[] BackgroundTypes =
        {
            NavigationBarBackgroundType.SolidColor,
            NavigationBarBackgroundType.Image,
            NavigationBarBackgroundType.Gradient
        };

        private static readonly NavigationBarImageFit[] ImageFits =
        {
            NavigationBarImageFit.Stretch,
            NavigationBarImageFit.Cover,
            NavigationBarImageFit.Contain,
            NavigationBarImageFit.Tile
        };

        private static readonly NavigationBarGradientType[] GradientTypes =
        {
            NavigationBarGradientType.Linear,
            NavigationBarGradientType.Radial
        };

        private static readonly Vector2[] RadialOrigins =
        {
            new Vector2(0.5f, 0.5f),
            new Vector2(0.2f, 0.5f),
            new Vector2(0.8f, 0.2f)
        };

        private static readonly float[] RadialRadii = { 0.5f, 1, 1.5f };

        private NavigationBar TopBar { get; }

        private NavigationBar BottomBar { get; }

        private SpriteTextPlus StatusText { get; }

        private YamlConfig<NavigationBarTestConfiguration> Configuration { get; }

        private string PlayerConfigurationPath { get; }

        private string ConfiguredImageAsset { get; set; } = WallpaperAsset;

        private string ExternalImagePath { get; set; }

        private string ImageLoadError { get; set; }

        private Texture2D ExternalImage { get; set; }

        private int BackgroundTypeIndex { get; set; }

        private int ImageFitIndex { get; set; }

        private int GradientTypeIndex { get; set; }

        private int PaletteIndex { get; set; }

        private int RadialOriginIndex { get; set; }

        private int RadialRadiusIndex { get; set; } = 1;

        private int InvalidBackgroundIndex { get; set; }

        private float GradientAngle { get; set; }

        private string Status
        {
            get => StatusText.Text;
            set => StatusText.Text = value;
        }

        public TestNavigationBarScreenView(Screen screen) : base(screen)
        {
            TopBar = CreateBar(Alignment.TopLeft);
            BottomBar = CreateBar(Alignment.BotLeft);

            StatusText = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), "Ready", 20)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 34
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "B: background  C: colors  F: image fit  G: gradient type  D: direction",
                14)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = -42,
                Tint = new Color(180, 190, 202)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "O: radial origin  R: radial radius  M: mutate + refresh  I: invalid fallback",
                14)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = -22,
                Tint = new Color(180, 190, 202)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-regular"),
                "S: save YAML  L: load YAML  X: reset saved overrides",
                14)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = -2,
                Tint = new Color(180, 190, 202)
            };

            var configurationDirectory = Path.Combine(AppContext.BaseDirectory, "NavigationBarTest");
            var mainConfigurationPath = Path.Combine(configurationDirectory, "navbar-main.yml");
            PlayerConfigurationPath = Path.Combine(configurationDirectory, "navbar-player.yml");
            Configuration = YamlConfig<NavigationBarTestConfiguration>.LoadOrCreate(
                mainConfigurationPath, PlayerConfigurationPath);
            ApplyConfiguration(Configuration.GetSnapshot(), "loaded from YAML");

            BottomBar.Background = new NavigationBarBackgroundOptions
            {
                Type = NavigationBarBackgroundType.Gradient,
                Gradient = CreateGradient(NavigationBarGradientType.Radial)
            };

            AddIconButton(TopBar, NavigationBarRegion.Left, "Play", true, true);
            AddIconButton(TopBar, NavigationBarRegion.Left, "Tools", true, false,
                CreateDropdownOptions("Editor", "Import", "Export"));
            AddIconButton(TopBar, NavigationBarRegion.Left, "Chat", true);
            AddIconButton(TopBar, NavigationBarRegion.Left, "Favorite", true);

            var profileButton = TopBar.AddRoundedButton(NavigationBarRegion.Right, new NavigationBarButtonOptions
            {
                Icon = Textures.Home,
                IconSize = new Vector2(16, 16),
                Text = "[WWWW] Nickname",
                Font = FontManager.GetWobbleFont("inter-bold"),
                FontSize = 12,
                WidthMode = ButtonSizeMode.Auto,
                Height = 26,
                AutoSizePadding = new Vector2(12, 0),
                CornerRadius = 3,
                BackgroundColor = ButtonBackground,
                AntiAliasedEdges = false,
                ClickAction = (sender, args) => Status = "Profile"
            });

            // const float profileRightPadding = 12;
            // profileButton.Width += profileRightPadding;
            // profileButton.Icon.X -= profileRightPadding / 2;
            // profileButton.Label.X -= profileRightPadding / 2;
            TopBar.RefreshLayout();

            AddIconButton(TopBar, NavigationBarRegion.Right, "Menu");

            TopBar.AddRoundedButton(NavigationBarRegion.Right, new NavigationBarButtonOptions
            {
                Text = LocalizationManager.Get("Navigation_BackToTests"),
                Font = FontManager.GetWobbleFont("inter-bold"),
                FontSize = 12,
                WidthMode = ButtonSizeMode.Auto,
                Height = 26,
                AutoSizePadding = new Vector2(12, 0),
                CornerRadius = 3,
                BackgroundColor = ButtonBackground,
                AntiAliasedEdges = false,
                ClickAction = (sender, args) => ScreenManager.ChangeScreen(new SelectionScreen())
            });

            AddIconButton(BottomBar, NavigationBarRegion.Left, "Website");
            AddIconButton(BottomBar, NavigationBarRegion.Left, "Discord");
            AddIconButton(BottomBar, NavigationBarRegion.Left, "GitHub");

            AddIconButton(BottomBar, NavigationBarRegion.Right, "Volume");
            AddIconButton(BottomBar, NavigationBarRegion.Right, "Settings", false, false,
                CreateDropdownOptions("Graphics", "Audio", "Input"));
            AddIconButton(BottomBar, NavigationBarRegion.Right, "Power");
        }

        public override void Update(GameTime gameTime)
        {
            RefreshViewportLayout();
            Container.Update(gameTime);

            if (KeyboardManager.IsUniqueKeyPress(Keys.B))
            {
                BackgroundTypeIndex = (BackgroundTypeIndex + 1) % BackgroundTypes.Length;
                ApplyCurrentBackground();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.F))
            {
                ImageFitIndex = (ImageFitIndex + 1) % ImageFits.Length;
                ApplyCurrentBackground();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.C))
            {
                PaletteIndex = (PaletteIndex + 1) % SolidColors.Length;
                ApplyCurrentBackground();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.G))
            {
                GradientTypeIndex = (GradientTypeIndex + 1) % GradientTypes.Length;
                ApplyCurrentBackground();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.D))
            {
                GradientAngle = (GradientAngle + 45) % 360;
                ApplyCurrentBackground();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.O))
            {
                RadialOriginIndex = (RadialOriginIndex + 1) % RadialOrigins.Length;
                ApplyCurrentBackground();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.R))
            {
                RadialRadiusIndex = (RadialRadiusIndex + 1) % RadialRadii.Length;
                ApplyCurrentBackground();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.M))
                MutateCurrentBackground();

            if (KeyboardManager.IsUniqueKeyPress(Keys.I))
            {
                InvalidBackgroundIndex = (InvalidBackgroundIndex + 1) % 7;
                ApplyInvalidBackground();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.S))
                SaveConfiguration();

            if (KeyboardManager.IsUniqueKeyPress(Keys.L))
                LoadConfiguration();

            if (KeyboardManager.IsUniqueKeyPress(Keys.X))
                ResetConfiguration();
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(PageBackground);
            Container.Draw(gameTime);
        }

        public override void Destroy()
        {
            Container.Destroy();
            DisposeExternalImage();
        }

        private NavigationBar CreateBar(Alignment alignment) => new NavigationBar(
            WindowManager.Width, BarHeight)
        {
            Parent = Container,
            Alignment = alignment,
            EdgePadding = 10,
            ItemSpacing = 7
        };

        private void ApplyCurrentBackground()
        {
            InvalidBackgroundIndex = 0;
            var backgroundType = BackgroundTypes[BackgroundTypeIndex];

            switch (backgroundType)
            {
                case NavigationBarBackgroundType.SolidColor:
                    TopBar.BackgroundColor = SolidColors[PaletteIndex];
                    break;
                case NavigationBarBackgroundType.Image:
                    TopBar.Background = new NavigationBarBackgroundOptions
                    {
                        Type = NavigationBarBackgroundType.Image,
                        Image = ResolveConfiguredImage(),
                        ImageFit = ImageFits[ImageFitIndex]
                    };
                    break;
                case NavigationBarBackgroundType.Gradient:
                    TopBar.Background = new NavigationBarBackgroundOptions
                    {
                        Type = NavigationBarBackgroundType.Gradient,
                        Gradient = CreateGradient(GradientTypes[GradientTypeIndex])
                    };
                    break;
            }

            UpdateStatus("assigned");
        }

        private NavigationBarGradientOptions CreateGradient(NavigationBarGradientType type) =>
            new NavigationBarGradientOptions
            {
                Type = type,
                AngleDegrees = GradientAngle,
                RadialOrigin = RadialOrigins[RadialOriginIndex],
                RadialRadius = RadialRadii[RadialRadiusIndex],
                Stops = CreateGradientStops()
            };

        private NavigationBarGradientStop[] CreateGradientStops()
        {
            var palette = GradientPalettes[PaletteIndex];
            return new[]
            {
                new NavigationBarGradientStop { Position = 0, Color = palette[0] },
                new NavigationBarGradientStop { Position = 0.45f, Color = palette[1] },
                new NavigationBarGradientStop { Position = 1, Color = palette[2] }
            };
        }

        private void SaveConfiguration()
        {
            if (InvalidBackgroundIndex != 0)
            {
                Status = "Save skipped · invalid fallback states are not persisted";
                return;
            }

            Configuration.SaveOverrides(CreateConfiguration());
            UpdateStatus(File.Exists(PlayerConfigurationPath)
                ? $"saved {Path.GetFileName(PlayerConfigurationPath)}"
                : "matches main YAML · no override file needed");
        }

        private void LoadConfiguration()
        {
            if (!Configuration.Reload())
            {
                Status = $"YAML load failed · {Configuration.Warnings[0]}";
                return;
            }

            var updateMethod = Configuration.Warnings.Count == 0
                ? "loaded from YAML"
                : $"loaded with {Configuration.Warnings.Count} warning(s)";
            ApplyConfiguration(Configuration.GetSnapshot(), updateMethod);
        }

        private void ResetConfiguration()
        {
            Configuration.ResetOverrides();
            ApplyConfiguration(Configuration.GetSnapshot(), "reset to main YAML");
        }

        private NavigationBarTestConfiguration CreateConfiguration()
        {
            var origin = RadialOrigins[RadialOriginIndex];
            var palette = GradientPalettes[PaletteIndex];
            return new NavigationBarTestConfiguration
            {
                BackgroundType = BackgroundTypes[BackgroundTypeIndex],
                SolidColor = ToConfigurationColor(SolidColors[PaletteIndex]),
                Image = new NavigationBarTestImageConfiguration
                {
                    Asset = ConfiguredImageAsset,
                    Fit = ImageFits[ImageFitIndex]
                },
                Gradient = new NavigationBarTestGradientConfiguration
                {
                    Type = GradientTypes[GradientTypeIndex],
                    AngleDegrees = ToConfigurationNumber(GradientAngle),
                    RadialOrigin = new NavigationBarTestPoint(
                        ToConfigurationNumber(origin.X), ToConfigurationNumber(origin.Y)),
                    RadialRadius = ToConfigurationNumber(RadialRadii[RadialRadiusIndex]),
                    Stops = new List<NavigationBarTestGradientStop>
                    {
                        new NavigationBarTestGradientStop(0, ToConfigurationColor(palette[0])),
                        new NavigationBarTestGradientStop(0.45m, ToConfigurationColor(palette[1])),
                        new NavigationBarTestGradientStop(1, ToConfigurationColor(palette[2]))
                    }
                }
            };
        }

        private void ApplyConfiguration(NavigationBarTestConfiguration configuration, string updateMethod)
        {
            configuration = configuration ?? new NavigationBarTestConfiguration();
            var image = configuration.Image ?? new NavigationBarTestImageConfiguration();
            var gradient = configuration.Gradient ?? new NavigationBarTestGradientConfiguration();

            ConfiguredImageAsset = image.Asset;

            BackgroundTypeIndex = FindIndex(BackgroundTypes, configuration.BackgroundType, 0);
            ImageFitIndex = FindIndex(ImageFits, image.Fit, 0);
            GradientTypeIndex = FindIndex(GradientTypes, gradient.Type, 0);
            PaletteIndex = FindPaletteIndex(configuration);
            GradientAngle = (float) gradient.AngleDegrees;

            var radialOrigin = gradient.RadialOrigin ?? new NavigationBarTestPoint(0.5m, 0.5m);
            RadialOriginIndex = FindVectorIndex(RadialOrigins,
                new Vector2((float) radialOrigin.X, (float) radialOrigin.Y), 0);
            RadialRadiusIndex = FindFloatIndex(RadialRadii, (float) gradient.RadialRadius, 1);
            InvalidBackgroundIndex = 0;

            switch (configuration.BackgroundType)
            {
                case NavigationBarBackgroundType.SolidColor:
                    TopBar.BackgroundColor = ToColor(configuration.SolidColor);
                    break;
                case NavigationBarBackgroundType.Image:
                    TopBar.Background = new NavigationBarBackgroundOptions
                    {
                        Type = NavigationBarBackgroundType.Image,
                        Image = ResolveConfiguredImage(true),
                        ImageFit = image.Fit
                    };
                    break;
                case NavigationBarBackgroundType.Gradient:
                    TopBar.Background = new NavigationBarBackgroundOptions
                    {
                        Type = NavigationBarBackgroundType.Gradient,
                        Gradient = new NavigationBarGradientOptions
                        {
                            Type = gradient.Type,
                            AngleDegrees = (float) gradient.AngleDegrees,
                            RadialOrigin = new Vector2((float) radialOrigin.X, (float) radialOrigin.Y),
                            RadialRadius = (float) gradient.RadialRadius,
                            Stops = ToGradientStops(gradient.Stops)
                        }
                    };
                    break;
                default:
                    TopBar.Background = null;
                    break;
            }

            UpdateStatus(updateMethod);
        }

        private int FindPaletteIndex(NavigationBarTestConfiguration configuration)
        {
            var gradientStops = configuration.Gradient?.Stops;
            if (gradientStops != null && gradientStops.Count == 3 &&
                gradientStops[0] != null && gradientStops[1] != null && gradientStops[2] != null)
            {
                for (var i = 0; i < GradientPalettes.Length; i++)
                {
                    if (ColorsEqual(gradientStops[0].Color, GradientPalettes[i][0]) &&
                        ColorsEqual(gradientStops[1].Color, GradientPalettes[i][1]) &&
                        ColorsEqual(gradientStops[2].Color, GradientPalettes[i][2]))
                        return i;
                }
            }

            for (var i = 0; i < SolidColors.Length; i++)
            {
                if (ColorsEqual(configuration.SolidColor, SolidColors[i]))
                    return i;
            }

            return 0;
        }

        private static NavigationBarGradientStop[] ToGradientStops(
            IReadOnlyList<NavigationBarTestGradientStop> stops)
        {
            if (stops == null)
                return null;

            var result = new NavigationBarGradientStop[stops.Count];
            for (var i = 0; i < stops.Count; i++)
            {
                var stop = stops[i];
                result[i] = stop == null
                    ? null
                    : new NavigationBarGradientStop
                    {
                        Position = (float) stop.Position,
                        Color = ToColor(stop.Color)
                    };
            }

            return result;
        }

        private static NavigationBarTestColor ToConfigurationColor(Color color) =>
            new NavigationBarTestColor(color.R, color.G, color.B, color.A);

        private static decimal ToConfigurationNumber(float value) => Math.Round((decimal) value, 4);

        private static Color ToColor(NavigationBarTestColor color) => color == null
            ? Color.Transparent
            : new Color(color.R, color.G, color.B, color.A);

        private static bool ColorsEqual(NavigationBarTestColor left, Color right) => left != null &&
                                                                                     left.R == right.R &&
                                                                                     left.G == right.G &&
                                                                                     left.B == right.B &&
                                                                                     left.A == right.A;

        private static int FindIndex<T>(IReadOnlyList<T> values, T value, int fallback)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(values[i], value))
                    return i;
            }

            return fallback;
        }

        private Texture2D ResolveConfiguredImage(bool forceReload = false)
        {
            ImageLoadError = null;

            if (string.Equals(ConfiguredImageAsset, WallpaperAsset, StringComparison.OrdinalIgnoreCase))
            {
                DisposeExternalImage();
                return WobbleAssets.Wallpaper;
            }

            if (string.IsNullOrWhiteSpace(ConfiguredImageAsset))
            {
                DisposeExternalImage();
                ImageLoadError = "asset path is empty";
                return null;
            }

            try
            {
                var path = ConfiguredImageAsset;
                if (!Path.IsPathRooted(path))
                    path = Path.Combine(Path.GetDirectoryName(PlayerConfigurationPath) ?? string.Empty, path);

                path = Path.GetFullPath(path);
                if (!File.Exists(path))
                    throw new FileNotFoundException("Image file was not found.", path);

                if (!forceReload && ExternalImage != null && !ExternalImage.IsDisposed &&
                    string.Equals(ExternalImagePath, path, StringComparison.OrdinalIgnoreCase))
                    return ExternalImage;

                var loadedImage = AssetLoader.LoadTexture2DFromFile(path);
                DisposeExternalImage();
                ExternalImage = loadedImage;
                ExternalImagePath = path;
                return ExternalImage;
            }
            catch (Exception e)
            {
                DisposeExternalImage();
                ImageLoadError = e.Message;
                return null;
            }
        }

        private void DisposeExternalImage()
        {
            ExternalImage?.Dispose();
            ExternalImage = null;
            ExternalImagePath = null;
        }

        private static int FindVectorIndex(IReadOnlyList<Vector2> values, Vector2 value, int fallback)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (Vector2.DistanceSquared(values[i], value) < 0.000001f)
                    return i;
            }

            return fallback;
        }

        private static int FindFloatIndex(IReadOnlyList<float> values, float value, int fallback)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (Math.Abs(values[i] - value) < 0.0001f)
                    return i;
            }

            return fallback;
        }

        private void MutateCurrentBackground()
        {
            InvalidBackgroundIndex = 0;
            var backgroundType = BackgroundTypes[BackgroundTypeIndex];

            if (TopBar.Background.Type != backgroundType ||
                backgroundType == NavigationBarBackgroundType.Gradient && TopBar.Background.Gradient == null)
                ApplyCurrentBackground();

            switch (backgroundType)
            {
                case NavigationBarBackgroundType.SolidColor:
                    PaletteIndex = (PaletteIndex + 1) % SolidColors.Length;
                    TopBar.Background.SolidColor = SolidColors[PaletteIndex];
                    break;
                case NavigationBarBackgroundType.Image:
                    ImageFitIndex = (ImageFitIndex + 1) % ImageFits.Length;
                    TopBar.Background.Image = ResolveConfiguredImage();
                    TopBar.Background.ImageFit = ImageFits[ImageFitIndex];
                    break;
                case NavigationBarBackgroundType.Gradient:
                    PaletteIndex = (PaletteIndex + 1) % GradientPalettes.Length;
                    TopBar.Background.Gradient.Type = GradientTypes[GradientTypeIndex];
                    TopBar.Background.Gradient.AngleDegrees = GradientAngle;
                    TopBar.Background.Gradient.RadialOrigin = RadialOrigins[RadialOriginIndex];
                    TopBar.Background.Gradient.RadialRadius = RadialRadii[RadialRadiusIndex];
                    TopBar.Background.Gradient.Stops = CreateGradientStops();
                    break;
            }

            TopBar.RefreshBackground();
            UpdateStatus("mutated + refreshed");
        }

        private void UpdateStatus(string updateMethod)
        {
            var backgroundType = BackgroundTypes[BackgroundTypeIndex];

            switch (backgroundType)
            {
                case NavigationBarBackgroundType.SolidColor:
                    var color = SolidColors[PaletteIndex];
                    Status = $"SolidColor · palette {PaletteIndex + 1}/{SolidColors.Length} · " +
                             $"rgb({color.R}, {color.G}, {color.B}) · {updateMethod}";
                    break;
                case NavigationBarBackgroundType.Image:
                    var assetName = string.Equals(ConfiguredImageAsset, WallpaperAsset,
                        StringComparison.OrdinalIgnoreCase)
                        ? WallpaperAsset
                        : Path.GetFileName(ConfiguredImageAsset);
                    Status = ImageLoadError == null
                        ? $"Image · {assetName} · {ImageFits[ImageFitIndex]} · {updateMethod}"
                        : $"Image failed · {assetName} · {ImageLoadError}";
                    break;
                case NavigationBarBackgroundType.Gradient:
                    var gradientType = GradientTypes[GradientTypeIndex];
                    var detail = gradientType == NavigationBarGradientType.Linear
                        ? $"{GradientAngle:0}°"
                        : $"origin {RadialOrigins[RadialOriginIndex]} · radius {RadialRadii[RadialRadiusIndex]:0.0}";
                    Status = $"Gradient · {gradientType} · palette {PaletteIndex + 1}/{GradientPalettes.Length} · " +
                             $"{detail} · {updateMethod}";
                    break;
            }
        }

        private void ApplyInvalidBackground()
        {
            switch (InvalidBackgroundIndex)
            {
                case 0:
                    ApplyCurrentBackground();
                    return;
                case 1:
                    TopBar.Background = new NavigationBarBackgroundOptions
                    {
                        Type = NavigationBarBackgroundType.Image,
                        Image = null
                    };
                    Status = "Invalid fallback: missing image";
                    return;
                case 2:
                    ApplyInvalidGradient(null, "missing stops");
                    return;
                case 3:
                    ApplyInvalidGradient(new[]
                    {
                        new NavigationBarGradientStop { Position = 0.5f, Color = Color.Blue },
                        new NavigationBarGradientStop { Position = 0.5f, Color = Color.Red }
                    }, "duplicate stops");
                    return;
                case 4:
                    ApplyInvalidGradient(new[]
                    {
                        new NavigationBarGradientStop { Position = -0.1f, Color = Color.Blue },
                        new NavigationBarGradientStop { Position = 1, Color = Color.Red }
                    }, "out-of-range stop");
                    return;
                case 5:
                    ApplyInvalidGradient(CreateInvalidGradientStops(), "non-finite angle", float.NaN);
                    return;
                default:
                    var gradient = CreateGradient(NavigationBarGradientType.Radial);
                    gradient.RadialRadius = 0;
                    TopBar.Background = new NavigationBarBackgroundOptions
                    {
                        Type = NavigationBarBackgroundType.Gradient,
                        Gradient = gradient
                    };
                    Status = "Invalid fallback: zero radial radius";
                    return;
            }
        }

        private void ApplyInvalidGradient(NavigationBarGradientStop[] stops, string description,
            float angle = 0)
        {
            TopBar.Background = new NavigationBarBackgroundOptions
            {
                Type = NavigationBarBackgroundType.Gradient,
                Gradient = new NavigationBarGradientOptions
                {
                    Stops = stops,
                    AngleDegrees = angle
                }
            };
            Status = $"Invalid fallback: {description}";
        }

        private static NavigationBarGradientStop[] CreateInvalidGradientStops() => new[]
        {
            new NavigationBarGradientStop { Position = 0, Color = Color.Blue },
            new NavigationBarGradientStop { Position = 1, Color = Color.Red }
        };

        private void AddIconButton(NavigationBar bar, NavigationBarRegion region, string action,
            bool expandLabelOnHover = false, bool alwaysShowLabel = false,
            NavigationBarDropdownOption[] dropdownOptions = null)
        {
            bar.AddRoundedButton(region, new NavigationBarButtonOptions
            {
                Icon = Textures.Home,
                IconSize = new Vector2(16, 16),
                Text = expandLabelOnHover ? action : null,
                Font = expandLabelOnHover || dropdownOptions != null
                    ? FontManager.GetWobbleFont("inter-bold")
                    : null,
                FontSize = 12,
                Width = 26,
                Height = 26,
                AutoSizePadding = new Vector2(12, 0),
                CornerRadius = 3,
                BackgroundColor = ButtonBackground,
                AntiAliasedEdges = false,
                ExpandLabelOnHover = expandLabelOnHover,
                AlwaysShowLabel = alwaysShowLabel,
                HoverExpansionDuration = 150,
                ExpandedLabelRightPadding = 0,
                ClickAction = (sender, args) => Status = action,
                DropdownOptions = dropdownOptions
            });
        }

        private NavigationBarDropdownOption[] CreateDropdownOptions(params string[] actions)
        {
            var options = new NavigationBarDropdownOption[actions.Length];

            for (var i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                options[i] = new NavigationBarDropdownOption
                {
                    Text = action,
                    ClickAction = (sender, args) => Status = action
                };
            }

            return options;
        }

        private void RefreshViewportLayout()
        {
            TopBar.Width = WindowManager.Width;
            BottomBar.Width = WindowManager.Width;
        }
    }
}
