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
    }
}
