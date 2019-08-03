/*
 *  This is the main application for the snake game.
 */

using Leopotam.Ecs;
using SnaekGaem.Src.Components;
using SnaekGaem.Src.Systems;
using SnaekGaem.Src.Tools;

using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace SnaekGaem.Src
{
    class Game
    {
        // Constants
        const int FRAMERATE = 100;
        const int MAXFRAMETIME = 1000 / FRAMERATE;

        // The ecs world instance
        public EcsWorld world { get; set; }

        // All the ecs systems
        public EcsSystems systems { get; set; }

        // Is the game over?
        public bool gameOver = false;

        // The current score
        public int score = 0;

        // The segment size of food and snake segment entities on the canvas
        public int segmentSize = 20;

        // Reference to main window
        MainWindow mainWindow = null;

        // All the entities that were created
        static List<EntityWithFlag> entities = new List<EntityWithFlag>(256);

        // Sets a new player score
        public void SetScore(int toAdd)
        {
            score += toAdd;
            mainWindow.DispatchNonBlocking(() =>
            {
                mainWindow.SetScoreText(score);
            });
        }

        // The constructor initializes all the fields
        public Game(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            // Create new ecs world instance
            Logger.Info("Creating world instance.");
            world = new EcsWorld();

            // Create the ecs systems
            Logger.Info("Creating systems.");
            systems = new EcsSystems(world)
                .Add(new SnakeSystem(this))
                .Add(new FoodSystem(this));

            // Initialize systems
            Logger.Info("Initializing systems.");
            systems.Initialize();

            // Set initial score
            Logger.Info("Setting initial score.");
            SetScore(0);

            // Setup game
            Logger.Info("Setting up game entities.");
            GameSetup();
        }

        // Destroy instances in destructor
        ~Game()
        {
            // Destroy all entities
            Logger.Info("Destroying all entities.");
            foreach (var entity in entities)
            {
                world.RemoveEntity(entity.entity);
            }

            // Destroy ecs world instance
            Logger.Info("Destroying world instance.");
            world.Dispose();
            world = null;

            // Destroy the ecs systems instance
            Logger.Info("Destroying systems.");
            systems.Dispose();
            systems = null;
        }

        // Gets the keyboard input from the UI thread
        public Coordinates GetKeyboardInput()
        {
            return mainWindow.CheckKeyboardInput();
        }

        // Main game loop.
        public void StartGameLoop()
        {
            Logger.Info("Starting game loop.");

            // Variables for framerate syncing and deltatime
            long frameStart;
            long frameTime;

            // Main game loop
            while (!gameOver)
            {
                // Get time of frame start
                frameStart = GetCurrentTime();

                // Update the app
                Update();

                // Remove flagged entities
                RemoveFlaggedEntities();

                // Remove one frame components
                world.RemoveOneFrameComponents();

                // Calculate frame time
                frameTime = GetCurrentTime() - frameStart;

                // Delay if time is left
                if (MAXFRAMETIME > frameTime)
                {
                    System.Threading.Thread.Sleep(Convert.ToInt32(MAXFRAMETIME - frameTime));
                }
            }

            // Remove entities
            RemoveAllEntities();
        }

        // Returns the max grid sizes based on the segment size
        // and the size of the canvas that is drawn on
        public Coordinates GetMaxGridSizes()
        {
            // Get window sizes from UI thread
            double windowWidth = 0;
            double windowHeight = 0;
            mainWindow.DispatchBlocking(() =>
            {
                windowWidth = mainWindow.canvasArea.Width;
                windowHeight = mainWindow.canvasArea.Height;
            });

            // Calculate max position on grid
            Coordinates maxGridSizes = new Coordinates();
            maxGridSizes.x = (Convert.ToInt32(Math.Floor(windowWidth / segmentSize))) * segmentSize;
            maxGridSizes.y = ((Convert.ToInt32(Math.Floor(windowHeight / segmentSize))) + 1) * segmentSize; // + 1 for window decorations

            return maxGridSizes;
        }

        // Updates the given object on the UI given the new margins
        public void DispatchUIChange(string toChange, Thickness newMargins)
        {
            mainWindow.DispatchBlocking(() =>
            {
                var rectOnCanvas = mainWindow.GetCanvasChildByName<Rectangle>(toChange);
                if (rectOnCanvas != null)
                {
                    rectOnCanvas.SetValue(Canvas.MarginProperty, newMargins);
                }
            });
        }

        // Returns the current time
        public long GetCurrentTime()
        {
            return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        }

        // Creates an entity with given pose component
        void CreateEntityWith<T>(ref T component, out EcsEntity newEntity) where T : class
        {
            // Check component type
            if (typeof(T).Equals(typeof(Food)))
            {
                // Cast to pose type
                Food food = (Food)Convert.ChangeType(component, typeof(Food));

                // Create entity
                Food entityFood = new Food();
                newEntity = world.CreateEntityWith(out entityFood);

                // Set correct pose
                entityFood.pose.position = food.pose.position;
                entityFood.pose.direction = food.pose.direction;

                // Add to entities
                entities.Add(new EntityWithFlag(newEntity, false));
            }
            else if (typeof(T).Equals(typeof(Segment)))
            {
                // Cast to pose type
                Segment pose = (Segment)Convert.ChangeType(component, typeof(Segment));

                // Create entity
                Segment entityPose = new Segment();
                newEntity = world.CreateEntityWith(out entityPose);

                // Set correct pose
                entityPose.pose.Set(pose.pose);

                // Add to entities
                entities.Add(new EntityWithFlag(newEntity, false));
            }
            else if (typeof(T).Equals(typeof(Snake)))
            {
                // Cast to snake type
                Snake snake = (Snake)Convert.ChangeType(component, typeof(Snake));

                // Create entity
                Snake snakeEntity = new Snake();
                newEntity = world.CreateEntityWith(out snakeEntity);

                // Set correct snake
                snakeEntity.segments = snake.segments;

                // Add to entities
                entities.Add(new EntityWithFlag(newEntity, false));
            }
            else
            {
                Logger.Warn("Unknown component type!");
                newEntity = new EcsEntity();
            }
        }

        // Marks an entity to be removed after the next update cycle
        public void SetDeletionFlag(int id)
        {
            // Get the index of the correct entity
            var entityIndex = entities.FindIndex(entity => entity.entity.Id == id);

            // Set the flag
            entities[entityIndex] = new EntityWithFlag(entities[entityIndex].entity, true);
        }

        // Removes all entities
        public void RemoveFlaggedEntities()
        {
            // LINQ with flagged entities
            var flaggedEntities = entities.Where(entity => entity.deletionFlag == true);

            // Remove from ecs world
            foreach(var flaggedEntity in flaggedEntities)
            {
                world.RemoveEntity(flaggedEntity.entity);
            }

            // Remove from canvas
            mainWindow.DispatchBlocking(() =>
            {
                foreach (EntityWithFlag entity in entities)
                {
                    if (entity.deletionFlag == true)
                    {
                        Rectangle entityRect = mainWindow.GetCanvasChildByName<Rectangle>(entity.entity.ToString());
                        mainWindow.RemoveRectangle(entityRect);
                    }
                }
            });

            // Remove from entities list
            entities.RemoveAll(entity => entity.deletionFlag == true);
        }

        // Removes all entities
        public void RemoveAllEntities()
        {
            // Set deletion flags
            for(int index = 0; index < entities.Count; ++index)
            {
                SetDeletionFlag(entities[index].entity.Id);
            }

            // Remove entities
            RemoveFlaggedEntities();
        }

        // Update the application
        public void Update()
        {
            // Update systems
            systems.Run();
        }

        // Sets up the initial entities for the game
        public void GameSetup()
        {
            // Create snake
            Snake snake = new Snake();
            EcsEntity snakeEntity;
            CreateEntityWith(ref snake, out snakeEntity);

            // Create snake head and add
            Segment snakeHead = CreateOnCanvas(true);
            world.GetComponent<Snake>(snakeEntity).segments.Add(snakeHead);

            // Create first food
            CreateOnCanvas(false);
        }

        public Segment CreateOnCanvas(bool isSnakeSegment)
        {
            // Create segment pose
            Segment segmentPose = new Segment();

            // New entity
            EcsEntity newEntity;

            if (isSnakeSegment)
            {
                // Get reference on snake
                Snake snake = world.GetComponent<Snake>(entities[0].entity);

                // Check it it's the first segment
                if (snake.segments.Count == 0)
                {
                    // If yes, set the initial movement direction
                    segmentPose.pose.direction = Coordinates.Right;

                    // Set position to be the origin
                    segmentPose.pose.position = new Coordinates(1, 1);
                }
                else if (snake.segments.Count == 1)
                {
                    // Place in opposite direction of snake head
                    Segment headPose = snake.segments[0];
                    Coordinates oppositeDir = Coordinates.GetOppositeDirection(headPose.pose.direction);
                    segmentPose.pose.position = headPose.pose.position - (oppositeDir * segmentSize);
                }
                else
                {
                    // Place in opposite direction of last snake segment
                    Segment lastPose = snake.segments[snake.segments.Count - 1];
                    Segment preLastPose = snake.segments[snake.segments.Count - 2];
                    Coordinates oppositeDir = Coordinates.GetOppositeDirection((lastPose.pose.position - preLastPose.pose.position) / segmentSize);
                    segmentPose.pose.position = lastPose.pose.position - (oppositeDir * segmentSize);
                }

                CreateEntityWith(ref segmentPose, out newEntity);
            }
            else  // Food
            {
                // Get max grid sizes
                Coordinates maxGridSizes = GetMaxGridSizes();

                // Set random position
                Random random = new Random();
                Food food = new Food();
                food.pose.position = new Coordinates(((random.Next(1, maxGridSizes.x / segmentSize) * segmentSize) + 1),
                                                       (((random.Next(1, maxGridSizes.y / segmentSize) * segmentSize) + 1)));

                segmentPose.pose = food.pose;
                CreateEntityWith(ref food, out newEntity);
            }

            // Create rectangle in UI thread
            mainWindow.DispatchNonBlocking(() =>
            {
                Rectangle segmentRec = new Rectangle()
                {
                    Width = segmentSize,
                    Height = segmentSize,
                    Name = newEntity.ToString(),
                    Margin = new Thickness(segmentPose.pose.position.x,
                                           segmentPose.pose.position.y,
                                           0, 0),
                    Fill = isSnakeSegment ? Brushes.Black : Brushes.Green,
                };
                mainWindow.CreateRectangle(segmentRec);
            });

            return segmentPose;
        }
    }
}
