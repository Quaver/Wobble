using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wobble.Graphics.UI.Buttons
{
    public static class ButtonManager
    {
        /// <summary>
        ///     The list of buttons that are currently drawn.
        /// </summary>
        public static ConcurrentDictionary<Button, byte> Buttons { get; } = new ConcurrentDictionary<Button, byte>();

        /// <summary>
        ///     Adds a button to the manager.
        /// </summary>
        /// <param name="btn"></param>
        public static void Add(Button btn)
        {
            Buttons.TryAdd(btn, 0);
        }

        /// <summary>
        ///     Removes a button from the manager.
        /// </summary>
        /// <param name="btn"></param>
        public static void Remove(Button btn)
        {
            Buttons.TryRemove(btn, out _);
        }

        public static void ResetDrawOrder()
        {
            lock (Buttons)
            {
                foreach (var (b, _) in Buttons)
                {
                    if (b == null)
                        continue;

                    b.DrawOrder = 0;
                }
            }
        }
    }
}
