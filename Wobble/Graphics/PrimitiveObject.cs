using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics
{
    public abstract class PrimitiveObject : Drawable3D
    {
        protected VertexPositionColorTexture[] Vertices;
        protected int[] Indices;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        public GraphicsDevice GraphicsDevice = GameBase.Game.GraphicsDevice;
        public BasicEffect Effect;
        public Texture2D Texture { get; set; }

        protected PrimitiveObject()
        {
            Effect = new BasicEffect(GraphicsDevice);
            RecalculateRectangles();
        }

        public void UpdatePrimitives()
        {
            SetupIndices();
            SetupVertices();
            CopyToBuffer();
        }

        public abstract void SetupIndices();
        public abstract void SetupVertices();

        public void CopyToBuffer()
        {
            _vertexBuffer = new VertexBuffer(GraphicsDevice,
                VertexPositionColorTexture.VertexDeclaration, Vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(Vertices);

            _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits,
                Indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(Indices);
        }

        public override void Draw(GameTime gameTime, Camera camera)
        {
            Effect.World = Transformation * camera.World.Matrix;
            Effect.Projection = camera.ProjectionMatrix;
            Effect.View = camera.ViewMatrix;
            Effect.VertexColorEnabled = true;
            Effect.TextureEnabled = Texture != null;
            Effect.Texture = Texture;
            GraphicsDevice.Indices = _indexBuffer;
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.Length / 3);
            }

            base.Draw(gameTime, camera);
        }
    }
}