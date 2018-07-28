using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Helpers;
using Wobble.Screens;
using Wobble.Tests.Screens.Tests.EasingAnimations;

namespace Wobble.Tests.Screens.Tests.Audio
{
    public class TestAudioScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The song that'll be played throughout this screen
        /// </summary>
        public AudioTrack Song { get; internal set; }

        /// <summary>
        ///     Ping pong sound effect.
        /// </summary>
        public AudioSample Train { get; }

        /// <summary>
        /// </summary>
        public TestAudioScreen()
        {
            Song = new AudioTrack(ResourceStore.virt___Send_My_Love_To_Mars);
            Train = new AudioSample(ResourceStore.train);

            Song?.Play();

            View = new TestAudioScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Song?.Dispose();
            Train?.Dispose();

            base.Destroy();
        }
    }
}
