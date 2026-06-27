using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Window;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Wobble.Graphics.UI.Debugging
{
    public class DebugOverlay : SpriteImGui
    {
        public bool Visible { get; private set; }

#if DEBUG
        private bool showCachedTextBounds;
        private bool showUncachedTextBounds;
        private bool showTextObjectLabels = true;
        private bool showVisibleTextObjectsOnly = true;
        private string textObjectSearch = "";
        private int selectedTextObjectId;
        private int highlightedTextObjectId;
        private bool showDrawableBounds;
        private bool showDrawableLabels = true;
        private bool showSelectedDrawableBoundsOnly;
        private bool showVisibleDrawablesOnly = true;
        private string drawableSearch = "";
        private int selectedDrawableId;
        private int highlightedDrawableId;
#endif

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.F3))
            {
                Visible = !Visible;

                if (!Visible)
                    Button.IsGloballyClickable = true;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            base.Draw(gameTime);
        }

        protected override void RenderImguiLayout()
        {
            ImGui.GetIO().MouseDrawCursor = true;

            ImGui.SetNextWindowPos(new Vector2(12, 12), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vector2(640, 720), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(360, 240), new Vector2(float.MaxValue, float.MaxValue));

            var open = Visible;
            if (ImGui.Begin("Wobble Performance", ref open,
                ImGuiWindowFlags.HorizontalScrollbar))
            {
                RenderTiming();
                ImGui.Separator();
                RenderScene();
                ImGui.Separator();
#if DEBUG
                RenderDrawableControls();
                ImGui.Separator();
#endif
                RenderTextStats();
#if DEBUG
                RenderTextObjectControls();
#endif
                ImGui.Separator();
                RenderRuntime();
                ImGui.Separator();
                RenderImGuiStats();
            }

            ImGui.End();
            Visible = open;

#if DEBUG
            DrawDrawableBounds();
            DrawTextObjectBounds();
#endif

            var io = ImGui.GetIO();
            Button.IsGloballyClickable = !Visible ||
                                         !(io.WantCaptureMouse || ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow));
        }

        public override void Destroy()
        {
            Button.IsGloballyClickable = true;
            base.Destroy();
        }

        private void RenderTiming()
        {
            ImGui.Text($"FPS / UPS: {PerformanceStats.FrameRate} / {PerformanceStats.UpdateRate}");
            ImGui.Text($"Frame: {PerformanceStats.FrameTimeMs:0.00} ms avg {PerformanceStats.AverageFrameTimeMs:0.00} ms");
            ImGui.Text($"Update: {PerformanceStats.UpdateTimeMs:0.00} ms avg {PerformanceStats.AverageUpdateTimeMs:0.00} ms");
            ImGui.Text($"Draw: {PerformanceStats.DrawTimeMs:0.00} ms avg {PerformanceStats.AverageDrawTimeMs:0.00} ms");

            if (ImGui.TreeNode("Phase timings"))
            {
                ImGui.Text($"Input/window: {PerformanceStats.InputUpdateTimeMs:0.00} ms");
                ImGui.Text($"Screen update: {PerformanceStats.ScreenUpdateTimeMs:0.00} ms");
                ImGui.Text($"Global UI update: {PerformanceStats.GlobalUiUpdateTimeMs:0.00} ms");
                ImGui.Text($"Audio/log update: {PerformanceStats.AudioLogUpdateTimeMs:0.00} ms");
                ImGui.Text($"Render targets: {PerformanceStats.ScheduledRenderTargetDrawTimeMs:0.00} ms");
                ImGui.Text($"Screen draw: {PerformanceStats.ScreenDrawTimeMs:0.00} ms");
                ImGui.Text($"Global UI draw: {PerformanceStats.GlobalUiDrawTimeMs:0.00} ms");
                ImGui.Text($"Overlay draw: {PerformanceStats.OverlayDrawTimeMs:0.00} ms");
                ImGui.TreePop();
            }
        }

        private void RenderScene()
        {
            ImGui.Text($"Screen: {PerformanceStats.CurrentScreenName}");
            ImGui.Text($"Drawables drawn: {PerformanceStats.DrawnDrawableCount}");
            ImGui.Text($"Scheduled render targets: {PerformanceStats.ScheduledRenderTargetDrawCount}");
        }

        private void RenderTextStats()
        {
            ImGui.Text("Text / fonts per second");
            ImGui.Text($"SpriteText texture rebuilds: {PerformanceStats.SpriteTextTextureRegenerationsPerSecond}");
            ImGui.Text($"SpriteTextPlus refreshes: {PerformanceStats.SpriteTextPlusRefreshesPerSecond}");
            ImGui.Text($"SpriteTextPlus cache builds: {PerformanceStats.SpriteTextPlusCacheBuildsPerSecond}");
            ImGui.Text($"SpriteTextPlus cached draws: {PerformanceStats.SpriteTextPlusCachedDrawsPerSecond}");
            ImGui.Text($"SpriteTextPlus uncached draws: {PerformanceStats.SpriteTextPlusUncachedDrawsPerSecond}");
        }

        private void RenderRuntime()
        {
            ImGui.Text($"Managed memory: {PerformanceStats.FormatBytes(PerformanceStats.ManagedMemoryBytes)}");
            ImGui.Text($"GC gen0: {PerformanceStats.Gen0Collections} (+{PerformanceStats.Gen0CollectionsDelta})");
            ImGui.Text($"GC gen1: {PerformanceStats.Gen1Collections} (+{PerformanceStats.Gen1CollectionsDelta})");
            ImGui.Text($"GC gen2: {PerformanceStats.Gen2Collections} (+{PerformanceStats.Gen2CollectionsDelta})");
            ImGui.Text($"Backbuffer: {PerformanceStats.BackBufferDescription}");
            ImGui.Text($"Virtual: {PerformanceStats.VirtualResolutionDescription}");
            ImGui.Text($"Scale: {PerformanceStats.ScreenScaleDescription}");
            ImGui.Text($"UI Scale: {PerformanceStats.UiScaleDescription}");
            ImGui.Text($"Text Scale: {PerformanceStats.EffectiveScreenScaleDescription}");
        }

        private void RenderImGuiStats()
        {
            ImGui.Text($"ImGui vertices: {PerformanceStats.ImGuiVertexCount}");
            ImGui.Text($"ImGui indices: {PerformanceStats.ImGuiIndexCount}");
        }

