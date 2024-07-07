using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Wobble.Logging;
using Wobble.Screens;

namespace Wobble.Graphics
{
    public class LayerManager
    {
        /// <summary>
        ///     Top-most layer that should have NO children
        /// </summary>
        public Layer TopLayer { get; private set; }

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
        ///     Default layer 
        /// </summary>
        public Layer DefaultLayer { get; private set; }

        /// <summary>
        ///     Layer to draw background images
        /// </summary>
        public Layer BackgroundLayer { get; private set; }

        /// <summary>
        ///     Bottom-most layer that should contain NO children
        /// </summary>
        public Layer BottomLayer { get; private set; }

        /// <summary>
        ///     Readonly view of the layers
        /// </summary>
        public IReadOnlyDictionary<string, Layer> Layers => new ReadOnlyDictionary<string, Layer>(_layers);

        private readonly Dictionary<string, Layer> _layers = new Dictionary<string, Layer>();

        private readonly List<Layer> _sortedLayers = new List<Layer>();

        private int _defaultLayerIndex;

        /// <summary>
        ///     Sets up basic layers, including topmost and bottom-most layer.
        /// </summary>
        public void Initialize()
        {
            DefaultLayer = NewGlobalLayer("Default");
            TopLayer = NewGlobalLayer("Top");
            GlobalUILayer = NewGlobalLayer("GlobalUI");
            DialogLayer = NewGlobalLayer("Dialog");
            UILayer = NewGlobalLayer("UI");
            CursorLayer = NewGlobalLayer("Cursor");
            BackgroundLayer = NewGlobalLayer("Background");
            BottomLayer = NewGlobalLayer("Bottom");

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

            RecalculateZValues();

            ScreenManager.ScreenChanged += ScreenChanged;
        }

        /// <summary>
        ///     Creates a layer that persists among screens
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Layer NewGlobalLayer(string name)
        {
            if (_layers.TryGetValue(name, out var layer))
                return layer;
            layer = new Layer(name, this, _ => true);
            _layers.TryAdd(name, layer);
            return layer;
        }

        /// <summary>
        ///     Creates a layers that persists in one screen only
        /// </summary>
        /// <param name="name"></param>
        /// <param name="screen"></param>
        /// <returns></returns>
        public Layer NewLayer(string name, Screen screen)
        {
            if (_layers.TryGetValue(name, out var layer))
                return layer;
            layer = new Layer(name, this, s => s == screen);
            _layers.TryAdd(name, layer);
            return layer;
        }

        /// <summary>
        ///     Removes the layer and any constraints about it
        /// </summary>
        /// <param name="layerToRemove"></param>
        public void RemoveLayer(Layer layerToRemove)
        {
            _layers.Remove(layerToRemove.Name);
            foreach (var (_, layer) in _layers)
            {
                layer.RequiredLayersAbove.Remove(layerToRemove);
            }

            RecalculateZValues();
        }

        /// <summary>
        ///     Requires that the layers in <see cref="layersTopToBottom"/> will be ordered top to bottom from first to last.
        /// </summary>
        /// <param name="layersTopToBottom"></param>
        public static void RequireOrder(IReadOnlyList<Layer> layersTopToBottom)
        {
            if (layersTopToBottom.Count < 2)
                return;

            for (var i = 0; i < layersTopToBottom.Count - 1; i++)
            {
                layersTopToBottom[i].RequireAbove(layersTopToBottom[i + 1]);
            }
        }

        private void ScreenChanged(object sender, Screen screen)
        {
            var layersToRemove = new HashSet<Layer>();
            foreach (var layer in _layers.Values)
            {
                if (!layer.ShouldPersistIn(screen))
                    layersToRemove.Add(layer);
            }

            foreach (var layer in layersToRemove)
            {
                RemoveLayer(layer);
            }
        }

        /// <summary>
        ///     Runs Tarjan's SCC algorithm to do the following things in O(|V| + |E|) time:
        ///     * Find any SCC and thus cycle. It's trivial that an SCC must contain at least one cycle.
        ///     * Build a topologically sorted list of layers, ordered from top-most to bottom-most.
        /// </summary>
        /// <returns>List of SCCs that contain more than one layer (i.e. cycles)</returns>
        internal List<List<Layer>> RecalculateZValues()
        {
            var cycles = new List<List<Layer>>();
            if (_layers.Count == 0)
                return cycles;

            var currentIndex = 0;
            var order = 0;
            var stack = new Stack<Layer>();
            var stronglyConnectedComponents = new List<List<Layer>>();

            foreach (var layer in _layers.Values)
                layer.ResetTarjanData();

            foreach (var layer in _layers.Values)
            {
                if (layer.LayerTarjanData.Index == -1)
                    layer.StrongConnect(ref currentIndex, ref order, stack, stronglyConnectedComponents);
            }

            _sortedLayers.Clear();
            foreach (var stronglyConnectedComponent in stronglyConnectedComponents)
            {
                _sortedLayers.AddRange(stronglyConnectedComponent);
                if (stronglyConnectedComponent.Count <= 1)
                    continue;
                cycles.Add(stronglyConnectedComponent);
            }

            return cycles;
        }

        /// <summary>
        ///     Draws all layers that are below the Default layer
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawBackground(GameTime gameTime)
        {
            for (var i = _sortedLayers.Count - 1; i >= 0; i--)
            {
                var layer = _sortedLayers[i];
                if (layer == DefaultLayer)
                {
                    _defaultLayerIndex = i;
                    return;
                }

                layer.Draw(gameTime);
            }
        }

        /// <summary>
        ///     Draws the Default layer
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawDefault(GameTime gameTime) => DefaultLayer.Draw(gameTime);

        public void DrawAll(GameTime gameTime)
        {
            DrawBackground(gameTime);
            DrawDefault(gameTime);
            DrawForeground(gameTime);
        }

        /// <summary>
        ///     Draws all layers that are above the Default layer
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawForeground(GameTime gameTime)
        {
            for (var i = _defaultLayerIndex - 1; i >= 0; i--)
            {
                var layer = _sortedLayers[i];
                layer.Draw(gameTime);
            }
        }

        public void Dump()
        {
            Logger.Debug($"{_layers.Count} Layers:", LogType.Runtime);
            foreach (var layer in _sortedLayers)
            {
                layer.Dump();
            }
        }

        ~LayerManager()
        {
            ScreenManager.ScreenChanged -= ScreenChanged;
        }
    }
}