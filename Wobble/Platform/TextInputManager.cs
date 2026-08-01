using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace Wobble.Platform
{
    public static class TextInputManager
    {
        private static readonly Lazy<SdlTextInput> Sdl = new Lazy<SdlTextInput>(CreateSdlTextInput);

        public static void StartTextInput()
        {
            Sdl.Value?.StartTextInput();
        }

        public static void StopTextInput()
        {
            Sdl.Value?.StopTextInput();
        }

        public static void SetTextInputRectangle(Rectangle rectangle)
        {
            Sdl.Value?.SetTextInputRectangle(rectangle);
        }

        private static SdlTextInput CreateSdlTextInput()
        {
            if (GameBase.Game?.Window?.GetType().Name != "SdlGameWindow")
                return null;

            try
            {
                // Wobble references a prebuilt MonoGame assembly, so use its already-loaded SDL handle.
                var sdlType = GameBase.Game.Window.GetType().Assembly.GetType("Sdl");
                var nativeLibraryField = sdlType?.GetField("NativeLibrary", BindingFlags.Public | BindingFlags.Static);
                var nativeLibrary = nativeLibraryField != null ? (IntPtr)nativeLibraryField.GetValue(null) : IntPtr.Zero;

                return nativeLibrary == IntPtr.Zero ? null : new SdlTextInput(nativeLibrary);
            }
            catch
            {
                return null;
            }
        }

        private sealed class SdlTextInput
        {
            private readonly StartTextInputDelegate _startTextInput;
            private readonly StopTextInputDelegate _stopTextInput;
            private readonly SetTextInputRectangleDelegate _setTextInputRectangle;

            public SdlTextInput(IntPtr nativeLibrary)
            {
                _startTextInput = LoadDelegate<StartTextInputDelegate>(nativeLibrary, "SDL_StartTextInput");
                _stopTextInput = LoadDelegate<StopTextInputDelegate>(nativeLibrary, "SDL_StopTextInput");
                _setTextInputRectangle = LoadDelegate<SetTextInputRectangleDelegate>(nativeLibrary, "SDL_SetTextInputRect");
            }

            public void StartTextInput()
            {
                _startTextInput?.Invoke();
            }

            public void StopTextInput()
            {
                _stopTextInput?.Invoke();
            }

            public void SetTextInputRectangle(Rectangle rectangle)
            {
                if (_setTextInputRectangle == null)
                    return;

                var sdlRectangle = new SdlRectangle
                {
                    X = rectangle.X,
                    Y = rectangle.Y,
                    Width = rectangle.Width,
                    Height = rectangle.Height
                };

                _setTextInputRectangle(ref sdlRectangle);
            }

            private static T LoadDelegate<T>(IntPtr nativeLibrary, string name) where T : Delegate
            {
                return NativeLibrary.TryGetExport(nativeLibrary, name, out var address)
                    ? Marshal.GetDelegateForFunctionPointer<T>(address)
                    : null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SdlRectangle
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void StartTextInputDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void StopTextInputDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetTextInputRectangleDelegate(ref SdlRectangle rectangle);
    }
}
