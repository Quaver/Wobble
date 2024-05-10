using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics
{
    public class Camera : Drawable3D
    {
        private Vector3 _target = Vector3.Zero;

        public Vector3 Target
        {
            get => _target;
            set
            {
                _target = value;
                ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up); // Y up
            }
        }

        public float TargetX
        {
            get => Target.X;
            set => Target = new Vector3(value, Target.Y, Target.Z);
        }
        public float TargetY
        {
            get => Target.Y;
            set => Target = new Vector3(Target.X, value, Target.Z);
        }
        public float TargetZ
        {
            get => Target.Z;
            set => Target = new Vector3(Target.X, Target.Y, value);
        }

        public Matrix ProjectionMatrix { get; private set; }
        public Matrix ViewMatrix { get; private set; }
        public ProjectionMode ProjectionMode { get; }

        public Camera(World world, Vector3 target)
        {
            ProjectionMode = new ProjectionMode(GameBase.Game.Graphics.GraphicsDevice.Viewport.Bounds, this);
            World = world;
            Target = target;
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            CalculateMatrices();
        }

        public void CalculateMatrices()
        {
            ProjectionMatrix = ProjectionMode.GetProjectionMatrix();
            ViewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up); // Y up
        }

        public void Draw(GameTime gameTime)
        {
            World.Draw(gameTime, this);
        }
    }
}