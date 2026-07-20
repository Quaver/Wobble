using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mime;
using System.Runtime.InteropServices;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Wobble.Graphics.ImGUI
{
    public sealed class ImGuiRenderer : IDisposable
    {
        /// <summary>
        ///     ImGui renderers use separate contexts, so ImGui cannot arbitrate input between overlapping windows itself.
        ///     This list is populated in draw order and used on the next frame to give input to the topmost hovered context.
        /// </summary>
        private static readonly List<ImGuiRenderer> InputCandidates = new List<ImGuiRenderer>();

        private static TimeSpan InputFrameTime { get; set; } = TimeSpan.MinValue;

        private static ImGuiRenderer MouseInputOwner { get; set; }

        private static ImGuiRenderer KeyboardInputOwner { get; set; }

        private static bool WasAnyMouseButtonDown { get; set; }

        private static bool WasMouseButtonPressedThisFrame { get; set; }

        /// <summary>
        /// </summary>
        public IntPtr Context { get; }

        /// <summary>
        /// </summary>
        private static Game Game => GameBase.Game;

        /// <summary>
        /// </summary>
        private static GraphicsDevice GraphicsDevice => GameBase.Game.GraphicsDevice;

        /// <summary>
        /// </summary>
        private BasicEffect Effect { get; set; }

        /// <summary>
        /// </summary>
        private RasterizerState RasterizerState { get; }

        /// <summary>
        /// </summary>
        private byte[] VertexData { get; set; }

        /// <summary>
        /// </summary>
        private VertexBuffer VertexBuffer { get; set; }

        /// <summary>
        /// </summary>
        private int VertexBufferSize { get; set; }

        /// <summary>
        /// </summary>
        private byte[] IndexData { get; set; }

        /// <summary>
        /// </summary>
        private IndexBuffer IndexBuffer { get; set; }

        /// <summary>
        /// </summary>
        private int IndexBufferSize { get; set; }

        /// <summary>
        /// </summary>
        private Dictionary<IntPtr, TextureBinding> LoadedTextures { get; }

        /// <summary>
        /// </summary>
        private int TextureId { get; set; }

        /// <summary>
        /// </summary>
        private IntPtr? FontTextureId { get; set; }

        /// <summary>
        /// </summary>
        private int ScrollWheelValue { get; set; }

        /// <summary>
        /// </summary>
        private bool DestroyContext { get; }

        /// <summary>
        /// </summary>
        private ImGuiOptions Options { get; }

        /// <summary>
        /// </summary>
        public ImFontPtr DefaultFontPtr { get; private set; }

        /// <summary>
        /// </summary>
        private SharedFontAtlas FontAtlas { get; }

        /// <summary>
        ///     Whether this renderer had a hovered ImGui window on its most recently completed frame.
        /// </summary>
        private bool IsMouseHovered { get; set; }

        public float Scale { get; }

        public int LastVertexCount { get; private set; }

        public int LastIndexCount { get; private set; }

        /// <summary>
        ///     Whether this renderer received the mouse press that started on the current frame.
        /// </summary>
        public bool WasActivatedByMouse { get; private set; }

        /// <summary>
        /// </summary>
        public ImGuiRenderer(bool destroyContext = true, ImGuiOptions options = null, float scale = 1.0f)
        {
            DestroyContext = destroyContext;
            Options = options;
            Scale = scale;
            FontAtlas = SharedFontAtlasCache.Retain(options, scale);

            Context = ImGui.CreateContext(FontAtlas.Atlas);
            ImGui.SetCurrentContext(Context);

            LoadedTextures = new Dictionary<IntPtr, TextureBinding>();

            RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                FillMode = FillMode.Solid,
                MultiSampleAntiAlias = false,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = 0,
            };

            ImGui.GetStyle().ScaleAllSizes(Scale);

            SetupInput();
        }

#region ImGuiRenderer

        /// <summary>
        ///     Creates a texture and loads the font data from ImGui.
        ///     Should be called when the <see cref="Microsoft.Xna.Framework.Graphics.GraphicsDevice" />
        ///     is initialized but before any rendering is done
        /// </summary>
        public unsafe void RebuildFontAtlas()
        {
            var previousContext = ImGui.GetCurrentContext();

            if (previousContext != Context)
                ImGui.SetCurrentContext(Context);

            var io = ImGui.GetIO();
            FontAtlas.AssignFontPointers(Options, out var defaultFontPtr);
            DefaultFontPtr = defaultFontPtr;
            FontAtlas.EnsureTexture(GraphicsDevice);

            if (FontTextureId.HasValue && FontTextureId.Value != FontAtlas.TextureId)
                UnbindTexture(FontTextureId.Value);

            FontTextureId = FontAtlas.TextureId;
            LoadedTextures[FontTextureId.Value] = new TextureBinding(FontAtlas.Texture, false);

            // Let ImGui know where to find the texture
            io.Fonts.SetTexID(FontTextureId.Value);

            if (previousContext != Context)
                ImGui.SetCurrentContext(previousContext);
        }

        private static IntPtr GetGlyphRanges(ImFontAtlasPtr fonts, ImGuiGlyphRanges ranges)
        {
            switch (ranges)
            {
                case ImGuiGlyphRanges.ChineseFull:
                    return fonts.GetGlyphRangesChineseFull();
                case ImGuiGlyphRanges.ChineseSimplifiedCommon:
                    return fonts.GetGlyphRangesChineseSimplifiedCommon();
                case ImGuiGlyphRanges.Japanese:
                    return fonts.GetGlyphRangesJapanese();
                case ImGuiGlyphRanges.Korean:
                    return fonts.GetGlyphRangesKorean();
                case ImGuiGlyphRanges.Cyrillic:
                    return fonts.GetGlyphRangesCyrillic();
                case ImGuiGlyphRanges.Greek:
                    return fonts.GetGlyphRangesGreek();
                case ImGuiGlyphRanges.Thai:
                    return fonts.GetGlyphRangesThai();
                case ImGuiGlyphRanges.Vietnamese:
                    return fonts.GetGlyphRangesVietnamese();
                default:
                    return fonts.GetGlyphRangesDefault();
            }
        }

        /// <summary>
        ///     Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="MediaTypeNames.Image" />.
        ///     That pointer is then used by ImGui to let us know what texture to draw
        /// </summary>
        public IntPtr BindTexture(Texture2D texture)
        {
            var id = new IntPtr(TextureId++);

            LoadedTextures.Add(id, new TextureBinding(texture, true));

            return id;
        }

        /// <summary>
        ///     Removes a previously created texture pointer, releasing its reference and allowing it to be deallocated
        /// </summary>
        public void UnbindTexture(IntPtr textureId)
        {
            if (LoadedTextures.TryGetValue(textureId, out var texture) && texture.DisposeWithRenderer)
                texture.Texture.Dispose();

            LoadedTextures.Remove(textureId);
        }

        /// <summary>
        ///     Sets up ImGui for a new frame, should be called at frame start
        /// </summary>
        public void BeforeLayout(GameTime gameTime)
        {
            ImGui.GetIO().DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateInput(gameTime);

            ImGui.NewFrame();
        }

        /// <summary>
        /// Asks ImGui for the generated geometry data and sends it to the graphics pipeline, should be called after the UI is drawn using ImGui.** calls
        /// </summary>
        public void AfterLayout()
        {
            IsMouseHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);
            InputCandidates.Remove(this);
            InputCandidates.Add(this);

            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
        }

