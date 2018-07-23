using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wobble.Graphics.Shaders
{
    public class Shader : IDisposable
    {
        /// <summary>
        ///     The loaded shader effect.
        /// </summary>
        public Effect ShaderEffect { get; }

        /// <summary>
        ///     If the shader has already been disposed of.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     The parameters of the shader. Takes in any type of object.
        ///         Format: {param_name, value}
        /// </summary>
        public Dictionary<string, object> Parameters { get; }

        /// <summary>
        ///     Loads a shader from a byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parameters"></param>
        public Shader(byte[] data, Dictionary<string, object> parameters)
        {
            ShaderEffect = new Effect(GameBase.Game.GraphicsDevice, data);
            Parameters = parameters;
            SetParameters();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            ShaderEffect.Dispose();
            IsDisposed = true;
        }

        /// <summary>
        ///     Sets all the shader parameters based on what's in the dictionary.
        /// </summary>
        public void SetParameters()
        {
            foreach (var param in Parameters)
            {
                var type = param.Value.GetType();

                if (type == typeof(bool))
                    ShaderEffect.Parameters[param.Key].SetValue((bool) param.Value);
                else if (type == typeof(float))
                    ShaderEffect.Parameters[param.Key].SetValue((float) param.Value);
                else if (type == typeof(float[]))
                    ShaderEffect.Parameters[param.Key].SetValue((float[]) param.Value);
                else if (type == typeof(int))
                    ShaderEffect.Parameters[param.Key].SetValue((int) param.Value);
                else if (type == typeof(Texture))
                    ShaderEffect.Parameters[param.Key].SetValue((Texture) param.Value);
                else if (type == typeof(Matrix))
                    ShaderEffect.Parameters[param.Key].SetValue((Matrix) param.Value);
                else if (type == typeof(Matrix[]))
                    ShaderEffect.Parameters[param.Key].SetValue((Matrix[]) param.Value);
                else if (type == typeof(Quaternion))
                    ShaderEffect.Parameters[param.Key].SetValue((Quaternion) param.Value);
                else if (type == typeof(Vector2))
                    ShaderEffect.Parameters[param.Key].SetValue((Vector2) param.Value);
                else if (type == typeof(Vector2[]))
                    ShaderEffect.Parameters[param.Key].SetValue((Vector2[]) param.Value);
                else if (type == typeof(Vector3))
                    ShaderEffect.Parameters[param.Key].SetValue((Vector3) param.Value);
                else if (type == typeof(Vector3[]))
                    ShaderEffect.Parameters[param.Key].SetValue((Vector3[]) param.Value);
                else if (type == typeof(Vector4))
                    ShaderEffect.Parameters[param.Key].SetValue((Vector4) param.Value);
                else if (type == typeof(Vector4[]))
                    ShaderEffect.Parameters[param.Key].SetValue((Vector4[]) param.Value);
                else
                    throw new InvalidTypeParameterException($"ShaderEffect Parameter {param.Key} has invalid type: {type}");
            }
        }
    }
}