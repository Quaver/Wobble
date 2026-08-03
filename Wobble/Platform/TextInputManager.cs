using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
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

        public static bool IsTextCompositionActive => Sdl.IsValueCreated && Sdl.Value.IsTextCompositionActive;

        public static bool ConsumeTextCompositionCommitPending() =>
            Sdl.IsValueCreated && Sdl.Value.ConsumeTextCompositionCommitPending();

        public static void AcknowledgeTextInput()
        {
            if (Sdl.IsValueCreated)
                Sdl.Value.AcknowledgeTextInput();
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
            private readonly AddEventWatchDelegate _addEventWatch;
            private readonly EventFilterDelegate _eventFilter;
            private int _textCompositionActive;
            private int _textInputReceivedForComposition;
            private int _textCompositionCommitPending;

            public SdlTextInput(IntPtr nativeLibrary)
            {
                _startTextInput = LoadDelegate<StartTextInputDelegate>(nativeLibrary, "SDL_StartTextInput");
                _stopTextInput = LoadDelegate<StopTextInputDelegate>(nativeLibrary, "SDL_StopTextInput");
                _setTextInputRectangle = LoadDelegate<SetTextInputRectangleDelegate>(nativeLibrary, "SDL_SetTextInputRect");
                _addEventWatch = LoadDelegate<AddEventWatchDelegate>(nativeLibrary, "SDL_AddEventWatch");
                _eventFilter = SdlEventFilter;

                if (_addEventWatch != null)
                    _addEventWatch(_eventFilter, IntPtr.Zero);
            }

            public bool IsTextCompositionActive => Volatile.Read(ref _textCompositionActive) != 0;

            public bool ConsumeTextCompositionCommitPending() =>
                Interlocked.Exchange(ref _textCompositionCommitPending, 0) != 0;

            public void AcknowledgeTextInput()
            {
                Interlocked.Exchange(ref _textCompositionCommitPending, 0);
            }

            public void StartTextInput()
            {
                _startTextInput?.Invoke();
            }

            public void StopTextInput()
            {
                Volatile.Write(ref _textCompositionActive, 0);
                Volatile.Write(ref _textInputReceivedForComposition, 0);
                Interlocked.Exchange(ref _textCompositionCommitPending, 0);
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

            private int SdlEventFilter(IntPtr userdata, IntPtr sdlEvent)
            {
                if (sdlEvent == IntPtr.Zero)
                    return 1;

                var eventType = (uint)Marshal.ReadInt32(sdlEvent);
                if (eventType == SdlTextEditingEvent)
                {
                    // SDL_TextEditingEvent.text starts after type, timestamp and windowID.
                    var hasComposition = Marshal.ReadByte(IntPtr.Add(sdlEvent, 12)) != 0;
                    Volatile.Write(ref _textCompositionActive, hasComposition ? 1 : 0);

                    if (hasComposition)
                        Volatile.Write(ref _textInputReceivedForComposition, 0);
                    else if (Volatile.Read(ref _textInputReceivedForComposition) == 0)
                        Interlocked.Exchange(ref _textCompositionCommitPending, 1);
                }
                else if (eventType == SdlTextInputEvent)
                {
                    Volatile.Write(ref _textCompositionActive, 0);
                    Volatile.Write(ref _textInputReceivedForComposition, 1);
                    Interlocked.Exchange(ref _textCompositionCommitPending, 1);
                }

                return 1;
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void AddEventWatchDelegate(EventFilterDelegate filter, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int EventFilterDelegate(IntPtr userdata, IntPtr sdlEvent);

        // SDL2 event type values. These are stable public API constants.
        private const uint SdlTextEditingEvent = 0x302;
        private const uint SdlTextInputEvent = 0x303;
    }
}
