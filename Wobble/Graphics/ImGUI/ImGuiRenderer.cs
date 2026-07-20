using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Wobble.Graphics.ImGUI
{
    public sealed unsafe class ImGuiRenderer : IDisposable
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
        public ImGuiContextPtr Context { get; }

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
        private static long NextTextureId;

        private static readonly ImGuiErrorCallback RecoveredPluginErrorCallback = IgnoreRecoveredPluginError;

        private static unsafe void IgnoreRecoveredPluginError(ImGuiContext* context, void* userData, byte* message)
        {
            // Recovery is expected for legacy Lua plugins that scoped style pushes to End().
        }

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
            Context = ImGui.CreateContext();
            ImGui.SetCurrentContext(Context);
            FontAtlas = new SharedFontAtlas(options, scale, ImGui.GetIO().Fonts);

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

            FontAtlas.AssignFontPointers(Options, out var defaultFontPtr);
            DefaultFontPtr = defaultFontPtr;
            var io = ImGui.GetIO();
            io.FontDefault = defaultFontPtr;

            if (Options?.Fonts.Count > 0)
                ImGui.GetStyle().FontSizeBase = Options.Fonts[0].Size * Scale;

            if (previousContext != Context)
                ImGui.SetCurrentContext(previousContext);
        }

        /// <summary>
        ///     Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="MediaTypeNames.Image" />.
        ///     That pointer is then used by ImGui to let us know what texture to draw
        /// </summary>
        public IntPtr BindTexture(Texture2D texture)
        {
            var id = new IntPtr(Interlocked.Increment(ref NextTextureId));

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
            if (a.Character == '\t' || KeyboardInputOwner != this) return;

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
        private unsafe void SetupInput()
        {
            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.RendererHasTextures | ImGuiBackendFlags.RendererHasVtxOffset;
            io.ConfigErrorRecovery = true;
            io.ConfigErrorRecoveryEnableAssert = false;
            // Plugins written against pre-1.92 ImGui commonly scoped style pushes to a whole
            // window and relied on End() to recover the stack. Keep that safe recovery behavior,
            // but don't flood the editor UI or logs with a diagnostic for every recovered item.
            io.ConfigErrorRecoveryEnableDebugLog = false;
            io.ConfigErrorRecoveryEnableTooltip = false;
            var context = Context;
            context.ErrorCallback = (void*)Marshal.GetFunctionPointerForDelegate(RecoveredPluginErrorCallback);

            var platformIo = ImGui.GetPlatformIO();
            platformIo.RendererTextureMaxWidth = GraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef ? 16384 : 4096;
            platformIo.RendererTextureMaxHeight = GraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef ? 16384 : 4096;

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
            platformIo.PlatformSetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate(SetClipboardTextFnDelegate);
            platformIo.PlatformGetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate(GetClipboardTextFnDelegate);
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

            var keyboard = KeyboardInputOwner == this ? Keyboard.GetState() : new KeyboardState();
            var ownsMouseInput = MouseInputOwner == this;
            WasActivatedByMouse = ownsMouseInput && WasMouseButtonPressedThisFrame;

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
            io.AddMouseButtonEvent(0, ownsMouseInput && mouse.LeftButton == ButtonState.Pressed);
            io.AddMouseButtonEvent(1, ownsMouseInput && mouse.RightButton == ButtonState.Pressed);
            io.AddMouseButtonEvent(2, ownsMouseInput && mouse.MiddleButton == ButtonState.Pressed);
            var scrollDelta = mouse.ScrollWheelValue - ScrollWheelValue;
            io.AddMouseWheelEvent(0, !ownsMouseInput ? 0 : scrollDelta > 0 ? 1 :
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
            UpdateDynamicTextures(drawData);

            LastVertexCount = drawData.TotalVtxCount;
            LastIndexCount = drawData.TotalIdxCount;

            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers
            var lastViewport = GraphicsDevice.Viewport;
            var lastScissorBox = GraphicsDevice.ScissorRectangle;
            var lastBlendFactor = GraphicsDevice.BlendFactor;
            var lastBlendState = GraphicsDevice.BlendState;
            var lastRasterizerState = GraphicsDevice.RasterizerState;
            var lastDepthStencilState = GraphicsDevice.DepthStencilState;
            var lastSamplerState = GraphicsDevice.SamplerStates[0];

            // We are submitting vertex and index buffers without using SpriteBatch.
            // We need to end the batch so we are drawing on top of those that are not flushed
            GameBase.Game.TryEndBatch();
            GraphicsDevice.BlendFactor = Color.White;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.RasterizerState = RasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

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
            GraphicsDevice.SamplerStates[0] = lastSamplerState;
        }

        private unsafe void UpdateDynamicTextures(ImDrawDataPtr drawData)
        {
            for (var i = 0; i < drawData.Textures.Size; i++)
            {
                var textureData = drawData.Textures[i];

                switch (textureData.Status)
                {
                    case ImTextureStatus.WantCreate:
                        FontAtlas.CreateTexture(GraphicsDevice, textureData);
                        break;
                    case ImTextureStatus.WantUpdates:
                        FontAtlas.UpdateTexture(textureData);
                        break;
                    case ImTextureStatus.WantDestroy:
                        FontAtlas.DestroyTexture(textureData);
                        break;
                }
            }
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

                    var textureId = (IntPtr)drawCmd.GetTexID();

                    if (!TryGetTexture(textureId, out var texture))
                        throw new InvalidOperationException(
                            $"Could not find a texture with id '{textureId}', please check your bindings"
                        );

                    GraphicsDevice.ScissorRectangle = new Rectangle(
                        (int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                    );

                    var effect = UpdateEffect(texture);
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

        private bool TryGetTexture(IntPtr textureId, out Texture2D texture)
        {
            if (LoadedTextures.TryGetValue(textureId, out var binding))
            {
                texture = binding.Texture;
                return true;
            }

            return FontAtlas.TryGetTexture(textureId, out texture);
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
            {
                FontAtlas.Dispose();
                ImGui.DestroyContext(Context);
            }

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
            public ImFontAtlasPtr Atlas { get; }

            private ImFontPtr DefaultFontPtr { get; set; }

            private List<ImFontPtr> FontPointers { get; }

            private Dictionary<IntPtr, Texture2D> Textures { get; }

            public unsafe SharedFontAtlas(ImGuiOptions options, float scale, ImFontAtlasPtr atlas)
            {
                Atlas = atlas;
                Atlas.TexDesiredFormat = ImTextureFormat.Rgba32;
                FontPointers = new List<ImFontPtr>();
                Textures = new Dictionary<IntPtr, Texture2D>();

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
                        var config = ImGui.ImFontConfig();
                        config.MergeMode = true;
                        config.FontNo = (uint)fallback.Index;

                        Atlas.AddFontFromFileTTF(fallback.Path, font.Size * scale, config);

                        config.Destroy();
                    }
                }
            }

            public void AssignFontPointers(ImGuiOptions options, out ImFontPtr defaultFontPtr)
            {
                defaultFontPtr = DefaultFontPtr;

                if (defaultFontPtr.IsNull && FontPointers.Count > 0)
                    defaultFontPtr = FontPointers[0];

                if (options == null)
                    return;

                for (var i = 0; i < options.Fonts.Count && i < FontPointers.Count; i++)
                    options.Fonts[i].Context = FontPointers[i];
            }

            public bool TryGetTexture(IntPtr textureId, out Texture2D texture) =>
                Textures.TryGetValue(textureId, out texture);

            public unsafe void CreateTexture(GraphicsDevice graphicsDevice, ImTextureDataPtr textureData)
            {
                if (textureData.Format != ImTextureFormat.Rgba32)
                    throw new NotSupportedException($"Unsupported ImGui texture format: {textureData.Format}");

                var id = new IntPtr(Interlocked.Increment(ref NextTextureId));
                var texture = new Texture2D(graphicsDevice, textureData.Width, textureData.Height, false, SurfaceFormat.Color);
                Textures.Add(id, texture);
                UploadTexture(texture, textureData);
                textureData.SetTexID(id);
                textureData.SetStatus(ImTextureStatus.Ok);
            }

            public unsafe void UpdateTexture(ImTextureDataPtr textureData)
            {
                var id = (IntPtr)textureData.TexID;

                if (!Textures.TryGetValue(id, out var texture))
                    throw new InvalidOperationException($"Could not update ImGui texture with id '{id}'");

                UploadTexture(texture, textureData);
                textureData.SetStatus(ImTextureStatus.Ok);
            }

            public void DestroyTexture(ImTextureDataPtr textureData)
            {
                var id = (IntPtr)textureData.TexID;

                if (Textures.Remove(id, out var texture))
                    texture.Dispose();

                textureData.SetTexID(IntPtr.Zero);
                textureData.SetStatus(ImTextureStatus.Destroyed);
            }

            private static unsafe void UploadTexture(Texture2D texture, ImTextureDataPtr textureData)
            {
                var pixels = new byte[textureData.Width * textureData.Height * textureData.BytesPerPixel];
                Marshal.Copy((IntPtr)textureData.GetPixels(), pixels, 0, pixels.Length);
                texture.SetData(pixels);
            }

            public void Dispose()
            {
                foreach (var texture in Textures.Values)
                    texture.Dispose();

                Textures.Clear();
            }
        }
    }
}
