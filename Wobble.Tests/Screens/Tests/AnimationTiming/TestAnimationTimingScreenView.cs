using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Assets;

namespace Wobble.Tests.Screens.Tests.AnimationTiming
{
    public class TestAnimationTimingScreenView : ScreenView
    {
        private const int MovementDuration = 1000;
        private const double TargetChangeInterval = 1000;
        private const float TintTimeConstant = 120;
        private const double FrameTime = 1000f / 60f;
        private const float MovementLeft = -420;
        private const float MovementRight = 420;

        private static readonly Color BackgroundColor = new Color(17, 24, 32);
        private static readonly Color PanelColor = new Color(31, 41, 51);
        private static readonly Color MutedColor = new Color(143, 155, 166);
        private static readonly Color AccentColor = new Color(15, 186, 229);
        private static readonly Color PurpleColor = new Color(117, 92, 222);
        private static readonly Color SuccessColor = new Color(105, 230, 166);
        private static readonly Color FailureColor = new Color(255, 119, 119);
        private static readonly Color TintStart = new Color(40, 80, 120);
        private static readonly Color TintEnd = new Color(220, 130, 30);

        private static readonly List<string> RateOptions = new List<string>
        {
            "Unlimited", "60", "120", "180", "240", "500", "1000"
        };

        private WobbleTestsGame TestGame => GameBase.Game as WobbleTestsGame;

        private HorizontalSelector FpsSelector { get; }
        private HorizontalSelector UpsSelector { get; }
        private SpriteTextPlus MeasuredRatesText { get; }
        private SpriteTextPlus MovementStatusText { get; }
        private SpriteTextPlus DiagnosticsText { get; }
        private Sprite MotionSprite { get; }
        private Sprite TintSprite { get; set; }
        private ProgressBar ProgressBar { get; set; }
        private BindableDouble ProgressValue { get; set; }
        private AnimatableSprite FrameSprite { get; set; }

        private Vector3 _expectedTint = TintStart.ToVector3();
        private Color _tintTarget = TintEnd;
        private double _targetChangeTimer;
        private double _expectedFrameRemainder;
        private int _expectedFrame;
        private int _maximumTintError;
        private bool _framePositionMatches = true;

        private long _rateSampleTimestamp;
        private int _sampledUpdates;
        private int _sampledDraws;
        private int _measuredUps;
        private int _measuredFps;

        private long _movementStartTimestamp;
        private bool _movementToRight = true;
        private bool _movementResultAvailable;
        private bool _movementPassed;
        private double _lastMovementDuration;
        private double _diagnosticRefreshTimer;

        public TestAnimationTimingScreenView(Screen screen) : base(screen)
        {
            CreateHeader();

            FpsSelector = CreateRateSelector("TARGET FPS", -260, 180, (_, __) => ApplySelectedRates());
            UpsSelector = CreateRateSelector("TARGET UPS", 260, 1000, (_, __) => ApplySelectedRates());

            CreatePresetButton("180 / 1000", -260, () => ApplyPreset(180, 1000));
            CreatePresetButton("60 / 60", 0, () => ApplyPreset(60, 60));
            CreatePresetButton("UNLIMITED", 260, () => ApplyPreset(null, null));

            MeasuredRatesText = CreateText("Waiting for a one-second sample…", 18, 188, Color.White);

            CreateText("1000 MS QUEUED MOVEMENT", 14, 226, MutedColor);
            MotionSprite = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Position = new ScalableVector2(MovementLeft, 254),
                Size = new ScalableVector2(44, 28),
                Tint = AccentColor
            };
            MovementStatusText = CreateText("RUNNING", 14, 286, MutedColor);

            CreateDiagnosticPanels();

            DiagnosticsText = CreateText("Diagnostics are warming up…", 15, 648, Color.White);
        }

        public void Activate()
        {
            ApplyPreset(180, 1000);
            _rateSampleTimestamp = Stopwatch.GetTimestamp();
            _sampledUpdates = 0;
            _sampledDraws = 0;
            StartMovement();
        }

        private void CreateHeader()
        {
            CreateText("ANIMATION TIMING", 26, 18, Color.White, "inter-bold");
            CreateText("Change the real draw and logic rates, then compare motion, smoothing, hover, and sprite frames.",
                16, 54, MutedColor);
        }

        private HorizontalSelector CreateRateSelector(string label, float x, int selectedRate,
            Action<string, int> onChange)
        {
            CreateText(label, 13, 92, MutedColor).X = x;

            var selectedIndex = RateOptions.IndexOf(selectedRate.ToString());
            var selector = new HorizontalSelector(RateOptions, new ScalableVector2(210, 36),
                FontManager.GetWobbleFont("inter-semibold"), 15, Textures.LeftButtonSquare,
                Textures.RightButtonSquare, new ScalableVector2(34, 34), 8, onChange,
                selectedIndex, true)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                X = x,
                Y = 116,
                Tint = PanelColor
            };