#endregion ImGuiRenderer

#region Setup & Update

        private void OnWindowOnTextInput(object s, TextInputEventArgs a)
        {
            if (a.Character == '\t' || KeyboardInputOwner != this || !IsMouseHovered) return;

            var previousContext = ImGui.GetCurrentContext();

            if (previousContext != Context)
                ImGui.SetCurrentContext(Context);

            ImGui.GetIO().AddInputCharacter(a.Character);

            if (previousContext != Context)
                ImGui.SetCurrentContext(previousContext);
        }

        /// <summary>
        ///     Maps ImGui keys to XNA keys. We use this later on to tell ImGui what keys were pressed
        /// </summary>
        private void SetupInput()
        {
            var io = ImGui.GetIO();

            // MonoGame-specific //////////////////////

            Game.Window.TextInput += OnWindowOnTextInput;
            ///////////////////////////////////////////

            // FNA-specific ///////////////////////////
            //TextInputEXT.TextInput += c =>
            //{
            //    if (c == '\t') return;

            //    ImGui.AddInputCharacter(c);
            //};
            ///////////////////////////////////////////

            // ImGUI provides out-of-the-box clipboard only on Windows. For other platforms, we need to set up the function pointers.
            io.SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(SetClipboardTextFnDelegate);
            io.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(GetClipboardTextFnDelegate);
        }

        /*
         * ImGUI clipboard handling callbacks.
         *
         * For setting the clipboard text, ImGUI passes us a (const char*) to a UTF-8 encoded string.
         *
         * For getting the clipboard text, ImGUI expects a UTF-8 encoded (const char*). In ImGUI's example implementations,
         * they store the clipboard contents in a global char array and return the pointer to that char array. This means
         * that the contents of the returned pointer are overwritten on every call to GetClipboardTextFn().
         *
         * What we do here is a little stricter than that, we invalidate the pointer on every call to GetClipboardTextFn()
         * by freeing that buffer and allocating a new one. I think it's valid to do that given how example implementations
         * operate.
         */

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_set_clipboard_text_fn(
            IntPtr userData,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string text
        );

        private static readonly d_set_clipboard_text_fn SetClipboardTextFnDelegate = SetClipboardTextFn;

        private static void SetClipboardTextFn(IntPtr userData, string text) =>
            Platform.Clipboard.NativeClipboard.SetText(text);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_get_clipboard_text_fn(IntPtr userData);

        private static readonly d_get_clipboard_text_fn GetClipboardTextFnDelegate = GetClipboardTextFn;

        private static IntPtr GetClipboardTextFn(IntPtr userData)
        {
            Marshal.FreeCoTaskMem(ClipboardTextMemory);
            ClipboardTextMemory = Marshal.StringToCoTaskMemUTF8(Platform.Clipboard.NativeClipboard.GetText());
            return ClipboardTextMemory;
        }

        /// <summary>
        ///     Pointer to the latest clipboard contents buffer returned to ImGUI.
        /// </summary>
        private static IntPtr ClipboardTextMemory = IntPtr.Zero;

        /// <summary>
        ///     Updates the <see cref="Microsoft.Xna.Framework.Graphics.Effect" /> to the current matrices and texture
        /// </summary>
        private Effect UpdateEffect(Texture2D texture)
        {
            Effect = Effect ?? new BasicEffect(GraphicsDevice);

            var io = ImGui.GetIO();

            Effect.World = Matrix.Identity;
            Effect.View = Matrix.Identity;
            Effect.Projection = Matrix.CreateOrthographicOffCenter(0, io.DisplaySize.X, io.DisplaySize.Y, 0, -1f, 1f);
            Effect.TextureEnabled = true;
            Effect.Texture = texture;
            Effect.VertexColorEnabled = true;

            return Effect;
        }

        /// <summary>
        ///     Sends XNA input state to ImGui
        /// </summary>
        private void UpdateInput(GameTime gameTime)
        {
            var io = ImGui.GetIO();

            var mouse = Mouse.GetState();
            UpdateInputOwners(gameTime, mouse);

            var keyboard = KeyboardInputOwner == this && IsMouseHovered
                ? Keyboard.GetState()
                : new KeyboardState();
            // Contexts the cursor is outside still need to see the mouse state. ImGui uses an
            // outside click to dismiss open popups and combos, and forwarding it is safe because
            // none of the context's windows can be activated at that position. Input remains
            // exclusive when windows from multiple contexts overlap.
            var receivesMouseInput = MouseInputOwner == this || !IsMouseHovered;
            WasActivatedByMouse = MouseInputOwner == this && WasMouseButtonPressedThisFrame;

            io.AddKeyEvent(ImGuiKey.Tab, keyboard.IsKeyDown(Keys.Tab));
            io.AddKeyEvent(ImGuiKey.LeftArrow, keyboard.IsKeyDown(Keys.Left));
            io.AddKeyEvent(ImGuiKey.RightArrow, keyboard.IsKeyDown(Keys.Right));
            io.AddKeyEvent(ImGuiKey.UpArrow, keyboard.IsKeyDown(Keys.Up));
            io.AddKeyEvent(ImGuiKey.DownArrow, keyboard.IsKeyDown(Keys.Down));
            io.AddKeyEvent(ImGuiKey.PageUp, keyboard.IsKeyDown(Keys.PageUp));
            io.AddKeyEvent(ImGuiKey.PageDown, keyboard.IsKeyDown(Keys.PageDown));
            io.AddKeyEvent(ImGuiKey.Home, keyboard.IsKeyDown(Keys.Home));
            io.AddKeyEvent(ImGuiKey.End, keyboard.IsKeyDown(Keys.End));
            io.AddKeyEvent(ImGuiKey.Delete, keyboard.IsKeyDown(Keys.Delete));
            io.AddKeyEvent(ImGuiKey.Backspace, keyboard.IsKeyDown(Keys.Back));
            io.AddKeyEvent(ImGuiKey.Enter, keyboard.IsKeyDown(Keys.Enter));
            io.AddKeyEvent(ImGuiKey.Escape, keyboard.IsKeyDown(Keys.Escape));
            io.AddKeyEvent(ImGuiKey.A, keyboard.IsKeyDown(Keys.A));
            io.AddKeyEvent(ImGuiKey.C, keyboard.IsKeyDown(Keys.C));
            io.AddKeyEvent(ImGuiKey.V, keyboard.IsKeyDown(Keys.V));
            io.AddKeyEvent(ImGuiKey.X, keyboard.IsKeyDown(Keys.X));
            io.AddKeyEvent(ImGuiKey.Y, keyboard.IsKeyDown(Keys.Y));
            io.AddKeyEvent(ImGuiKey.Z, keyboard.IsKeyDown(Keys.Z));
            io.AddKeyEvent(ImGuiKey.ModShift, keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift));
            io.AddKeyEvent(ImGuiKey.ModCtrl, keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl));
            io.AddKeyEvent(ImGuiKey.ModAlt, keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt));
            io.AddKeyEvent(ImGuiKey.ModSuper, keyboard.IsKeyDown(Keys.LeftWindows) || keyboard.IsKeyDown(Keys.RightWindows));

            io.DisplaySize = new System.Numerics.Vector2(
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight
            );

            io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);

            io.AddMousePosEvent(mouse.X, mouse.Y);
            io.AddMouseButtonEvent(0, receivesMouseInput && mouse.LeftButton == ButtonState.Pressed);
            io.AddMouseButtonEvent(1, receivesMouseInput && mouse.RightButton == ButtonState.Pressed);
            io.AddMouseButtonEvent(2, receivesMouseInput && mouse.MiddleButton == ButtonState.Pressed);
            var scrollDelta = mouse.ScrollWheelValue - ScrollWheelValue;
            io.AddMouseWheelEvent(0, !receivesMouseInput ? 0 : scrollDelta > 0 ? 1 :
                scrollDelta < 0 ? -1 : 0);

            ScrollWheelValue = mouse.ScrollWheelValue;
        }

        /// <summary>
        ///     Selects the last-drawn hovered renderer and keeps mouse ownership locked for the duration of a drag.
        /// </summary>
        private void UpdateInputOwners(GameTime gameTime, MouseState mouse)
        {
            if (InputFrameTime == gameTime.TotalGameTime)
                return;

            var isAnyMouseButtonDown = mouse.LeftButton == ButtonState.Pressed
                                       || mouse.RightButton == ButtonState.Pressed
                                       || mouse.MiddleButton == ButtonState.Pressed;
            WasMouseButtonPressedThisFrame = !WasAnyMouseButtonDown && isAnyMouseButtonDown;

            if (!WasAnyMouseButtonDown)
            {
                MouseInputOwner = null;

                for (var i = InputCandidates.Count - 1; i >= 0; i--)
                {
                    if (!InputCandidates[i].IsMouseHovered)
                        continue;

                    MouseInputOwner = InputCandidates[i];
                    break;
                }

                if (isAnyMouseButtonDown)
                    KeyboardInputOwner = MouseInputOwner;
            }

            WasAnyMouseButtonDown = isAnyMouseButtonDown;
            InputCandidates.Clear();
            InputFrameTime = gameTime.TotalGameTime;
        }

