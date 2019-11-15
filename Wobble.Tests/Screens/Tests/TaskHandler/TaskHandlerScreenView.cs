using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble.Scheduling;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.TaskHandler
{
    public class TaskHandlerScreenView : ScreenView
    {
        /// <summary>
        ///    Task Handler for testing
        /// </summary>
        public TaskHandler<int, int> RandomNumbersTask { get; }

        /// <summary>
        ///    Random Number Generator
        /// </summary>
        private Random RNG { get; } = new Random();

        /// <summary>
        ///    User will press this to Run Random Numbers Task
        /// </summary>
        private TextButton TestButtonMultiple { get; }

        /// <summary>
        ///    User will press this to Run Random Numbers Task
        /// </summary>
        private TextButton TestButtonSingle { get; }

        /// <summary>
        ///    Displays progression of Random Numbers Task
        /// </summary>
        private SpriteText ProgressionText { get; }

        /// <summary>
        ///    Displays result of Random Numbers Task
        /// </summary>
        private SpriteText ResultText { get; }

        /// <summary>
        ///     Will get updated when the MultipleTasks Button gets pressed
        /// </summary>
        private SpriteText MultipleTasksText { get; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TaskHandlerScreenView(Screen screen) : base(screen)
        {
            RandomNumbersTask = new TaskHandler<int, int>(TestFunction);
            RandomNumbersTask.OnCompleted += OnTaskComplete;
            RandomNumbersTask.OnCancelled += OnTaskCancelled;
            RandomNumbersTask.OnStarted += OnTaskStarted;

            TestButtonMultiple = new TextButton(WobbleAssets.WhiteBox, "exo2-medium", "Start Multiple Task", 16)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(350, 60),
                Text =
                {
                    Tint = Color.Black,
                },
                Y = -60
            };
            TestButtonMultiple.Clicked += TestButtonMultipleClicked;

            TestButtonSingle = new TextButton(WobbleAssets.WhiteBox, "exo2-medium", "Start Single Task", 16)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(350, 60),
                Text =
                {
                    Tint = Color.Black,
                },
                Y = -130
            };
            TestButtonSingle.Clicked += TestButtonSingleClicked;

            MultipleTasksText = new SpriteText("exo2-medium", "", 14)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Y = -200
            };

            ProgressionText = new SpriteText("exo2-medium", "", 14)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Y = 10
            };

            ResultText = new SpriteText("exo2-medium", "", 14)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Y = 40
            };
        }

        /// <summary>
        ///    Call when the button is pressed.
        /// </summary>
        private void TestButtonMultipleClicked(object sender, EventArgs args)
        {
            if (RandomNumbersTask.IsRunning)
            {
                RandomNumbersTask.Cancel();
                return;
            }

            // Initial Input
            var input = RNG.Next(1000, 9999);
            var initial = input;
            //Console.WriteLine($"INITIAL INPUT: {input}");

            // This Code will Run Random Numbers Task 10 times at once. Previous tasks will automatically be cancelled.
            for (var i = 0; i < 10; i++)
            {
                RandomNumbersTask.Run(input, 2000);
                input = RNG.Next(1000, 9999);
                if (i > 4) Thread.Sleep(i * 2);
            }

            // The previous tasks should've been cancelled and the output in the current task should match the final input.
            //Console.WriteLine($"FINAL INPUT: {input}");
            RandomNumbersTask.Run(input, 2000);
            MultipleTasksText.Text = $"Initial Input: {initial}";
        }

        private void TestButtonSingleClicked(object sender, EventArgs args)
        {
            MultipleTasksText.Text = "";
            if (RandomNumbersTask.IsRunning)
            {
                RandomNumbersTask.Cancel();
                return;
            }

            var input = RNG.Next(1000, 9999);
            RandomNumbersTask.Run(input, 2000);
        }

        private void OnTaskStarted(object sender, TaskStartedEventArgs<int> args)
        {
            TestButtonMultiple.Tint = Color.Red;
            TestButtonMultiple.Text.Text = "Cancel Task";
            TestButtonSingle.Tint = Color.Red;
            TestButtonSingle.Text.Text = "Cancel Task";
            ProgressionText.Text = $"Computing... Current Input = {args.Input}";
        }

        /// <summary>
        ///    Call when Random Numbers Task is completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnTaskComplete(object sender, TaskCompleteEventArgs<int, int> args)
        {
            ResultText.Text = $"Result = {args.Result}";
            ResultText.Tint = Color.Green;
            TestButtonMultiple.Tint = Color.White;
            TestButtonMultiple.Text.Text = "Start Multiple Tasks";
            TestButtonSingle.Tint = Color.White;
            TestButtonSingle.Text.Text = "Start Single Task";
            ProgressionText.Text = "Completed.";
        }

        /// <summary>
        ///    Call when Random Numbers Task is cancelled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnTaskCancelled(object sender, TaskCancelledEventArgs<int> args)
        {
            ResultText.Text = $"Cancelled. Previous Input = {args.Input}";
            ResultText.Tint = Color.DarkRed;
            TestButtonMultiple.Tint = Color.White;
            TestButtonMultiple.Text.Text = "Start Multiple Tasks";
            TestButtonSingle.Tint = Color.White;
            TestButtonSingle.Text.Text = "Start Single Task";
            ProgressionText.Text = "Task Cancelled.";
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
            //base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            RandomNumbersTask.Dispose();
            Container?.Destroy();
        }

        /// <summary>
        ///    This method will keep adding by one until it reaches the input value * 1000.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int TestFunction(int input, CancellationToken token)
        {
            var temp = 0;

            while (temp < input * 1000) temp++;
            return temp / 1000;
        }
    }
}
