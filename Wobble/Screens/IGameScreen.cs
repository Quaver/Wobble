using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Wobble.Screens
{
    public interface IGameScreen
    {
        void Update(GameTime gameTime);

        void Draw(GameTime gameTime);

        void Destroy();
    }
}
