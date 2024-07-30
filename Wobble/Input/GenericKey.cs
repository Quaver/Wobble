using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Wobble.Helpers;
using Wobble.Logging;

namespace Wobble.Input
{
    /// <summary>
    /// Either a keyboard or a joystick key.
    /// </summary>
    public class GenericKey
    {
        private Keys? keyboardKey;

        public Keys? KeyboardKey
        {
            get => keyboardKey;
            set
            {
                Debug.Assert(value != null);
                joystickKey = null;
                keyboardKey = value;
                mouseButton = null;
                scrollDirection = null;
            }
        }

        private int? joystickKey;

        public int? JoystickKey
        {
            get => joystickKey;
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(value >= 0);
                keyboardKey = null;
                joystickKey = value;
            }
        }

        private MouseButton? mouseButton;
        private MouseScrollDirection? scrollDirection;

        public MouseButton? MouseButton
        {
            get => mouseButton;
            set
            {
                Debug.Assert(value != null);
                scrollDirection = null;
                mouseButton = value;
                joystickKey = null;
                keyboardKey = null;
            }
        }

        public MouseScrollDirection? ScrollDirection
        {
            get => scrollDirection;
            set
            {
                Debug.Assert(value != null);
                mouseButton = null;
                scrollDirection = value;
                joystickKey = null;
                keyboardKey = null;
            }
        }

        public string GetName()
        {
            if (KeyboardKey != null)
                return XnaKeyHelper.GetStringFromKey((Keys)KeyboardKey);

            if (JoystickKey != null)
                return $"GP_{JoystickKey}";

            if (MouseButton != null)
                return $"M_{MouseButton}";

            if (ScrollDirection != null)
                return $"M_{ScrollDirection}";

            Logger.Error("Both keyboard and joystick key is null in GenericKey", LogType.Runtime);
            return "INVALID";
        }

        protected bool Equals(GenericKey other) => keyboardKey == other.keyboardKey
                                                   && joystickKey == other.joystickKey
                                                   && mouseButton == other.mouseButton
                                                   && scrollDirection == other.scrollDirection;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GenericKey)obj);
        }

        public override int GetHashCode() => HashCode.Combine(keyboardKey, joystickKey, mouseButton, scrollDirection);

        public override string ToString()
        {
            if (KeyboardKey != null)
                return KeyboardKey.ToString();

            if (JoystickKey != null)
                return $"GP_{JoystickKey}";

            if (MouseButton != null)
                return $"M_{MouseButton}";

            if (ScrollDirection != null)
                return $"M_{ScrollDirection}";

            Logger.Error("Both keyboard and joystick key is null in GenericKey", LogType.Runtime);
            return "INVALID";
        }

        public static bool TryParse(string value, out GenericKey result)
        {
            result = new GenericKey();
            if (value == null)
                return false;

            if (value.StartsWith("GP_"))
            {
                if (int.TryParse(value.Substring(3), out var button) && button >= 0)
                {
                    result = new GenericKey { JoystickKey = button };
                    return true;
                }
            }

            if (value.StartsWith("M_"))
            {
                if (Enum.TryParse<MouseButton>(value.Substring(2), out var mouseButton))
                {
                    result = new GenericKey { MouseButton = mouseButton };
                    return true;
                }

                if (Enum.TryParse<MouseScrollDirection>(value.Substring(2), out var scrollDirection))
                {
                    result = new GenericKey { ScrollDirection = scrollDirection };
                    return true;
                }
            }

            if (Enum.TryParse(value, out Keys key))
            {
                result = new GenericKey { KeyboardKey = key };
                return true;
            }

            return false;
        }

        public GenericKey Clone()
        {
            return new GenericKey
            {
                joystickKey = joystickKey,
                keyboardKey = keyboardKey,
                mouseButton = mouseButton,
                scrollDirection = scrollDirection
            };
        }
    }
}