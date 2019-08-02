/*
 * This file contains the main function for the snake game.
 */

using SnaekGaem.Src.Tools;
using System;

namespace SnaekGaem.Src
{
    /*
     *  The main class for the snake game.
     */
    class SnakeGame
    {
        // Constants
        const int FRAMERATE = 100;
        const int MAXFRAMETIME = 1000 / FRAMERATE;

        // Reference to the main application
        Game game = null;

        // Reference to the main window
        MainWindow mainWindow = null;

        // Set main window reference
        public SnakeGame(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        // Called once at startup
        public void GameSetup()
        {
            // Create the application
            Logger.Info("Creating main application.");
            game = new Game(mainWindow);

            // Setup game entities
            Logger.Info("Setting up game entities.");
            game.GameSetup();
        }

        // Main game loop.
        public void StartGameLoop()
        {
            Logger.Info("Starting game loop.");

            // Variables for framerate syncing and deltatime
            long frameStart;
            long frameTime;

            // Main game loop
            while (!game.gameOver)
            {
                // Get time of frame start
                frameStart = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;

                // Update the app
                game.Update();

                // Remove flagged entities
                game.RemoveFlaggedEntities();

                // Remove one frame components
                game.world.RemoveOneFrameComponents();

                // Calculate frame time
                frameTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - frameStart;

                // Delay if time is left
                if(MAXFRAMETIME > frameTime)
                {
                    System.Threading.Thread.Sleep(Convert.ToInt32(MAXFRAMETIME - frameTime));
                }
            }

            // Remove entities
            game.RemoveAllEntities();
        }
    }
}