#endregion Setup & Update

#region Internals

        /// <summary>
        ///     Gets the geometry as set up by ImGui and sends it to the graphics device
        /// </summary>
        private void RenderDrawData(ImDrawDataPtr drawData)
        {
            LastVertexCount = drawData.TotalVtxCount;
            LastIndexCount = drawData.TotalIdxCount;

            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers
            var lastViewport = GraphicsDevice.Viewport;
            var lastScissorBox = GraphicsDevice.ScissorRectangle;
            var lastBlendFactor = GraphicsDevice.BlendFactor;
            var lastBlendState = GraphicsDevice.BlendState;
            var lastRasterizerState = GraphicsDevice.RasterizerState;
            var lastDepthStencilState = GraphicsDevice.DepthStencilState;

            // We are submitting vertex and index buffers without using SpriteBatch.
            // We need to end the batch so we are drawing on top of those that are not flushed
            GameBase.Game.TryEndBatch();
            GraphicsDevice.BlendFactor = Color.White;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.RasterizerState = RasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            // Setup projection
            GraphicsDevice.Viewport = new Viewport(
                0,
                0,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight
            );

            UpdateBuffers(drawData);

            RenderCommandLists(drawData);

            // Restore modified state
            GraphicsDevice.Viewport = lastViewport;
            GraphicsDevice.ScissorRectangle = lastScissorBox;
            GraphicsDevice.BlendFactor = lastBlendFactor;
            GraphicsDevice.BlendState = lastBlendState;
            GraphicsDevice.RasterizerState = lastRasterizerState;
            GraphicsDevice.DepthStencilState = lastDepthStencilState;
        }

        /// <summary>
        /// </summary>
        /// <param name="drawData"></param>
        private unsafe void UpdateBuffers(ImDrawDataPtr drawData)
        {
            if (drawData.TotalVtxCount == 0)
                return;

            // Expand buffers if we need more room
            if (drawData.TotalVtxCount > VertexBufferSize)
            {
                VertexBuffer?.Dispose();

                VertexBufferSize = (int)(drawData.TotalVtxCount * 1.5f);

                VertexBuffer = new VertexBuffer(
                    GraphicsDevice,
                    DrawVertDeclaration.Declaration,
                    VertexBufferSize,
                    BufferUsage.None
                );

                VertexData = new byte[VertexBufferSize * DrawVertDeclaration.Size];
            }

            if (drawData.TotalIdxCount > IndexBufferSize)
            {
                IndexBuffer?.Dispose();

                IndexBufferSize = (int)(drawData.TotalIdxCount * 1.5f);

                IndexBuffer = new IndexBuffer(
                    GraphicsDevice,
                    IndexElementSize.SixteenBits,
                    IndexBufferSize,
                    BufferUsage.None
                );

                IndexData = new byte[IndexBufferSize * sizeof(ushort)];
            }

            // Copy ImGui's vertices and indices to a set of managed byte arrays
            var vtxOffset = 0;
            var idxOffset = 0;

            for (var n = 0; n < drawData.CmdListsCount; n++)
            {
                var cmdList = drawData.CmdLists[n];

                fixed (void* vtxDstPtr = &VertexData[vtxOffset * DrawVertDeclaration.Size])
                fixed (void* idxDstPtr = &IndexData[idxOffset * sizeof(ushort)])
                {
                    Buffer.MemoryCopy(
                        (void*)cmdList.VtxBuffer.Data,
                        vtxDstPtr,
                        VertexData.Length,
                        cmdList.VtxBuffer.Size * DrawVertDeclaration.Size
                    );

                    Buffer.MemoryCopy(
                        (void*)cmdList.IdxBuffer.Data,
                        idxDstPtr,
                        IndexData.Length,
                        cmdList.IdxBuffer.Size * sizeof(ushort)
                    );
                }

                vtxOffset += cmdList.VtxBuffer.Size;
                idxOffset += cmdList.IdxBuffer.Size;
            }

            // Copy the managed byte arrays to the gpu vertex- and index buffers
            VertexBuffer.SetData(VertexData, 0, drawData.TotalVtxCount * DrawVertDeclaration.Size);
            IndexBuffer.SetData(IndexData, 0, drawData.TotalIdxCount * sizeof(ushort));
        }

        /// <summary>
        /// </summary>
        /// <param name="drawData"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void RenderCommandLists(ImDrawDataPtr drawData)
        {
            var lastIndecies = GraphicsDevice.Indices;
            var lastScissorRectangle = GraphicsDevice.ScissorRectangle;

            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;

            var vtxOffset = 0;
            var idxOffset = 0;

            for (var n = 0; n < drawData.CmdListsCount; n++)
            {
                var cmdList = drawData.CmdLists[n];

                for (var cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
                {
                    var drawCmd = cmdList.CmdBuffer[cmdi];

                    if (!LoadedTextures.ContainsKey(drawCmd.TextureId))
                        throw new InvalidOperationException(
                            $"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings"
                        );

                    GraphicsDevice.ScissorRectangle = new Rectangle(
                        (int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                    );

                    var effect = UpdateEffect(LoadedTextures[drawCmd.TextureId].Texture);
                    var vertexOffset = vtxOffset + (int)drawCmd.VtxOffset;
                    var indexOffset = idxOffset + (int)drawCmd.IdxOffset;

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

#pragma warning disable CS0618 // // FNA does not expose an alternative method.
                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            vertexOffset,
                            0,
                            cmdList.VtxBuffer.Size,
                            indexOffset,
                            (int)drawCmd.ElemCount / 3
                        );
#pragma warning restore CS0618
                    }
                }

                vtxOffset += cmdList.VtxBuffer.Size;
                idxOffset += cmdList.IdxBuffer.Size;
            }

            GraphicsDevice.SetVertexBuffer(null);
            GraphicsDevice.Indices = lastIndecies;
            GraphicsDevice.ScissorRectangle = lastScissorRectangle;
        }

