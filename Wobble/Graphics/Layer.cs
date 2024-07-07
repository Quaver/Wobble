using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Wobble.Logging;

namespace Wobble.Graphics
{
    public class Layer
    {
        /// <summary>
        ///     Unique identifier of the layer
        /// </summary>
        public string Name { get; }

        public LayerFlags LayerFlags { get; set; } = LayerFlags.None;

        /// <summary>
        ///     The layers that need to be drawn above this layer
        /// </summary>
        internal readonly HashSet<Layer> RequiredLayersAbove = new HashSet<Layer>();

        /// <summary>
        ///     Stores the data required to run Tarjan's strongly connected components algorithm
        /// </summary>
        internal TarjanData LayerTarjanData;

        private readonly List<Drawable> _drawables = new List<Drawable>();
        private readonly LayerManager _layerManager;

        internal Layer(string name, LayerManager layerManager)
        {
            _layerManager = layerManager;
            Name = name;
        }

        /// <summary>
        ///     Tries to add a constraint that this layer should be drawn below <see cref="upperLayer"/>
        /// </summary>
        /// <param name="upperLayer"></param>
        /// <returns>Whether the constraint is successfully applied. If not, nothing will be changed.</returns>
        public bool RequireBelow(Layer upperLayer)
        {
            // Don't allow different layer managers
            if (upperLayer._layerManager != _layerManager)
                return false;

            // Don't allow adding layers above the Top layer, or adding layers below the Bottom layer
            if (LayerFlags.HasFlag(LayerFlags.Top) || upperLayer.LayerFlags.HasFlag(LayerFlags.Bottom))
                return false;

            RequiredLayersAbove.Add(upperLayer);
            var cycles = _layerManager.RecalculateZValues();
            if (cycles.Count <= 0)
                return true;

            // Build a representation of the cycle formed
            var sb = new StringBuilder();
            foreach (var cycle in cycles)
            {
                foreach (var layer in cycle)
                {
                    sb.Append(layer.Name);
                    sb.Append(" -> ");
                }

                sb.Append(cycle[0].Name);
                sb.Append(", ");
            }

            Logger.Warning(
                $"Unable to make layer '{Name}' below layer '{upperLayer.Name}'" +
                $" since the following cycle(s) would be introduced: {sb}",
                LogType.Runtime);

            // Revert the changes
            RequiredLayersAbove.Remove(upperLayer);
            _layerManager.RecalculateZValues();
            return false;
        }

        /// <summary>
        ///     Tries to add a constraint that this layer should be drawn above <see cref="lowerLayer"/>
        /// </summary>
        /// <param name="lowerLayer"></param>
        public bool RequireAbove(Layer lowerLayer)
        {
            return lowerLayer.RequireBelow(this);
        }

        /// <summary>
        ///     Removes the constraint that this layer should be below <see cref="upperLayer"/>
        /// </summary>
        /// <param name="upperLayer"></param>
        /// <returns>Whether this constraint was present before calling</returns>
        public bool StopRequireBelow(Layer upperLayer)
        {
            if (!RequiredLayersAbove.Remove(upperLayer))
                return false;
            _layerManager.RecalculateZValues();
            return true;
        }

        /// <summary>
        ///     Removes the constraint that this layer should be above <see cref="lowerLayer"/>
        /// </summary>
        /// <param name="lowerLayer"></param>
        /// <returns></returns>
        public bool StopRequireAbove(Layer lowerLayer)
        {
            return lowerLayer.StopRequireBelow(this);
        }

        /// <summary>
        ///     Tries to add a constraint that this layer should be drawn between <see cref="lowerLayer"/> and <see cref="upperLayer"/>.
        /// </summary>
        /// <param name="lowerLayer"></param>
        /// <param name="upperLayer"></param>
        /// <returns></returns>
        public bool RequireBetween(Layer lowerLayer, Layer upperLayer)
        {
            if (!RequireAbove(lowerLayer))
                return false;
            if (!RequireBelow(upperLayer))
            {
                StopRequireAbove(lowerLayer);
                return false;
            }

            return true;
        }

        internal void AddDrawable(Drawable drawable)
        {
            if (LayerFlags.HasFlag(LayerFlags.NoChildren))
                throw new InvalidOperationException(
                    $"Cannot add drawable to layer '{Name}' since it's flagged NoChildren");
            _drawables.Add(drawable);
        }

        internal void RemoveDrawable(Drawable drawable) => _drawables.Remove(drawable);

        public void Draw(GameTime gameTime)
        {
            foreach (var drawable in _drawables)
            {
                drawable.Draw(gameTime);
            }
        }

        internal void ResetTarjanData() => LayerTarjanData = new TarjanData { Index = -1 };

        /// <summary>
        ///     Part of Tarjan's SCC algorithm.
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <param name="order"></param>
        /// <param name="stack"></param>
        /// <param name="stronglyConnectedComponents"></param>
        internal void StrongConnect(ref int currentIndex, ref int order,
            Stack<Layer> stack,
            List<List<Layer>> stronglyConnectedComponents)
        {
            LayerTarjanData.LowLink = currentIndex;
            LayerTarjanData.Index = currentIndex;
            currentIndex++;
            stack.Push(this);
            LayerTarjanData.InStack = true;

            foreach (var layer in RequiredLayersAbove)
            {
                if (layer.LayerTarjanData.Index == -1)
                {
                    layer.StrongConnect(ref currentIndex, ref order, stack, stronglyConnectedComponents);
                    LayerTarjanData.LowLink = Math.Min(LayerTarjanData.LowLink, layer.LayerTarjanData.LowLink);
                }
                else if (layer.LayerTarjanData.InStack)
                {
                    LayerTarjanData.LowLink = Math.Min(LayerTarjanData.LowLink, layer.LayerTarjanData.Index);
                }
            }

            if (LayerTarjanData.LowLink != LayerTarjanData.Index)
                return;

            LayerTarjanData.Order = order++;
            var stronglyConnectComponent = new List<Layer>();
            Layer v;
            do
            {
                v = stack.Pop();
                v.LayerTarjanData.InStack = false;
                v.LayerTarjanData.Order = LayerTarjanData.Order;
                stronglyConnectComponent.Add(v);
            } while (v != this);

            stronglyConnectedComponents.Add(stronglyConnectComponent);
        }

        public void Dump()
        {
            var drawablesDump =
                _drawables.Count <= 10 ? string.Join(", ", _drawables.Select(d => d.GetType().Name)) : "";
            Logger.Debug($"Layer '{Name}' ({LayerTarjanData.Order}): {_drawables.Count} drawables {drawablesDump}",
                LogType.Runtime);
        }

        public void Destroy()
        {
            _drawables.Clear();
            _layerManager?.RemoveLayer(this);
        }

        ~Layer()
        {
            Destroy();
        }

        internal struct TarjanData
        {
            /// <summary>
            ///     Whether the layer was in the stack
            /// </summary>
            public bool InStack;

            /// <summary>
            ///     The order the layer is visited in DFS
            /// </summary>
            public int Index;

            /// <summary>
            ///     The lowest order of the layer reachable from (i.e. above) this layer
            /// </summary>
            public int LowLink;

            /// <summary>
            ///     The order of the SCC. This is the reverse index of the layer when topologically sorted.
            /// </summary>
            public int Order;
        }
    }
}