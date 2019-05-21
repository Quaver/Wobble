using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wobble.Assets;
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
        ///     Replayable Train sound effect.
        /// </summary>
        public AudioSample Train { get; }

        /// <summary>
        ///     Replayable hitsound sample.
        /// </summary>
        public AudioSample HitSound { get; }

        /// <summary>
        /// </summary>
        public TestAudioScreen()
        {
            Song = new AudioTrack(GameBase.Game.Resources.Get("Wobble.Tests.Resources/Tracks/virt - Send My Love To Mars.mp3"));
            Train = new AudioSample(GameBase.Game.Resources.Get("Wobble.Tests.Resources/SFX/train.wav"));
            HitSound = new AudioSample(GameBase.Game.Resources.Get("Wobble.Tests.Resources/SFX/sound-hit.wav"));

            Song?.Play();

            View = new TestAudioScreenView(this);
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
