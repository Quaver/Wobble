using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics
{
    public class Quad : PrimitiveObject
    {
        public override void SetupIndices()
        {
            Indices = new[]
            {
                0, 1, 2,
                0, 2, 3
            };
        }

        public override void SetupVertices()
        {
            Vertices = new[]
            {
                new VertexPositionColorTexture(Vector3.Zero * Size, Color.White, Vector2.Zero),
                new VertexPositionColorTexture(Vector3.UnitX * Size, Color.White, Vector2.UnitX),
                new VertexPositionColorTexture((Vector3.UnitX + Vector3.UnitY) * Size, Color.White, Vector2.UnitX + Vector2.UnitY),
                new VertexPositionColorTexture(Vector3.UnitY * Size, Color.White, Vector2.UnitY),
            };
        }

        public override void Draw(GameTime gameTime, Camera camera)
        {
            base.Draw(gameTime, camera);
        }
    }
}