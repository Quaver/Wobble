using Wobble.Screens;

namespace GreenBox.Screens
{
    public class GreenBoxScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     In the constructor, set the view to this screen's implemented view.
        /// </summary>
        public GreenBoxScreen() => View = new GreenBoxScreenView(this);
    }
}