#endregion Internals

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            InputCandidates.Remove(this);

            if (MouseInputOwner == this)
                MouseInputOwner = null;

            if (KeyboardInputOwner == this)
                KeyboardInputOwner = null;

            if (DestroyContext)
                ImGui.DestroyContext(Context);

            foreach (var texture in LoadedTextures.Values)
            {
                if (texture.DisposeWithRenderer)
                    texture.Texture.Dispose();
            }

            LoadedTextures.Clear();
            Effect?.Dispose();
            RasterizerState?.Dispose();
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
            Game.Window.TextInput -= OnWindowOnTextInput;

            if (DestroyContext)
                SharedFontAtlasCache.Release(FontAtlas);
        }

        private sealed class TextureBinding
        {
            public Texture2D Texture { get; }

            public bool DisposeWithRenderer { get; }

            public TextureBinding(Texture2D texture, bool disposeWithRenderer)
            {
                Texture = texture;
                DisposeWithRenderer = disposeWithRenderer;
            }
        }

        private sealed class SharedFontAtlas
        {
            public string Key { get; }

            public ImFontAtlasPtr Atlas { get; }

            public IntPtr TextureId { get; } = new IntPtr(-1);

            public Texture2D Texture { get; private set; }

            public int ReferenceCount { get; set; }

            private ImFontPtr DefaultFontPtr { get; set; }

            private List<ImFontPtr> FontPointers { get; }

            public unsafe SharedFontAtlas(string key, ImGuiOptions options, float scale)
            {
                Key = key;
                Atlas = new ImFontAtlasPtr(ImGuiNative.ImFontAtlas_ImFontAtlas());
                FontPointers = new List<ImFontPtr>();

                if (options == null || options.LoadDefaultFont)
                    DefaultFontPtr = Atlas.AddFontDefault();

                if (options == null)
                    return;

                foreach (var font in options.Fonts)
                {
                    var fontPtr = Atlas.AddFontFromFileTTF(font.Path, font.Size * scale);
                    FontPointers.Add(fontPtr);

                    foreach (var fallback in font.Fallbacks)
                    {
                        var config = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());
                        config.MergeMode = true;
                        config.FontNo = fallback.Index;

                        Atlas.AddFontFromFileTTF(fallback.Path, font.Size * scale, config,
                            GetGlyphRanges(Atlas, fallback.GlyphRanges));

                        config.Destroy();
                    }
                }
            }

            public void AssignFontPointers(ImGuiOptions options, out ImFontPtr defaultFontPtr)
            {
                defaultFontPtr = DefaultFontPtr;

                if (options == null)
                    return;

                for (var i = 0; i < options.Fonts.Count && i < FontPointers.Count; i++)
                    options.Fonts[i].Context = FontPointers[i];
            }

            public unsafe void EnsureTexture(GraphicsDevice graphicsDevice)
            {
                if (Texture != null)
                    return;

                Atlas.GetTexDataAsRGBA32(out byte* pixelData, out var width, out var height, out var bytesPerPixel);

                var pixels = new byte[width * height * bytesPerPixel];
                Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

                Texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
                Texture.SetData(pixels);

                Atlas.SetTexID(TextureId);
                Atlas.ClearTexData();
            }

            public unsafe void Dispose()
            {
                Texture?.Dispose();
                Texture = null;
                ImGuiNative.ImFontAtlas_destroy(Atlas.NativePtr);
            }
        }

        private static class SharedFontAtlasCache
        {
            private static readonly Dictionary<string, SharedFontAtlas> Cache = new Dictionary<string, SharedFontAtlas>();

            public static SharedFontAtlas Retain(ImGuiOptions options, float scale)
            {
                var key = CreateKey(options, scale);

                if (!Cache.TryGetValue(key, out var atlas))
                {
                    atlas = new SharedFontAtlas(key, options, scale);
                    Cache.Add(key, atlas);
                }

                atlas.ReferenceCount++;
                return atlas;
            }

            public static void Release(SharedFontAtlas atlas)
            {
                if (atlas == null)
                    return;

                atlas.ReferenceCount--;

                if (atlas.ReferenceCount > 0)
                    return;

                Cache.Remove(atlas.Key);
                atlas.Dispose();
            }

            private static string CreateKey(ImGuiOptions options, float scale)
            {
                var key = "scale=" + scale.ToString("R", CultureInfo.InvariantCulture);

                if (options == null)
                    return key + ";default";

                key += ";loadDefault=" + options.LoadDefaultFont;

                foreach (var font in options.Fonts)
                {
                    key += ";font=" + font.Path + "," + font.Size.ToString(CultureInfo.InvariantCulture);

                    foreach (var fallback in font.Fallbacks)
                    {
                        key += ";fallback=" + fallback.Path + "," +
                               fallback.Index.ToString(CultureInfo.InvariantCulture) + "," +
                               fallback.GlyphRanges;
                    }
                }

                return key;
            }
        }
    }
}