#if DEBUG
        private void RenderTextObjectControls()
        {
            if (!ImGui.TreeNode("Text objects"))
                return;

            ImGui.Checkbox("Cached bounds", ref showCachedTextBounds);
            ImGui.SameLine();
            ImGui.Checkbox("Uncached bounds", ref showUncachedTextBounds);

            ImGui.Checkbox("Labels", ref showTextObjectLabels);
            ImGui.SameLine();
            ImGui.Checkbox("Visible only", ref showVisibleTextObjectsOnly);

            ImGui.InputText("Search", ref textObjectSearch, 128);

            var objects = GetFilteredTextObjects();
            ImGui.Text($"{objects.Count} SpriteTextPlus objects");

            highlightedTextObjectId = selectedTextObjectId;

            if (ImGui.BeginChild("SpriteTextPlusObjectList", new Vector2(0, 180), ImGuiChildFlags.Border))
            {
                for (var i = 0; i < objects.Count; i++)
                {
                    var info = objects[i];
                    var selected = selectedTextObjectId == info.Id;
                    var label = FormatTextObjectListLabel(info);

                    if (ImGui.Selectable(label, selected))
                        selectedTextObjectId = info.Id;

                    if (ImGui.IsItemHovered())
                        highlightedTextObjectId = info.Id;
                }
            }

            ImGui.EndChild();
            ImGui.TreePop();
        }

        private void RenderDrawableControls()
        {
            if (!ImGui.TreeNode("Drawables"))
                return;

            ImGui.Checkbox("Bounds##drawables", ref showDrawableBounds);
            ImGui.SameLine();
            ImGui.Checkbox("Selected only##drawables", ref showSelectedDrawableBoundsOnly);
            ImGui.SameLine();
            ImGui.Checkbox("Labels##drawables", ref showDrawableLabels);

            ImGui.Checkbox("Visible only##drawables", ref showVisibleDrawablesOnly);

            ImGui.InputText("Search##drawables", ref drawableSearch, 128);

            var groups = GetFilteredDrawableGroups();
            var drawnTotal = 0;
            var liveTotal = 0;

            for (var i = 0; i < groups.Count; i++)
            {
                drawnTotal += groups[i].DrawnCount;
                liveTotal += groups[i].LiveCount;
            }

            ImGui.Text($"{groups.Count} types, {drawnTotal} drawn samples, {liveTotal} live objects");

            highlightedDrawableId = selectedDrawableId;

            if (ImGui.BeginChild("DrawableTypeList", new Vector2(0, 260), ImGuiChildFlags.Border))
            {
                for (var i = 0; i < groups.Count; i++)
                    RenderDrawableTypeGroup(groups[i]);
            }

            ImGui.EndChild();
            ImGui.TreePop();
        }

        private void RenderDrawableTypeGroup(DrawableTypeDebugInfo group)
        {
            var open = ImGui.TreeNode($"{group.TypeName} drawn {group.DrawnCount} live {group.LiveCount}##drawable-type-{group.TypeName}");

            if (!open)
                return;

            for (var i = 0; i < group.Objects.Count; i++)
                RenderDrawableObject(group.Objects[i]);

            ImGui.TreePop();
        }

        private void RenderDrawableObject(DrawableDebugInfo info)
        {
            var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

            if (selectedDrawableId == info.Id)
                flags |= ImGuiTreeNodeFlags.Selected;

            var open = ImGui.TreeNodeEx(FormatDrawableObjectLabel(info), flags);

            if (ImGui.IsItemClicked())
                selectedDrawableId = info.Id;

            if (ImGui.IsItemHovered())
                highlightedDrawableId = info.Id;

            if (!open)
                return;

            if (ImGui.Button($"Show bounds##drawable-bounds-{info.Id}"))
            {
                selectedDrawableId = info.Id;
                highlightedDrawableId = info.Id;
                showDrawableBounds = true;
                showSelectedDrawableBoundsOnly = true;
            }

            ImGui.SameLine();

            if (ImGui.Button($"Show all bounds##drawable-bounds-all-{info.Id}"))
            {
                selectedDrawableId = info.Id;
                highlightedDrawableId = info.Id;
                showDrawableBounds = true;
                showSelectedDrawableBoundsOnly = false;
            }

            ImGui.Text($"Parent: {info.ParentTypeName}");
            ImGui.Text($"Draw order: {info.DrawOrder}");
            ImGui.Text($"Children: {info.ChildCount}");
            ImGui.Text($"Visible: {info.Visible}");
            ImGui.Text($"Disposed: {info.IsDisposed}");
            ImGui.Text($"Rect: {info.ScreenRectangle.X:0.0}, {info.ScreenRectangle.Y:0.0}, {info.ScreenRectangle.Width:0.0}, {info.ScreenRectangle.Height:0.0}");
            ImGui.Text($"Position: {info.Position.X.Value:0.0}, {info.Position.Y.Value:0.0} scale {info.Position.X.Scale:0.00}, {info.Position.Y.Scale:0.00}");
            ImGui.Text($"Size: {info.Size.X.Value:0.0}, {info.Size.Y.Value:0.0} scale {info.Size.X.Scale:0.00}, {info.Size.Y.Scale:0.00}");
            ImGui.Text($"Rotation: {info.Rotation:0.00}");
            ImGui.Text($"Scale: {info.Scale.X:0.00}, {info.Scale.Y:0.00}");
            ImGui.Text($"SpriteBatchOptions: {info.HasSpriteBatchOptions}");

            if (!string.IsNullOrEmpty(info.SpriteTextureDescription))
                ImGui.Text($"Texture: {info.SpriteTextureDescription}");

            if (info.TextCached.HasValue)
            {
                ImGui.Text($"Text cached: {info.TextCached.Value}");
                ImGui.Text($"Font size: {info.FontSize}");
                ImGui.Text($"Lines: {info.LineCount}");
                ImGui.Text($"Text: {info.TextPreview}");
            }

            ImGui.TreePop();
        }

        private void DrawTextObjectBounds()
        {
            if (!showCachedTextBounds && !showUncachedTextBounds)
                return;

            var drawList = ImGui.GetForegroundDrawList();
            var objects = GetFilteredTextObjects();

            for (var i = 0; i < objects.Count; i++)
            {
                var info = objects[i];

                if (info.IsCached && !showCachedTextBounds)
                    continue;

                if (!info.IsCached && !showUncachedTextBounds)
                    continue;

                var rect = info.ScreenRectangle;

                if (rect.Width <= 0 || rect.Height <= 0)
                    continue;

                var min = ToDisplayPosition(rect.X, rect.Y);
                var max = ToDisplayPosition(rect.X + rect.Width, rect.Y + rect.Height);
                var highlighted = info.Id == highlightedTextObjectId;
                var color = highlighted ? HighlightColor : info.IsCached ? CachedColor : UncachedColor;

                drawList.AddRect(min, max, ImGui.GetColorU32(color), 0, ImDrawFlags.None, highlighted ? 2.5f : 1.5f);

                if (showTextObjectLabels)
                    drawList.AddText(min + new Vector2(3, 3), ImGui.GetColorU32(color), FormatTextObjectOverlayLabel(info));
            }
        }

        private void DrawDrawableBounds()
        {
            if (!showDrawableBounds)
                return;

            var drawList = ImGui.GetForegroundDrawList();
            var groups = GetFilteredDrawableGroups();

            for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                var group = groups[groupIndex];

                for (var i = 0; i < group.Objects.Count; i++)
                {
                    var info = group.Objects[i];

                    if (showSelectedDrawableBoundsOnly && info.Id != selectedDrawableId)
                        continue;

                    var rect = info.ScreenRectangle;

                    if (rect.Width <= 0 || rect.Height <= 0)
                        continue;

                    var min = ToDisplayPosition(rect.X, rect.Y);
                    var max = ToDisplayPosition(rect.X + rect.Width, rect.Y + rect.Height);
                    var highlighted = info.Id == highlightedDrawableId;
                    var color = highlighted ? HighlightColor : GetDrawableColor(info);

                    drawList.AddRect(min, max, ImGui.GetColorU32(color), 0, ImDrawFlags.None, highlighted ? 2.5f : 1.0f);

                    if (showDrawableLabels)
                        drawList.AddText(min + new Vector2(3, 15), ImGui.GetColorU32(color), FormatDrawableBoundsLabel(info));
                }
            }
        }

        private List<SpriteTextPlusDebugInfo> GetFilteredTextObjects()
        {
            var objects = SpriteTextPlusDebugRegistry.Snapshot(showVisibleTextObjectsOnly);

            if (string.IsNullOrWhiteSpace(textObjectSearch))
                return objects;

            var result = new List<SpriteTextPlusDebugInfo>();

            for (var i = 0; i < objects.Count; i++)
            {
                if (MatchesTextObjectFilter(objects[i]))
                    result.Add(objects[i]);
            }

            return result;
        }

        private bool MatchesTextObjectFilter(SpriteTextPlusDebugInfo info)
        {
            var filter = textObjectSearch.Trim();

            if (filter.Length == 0)
                return true;

            if (filter[0] == '#')
                filter = filter.Substring(1);

            return info.Id.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                   || info.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                   || (info.IsCached ? "cached" : "uncached").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private List<DrawableTypeDebugInfo> GetFilteredDrawableGroups()
        {
            var groups = DrawableDebugRegistry.Snapshot(showVisibleDrawablesOnly);

            if (string.IsNullOrWhiteSpace(drawableSearch))
                return groups;

            var filtered = new List<DrawableTypeDebugInfo>();

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var objects = new List<DrawableDebugInfo>();

                for (var j = 0; j < group.Objects.Count; j++)
                {
                    if (MatchesDrawableFilter(group.Objects[j]))
                        objects.Add(group.Objects[j]);
                }

                if (objects.Count == 0 && group.TypeName.IndexOf(drawableSearch.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                filtered.Add(new DrawableTypeDebugInfo(group.TypeName, SumDrawableDrawCounts(objects), group.LiveCount, objects));
            }

            filtered.Sort((a, b) =>
            {
                var countCompare = b.DrawnCount.CompareTo(a.DrawnCount);
                return countCompare != 0 ? countCompare : string.Compare(a.TypeName, b.TypeName, StringComparison.Ordinal);
            });

            return filtered;
        }

        private bool MatchesDrawableFilter(DrawableDebugInfo info)
        {
            var filter = drawableSearch.Trim();

            if (filter.Length == 0)
                return true;

            if (filter[0] == '#')
                filter = filter.Substring(1);

            return info.Id.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                   || info.TypeName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                   || info.ParentTypeName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                   || (info.TextPreview ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int SumDrawableDrawCounts(List<DrawableDebugInfo> objects)
        {
            var count = 0;

            for (var i = 0; i < objects.Count; i++)
                count += objects[i].DrawCount;

            return count;
        }

        private string FormatDrawableObjectLabel(DrawableDebugInfo info) =>
            $"#{info.Id} {info.TypeName} x{info.DrawCount} order {info.DrawOrder} {info.ScreenRectangle.Width:0}x{info.ScreenRectangle.Height:0}##drawable-{info.Id}";

        private string FormatDrawableBoundsLabel(DrawableDebugInfo info) =>
            $"#{info.Id} {info.TypeName} x{info.DrawCount}";

        private static Vector4 GetDrawableColor(DrawableDebugInfo info)
        {
            if (info.TextCached.HasValue)
                return info.TextCached.Value ? CachedColor : UncachedColor;

            if (info.TypeName.IndexOf("Sprite", StringComparison.OrdinalIgnoreCase) >= 0)
                return SpriteDrawableColor;

            return GenericDrawableColor;
        }

        private string FormatTextObjectListLabel(SpriteTextPlusDebugInfo info)
        {
            var cacheState = info.IsCached ? "cached" : "uncached";
            var visibleState = info.Visible ? "visible" : "hidden";
            var maxWidth = info.MaxWidth.HasValue ? $" max {info.MaxWidth.Value:0}" : "";
            return $"#{info.Id} {cacheState} {visibleState} {info.ScreenRectangle.Width:0}x{info.ScreenRectangle.Height:0} size {info.FontSize}{maxWidth} lines {info.LineCount} \"{Preview(info.Text)}\"##stp-{info.Id}";
        }

        private string FormatTextObjectOverlayLabel(SpriteTextPlusDebugInfo info)
        {
            var cacheState = info.IsCached ? "cached" : "uncached";
            return $"#{info.Id} {cacheState} {info.ScreenRectangle.Width:0}x{info.ScreenRectangle.Height:0}";
        }

        private static string Preview(string text)
        {
            var preview = (text ?? "").Replace("\r", " ").Replace("\n", " ");

            if (preview.Length <= 48)
                return preview;

            return preview.Substring(0, 45) + "...";
        }

        private static Vector2 ToDisplayPosition(float x, float y) =>
            new Vector2(x * WindowManager.ScreenScale.X, y * WindowManager.ScreenScale.Y);

        private static readonly Vector4 CachedColor = new Vector4(0.20f, 1.00f, 0.35f, 1.00f);
        private static readonly Vector4 UncachedColor = new Vector4(1.00f, 0.55f, 0.10f, 1.00f);
        private static readonly Vector4 HighlightColor = new Vector4(0.10f, 0.90f, 1.00f, 1.00f);
        private static readonly Vector4 SpriteDrawableColor = new Vector4(0.20f, 0.55f, 1.00f, 1.00f);
        private static readonly Vector4 GenericDrawableColor = new Vector4(0.88f, 0.88f, 0.88f, 1.00f);
#endif
    }
}
