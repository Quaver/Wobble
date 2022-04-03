using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;

namespace Wobble.Graphics.Sprites
{
    /// <inheritdoc />
    /// <summary>
    ///     An animatable sprite. When using this, it is important NOT to manually set the image property.
    /// </summary>
    public class AnimatableSprite : Sprite
    {
        /// <summary>
        ///     The animation frames
        /// </summary>
        public List<Texture2D> Frames { get; private set; }

        /// <summary>
        ///     The current animation frame we're on.
        /// </summary>
        public int CurrentFrame { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public int DefaultFrame { get; set; }

        /// <summary>
        ///     
        /// </summary>
        public int LastFrame { get; private set; }

        /// <summary>
        ///     If the animation is currently looping.
        /// </summary>
        public bool IsLooping { get; private set; }

        /// <summary>
        ///     Animation frame time.
        /// </summary>
        public int LoopFramesPerSecond { get; private set; }

        /// <summary>
        ///     The amount of time since the last frame in the animation.
        /// </summary>
        public double TimeSinceLastAnimFrame { get; private set; }

        /// <summary>
        ///     The direction the animations will loop.
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        ///     The given frame the loop began on.
        /// </summary>
        private int FrameLoopStartedOn { get; set; }

        /// <summary>
        ///     The amount of times to loop.
        /// </summary>
        public int TimesToLoop { get; private set; }

        /// <summary>
        ///     The amount of times looped so far.
        /// </summary>
        public int TimesLooped { get; private set; }

        /// <summary>
        ///     Emitted when the sprite has finished its loop.
        /// </summary>
        public EventHandler FinishedLooping { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - if you only have the image itself, but also the rows and columns
        /// </summary>
        /// <param name="spritesheet"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public AnimatableSprite(Texture2D spritesheet, int rows, int columns)
        {
            Frames = AssetLoader.LoadSpritesheetFromTexture(spritesheet, rows, columns);
            Image = Frames[CurrentFrame];
            LastFrame = Frames.Count;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - If you already have the animation frames.
        /// </summary>
        /// <param name="frames"></param>
        public AnimatableSprite(List<Texture2D> frames)
        {
            Frames = frames;
            Image = Frames[CurrentFrame];
            LastFrame = Frames.Count;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            PerformLoopAnimation(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        ///     Changes the sprite's image to a specified frame.
        /// </summary>
        /// <param name="i"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ChangeTo(int i)
        {
            if (i > Frames.Count || i < 0)
                throw new ArgumentOutOfRangeException();

            CurrentFrame = i;
            Image = Frames[CurrentFrame];
        }

        /// <summary>
        ///     Changes the sprites image to the next frame.
        /// </summary>
        public void ChangeToNext()
        {
            if (CurrentFrame + 1 > LastFrame - 1)
                CurrentFrame = DefaultFrame;
            else
                CurrentFrame++;

            Image = Frames[CurrentFrame];
        }

        /// <summary>
        ///     Changes the sprite to the previous frame.
        /// </summary>
        public void ChangeToPrevious()
        {
            if (CurrentFrame - 1 < DefaultFrame)
                CurrentFrame = LastFrame - 1;
            else
                CurrentFrame--;

            Image = Frames[CurrentFrame];
        }

        /// <summary>
        ///     Adds a frame to the list
        /// </summary>
        /// <param name="frame"></param>
        public void AddFrame(Texture2D frame) => Frames.Add(frame);

        /// <summary>
        ///     Removes a frame from the list.
        /// </summary>
        /// <param name="frame"></param>
        public void RemoveFrame(Texture2D frame) => Frames.Remove(frame);

        /// <summary>
        ///     Removes a frame a given index.
        /// </summary>
        /// <param name="i"></param>
        /// <exception cref="ArgumentException"></exception>
        public void RemoveAt(int i)
        {
            if (i > Frames.Count || i < 0)
                throw new ArgumentOutOfRangeException();

            if (CurrentFrame == i)
                ChangeToNext();

            Frames.RemoveAt(i);
        }

        /// <summary>
        ///     Start the animation frame loop at a given FPS.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="fps"></param>
        /// <param name="timesToLoop">The amount of times to loop. If 0, it'll loop infinitely.</param>
        public void StartLoop(Direction direction, int fps, int timesToLoop = 0, int lastFrame = 0)
        {
            Direction = direction;
            LoopFramesPerSecond = fps;
            IsLooping = true;
            CurrentFrame = DefaultFrame;
            FrameLoopStartedOn = CurrentFrame;
            TimesLooped = 0;
            TimesToLoop = timesToLoop;
            if (lastFrame != 0)
                LastFrame = lastFrame;
        }

        /// <summary>
        ///     To stop the animation frame loop.
        /// </summary>
        public void StopLoop() => IsLooping = false;

        /// <summary>
        ///    Replaces all the frames with some new ones.
        /// </summary>
        /// <param name="newFrames"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ReplaceFrames(List<Texture2D> newFrames)
        {
            if (newFrames.Count == 0)
                throw new ArgumentException("The new frames added must be greater than 0.");

            Frames = newFrames;
            ChangeTo(0);
        }

        /// <summary>
        ///     Handles the looping of the animation frames.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void PerformLoopAnimation(GameTime gameTime)
        {
            if (!IsLooping || Frames.Count <= 1)
                return;

            TimeSinceLastAnimFrame += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (!(TimeSinceLastAnimFrame >= 1000f / LoopFramesPerSecond))
                return;

            switch (Direction)
            {
                case Direction.Forward:
                    ChangeToNext();
                    break;
                case Direction.Backward:
                    ChangeToPrevious();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TimeSinceLastAnimFrame = 0;

            // If we're back on the frame we've started on, then we need to increment our counter.
            if (FrameLoopStartedOn != CurrentFrame)
                return;

            TimesLooped++;
            FinishedLooping?.Invoke(this, null);

            // Automatically stop the loop if we've looped the specified amount of times.
            if (TimesToLoop != 0 && TimesLooped == TimesToLoop)
                StopLoop();
        }
    }
}