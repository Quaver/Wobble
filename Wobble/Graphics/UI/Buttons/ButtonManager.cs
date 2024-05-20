using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emik;

namespace Wobble.Graphics.UI.Buttons
{
    public static class ButtonManager
    {
        /// <summary>
        ///     The list of buttons that are currently drawn.
        /// </summary>
        public static Concurrent.List<Button> Buttons { get; } = new Concurrent.List<Button>();

        /// <summary>
        ///     Adds a button to the manager.
        /// </summary>
        /// <param name="btn"></param>
        public static void Add(Button btn)
        {
            Buttons.Add(btn);
        }

        /// <summary>
        ///     Removes a button from the manager.
        /// </summary>
        /// <param name="btn"></param>
        public static void Remove(Button btn)
        {
            Buttons.Remove(btn);
        }

        public static void ResetDrawOrder()
        {
            lock (Buttons)
            {
                foreach (var b in Buttons)
                {
                    if (b == null)
                        continue;

                    b.DrawOrder = 0;
                }
            }
        }
    }
}
