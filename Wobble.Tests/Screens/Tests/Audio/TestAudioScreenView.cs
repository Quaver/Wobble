using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Assets;
using Wobble.Audio.Tracks;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.Audio
{
    public class TestAudioScreenView : ScreenView
    {
        /// <summary>
        ///     Progress bar that displays the song time progress.
        /// </summary>
        public ProgressBar AudioTimeProgress { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestAudioScreenView(Screen screen) : base(screen)
        {
            var audioScreen = (TestAudioScreen) Screen;

            // Create the progress bar that displays the audio time.
            AudioTimeProgress = new ProgressBar(new Vector2(WindowManager.VirtualScreen.X, 10), 0,
                                                audioScreen.Song.Length, audioScreen.Song.Position, Color.DarkGray, Color.Red)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            new SpriteText("exo2-regular",
                "Press Space to pause or resume the song.\nUse the mouse wheel to adjust the song volume.\nPress T and Y to play a hitsound.", 10)
            {
                Parent = Container,
            };

            // Create play/pause button
            /*PlayButton = new TextButton(WobbleAssets.WhiteBox, Fonts.AllerRegular16, "Pause", 0.95f, (sender, args) =>
            {
                var song = audioScreen.Song;

                // If the song is already stopped, load another stream and
                // begin to play it again.
                if (song.IsStopped || song.IsDisposed)
                {
                    song = new AudioTrack(GameBase.Game.Resources.Get("Wobble.Tests.Resources/Tracks/virt - Send My Love To Mars.mp3"));
                    song.Play();

                    PlayButton.Text.Text = "Pause";

                    // Set the new audio song.
                    audioScreen.Song = song;
                    return;
                }

                // If the song is playing, pause it.
                if (song.IsPlaying)
                {
                    song.Pause();
                    PlayButton.Text.Text = "Play";
                }
                // If the song is paused, play it.
                else if (song.IsPaused)
                {
                    song.Play();
                    PlayButton.Text.Text = "Pause";
                }
            })
            {
                Parent = Container,
                Size = new ScalableVector2(125, 50),
                Alignment = Alignment.MidCenter,
                X = -200,
                Text = { TextColor = Color.Black }
            };

            // Create increase audio rate button
            IncreaseAudioRateButton = new TextButton(WobbleAssets.WhiteBox, Fonts.AllerRegular16, "Increase Rate", 0.80f, (sender, args) =>
            {
                audioScreen.Song.Rate += 0.1f;
            })
            {
                Parent = Container,
                Size = new ScalableVector2(125, 50),
                Alignment = Alignment.MidCenter,
                X = -50,
                Text = { TextColor = Color.Black }
            };

            // Create text that displays the current audio time.
            CurrentTime = new SpriteText(Fonts.AllerRegular16, "Audio Time: 0ms")
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 20,
                TextColor = Color.White,
                TextScale = 0.85f
            };

            // Create text that displays audio rate.
            AudioRate = new SpriteText(Fonts.AllerRegular16, "Audio Rate: 1.0x")
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 50,
                TextColor = Color.White,
                TextScale = 0.85f
            };

            // Create button to play a train sound
            PlayTrainSoundButton = new TextButton(WobbleAssets.WhiteBox, Fonts.AllerRegular16, "Play Train Sound", 0.75f, (sender, args) =>
            {
                audioScreen.Train.CreateChannel().Play();
            })
            {
                Parent = Container,
                Size = new ScalableVector2(125, 50),
                Alignment = Alignment.MidCenter,
                X = 100,
                Text = { TextColor = Color.Black }
            };*/
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var audioScreen = (TestAudioScreen) Screen;

            // Update the bindable's value for the song progress bar.
            AudioTimeProgress.Bindable.Value = audioScreen.Song.Time;

            var song = audioScreen.Song;
            if (song != null)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.Space))
                {
                    if (song.IsPlaying)
                        song.Pause();
                    else
                        song.Play();
                }

                if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue)
                    song.Volume += 9;
                else if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue)
                    song.Volume -= 9;
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.T) || KeyboardManager.IsUniqueKeyPress(Keys.Y))
                audioScreen.HitSound?.CreateChannel().Play();

            // When the song stops, reset the button text.
            //f (audioScreen.Song.IsStopped || audioScreen.Song.IsDisposed)
             //   PlayButton.Text.Text = "Play";

            // Update current time and audio rate text.
            //CurrentTime.Text = $"Position: {audioScreen.Song.Time}ms";
            //AudioRate.Text = $"Audio Rate: {audioScreen.Song.Rate}x";

            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();
        }
    }
}
