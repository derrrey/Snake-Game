/*
 *  This system moves entities corresponding to their directional vectors.
 */

using Leopotam.Ecs;
using SnaekGaem.Src.Components;
using SnaekGaem.Src.Tools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Input;

namespace SnaekGaem.Src.Systems
{
    // Use Code-Injection for filter
    [EcsInject]
    class MovementSystem : IEcsRunSystem
    {
        // Filter for entities with a pose component
        EcsFilter<Snake> poseFilteredEntities = null;

        // Reference to main window
        MainWindow mainWindow = null;

        // Set references
        public MovementSystem(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        // Nothing to do on destroy
        public void Destroy() {}

        // Iterate over entities with pose component and move in corresponce to the directional vector
        public void Run()
        {
            // Check for keyboard inputs
            Coordinates newDirection = mainWindow.CheckKeyboardInput();

            // Move entities
            MoveEntities(ref newDirection);
        }

        void MoveEntities(ref Coordinates newDirection)
        {
            // Move entities in respect to their directional vectors
            foreach (var id in poseFilteredEntities)
            {
                // Get the corresponding pose component
                ref var snake = ref poseFilteredEntities.Components1[id];

                // Iterate through snake segments from back to front
                for (int segmentIndex = snake.segments.Count - 1; segmentIndex >= 0; --segmentIndex)
                {
                    // If not head, set segment to successor
                    if (segmentIndex != 0)
                    {
                        snake.segments[segmentIndex].position = snake.segments[segmentIndex - 1].position;
                    }
                    // Move head in pose direction
                    else
                    {
                        // Check if new direction is set
                        if (newDirection != Coordinates.None)
                        {
                            snake.segments[segmentIndex].direction = newDirection;
                        }

                        snake.segments[segmentIndex].position += snake.segments[segmentIndex].direction;
                    }

                    // Calculate new margins
                    Thickness newMargins = new Thickness(snake.segments[segmentIndex].position.x,
                                                         snake.segments[segmentIndex].position.y,
                                                         0, 0);

                    // Dispatch UI change to UI thread
                    mainWindow.Dispatch(new Action(() =>
                    {
                        var myRect = (Rectangle)mainWindow.FindName("TestRect");
                        myRect.SetValue(Canvas.MarginProperty, newMargins);
                    }));

                    // Debug output
                    Logger.Info(snake.segments[segmentIndex].position.x + ", " +
                        snake.segments[segmentIndex].position.y);
                }
            }
        }
    }
}
