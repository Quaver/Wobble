using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Bindables;
using Wobble.Graphics.UI.Buttons;

namespace Wobble.Graphics.UI.Form
{
    public class Checkbox : Button
    {
        /// <summary>
        ///     The value that the checkbox is binded to.
        /// </summary>
        public Bindable<bool> BindedValue { get; }

        /// <summary>
        ///     If true, when the checkbox is destroyed, it will also dispose of the Bindable's event handlers.
        /// </summary>
        public bool DisposeBindableOnDestroy { get; }

        /// <summary>
        ///     The image displayed when the checkbox is active.
        /// </summary>
        public Texture2D ActiveImage { get; set; }

        /// <summary>
        ///     The image displayed when the checkbox is not active.
        /// </summary>
        public Texture2D InactiveImage { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="bindedValue"></param>
        /// <param name="size"></param>
        /// <param name="inactiveImage"></param>
        /// <param name="disposeBindableOnDestroy"></param>
        /// <param name="activeImage"></param>
        public Checkbox(Bindable<bool> bindedValue, Vector2 size, Texture2D activeImage, Texture2D inactiveImage, bool disposeBindableOnDestroy)
        {
            BindedValue = bindedValue;
            DisposeBindableOnDestroy = disposeBindableOnDestroy;
            ActiveImage = activeImage;
            InactiveImage = inactiveImage;

            Size = new ScalableVector2(size.X, size.Y);
            SetCheckboxImage();

            BindedValue.ValueChanged += OnBindedValueChanged;

            Clicked += (sender, args) => BindedValue.Value = !BindedValue.Value;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (DisposeBindableOnDestroy)
                BindedValue.Dispose();

            base.Destroy();
        }

        /// <summary>
        ///    When the bindable's value changes, this'll be called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnBindedValueChanged(object sender, BindableValueChangedEventArgs<bool> e) => SetCheckboxImage();

        /// <summary>
        ///     Sets the checkbox image based on the bindable's value.
        /// </summary>
        private void SetCheckboxImage() => Image = BindedValue.Value ? ActiveImage : InactiveImage;
    }
}