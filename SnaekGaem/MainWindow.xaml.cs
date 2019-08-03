using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

using SnaekGaem.Src;
using SnaekGaem.Src.Tools;

namespace SnaekGaem
{
    // Logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Window setup
            InitializeComponent();

            // Start game in new thread
            StartGame();
        }

        // Starts the game in a new thread
        void StartGame()
        {
            Thread gameThread = new Thread(GameThread);
            gameThread.Start();
        }

        // The main game method that is run in an extra thread
        void GameThread()
        {
            // Game setup
            Game game = new Game(this);

            // Start game loop
            game.StartGameLoop();

            // Set game over screen
            GameOverScreen(true);

            // Check keyboard input
            // Does the player want to retry?
            bool retry = false;
            bool quit = false;
            while(!retry && !quit)
            {
                DispatchBlocking(new Action(() => {
                    if (Keyboard.IsKeyDown(Key.R))
                    {
                        retry = true;
                    }
                    else if (Keyboard.IsKeyDown(Key.Q))
                    {
                        quit = true;
                    }
                }));
            }

            // If the player wants to retry, restart the method
            if (retry)
            {
                GameOverScreen(false);
                GameThread();
            }
            // Otherwise quit the application
            else if (quit)
            {
                DispatchNonBlocking(new Action(() =>
                {
                    Application.Current.Shutdown();
                }));
            }
        }

        // Sets/Resets a game over screen layout with the score in the middle
        // and a small text beneath it
        void GameOverScreen(bool setLayout)
        {
            // Set/Reset game over screen
            DispatchNonBlocking(new Action(() =>
            {
                // Move score box
                scoreText.HorizontalAlignment = setLayout ? HorizontalAlignment.Center : HorizontalAlignment.Right;
                scoreText.VerticalAlignment = setLayout? VerticalAlignment.Center : VerticalAlignment.Top;

                // Show/Hide game over/retry text
                gameOverText.Visibility = setLayout ? Visibility.Visible : Visibility.Hidden;
            }));
        }

        // Invokes an action to be executed by the main UI thread (non-blocking)
        public void DispatchNonBlocking(Action action)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        // Invokes an action to be executed by the main UI thread (blocking)
        public void DispatchBlocking(Action action)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        // Gets the keyboard input and returns the corresponding Coordinates
        public Coordinates CheckKeyboardInput()
        {
            Coordinates newDirection = Coordinates.None;

            // Use dispatcher to get input from UI threaad
            DispatchBlocking(() =>
            {
                // Check for all possible inputs and set direction
                if (Keyboard.IsKeyDown(Key.W))
                {
                    newDirection = Coordinates.Up;
                }
                else if (Keyboard.IsKeyDown(Key.A))
                {
                    newDirection = Coordinates.Left;
                }
                else if (Keyboard.IsKeyDown(Key.S))
                {
                    newDirection = Coordinates.Down;
                }
                else if (Keyboard.IsKeyDown(Key.D))
                {
                    newDirection = Coordinates.Right;
                }
            });

            return newDirection;
        }

        // Sets the given score as new score text
        public void SetScoreText(int score)
        {
            scoreText.Text = $"Score: {score}";
        }

        // Creates a given rectangle on the main canvas
        public void CreateRectangle(Rectangle newRect)
        {
            canvasArea.Children.Add(newRect);
            Canvas.SetTop(newRect, 0);
            Canvas.SetLeft(newRect, 0);
        }

        // Removes a rectangle from the main canvas
        public void RemoveRectangle(Rectangle rect)
        {
            canvasArea.Children.Remove(rect);
        }

        // Finds a specific child in the visual tree of the main canvas area
        public T GetCanvasChildByName<T>(string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(canvasArea); i++)
            {
                var child = VisualTreeHelper.GetChild(canvasArea, i);
                string controlName = child.GetValue(Control.NameProperty) as string;
                if (controlName == name)
                {
                    return child as T;
                }
            }
            return null;
        }
    }
}
