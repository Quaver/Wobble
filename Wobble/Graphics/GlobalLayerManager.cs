namespace Wobble.Graphics
{
    public class GlobalLayerManager : LayerManager
    {
        

        /// <summary>
        ///     Layer to draw the cursor
        /// </summary>
        public Layer CursorLayer { get; private set; }

        /// <summary>
        ///     Global UI layer
        /// </summary>
        public Layer GlobalUILayer { get; private set; }

        /// <summary>
        ///     Layer to draw dialogs
        /// </summary>
        public Layer DialogLayer { get; private set; }

        /// <summary>
        ///     UI layer of the current screen
        /// </summary>
        public Layer UILayer { get; private set; }

        /// <summary>
        ///     Layer to draw background images
        /// </summary>
        public Layer BackgroundLayer { get; private set; }

        protected override void InitializeLayers()
        {
            GlobalUILayer = NewLayer("GlobalUI");
            DialogLayer = NewLayer("Dialog");
            UILayer = NewLayer("UI");
            CursorLayer = NewLayer("Cursor");
            BackgroundLayer = NewLayer("Background");

            TopLayer.LayerFlags = LayerFlags.Top | LayerFlags.NoChildren;
            BottomLayer.LayerFlags = LayerFlags.Bottom | LayerFlags.NoChildren;

            RequireOrder(new[]
            {
                TopLayer,
                CursorLayer,
                GlobalUILayer,
                DialogLayer,
                UILayer,
                DefaultLayer,
                BackgroundLayer,
                BottomLayer
            });
        }
    }
}