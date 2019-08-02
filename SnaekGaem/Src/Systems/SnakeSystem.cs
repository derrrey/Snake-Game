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
using System.Linq;

namespace SnaekGaem.Src.Systems
{
    // Use Code-Injection for filter
    [EcsInject]
    class SnakeSystem : IEcsRunSystem
    {
        // Filter for the snake entity
        EcsFilter<Snake> snakeFilter = null;

        // Filter for the snake segments
        EcsFilter<Pose> segmentFilter = null;

        // Reference to main window
        MainWindow mainWindow = null;

        // Reference to the main app
        Game game = null;

        // Current movement direction vector
        Coordinates currentDirection = Coordinates.None;

        // Last frame where the entities were moved
        long lastMovementms = 0;
        const int TICKSPERSECOND = 5;
        const long WAITTIME = 1000 / TICKSPERSECOND;

        // Set references
        public SnakeSystem(MainWindow mainWindow, Game game)
        {
            this.mainWindow = mainWindow;
            this.game = game;
        }

        // Nothing to do on destroy
        public void Destroy() {}

        // Iterate over entities with pose component and move in corresponce to the directional vector
        public void Run()
        {
            // Check for game over
            if (GameOver())
            {
                // Set game over state for game
                game.gameOver = true;
            }

            // Check if the snake has to grow
            if (snakeFilter.Components1[0] != null && snakeFilter.Components1[0].shouldGrow)
            {
                Pose newSegPose = game.CreateOnCanvas(true);
                snakeFilter.Components1[0].segments.Add(newSegPose);
                snakeFilter.Components1[0].shouldGrow = false;
            }

            // Check for keyboard inputs
            Coordinates newDirection = mainWindow.CheckKeyboardInput();
            if (newDirection != Coordinates.None)
                currentDirection = newDirection;

            // Check if entities have to be moved
            long currentTime = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
            if (lastMovementms == 0 || (currentTime - lastMovementms >= WAITTIME))
            {
                // Move entities
                MoveEntities();

                // Set last movement time
                lastMovementms = currentTime;
            }
        }

        bool GameOver()
        {
            // Only check when there are components attached to the snake
            if (snakeFilter.Components1[0] != null && snakeFilter.Components1[0].segments.Count > 0)
            {
                // The game is over when the snake's head overlaps with a segment
                Pose snakeHead = snakeFilter.Components1[0].segments[0];
                return snakeFilter.Components1[0].segments.Where(seg => seg.position == snakeHead.position).Count() > 1;
            }
            return false;
        }

        void MoveEntities()
        {
            // Move entities in respect to their directional vectors
            foreach (var id in snakeFilter)
            {
                // Get the corresponding pose component
                ref var snake = ref snakeFilter.Components1[id];

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
                        if (currentDirection != Coordinates.None)
                        {
                            snake.segments[segmentIndex].direction = currentDirection;
                        }

                        snake.segments[segmentIndex].position += (snake.segments[segmentIndex].direction * game.segmentSize);
                    }

                    // Calculate new margins
                    Thickness newMargins = new Thickness(snake.segments[segmentIndex].position.x,
                                                         snake.segments[segmentIndex].position.y,
                                                         0, 0);

                    // Dispatch UI change to UI thread
                    mainWindow.DispatchBlocking(new Action(() =>
                    {
                        var myRect = mainWindow.GetCanvasChildByName<Rectangle>(segmentFilter.Entities[segmentIndex].ToString());
                        if (myRect != null)
                        {
                            myRect.SetValue(Canvas.MarginProperty, newMargins);
                        }
                    }));
                }
            }
        }
    }
}
