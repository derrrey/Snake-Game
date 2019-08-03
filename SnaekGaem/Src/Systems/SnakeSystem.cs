/*
 *  This system moves entities corresponding to their directional vectors.
 */

using Leopotam.Ecs;
using SnaekGaem.Src.Components;
using SnaekGaem.Src.Tools;
using System.Windows;
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
        EcsFilter<Segment> segmentFilter = null;

        // Reference to the main app
        Game game = null;

        // Current movement direction vector
        Coordinates currentDirection = Coordinates.None;

        // Last frame where the entities were moved
        long lastMovementms = 0;

        // The number of ticks per second for the snake movement
        const int TICKSPERSECOND = 10;

        // The time period between snake movements
        const long WAITTIMEMS = 1000 / TICKSPERSECOND;

        // Set references
        public SnakeSystem(Game game)
        {
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
                // Set game over state for game and return
                game.gameOver = true;
                return;
            }

            // Check if the snake has to grow
            if (snakeFilter.Components1[0] != null && snakeFilter.Components1[0].shouldGrow)
            {
                Segment newSegPose = game.CreateOnCanvas(true);
                snakeFilter.Components1[0].segments.Add(newSegPose);
                snakeFilter.Components1[0].shouldGrow = false;
            }

            // Check for keyboard inputs
            Coordinates newDirection = game.GetKeyboardInput();
            if (newDirection != Coordinates.None)
                currentDirection = newDirection;

            // Check if entities have to be moved
            long currentTime = game.GetCurrentTime();
            if (lastMovementms == 0 || (currentTime - lastMovementms >= WAITTIMEMS))
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
                Segment snakeHead = snakeFilter.Components1[0].segments[0];
                return snakeFilter.Components1[0].segments.Where(seg => seg.pose.position == snakeHead.pose.position).Count() > 1;
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
                        snake.segments[segmentIndex].pose.position = snake.segments[segmentIndex - 1].pose.position;
                    }
                    // Move head in pose direction
                    else
                    {
                        // Check if new direction is set
                        if (currentDirection != Coordinates.None)
                        {
                            snake.segments[segmentIndex].pose.direction = currentDirection;
                        }

                        // Move in direction
                        snake.segments[segmentIndex].pose.position += (snake.segments[segmentIndex].pose.direction * game.segmentSize);

                        // Get max grid sizes
                        Coordinates maxGridSizes = game.GetMaxGridSizes();

                        // If the snake is out of bounds, move to opposite side
                        if (snake.segments[segmentIndex].pose.position.x < 0)
                        {
                            snake.segments[segmentIndex].pose.position.x += maxGridSizes.x;
                        }
                        if (snake.segments[segmentIndex].pose.position.y < 0)
                        {
                            snake.segments[segmentIndex].pose.position.y += maxGridSizes.y;
                        }
                        if (snake.segments[segmentIndex].pose.position.x > maxGridSizes.x)
                        {
                            snake.segments[segmentIndex].pose.position.x = 1;
                        }
                        if (snake.segments[segmentIndex].pose.position.y > maxGridSizes.y)
                        {
                            snake.segments[segmentIndex].pose.position.y = 1;
                        }
                    }

                    // Calculate new margins
                    Thickness newMargins = new Thickness(snake.segments[segmentIndex].pose.position.x,
                                                         snake.segments[segmentIndex].pose.position.y,
                                                         0, 0);

                    // Dispatch UI change to game and UI
                    game.DispatchUIChange(segmentFilter.Entities[segmentIndex].ToString(), newMargins);
                }
            }
        }
    }
}
