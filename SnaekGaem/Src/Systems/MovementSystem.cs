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
            // Move entities in respect to their directional vectors
            foreach (var id in poseFilteredEntities)
            {
                // Get the corresponding pose component
                ref var snake = ref poseFilteredEntities.Components1[id];

                // Iterate through snake segments from back to front
                for(int segmentIndex = snake.segments.Count - 1; segmentIndex >= 0; --segmentIndex)
                {
                    // If not head, set segment to successor
                    if (segmentIndex != 0)
                    {
                        snake.segments[segmentIndex].position = snake.segments[segmentIndex - 1].position;
                    }
                    // Move head in pose direction
                    else
                    {
                        snake.segments[segmentIndex].position += snake.segments[segmentIndex].direction;
                    }

                    // Dispatch UI change to UI thread
                    mainWindow.Dispatch(new Thickness(snake.segments[segmentIndex].position.x,
                        snake.segments[segmentIndex].position.y,
                        0, 0));

                    // Debug output
                    Logger.Info(snake.segments[segmentIndex].position.x + ", " +
                        snake.segments[segmentIndex].position.y);
                }
            }
        }
    }
}
