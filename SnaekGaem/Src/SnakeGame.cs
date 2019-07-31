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
        const int FRAMERATE = 1;
        const int MAXFRAMETIME = 1000 / FRAMERATE;

        // Reference to the main application
        Application mainApp = null;

        // Called once at startup
        public void GameSetup()
        {
            // Create the application
            Logger.Info("Creating main application.");
            mainApp = new Application();

            // Setup game entities
            Logger.Info("Setting up game entities.");
            mainApp.GameSetup();
        }

        // Main game loop.
        public void StartGameLoop()
        {
            Logger.Info("Starting game loop.");

            // Variables for framerate syncing and deltatime
            long frameStart;
            long frameTime;

            // Main game loop
            while (true)
            {
                // Get time of frame start
                frameStart = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;

                // Update the app
                mainApp.Update();

                // Remove flagged entities
                mainApp.RemoveFlaggedEntities();

                // Remove one frame components
                mainApp.world.RemoveOneFrameComponents();

                // Calculate frame time
                frameTime = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) - frameStart;

                // Delay if time is left
                if(MAXFRAMETIME > frameTime)
                {
                    System.Threading.Thread.Sleep(Convert.ToInt32(MAXFRAMETIME - frameTime));
                }
            }
        }
    }
}
