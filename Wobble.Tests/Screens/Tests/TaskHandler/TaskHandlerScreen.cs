using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.TaskHandler
{
    public class TaskHandlerScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TaskHandlerScreen() => View = new TaskHandlerScreenView(this);
    }
}
