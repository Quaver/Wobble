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
            SetParameters(false);
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
        public void SetParameters(bool setDictionaryValues)
        {
            foreach (var param in Parameters)
                SetParameter(param.Key, param.Value, setDictionaryValues);
        }

        /// <summary>
        ///     Sets an individual parameter
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="setDictionaryValue"></param>
        /// <exception cref="InvalidTypeParameterException"></exception>
        public void SetParameter(string key, object value, bool setDictionaryValue)
        {
            // Set the parameter in the dictionary.
            if (setDictionaryValue)
                Parameters[key] = value;

            var type = value.GetType();

            // Set the parameter in the shader.
            if (type == typeof(bool))
                ShaderEffect.Parameters[key].SetValue((bool) value);
            else if (type == typeof(float))
                ShaderEffect.Parameters[key].SetValue((float) value);
            else if (type == typeof(float[]))
                ShaderEffect.Parameters[key].SetValue((float[]) value);
            else if (type == typeof(int))
                ShaderEffect.Parameters[key].SetValue((int) value);
            else if (type == typeof(Texture))
                ShaderEffect.Parameters[key].SetValue((Texture) value);
            else if (type == typeof(Matrix))
                ShaderEffect.Parameters[key].SetValue((Matrix) value);
            else if (type == typeof(Matrix[]))
                ShaderEffect.Parameters[key].SetValue((Matrix[]) value);
            else if (type == typeof(Quaternion))
                ShaderEffect.Parameters[key].SetValue((Quaternion) value);
            else if (type == typeof(Vector2))
                ShaderEffect.Parameters[key].SetValue((Vector2) value);
            else if (type == typeof(Vector2[]))
                ShaderEffect.Parameters[key].SetValue((Vector2[]) value);
            else if (type == typeof(Vector3))
                ShaderEffect.Parameters[key].SetValue((Vector3) value);
            else if (type == typeof(Vector3[]))
                ShaderEffect.Parameters[key].SetValue((Vector3[]) value);
            else if (type == typeof(Vector4))
                ShaderEffect.Parameters[key].SetValue((Vector4) value);
            else if (type == typeof(Vector4[]))
                ShaderEffect.Parameters[key].SetValue((Vector4[]) value);
            else
                throw new InvalidTypeParameterException($"ShaderEffect Parameter {key} has invalid type: {type}");
        }
    }
}