            selector.SelectedItemText.Tint = Color.White;
            StyleSelectorButton(selector.RoundedButtonSelectLeft);
            StyleSelectorButton(selector.RoundedButtonSelectRight);
            return selector;
        }

        private static void StyleSelectorButton(RoundedButton button)
        {
            button.CornerRadius = 7;
            button.Tint = PurpleColor;
            button.Label.Tint = Color.White;
        }

        private void CreatePresetButton(string text, float x, Action action)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Position = new ScalableVector2(x, 158),
                Size = new ScalableVector2(160, 30),
                CornerRadius = 6,
                Tint = PanelColor
            };
            button.SetLabel(FontManager.GetWobbleFont("inter-semibold"), text, 13, Color.White);
        }

        private void CreateDiagnosticPanels()
        {
            var tintPanel = CreatePanel(-410, 324, "TINT DAMPING");
            TintSprite = new Sprite
            {
                Parent = tintPanel,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(150, 54),
                Tint = TintStart
            };

            var progressPanel = CreatePanel(0, 324, "PROGRESS DAMPING");
            ProgressValue = new BindableDouble(100, 0, 100);
            ProgressBar = new ProgressBar(new Vector2(260, 22), ProgressValue, new Color(53, 63, 74),
                AccentColor, false)
            {
                Parent = progressPanel,
                Alignment = Alignment.MidCenter
            };

            var framePanel = CreatePanel(410, 324, "60 FPS SPRITE LOOP");
            FrameSprite = new AnimatableSprite(Textures.TestSpriteSheet)
            {
                Parent = framePanel,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(76, 76)
            };
            FrameSprite.StartLoop(Direction.Forward, 60);

            CreateText("INTERACTIVE CONTROLS", 14, 474, MutedColor);

            var hoverButton = new RoundedButton
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Position = new ScalableVector2(-240, 508),
                Size = new ScalableVector2(54, 44),
                CornerRadius = 9,
                Tint = PurpleColor,
                ExpandLabelOnHover = true,
                HoverExpansionDuration = 150
            };
            hoverButton.SetIcon(WobbleAssets.WhiteBox, new Vector2(16, 16));
            hoverButton.SetLabel(FontManager.GetWobbleFont("inter-semibold"), "HOVER TIMING", 15, Color.White);

            var textbox = new Textbox(new ScalableVector2(320, 42),
                FontManager.GetWobbleFont("inter-medium"), 16, string.Empty, "Type text, then press Ctrl+A")
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Position = new ScalableVector2(210, 508),
                Tint = PanelColor
            };

            CreateText("Hover the button; use the textbox selection to compare feedback at each rate.",
                14, 568, MutedColor);
        }

        private Container CreatePanel(float x, float y, string title)
        {
            var panel = new Container
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Position = new ScalableVector2(x, y),
                Size = new ScalableVector2(330, 126)
            };

            new Sprite
            {
                Parent = panel,
                Size = panel.Size,
                Tint = PanelColor
            };

            new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), title, 13)
            {
                Parent = panel,
                Alignment = Alignment.TopCenter,
                Y = 12,
                Tint = MutedColor
            };

            return panel;
        }

        private SpriteTextPlus CreateText(string text, int size, float y, Color color,
            string font = "inter-medium") => new SpriteTextPlus(FontManager.GetWobbleFont(font), text, size)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = y,
            Tint = color
        };

        private void ApplyPreset(int? fps, int? ups)
        {
            SetSelectorRate(FpsSelector, fps);
            SetSelectorRate(UpsSelector, ups);
            ApplySelectedRates();
        }

        private static void SetSelectorRate(HorizontalSelector selector, int? rate)
        {
            var value = rate?.ToString() ?? "Unlimited";
            var index = selector.Options.IndexOf(value);

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(rate), rate, "Unsupported timing-test rate.");

            selector.SelectedIndex = index;
            selector.SelectedItemText.Text = value;
        }

        private void ApplySelectedRates()
        {
            if (FpsSelector == null || UpsSelector == null)
                return;

            TestGame?.SetTestFrameRates(ParseRate(FpsSelector), ParseRate(UpsSelector));
            _rateSampleTimestamp = Stopwatch.GetTimestamp();
            _sampledUpdates = 0;
            _sampledDraws = 0;
        }

        private static int? ParseRate(HorizontalSelector selector) =>
            selector.Options[selector.SelectedIndex] == "Unlimited"
                ? (int?) null
                : int.Parse(selector.Options[selector.SelectedIndex]);

        public override void Update(GameTime gameTime)
        {
            _sampledUpdates++;
            var elapsedMilliseconds = gameTime.ElapsedGameTime.TotalMilliseconds;

            UpdateAutomaticTargets(elapsedMilliseconds);
            UpdateExpectedTint(elapsedMilliseconds);
            UpdateExpectedFrame(elapsedMilliseconds);

            TintSprite.FadeToColor(_tintTarget, elapsedMilliseconds, TintTimeConstant);
            Container?.Update(gameTime);

            CheckMovementCompletion();
            CheckTintResult();
            _framePositionMatches = FrameSprite.CurrentFrame == _expectedFrame;
            UpdateMeasuredRates();

            _diagnosticRefreshTimer += elapsedMilliseconds;
            if (_diagnosticRefreshTimer >= 100)
            {
                _diagnosticRefreshTimer %= 100;
                RefreshDiagnostics();
            }
        }

        private void UpdateAutomaticTargets(double elapsedMilliseconds)
        {
            _targetChangeTimer += elapsedMilliseconds;

            while (_targetChangeTimer >= TargetChangeInterval)
            {
                _targetChangeTimer -= TargetChangeInterval;
                _tintTarget = _tintTarget == TintEnd ? TintStart : TintEnd;
                ProgressValue.Value = ProgressValue.Value > 0 ? 0 : 100;
                _maximumTintError = 0;
            }
        }

        private void UpdateExpectedTint(double elapsedMilliseconds)
        {
            var amount = elapsedMilliseconds <= 0
                ? 0
                : (float) (1 - Math.Exp(-elapsedMilliseconds / TintTimeConstant));
            _expectedTint = Vector3.Lerp(_expectedTint, _tintTarget.ToVector3(), amount);
        }

        private void UpdateExpectedFrame(double elapsedMilliseconds)
        {
            _expectedFrameRemainder += elapsedMilliseconds;

            while (_expectedFrameRemainder >= FrameTime)
            {
                _expectedFrameRemainder -= FrameTime;
                _expectedFrame = (_expectedFrame + 1) % FrameSprite.Frames.Count;
            }
        }

        private void CheckTintResult()
        {
            var expected = new Color(
                (int) Math.Round(_expectedTint.X * byte.MaxValue),
                (int) Math.Round(_expectedTint.Y * byte.MaxValue),
                (int) Math.Round(_expectedTint.Z * byte.MaxValue));
            var error = Math.Max(Math.Abs(expected.R - TintSprite.Tint.R),
                Math.Max(Math.Abs(expected.G - TintSprite.Tint.G),
                    Math.Abs(expected.B - TintSprite.Tint.B)));
            _maximumTintError = Math.Max(_maximumTintError, error);
        }

        private void StartMovement()
        {
            MotionSprite.ClearAnimations();
            MotionSprite.MoveToX(_movementToRight ? MovementRight : MovementLeft, Easing.Linear,
                MovementDuration);
            _movementStartTimestamp = Stopwatch.GetTimestamp();
        }

        private void CheckMovementCompletion()
        {
            if (MotionSprite.Animations.Count == 0 || !MotionSprite.Animations[0].Done)
                return;

            _lastMovementDuration = Stopwatch.GetElapsedTime(_movementStartTimestamp).TotalMilliseconds;
            var updateTolerance = TestGame?.TestTargetUps is int ups ? 1000d / ups : 0;
            _movementPassed = Math.Abs(_lastMovementDuration - MovementDuration) <= 35 + updateTolerance;
            _movementResultAvailable = true;
            _movementToRight = !_movementToRight;
            StartMovement();
        }

        private void UpdateMeasuredRates()
        {
            if (_rateSampleTimestamp == 0)
                _rateSampleTimestamp = Stopwatch.GetTimestamp();

            var elapsed = Stopwatch.GetElapsedTime(_rateSampleTimestamp).TotalSeconds;
            if (elapsed < 1)
                return;

            _measuredUps = (int) Math.Round(_sampledUpdates / elapsed);
            _measuredFps = (int) Math.Round(_sampledDraws / elapsed);
            _sampledUpdates = 0;
            _sampledDraws = 0;
            _rateSampleTimestamp = Stopwatch.GetTimestamp();

            var targetFps = TestGame?.TestTargetFps?.ToString() ?? "Unlimited";
            var targetUps = TestGame?.TestTargetUps?.ToString() ?? "Unlimited";
            MeasuredRatesText.Text =
                $"Requested: {targetFps} FPS / {targetUps} UPS     Measured: {_measuredFps} FPS / {_measuredUps} UPS";
        }

        private void RefreshDiagnostics()
        {
            var movementResult = !_movementResultAvailable
                ? "MOVEMENT: WARMING UP"
                : $"MOVEMENT: {(_movementPassed ? "PASS" : "FAIL")} ({_lastMovementDuration:0.0} ms)";
            var tintPassed = _maximumTintError <= 1;
            var frameResult = _framePositionMatches ? "PASS" : "FAIL";

            MovementStatusText.Text = movementResult;
            MovementStatusText.Tint = !_movementResultAvailable
                ? MutedColor
                : _movementPassed ? SuccessColor : FailureColor;

            DiagnosticsText.Text =
                $"TINT: {(tintPassed ? "PASS" : "FAIL")} (max error {_maximumTintError})     " +
                $"SPRITE FRAME: {frameResult} ({FrameSprite.CurrentFrame}/{_expectedFrame})";
            DiagnosticsText.Tint = tintPassed && _framePositionMatches ? SuccessColor : FailureColor;
        }

        public override void Draw(GameTime gameTime)
        {
            _sampledDraws++;
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container?.Draw(gameTime);
        }

        public override void Destroy()
        {
            TestGame?.ResetTestFrameRates();
            ProgressValue?.Dispose();
            Container?.Destroy();
        }
    }
}
