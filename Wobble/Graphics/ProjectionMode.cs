using System;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics
{
    public class ProjectionMode
    {
        private Rectangle _viewportBounds;
        private ProjectionType _projectionType;
        private float _fieldOfView = 45f;
        private float _nearPlaneDistance = 1f;
        private float _farPlaneDistance = 1000f;
        private float _zNearPlane = 0;
        private float _zFarPlane = 1;

        public Camera Camera { get; set; }

        public Rectangle ViewportBounds
        {
            get => _viewportBounds;
            set => _viewportBounds = value;
        }

        public ProjectionMode(Rectangle viewportBounds, Camera camera)
        {
            ViewportBounds = viewportBounds;
            Camera = camera;
        }

        public ProjectionType ProjectionType
        {
            get => _projectionType;
            set
            {
                _projectionType = value;
                Camera.CalculateMatrices();
            }
        }

        public float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                _fieldOfView = value;
                Camera.CalculateMatrices();
            }
        }

        public float NearPlaneDistance
        {
            get => _nearPlaneDistance;
            set
            {
                _nearPlaneDistance = value;
                Camera.CalculateMatrices();
            }
        }

        public float FarPlaneDistance
        {
            get => _farPlaneDistance;
            set
            {
                _farPlaneDistance = value;
                Camera.CalculateMatrices();
            }
        }

        public float ZNearPlane
        {
            get => _zNearPlane;
            set
            {
                _zNearPlane = value;
                Camera.CalculateMatrices();
            }
        }

        public float ZFarPlane
        {
            get => _zFarPlane;
            set
            {
                _zFarPlane = value;
                Camera.CalculateMatrices();
            }
        }

        public Matrix GetProjectionMatrix()
        {
            switch (ProjectionType)
            {
                case ProjectionType.Perspective:
                    return Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(FieldOfView), GameBase.Game.Graphics.GraphicsDevice.Viewport.AspectRatio,
                        NearPlaneDistance, FarPlaneDistance);
                case ProjectionType.Orthographic:
                    return Matrix.CreateOrthographicOffCenter(ViewportBounds, ZNearPlane, ZFarPlane);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}