using Microsoft.Xna.Framework;

namespace Wobble.Graphics
{
    public class World : Drawable3D
    {
        public Matrix Matrix { get; }

        public World()
        { 
            Matrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
            RecalculateRectangles();
        }

        public Camera CreateCamera(Vector3 target)
        {
            return new Camera(this, target);
        }
    }
}