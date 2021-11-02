using System;
using System.Collections.Generic;
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

        public string GetName()
        {
            if (KeyboardKey != null)
                return XnaKeyHelper.GetStringFromKey((Keys)KeyboardKey);

            if (JoystickKey != null)
                return $"GP_{JoystickKey}";

            Logger.Error("Both keyboard and joystick key is null in GenericKey", LogType.Runtime);
            return "INVALID";
        }

        protected bool Equals(GenericKey other) => keyboardKey == other.keyboardKey && joystickKey == other.joystickKey;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GenericKey)obj);
        }

        public override int GetHashCode() => HashCode.Combine(keyboardKey, joystickKey);

        public override string ToString()
        {
            if (KeyboardKey != null)
                return KeyboardKey.ToString();

            if (JoystickKey != null)
                return $"GP_{JoystickKey}";

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
                int button;
                if (int.TryParse(value.Substring(3), out button) && button >= 0)
                {
                    result = new GenericKey { JoystickKey = button };
                    return true;
                }
            }

            Keys key;
            if (Keys.TryParse(value, out key))
            {
                result = new GenericKey { KeyboardKey = key };
                return true;
            }

            return false;
        }
    }
}