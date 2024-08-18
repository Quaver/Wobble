using System;
using System.Collections.Generic;
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
        private Dictionary<IntPtr, Texture2D> LoadedTextures { get; set; }

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
        private List<int> Keys { get; } = new List<int>();

        /// <summary>
        /// </summary>
        private bool DestroyContext { get; }

        /// <summary>
        /// </summary>
        private ImGuiOptions Options { get; }

        // ReSharper disable once InconsistentNaming
        private ImGuiIOPtr IO { get; set; }

        /// <summary>
        /// </summary>
        public ImFontPtr DefaultFontPtr { get; private set; }

        public float Scale { get; }

        /// <summary>
        /// </summary>
        public ImGuiRenderer(bool destroyContext = true, ImGuiOptions options = null, float scale = 1.0f)
        {
            DestroyContext = destroyContext;
            Options = options;
            Scale = scale;

            Context = ImGui.CreateContext();
            ImGui.SetCurrentContext(Context);
            IO = ImGui.GetIO();

            LoadedTextures = new Dictionary<IntPtr, Texture2D>();

            RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                FillMode = FillMode.Solid,
                MultiSampleAntiAlias = false,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = 0
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
            if (Options != null)
            {
                if (Options.LoadDefaultFont)
                    DefaultFontPtr = IO.Fonts.AddFontDefault();

                foreach (var font in Options.Fonts)
                    font.Context = IO.Fonts.AddFontFromFileTTF(font.Path, font.Size * Scale);
            }

            IO.Fonts.GetTexDataAsRGBA32(out var pixelData, out var width, out var height, out var bytesPerPixel);

            // Copy the data to a managed array
            var pixels = new byte[width * height * bytesPerPixel];
            Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

            // Create and register the texture as an XNA texture
            var tex2D = new Texture2D(GraphicsDevice, width, height, false, SurfaceFormat.Color);
            tex2D.SetData(pixels);

            // Should a texture already have been build previously, unbind it first so it can be deallocated
            if (FontTextureId.HasValue)
                UnbindTexture(FontTextureId.Value);

            // Bind the new texture to an ImGui-friendly id
            FontTextureId = BindTexture(tex2D);

            // Let ImGui know where to find the texture
            IO.Fonts.SetTexID(FontTextureId.Value);
            IO.Fonts.ClearTexData(); // Clears CPU side texture data
        }

        /// <summary>
        ///     Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="MediaTypeNames.Image" />.
        ///     That pointer is then used by ImGui to let us know what texture to draw
        /// </summary>
        public IntPtr BindTexture(Texture2D texture)
        {
            var id = new IntPtr(TextureId++);

            LoadedTextures.Add(id, texture);

            return id;
        }

        /// <summary>
        ///     Removes a previously created texture pointer, releasing its reference and allowing it to be deallocated
        /// </summary>
        public void UnbindTexture(IntPtr textureId) => LoadedTextures.Remove(textureId);

        /// <summary>
        ///     Sets up ImGui for a new frame, should be called at frame start
        /// </summary>
        public void BeforeLayout(GameTime gameTime)
        {
            IO.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateInput();

            ImGui.NewFrame();
        }

        /// <summary>
        /// Asks ImGui for the generated geometry data and sends it to the graphics pipeline, should be called after the UI is drawn using ImGui.** calls
        /// </summary>
        public void AfterLayout()
        {
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
        }

        #endregion ImGuiRenderer

        #region Setup & Update

        /// <summary>
        ///     Maps ImGui keys to XNA keys. We use this later on to tell ImGui what keys were pressed
        /// </summary>
        private void SetupInput()
        {
            Keys.Add(IO.KeyMap[(int)ImGuiKey.Tab] = (int)Microsoft.Xna.Framework.Input.Keys.Tab);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Microsoft.Xna.Framework.Input.Keys.Left);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.RightArrow] = (int)Microsoft.Xna.Framework.Input.Keys.Right);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.UpArrow] = (int)Microsoft.Xna.Framework.Input.Keys.Up);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.DownArrow] = (int)Microsoft.Xna.Framework.Input.Keys.Down);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.PageUp] = (int)Microsoft.Xna.Framework.Input.Keys.PageUp);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.PageDown] = (int)Microsoft.Xna.Framework.Input.Keys.PageDown);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.Home] = (int)Microsoft.Xna.Framework.Input.Keys.Home);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.End] = (int)Microsoft.Xna.Framework.Input.Keys.End);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.Delete] = (int)Microsoft.Xna.Framework.Input.Keys.Delete);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.Backspace] = (int)Microsoft.Xna.Framework.Input.Keys.Back);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.Enter] = (int)Microsoft.Xna.Framework.Input.Keys.Enter);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.Escape] = (int)Microsoft.Xna.Framework.Input.Keys.Escape);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.A] = (int)Microsoft.Xna.Framework.Input.Keys.A);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.C] = (int)Microsoft.Xna.Framework.Input.Keys.C);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.V] = (int)Microsoft.Xna.Framework.Input.Keys.V);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.X] = (int)Microsoft.Xna.Framework.Input.Keys.X);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.Y] = (int)Microsoft.Xna.Framework.Input.Keys.Y);
            Keys.Add(IO.KeyMap[(int)ImGuiKey.Z] = (int)Microsoft.Xna.Framework.Input.Keys.Z);

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

            IO.Fonts.AddFontDefault();

            // ImGUI provides out-of-the-box clipboard only on Windows. For other platforms, we need to set up the function pointers.
            IO.SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(SetClipboardTextFnDelegate);
            IO.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate(GetClipboardTextFnDelegate);
        }

        private void OnWindowOnTextInput(object s, TextInputEventArgs a)
        {
            if (a.Character == '\t') return;

            IO.AddInputCharacter(a.Character);
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
        private delegate void d_set_clipboard_text_fn(IntPtr userData, [MarshalAs(UnmanagedType.LPUTF8Str)] string text);
        private static readonly d_set_clipboard_text_fn SetClipboardTextFnDelegate = SetClipboardTextFn;

        private static void SetClipboardTextFn(IntPtr userData, string text) => Platform.Clipboard.NativeClipboard.SetText(text);

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

            Effect.World = Matrix.Identity;
            Effect.View = Matrix.Identity;
            Effect.Projection = Matrix.CreateOrthographicOffCenter(0, IO.DisplaySize.X, IO.DisplaySize.Y, 0, -1f, 1f);
            Effect.TextureEnabled = true;
            Effect.Texture = texture;
            Effect.VertexColorEnabled = true;

            return Effect;
        }

        /// <summary>
        ///     Sends XNA input state to ImGui
        /// </summary>
        private void UpdateInput()
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            foreach (var t in Keys)
                IO.KeysDown[t] = keyboard.IsKeyDown((Keys)t);

            IO.KeyShift = keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);
            IO.KeyCtrl = keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl);
            IO.KeyAlt = keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt);
            IO.KeySuper = keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftWindows) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightWindows);

            IO.DisplaySize = new System.Numerics.Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            IO.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);

            IO.MousePos = new System.Numerics.Vector2(mouse.X, mouse.Y);

            IO.MouseDown[0] = mouse.LeftButton == ButtonState.Pressed;
            IO.MouseDown[1] = mouse.RightButton == ButtonState.Pressed;
            IO.MouseDown[2] = mouse.MiddleButton == ButtonState.Pressed;

            var scrollDelta = mouse.ScrollWheelValue - ScrollWheelValue;
            IO.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
            ScrollWheelValue = mouse.ScrollWheelValue;
        }

        #endregion Setup & Update

        #region Internals

        /// <summary>
        ///     Gets the geometry as set up by ImGui and sends it to the graphics device
        /// </summary>
        private void RenderDrawData(ImDrawDataPtr drawData)
        {
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers
            var lastViewport = GraphicsDevice.Viewport;
            var lastScissorBox = GraphicsDevice.ScissorRectangle;
            var lastBlendFactor = GraphicsDevice.BlendFactor;
            var lastBlendState = GraphicsDevice.BlendState;
            var lastRasterizerState = GraphicsDevice.RasterizerState;
            var lastDepthStencilState = GraphicsDevice.DepthStencilState;

            GraphicsDevice.BlendFactor = Color.White;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.RasterizerState = RasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            drawData.ScaleClipRects(IO.DisplayFramebufferScale);

            // Setup projection
            GraphicsDevice.Viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

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
            {
                return;
            }

            // Expand buffers if we need more room
            if (drawData.TotalVtxCount > VertexBufferSize)
            {
                VertexBuffer?.Dispose();

                VertexBufferSize = (int)(drawData.TotalVtxCount * 1.5f);
                VertexBuffer = new VertexBuffer(GraphicsDevice, DrawVertDeclaration.Declaration, VertexBufferSize, BufferUsage.None);
                VertexData = new byte[VertexBufferSize * DrawVertDeclaration.Size];
            }

            if (drawData.TotalIdxCount > IndexBufferSize)
            {
                IndexBuffer?.Dispose();

                IndexBufferSize = (int)(drawData.TotalIdxCount * 1.5f);
                IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, IndexBufferSize, BufferUsage.None);
                IndexData = new byte[IndexBufferSize * sizeof(ushort)];
            }

            // Copy ImGui's vertices and indices to a set of managed byte arrays
            var vtxOffset = 0;
            var idxOffset = 0;

            for (var n = 0; n < drawData.CmdListsCount; n++)
            {
                var cmdList = drawData.CmdListsRange[n];

                fixed (void* vtxDstPtr = &VertexData[vtxOffset * DrawVertDeclaration.Size])
                fixed (void* idxDstPtr = &IndexData[idxOffset * sizeof(ushort)])
                {
                    Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, VertexData.Length, cmdList.VtxBuffer.Size * DrawVertDeclaration.Size);
                    Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, IndexData.Length, cmdList.IdxBuffer.Size * sizeof(ushort));
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
                var cmdList = drawData.CmdListsRange[n];

                for (var cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
                {
                    var drawCmd = cmdList.CmdBuffer[cmdi];

                    if (!LoadedTextures.ContainsKey(drawCmd.TextureId))
                    {
                        throw new InvalidOperationException($"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
                    }

                    GraphicsDevice.ScissorRectangle = new Rectangle(
                        (int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                    );

                    var effect = UpdateEffect(LoadedTextures[drawCmd.TextureId]);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

#pragma warning disable CS0618 // // FNA does not expose an alternative method.
                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            vtxOffset,
                            0,
                            cmdList.VtxBuffer.Size,
                            idxOffset,
                            (int)drawCmd.ElemCount / 3
                        );
#pragma warning restore CS0618
                    }

                    idxOffset += (int)drawCmd.ElemCount;
                }

                vtxOffset += cmdList.VtxBuffer.Size;
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
            if (DestroyContext)
                ImGui.DestroyContext(Context);

            Effect?.Dispose();
            RasterizerState?.Dispose();
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
            Game.Window.TextInput -= OnWindowOnTextInput;
        }
    }
}