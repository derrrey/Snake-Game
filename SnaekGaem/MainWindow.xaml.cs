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

        // Invokes an action to be executed by the main UI thread
        public void Dispatch(Action action)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
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
    }
}
