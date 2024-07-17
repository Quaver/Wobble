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
        ///     Indicates whether the layer will be drawn.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        ///     The layers that need to be drawn above this layer
        /// </summary>
        private readonly HashSet<Layer> requiredUpperLayers = new HashSet<Layer>();

        /// <summary>
        ///     The layers that need to be drawn below this layer
        /// </summary>
        private readonly HashSet<Layer> requiredLowerLayers = new HashSet<Layer>();

        /// <summary>
        ///     Stores the data required to run Tarjan's strongly connected components algorithm
        /// </summary>
        internal TarjanData LayerTarjanData;

        private readonly List<Drawable> drawables = new List<Drawable>();
        private readonly LayerManager layerManager;

        internal Layer(string name, LayerManager layerManager)
        {
            this.layerManager = layerManager;
            Name = name;
        }

        private void AddRequiredUpperLayer(Layer upperLayer)
        {
            requiredUpperLayers.Add(upperLayer);
            upperLayer.requiredLowerLayers.Add(this);
        }

        internal bool RemoveRequiredUpperLayer(Layer upperLayer)
        {
            upperLayer.requiredLowerLayers.Remove(this);
            return requiredUpperLayers.Remove(upperLayer);
        }

        /// <summary>
        ///     Tries to add a constraint that this layer should be drawn below <see cref="upperLayer"/>
        /// </summary>
        /// <param name="upperLayer"></param>
        /// <returns>Whether the constraint is successfully applied. If not, nothing will be changed.</returns>
        public bool RequireBelow(Layer upperLayer)
        {
            // Don't allow different layer managers
            if (upperLayer.layerManager != layerManager)
                return false;

            // Don't allow adding layers above the Top layer, or adding layers below the Bottom layer
            if (LayerFlags.HasFlag(LayerFlags.Top) || upperLayer.LayerFlags.HasFlag(LayerFlags.Bottom))
                return false;

            AddRequiredUpperLayer(upperLayer);
            var cycles = layerManager.RecalculateZValues();
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
            RemoveRequiredUpperLayer(upperLayer);
            layerManager.RecalculateZValues();
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
            if (!RemoveRequiredUpperLayer(upperLayer))
                return false;
            layerManager.RecalculateZValues();
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


        /// <summary>
        ///     Wraps the layer between two layers, {<see cref="Name"/>}.Lower and {<see cref="Name"/>}.Upper
        ///     Any constraint applied to it will be moved to the new lower and upper layer.
        ///     This is useful if you want to put something immediately above or below the layer.
        /// </summary>
        /// <returns></returns>
        public (Layer LowerLayer, Layer UpperLayer) Wrap()
        {
            var lowerLayer = layerManager.NewLayer($"{Name}.Lower");
            var upperLayer = layerManager.NewLayer($"{Name}.Upper");

            foreach (var requiredLowerLayer in requiredLowerLayers)
            {
                requiredLowerLayer.requiredUpperLayers.Remove(this);
                requiredLowerLayer.requiredUpperLayers.Add(lowerLayer);
                lowerLayer.requiredLowerLayers.Add(lowerLayer);
            }

            requiredLowerLayers.Clear();
            requiredLowerLayers.Add(lowerLayer);

            foreach (var requiredUpperLayer in requiredUpperLayers)
            {
                requiredUpperLayer.requiredLowerLayers.Remove(this);
                requiredUpperLayer.requiredLowerLayers.Add(upperLayer);
                upperLayer.requiredUpperLayers.Add(requiredUpperLayer);
            }

            requiredUpperLayers.Clear();
            requiredUpperLayers.Add(upperLayer);

            lowerLayer.requiredUpperLayers.Add(this);
            upperLayer.requiredLowerLayers.Add(this);

            layerManager.RecalculateZValues();
            return (lowerLayer, upperLayer);
        }

        /// <summary>
        ///     Removes every relation to this layer. The layer's constraints are applied to its upper and lower layers
        ///     to preserve order.
        /// </summary>
        /// <returns></returns>
        public bool Isolate()
        {
            if (LayerFlags.HasFlag(LayerFlags.Top) || LayerFlags.HasFlag(LayerFlags.Bottom))
                return false;

            foreach (var requiredLowerLayer in requiredLowerLayers)
            {
                requiredLowerLayer.requiredUpperLayers.Remove(this);
                requiredLowerLayer.requiredUpperLayers.UnionWith(requiredUpperLayers);
            }


            foreach (var requiredUpperLayer in requiredUpperLayers)
            {
                requiredUpperLayer.requiredLowerLayers.Remove(this);
                requiredUpperLayer.requiredLowerLayers.UnionWith(requiredLowerLayers);
            }

            requiredLowerLayers.Clear();
            requiredUpperLayers.Clear();

            layerManager.RecalculateZValues();
            return true;
        }

        internal void AddDrawable(Drawable drawable)
        {
            if (LayerFlags.HasFlag(LayerFlags.NoChildren))
                throw new InvalidOperationException(
                    $"Cannot add drawable to layer '{Name}' since it's flagged NoChildren");
            drawables.Add(drawable);
        }

        internal void RemoveDrawable(Drawable drawable) => drawables.Remove(drawable);

        public void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            foreach (var drawable in drawables)
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

            foreach (var layer in requiredUpperLayers)
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
                drawables.Count <= 10 ? string.Join(", ", drawables.Select(d => d.GetType().Name)) : "";
            Logger.Debug($"Layer '{Name}' ({LayerTarjanData.Order}): {drawables.Count} drawables {drawablesDump}",
                LogType.Runtime);
        }

        public void Destroy()
        {
            drawables.Clear();
            layerManager?.RemoveLayer(this);
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