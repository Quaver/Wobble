using Microsoft.Xna.Framework;

namespace Wobble.Graphics
{
    public class LayeredContainer : Container
    {
        /// <summary>
        ///     Children with a set layer will be drawn by a separate layer manager
        /// </summary>
        public LayerManager LayerManager { get; private set; }

        public LayeredContainer()
        {
            InitializeLayerManager();
        }

        protected virtual void InitializeLayerManager()
        {
            LayerManager = new LayerManager();
            LayerManager.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            LayerManager.DrawBackground(gameTime);
            base.Draw(gameTime);
            LayerManager.DrawDefault(gameTime);
            LayerManager.DrawForeground(gameTime);
        }

        public override void Destroy()
        {
            base.Destroy();
            LayerManager?.Destroy();
        }
    }
}