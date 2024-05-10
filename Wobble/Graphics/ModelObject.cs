using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics
{
    public class ModelObject : Drawable3D
    {
        private Model _model;

        public Model Model
        {
            get => _model;
            set
            {
                _model = value;
                CalculateModelBoundingBox();
            }
        }

        public BoundingBox ModelBoundingBox { get; private set; }

        public ModelObject(Model model)
        {
            Model = model;

            RecalculateRectangles();
        }

        public void CalculateModelBoundingBox()
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    var vertexBufferSize = meshPart.NumVertices * vertexStride;

                    var vertexDataSize = vertexBufferSize / sizeof(float);
                    var vertexData = new float[vertexDataSize];
                    meshPart.VertexBuffer.GetData(vertexData);

                    for (var i = 0; i < vertexDataSize; i += vertexStride / sizeof(float))
                    {
                        var vertex = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
                        min = Vector3.Min(min, vertex);
                        max = Vector3.Max(max, vertex);
                    }
                }
            }

            ModelBoundingBox = new BoundingBox(min, max);
        }

        public override void Draw(GameTime gameTime, Camera camera)
        {
            foreach (var mesh in Model.Meshes)
            {
                foreach (var effect in mesh.Effects.Cast<BasicEffect>())
                {
                    effect.EnableDefaultLighting();
                    // effect.AmbientLightColor = new Vector3(1f, 0, 0);
                    effect.View = camera.ViewMatrix;
                    effect.World = Transformation * camera.World.Matrix;
                    effect.Projection = camera.ProjectionMatrix;
                }

                mesh.Draw();
            }

            base.Draw(gameTime, camera);
        }
    }
}