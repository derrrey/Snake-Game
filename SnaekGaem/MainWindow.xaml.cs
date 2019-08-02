using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

using SnaekGaem.Src;
using SnaekGaem.Src.Tools;

namespace SnaekGaem
{
    // Logik for MainWindow.xaml
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Window setup
            InitializeComponent();

            // Start game in new thread
            Thread gameThread = new Thread(Game);
            gameThread.Start();
        }

        void Game()
        {
            // Game setup
            SnakeGame game = new SnakeGame(this);
            game.GameSetup();

            // Start game loop
            game.StartGameLoop();
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
            Dispatcher.Invoke(() =>
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

        // Sets the game over screen
        public void GameOver()
        {
            // Move score box to the middle of the screen
            scoreText.HorizontalAlignment = HorizontalAlignment.Center;
            scoreText.VerticalAlignment = VerticalAlignment.Center;

            // Show game over / retry text
            gameOverText.Visibility = Visibility.Visible;
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
