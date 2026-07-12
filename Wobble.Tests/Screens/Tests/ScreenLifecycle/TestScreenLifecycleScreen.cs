using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.ScreenLifecycle
{
    public static class TestScreenLifecycleSession
    {
        private static TestScreenLifecycleScreen RetainedScreen { get; set; }

        public static int Constructed { get; internal set; }
        public static int Activated { get; internal set; }
        public static int Destroyed { get; internal set; }
        public static int ScheduledWork { get; internal set; }
        public static int RemovedChildren { get; internal set; }
        internal static bool Automatic { get; set; }
        internal static int RemainingAutomaticSwitches { get; set; }

        public static Screen Create()
        {
            if (RetainedScreen != null && !RetainedScreen.View.Container.IsDisposed)
                RetainedScreen.Destroy();

            Constructed = 0;
            Activated = 0;
            Destroyed = 0;
            ScheduledWork = 0;
            RemovedChildren = 0;
            Automatic = false;
            RemainingAutomaticSwitches = 0;
            RetainedScreen = new TestScreenLifecycleScreen(true);
            return RetainedScreen;
        }

        internal static void SwitchFrom(TestScreenLifecycleScreen screen)
        {
            var next = screen.IsRetained ? new TestScreenLifecycleScreen(false) : RetainedScreen;
            ScreenManager.ChangeScreen(next, new[] { TestScreenLifecycleScreenView.PersistentKey });
        }

        internal static void Exit(TestScreenLifecycleScreen screen)
        {
            // The retained screen opts back into normal teardown before leaving the test.
            if (RetainedScreen != null)
                RetainedScreen.AutomaticallyDestroyOnScreenSwitch = true;

            Automatic = false;
            RemainingAutomaticSwitches = 0;
            ScreenManager.ChangeScreen(new Selection.SelectionScreen());
            RetainedScreen = null;
        }
    }

    public sealed class TestScreenLifecycleScreen : Screen
    {
        public bool IsRetained { get; }
        public sealed override ScreenView View { get; protected set; }

        internal TestScreenLifecycleScreen(bool retained)
        {
            IsRetained = retained;
            AutomaticallyDestroyOnScreenSwitch = !retained;
            TestScreenLifecycleSession.Constructed++;
            View = new TestScreenLifecycleScreenView(this);
        }

        public override void OnActivated()
        {
            TestScreenLifecycleSession.Activated++;
            ((TestScreenLifecycleScreenView) View).Activate();
        }

        public override void Destroy()
        {
            TestScreenLifecycleSession.Destroyed++;
            base.Destroy();
        }
    }
}
