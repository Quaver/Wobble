using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics
{
    public enum FlexDirection
    {
        Row,
        RowReverse,
        Column,
        ColumnReverse
    }

    public enum FlexWrap
    {
        NoWrap,
        Wrap,
        WrapReverse
    }

    public enum FlexJustifyContent
    {
        FlexStart,
        Center,
        FlexEnd,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly
    }

    public enum FlexAlignItems
    {
        FlexStart,
        Center,
        FlexEnd,
        Stretch
    }

    public enum FlexAlignContent
    {
        FlexStart,
        Center,
        FlexEnd,
        Stretch,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly
    }

    public enum FlexAlignSelf
    {
        Auto,
        FlexStart,
        Center,
        FlexEnd,
        Stretch
    }

    public sealed class FlexItemOptions
    {
        private float _grow;
        private float _shrink = 1;
        private float? _basis;
        private int _order;
        private FlexAlignSelf _alignSelf = FlexAlignSelf.Auto;

        internal event EventHandler Changed;

        public float Grow
        {
            get => _grow;
            set => SetProperty(ref _grow, value);
        }

        public float Shrink
        {
            get => _shrink;
            set => SetProperty(ref _shrink, value);
        }

        public float? Basis
        {
            get => _basis;
            set => SetProperty(ref _basis, value);
        }

        public int Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public FlexAlignSelf AlignSelf
        {
            get => _alignSelf;
            set => SetProperty(ref _alignSelf, value);
        }

        private void SetProperty<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Arranges its direct children using CSS-style flexbox rules.
    /// </summary>
    public class FlexContainer : Container
    {
        private sealed class ItemState
        {
            public Drawable Drawable { get; }
            public float NaturalWidth { get; set; }
            public float NaturalHeight { get; set; }
            public float? LayoutWidth { get; set; }
            public float? LayoutHeight { get; set; }

            public ItemState(Drawable drawable)
            {
                Drawable = drawable;
                NaturalWidth = drawable.Width;
                NaturalHeight = drawable.Height;
            }
        }

        private sealed class LayoutItem
        {
            public ItemState State { get; set; }
            public FlexItemOptions Options { get; set; }
            public int InsertionIndex { get; set; }
            public float MainSize { get; set; }
            public float CrossSize { get; set; }
        }

        private sealed class FlexLine
        {
            public List<LayoutItem> Items { get; } = new List<LayoutItem>();
            public float CrossSize { get; set; }
        }

        private readonly Dictionary<Drawable, FlexItemOptions> _itemOptions =
            new Dictionary<Drawable, FlexItemOptions>();

        private readonly Dictionary<Drawable, ItemState> _itemStates =
            new Dictionary<Drawable, ItemState>();

        private readonly List<Drawable> _lastChildOrder = new List<Drawable>();

        private FlexDirection _direction = FlexDirection.Row;
        private FlexWrap _wrap = FlexWrap.NoWrap;
        private FlexJustifyContent _justifyContent = FlexJustifyContent.FlexStart;
        private FlexAlignItems _alignItems = FlexAlignItems.Stretch;
        private FlexAlignContent _alignContent = FlexAlignContent.Stretch;
        private float _rowGap;
        private float _columnGap;
        private bool _layoutDirty = true;
        private bool _isLayouting;

        public FlexDirection Direction
        {
            get => _direction;
            set => SetLayoutProperty(ref _direction, value);
        }

        public FlexWrap Wrap
        {
            get => _wrap;
            set => SetLayoutProperty(ref _wrap, value);
        }

        public FlexJustifyContent JustifyContent
        {
            get => _justifyContent;
            set => SetLayoutProperty(ref _justifyContent, value);
        }

        public FlexAlignItems AlignItems
        {
            get => _alignItems;
            set => SetLayoutProperty(ref _alignItems, value);
        }

        public FlexAlignContent AlignContent
        {
            get => _alignContent;
            set => SetLayoutProperty(ref _alignContent, value);
        }

        public float Gap
        {
            get => RowGap == ColumnGap ? RowGap : 0;
            set
            {
                value = Math.Max(0, value);
                if (NearlyEqual(_rowGap, value) && NearlyEqual(_columnGap, value))
                    return;

                _rowGap = value;
                _columnGap = value;
                _layoutDirty = true;
            }
        }

        public float RowGap
        {
            get => _rowGap;
            set => SetGap(ref _rowGap, value);
        }

        public float ColumnGap
        {
            get => _columnGap;
            set => SetGap(ref _columnGap, value);
        }

        public FlexContainer()
        {
            SizeChanged += OnContainerSizeChanged;
        }

        public FlexContainer(ScalableVector2 size, ScalableVector2 position) : base(size, position)
        {
            SizeChanged += OnContainerSizeChanged;
        }

        public FlexContainer(float x, float y, float width, float height) : base(x, y, width, height)
        {
            SizeChanged += OnContainerSizeChanged;
        }

        public void SetItemOptions(Drawable child, FlexItemOptions options)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (child.Parent != this)
                throw new ArgumentException("The drawable must be a direct child of this flex container.", nameof(child));

            if (_itemOptions.TryGetValue(child, out var previous))
                previous.Changed -= OnItemOptionsChanged;

            _itemOptions[child] = options;
            options.Changed += OnItemOptionsChanged;
            TrackChild(child);
            _layoutDirty = true;
        }

        public bool RemoveItemOptions(Drawable child)
        {
            if (child == null)
                return false;

            var removed = _itemOptions.TryGetValue(child, out var options);
            if (removed)
            {
                options.Changed -= OnItemOptionsChanged;
                _itemOptions.Remove(child);
            }
            _layoutDirty |= removed;
            return removed;
        }

        public void RefreshLayout()
        {
            SynchronizeChildren();
            LayoutChildren();
        }

        public override void Update(GameTime gameTime)
        {
            SynchronizeChildren();
            if (_layoutDirty)
                LayoutChildren();

            base.Update(gameTime);

            SynchronizeChildren();
            if (_layoutDirty)
                LayoutChildren();
        }

        public override void Destroy()
        {
            SizeChanged -= OnContainerSizeChanged;
            foreach (var state in _itemStates.Values)
                state.Drawable.SizeChanged -= OnChildSizeChanged;
            foreach (var options in _itemOptions.Values)
                options.Changed -= OnItemOptionsChanged;

            _itemStates.Clear();
            _itemOptions.Clear();
            _lastChildOrder.Clear();
            base.Destroy();
        }

        private void SynchronizeChildren()
        {
            var orderedChildren = Children.Where(x => x != null && x != Border).ToList();
            var current = new HashSet<Drawable>(orderedChildren);

            if (!_lastChildOrder.SequenceEqual(orderedChildren))
            {
                _lastChildOrder.Clear();
                _lastChildOrder.AddRange(orderedChildren);
                _layoutDirty = true;
            }

            foreach (var child in current)
                TrackChild(child);

            foreach (var removed in _itemStates.Keys.Where(x => !current.Contains(x)).ToArray())
            {
                var state = _itemStates[removed];
                removed.SizeChanged -= OnChildSizeChanged;

                if (!removed.IsDisposed && removed.Parent == null)
                {
                    _isLayouting = true;
                    removed.Size = new ScalableVector2(state.NaturalWidth, state.NaturalHeight,
                        removed.Size.X.Scale, removed.Size.Y.Scale);
                    _isLayouting = false;
                }

                _itemStates.Remove(removed);
                if (_itemOptions.TryGetValue(removed, out var options))
                {
                    options.Changed -= OnItemOptionsChanged;
                    _itemOptions.Remove(removed);
                }
                _layoutDirty = true;
            }
        }

        private void TrackChild(Drawable child)
        {
            if (_itemStates.ContainsKey(child))
                return;

            _itemStates[child] = new ItemState(child);
            child.SizeChanged += OnChildSizeChanged;
            _layoutDirty = true;
        }

        private void LayoutChildren()
        {
            if (_isLayouting)
                return;

            _isLayouting = true;
            try
            {
                var row = Direction == FlexDirection.Row || Direction == FlexDirection.RowReverse;
                var reverseMain = Direction == FlexDirection.RowReverse || Direction == FlexDirection.ColumnReverse;
                var reverseCross = Wrap == FlexWrap.WrapReverse;
                var availableMain = Math.Max(0, row ? Width : Height);
                var availableCross = Math.Max(0, row ? Height : Width);
                var mainGap = row ? ColumnGap : RowGap;
                var lineGap = row ? RowGap : ColumnGap;

                var items = CreateLayoutItems(row);
                var lines = CreateLines(items, availableMain, mainGap);
                if (lines.Count == 0)
                {
                    _layoutDirty = false;
                    return;
                }

                foreach (var line in lines)
                    ResolveMainSizes(line, availableMain, mainGap);

                ResolveLineCrossSizes(lines, availableCross, lineGap, out var crossOffset, out var distributedLineGap);

                var lineCrossPosition = crossOffset;
                foreach (var line in lines)
                {
                    LayoutLine(line, row, reverseMain, reverseCross, availableMain, availableCross,
                        mainGap, lineCrossPosition);
                    lineCrossPosition += line.CrossSize + distributedLineGap;
                }

                _layoutDirty = false;
            }
            finally
            {
                _isLayouting = false;
            }
        }

        private List<LayoutItem> CreateLayoutItems(bool row)
        {
            var result = new List<LayoutItem>();
            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child == null || child == Border || !_itemStates.TryGetValue(child, out var state))
                    continue;

                var options = _itemOptions.TryGetValue(child, out var configured)
                    ? configured
                    : new FlexItemOptions();
                var naturalMain = row ? state.NaturalWidth : state.NaturalHeight;
                var naturalCross = row ? state.NaturalHeight : state.NaturalWidth;

                result.Add(new LayoutItem
                {
                    State = state,
                    Options = options,
                    InsertionIndex = i,
                    MainSize = Math.Max(0, options.Basis ?? naturalMain),
                    CrossSize = Math.Max(0, naturalCross)
                });
            }

            return result.OrderBy(x => x.Options.Order).ThenBy(x => x.InsertionIndex).ToList();
        }

        private List<FlexLine> CreateLines(IReadOnlyList<LayoutItem> items, float availableMain, float mainGap)
        {
            var lines = new List<FlexLine>();
            var current = new FlexLine();

            foreach (var item in items)
            {
                var occupied = current.Items.Sum(x => x.MainSize) + mainGap * current.Items.Count;
                if (Wrap != FlexWrap.NoWrap && current.Items.Count > 0 && occupied + item.MainSize > availableMain)
                {
                    lines.Add(current);
                    current = new FlexLine();
                }

                current.Items.Add(item);
                current.CrossSize = Math.Max(current.CrossSize, item.CrossSize);
            }

            if (current.Items.Count > 0)
                lines.Add(current);

            return lines;
        }

        private static void ResolveMainSizes(FlexLine line, float availableMain, float mainGap)
        {
            var gapTotal = mainGap * Math.Max(0, line.Items.Count - 1);
            var free = availableMain - gapTotal - line.Items.Sum(x => x.MainSize);

            if (free > 0)
            {
                var totalGrow = line.Items.Sum(x => Math.Max(0, x.Options.Grow));
                if (totalGrow > 0)
                {
                    foreach (var item in line.Items)
                        item.MainSize += free * Math.Max(0, item.Options.Grow) / totalGrow;
                }
            }
            else if (free < 0)
            {
                var deficit = -free;
                for (var pass = 0; pass < line.Items.Count && deficit > 0.001f; pass++)
                {
                    var shrinkFactor = line.Items.Sum(x => x.MainSize > 0
                        ? Math.Max(0, x.Options.Shrink) * x.MainSize
                        : 0);
                    if (shrinkFactor <= 0)
                        break;

                    var applied = 0f;
                    foreach (var item in line.Items)
                    {
                        var factor = Math.Max(0, item.Options.Shrink) * item.MainSize;
                        if (factor <= 0)
                            continue;

                        var reduction = Math.Min(item.MainSize, deficit * factor / shrinkFactor);
                        item.MainSize -= reduction;
                        applied += reduction;
                    }

                    if (applied <= 0.001f)
                        break;
                    deficit -= applied;
                }
            }
        }

        private void ResolveLineCrossSizes(IReadOnlyList<FlexLine> lines, float availableCross, float lineGap,
            out float offset, out float distributedGap)
        {
            if (lines.Count == 1)
            {
                lines[0].CrossSize = availableCross;
                offset = 0;
                distributedGap = 0;
                return;
            }

            var occupied = lines.Sum(x => x.CrossSize) + lineGap * Math.Max(0, lines.Count - 1);
            var free = Math.Max(0, availableCross - occupied);
            offset = 0;
            distributedGap = lineGap;

            switch (AlignContent)
            {
                case FlexAlignContent.Center:
                    offset = free / 2;
                    break;
                case FlexAlignContent.FlexEnd:
                    offset = free;
                    break;
                case FlexAlignContent.Stretch:
                    var growth = free / lines.Count;
                    foreach (var line in lines)
                        line.CrossSize += growth;
                    break;
                case FlexAlignContent.SpaceBetween:
                    if (lines.Count > 1)
                        distributedGap += free / (lines.Count - 1);
                    break;
                case FlexAlignContent.SpaceAround:
                    distributedGap += free / lines.Count;
                    offset = free / lines.Count / 2;
                    break;
                case FlexAlignContent.SpaceEvenly:
                    distributedGap += free / (lines.Count + 1);
                    offset = free / (lines.Count + 1);
                    break;
            }
        }

        private void LayoutLine(FlexLine line, bool row, bool reverseMain, bool reverseCross,
            float availableMain, float availableCross, float mainGap, float lineCrossPosition)
        {
            var occupied = line.Items.Sum(x => x.MainSize) + mainGap * Math.Max(0, line.Items.Count - 1);
            var free = Math.Max(0, availableMain - occupied);
            GetMainDistribution(line.Items.Count, free, mainGap, out var mainOffset, out var distributedGap);

            var mainPosition = mainOffset;
            foreach (var item in line.Items)
            {
                var alignment = ResolveAlignment(item.Options.AlignSelf);
                var itemCrossSize = alignment == FlexAlignItems.Stretch ? line.CrossSize : item.CrossSize;
                var itemCrossOffset = GetCrossOffset(alignment, line.CrossSize, itemCrossSize);
                var physicalMain = reverseMain
                    ? availableMain - mainPosition - item.MainSize
                    : mainPosition;
                var physicalCross = reverseCross
                    ? availableCross - lineCrossPosition - itemCrossOffset - itemCrossSize
                    : lineCrossPosition + itemCrossOffset;

                var child = item.State.Drawable;
                child.Alignment = Alignment.TopLeft;
                child.Size = row
                    ? new ScalableVector2(item.MainSize, itemCrossSize, child.Size.X.Scale, child.Size.Y.Scale)
                    : new ScalableVector2(itemCrossSize, item.MainSize, child.Size.X.Scale, child.Size.Y.Scale);
                item.State.LayoutWidth = child.Width;
                item.State.LayoutHeight = child.Height;
                child.Position = row
                    ? new ScalableVector2(physicalMain, physicalCross)
                    : new ScalableVector2(physicalCross, physicalMain);

                mainPosition += item.MainSize + distributedGap;
            }
        }

        private void GetMainDistribution(int count, float free, float mainGap,
            out float offset, out float distributedGap)
        {
            offset = 0;
            distributedGap = mainGap;

            switch (JustifyContent)
            {
                case FlexJustifyContent.Center:
                    offset = free / 2;
                    break;
                case FlexJustifyContent.FlexEnd:
                    offset = free;
                    break;
                case FlexJustifyContent.SpaceBetween:
                    if (count > 1)
                        distributedGap += free / (count - 1);
                    break;
                case FlexJustifyContent.SpaceAround:
                    if (count > 0)
                    {
                        distributedGap += free / count;
                        offset = free / count / 2;
                    }
                    break;
                case FlexJustifyContent.SpaceEvenly:
                    distributedGap += free / (count + 1);
                    offset = free / (count + 1);
                    break;
            }
        }

        private FlexAlignItems ResolveAlignment(FlexAlignSelf alignSelf)
        {
            switch (alignSelf)
            {
                case FlexAlignSelf.FlexStart:
                    return FlexAlignItems.FlexStart;
                case FlexAlignSelf.Center:
                    return FlexAlignItems.Center;
                case FlexAlignSelf.FlexEnd:
                    return FlexAlignItems.FlexEnd;
                case FlexAlignSelf.Stretch:
                    return FlexAlignItems.Stretch;
                default:
                    return AlignItems;
            }
        }

        private static float GetCrossOffset(FlexAlignItems alignment, float lineSize, float itemSize)
        {
            switch (alignment)
            {
                case FlexAlignItems.Center:
                    return (lineSize - itemSize) / 2;
                case FlexAlignItems.FlexEnd:
                    return lineSize - itemSize;
                default:
                    return 0;
            }
        }

        private void OnContainerSizeChanged(object sender, ScalableVector2 args)
        {
            if (!_isLayouting)
                _layoutDirty = true;
        }

        private void OnChildSizeChanged(object sender, ScalableVector2 args)
        {
            if (_isLayouting || !(sender is Drawable child) || !_itemStates.TryGetValue(child, out var state))
                return;

            if (!state.LayoutWidth.HasValue || !NearlyEqual(child.Width, state.LayoutWidth.Value))
                state.NaturalWidth = child.Width;
            if (!state.LayoutHeight.HasValue || !NearlyEqual(child.Height, state.LayoutHeight.Value))
                state.NaturalHeight = child.Height;
            _layoutDirty = true;
        }

        private void OnItemOptionsChanged(object sender, EventArgs args) => _layoutDirty = true;

        private void SetGap(ref float field, float value)
        {
            value = Math.Max(0, value);
            if (NearlyEqual(field, value))
                return;

            field = value;
            _layoutDirty = true;
        }

        private void SetLayoutProperty<T>(ref T field, T value) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;
            _layoutDirty = true;
        }

        private static bool NearlyEqual(float first, float second) => Math.Abs(first - second) < 0.001f;
    }
}
