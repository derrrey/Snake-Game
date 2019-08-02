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

namespace SnaekGaem.Src
{
    class Game
    {
        // Constants
        public int segmentSize { get; }

        // The ecs world instance
        public EcsWorld world { get; set; }

        // All the ecs systems
        public EcsSystems systems { get; set; }

        // Is the game over?
        public bool gameOver = false;

        // The current score
        public int score = 0;

        // Reference to main window
        MainWindow mainWindow = null;

        // Struct with an ecs entity and a deletion flag
        struct EntityWithFlag
        {
            public EcsEntity entity { get; set; }
            public bool deletionFlag { get; set; }

            public EntityWithFlag(EcsEntity entity, bool deletionFlag)
            {
                this.entity = entity;
                this.deletionFlag = deletionFlag;
            }
        }

        // Sets a new player score
        public void SetScore(int toAdd)
        {
            score += toAdd;
            mainWindow.DispatchNonBlocking(new Action(() =>
            {
                mainWindow.SetScoreText(score);
            }));
        }

        // All the entities that were created
        static List<EntityWithFlag> entities = new List<EntityWithFlag>();

        // The constructor initializes all the fields
        public Game(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            segmentSize = 20;

            // Create new ecs world instance
            Logger.Info("Creating world instance.");
            world = new EcsWorld();

            // Create the ecs systems
            Logger.Info("Creating systems.");
            systems = new EcsSystems(world)
                .Add(new SnakeSystem(mainWindow, this))
                .Add(new FoodSystem(mainWindow, this));

            // Initialize systems
            Logger.Info("Initializing systems.");
            systems.Initialize();

            // Set initial score
            SetScore(0);
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
            else if (typeof(T).Equals(typeof(Pose)))
            {
                // Cast to pose type
                Pose pose = (Pose)Convert.ChangeType(component, typeof(Pose));

                // Create entity
                Pose entityPose = new Pose();
                newEntity = world.CreateEntityWith(out entityPose);

                // Set correct pose
                entityPose.Set(ref pose);

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
            Logger.Info("Removing flagged entities.");

            // LINQ with flagged entities
            var flaggedEntities = entities.Where(entity => entity.deletionFlag == true);

            // Remove from ecs world
            foreach(var flaggedEntity in flaggedEntities)
            {
                world.RemoveEntity(flaggedEntity.entity);
            }

            // Remove from entities list
            entities.RemoveAll(entity => entity.deletionFlag == true);
        }

        // Update the application
        public void Update()
        {
            Logger.Info("Updating frame.");

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
            Pose snakeHead = CreateOnCanvas(true);
            world.GetComponent<Snake>(snakeEntity).segments.Add(snakeHead);
        }

        public Pose CreateOnCanvas(bool isSnakeSegment)
        {
            // Create segment pose
            Pose segmentPose = new Pose();

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
                    segmentPose.direction = Coordinates.Right;

                    // Set position to be the origin
                    segmentPose.position = new Coordinates(1, 1);
                }
                else if (snake.segments.Count == 1)
                {
                    // Place in opposite direction of snake head
                    Pose headPose = snake.segments[0];
                    Coordinates oppositeDir = Coordinates.GetOppositeDirection(headPose.direction);
                    segmentPose.position = headPose.position - (oppositeDir * segmentSize);
                }
                else
                {
                    // Place in opposite direction of last snake segment
                    Pose lastPose = snake.segments[snake.segments.Count - 1];
                    Pose preLastPose = snake.segments[snake.segments.Count - 2];
                    Coordinates oppositeDir = Coordinates.GetOppositeDirection((lastPose.position - preLastPose.position) / segmentSize);
                    segmentPose.position = lastPose.position - (oppositeDir * segmentSize);
                }

                CreateEntityWith(ref segmentPose, out newEntity);
            }
            else  // Food
            {
                // Get window sizes from UI thread
                double windowWidth = 0.0;
                double windowHeight = 0.0;

                mainWindow.DispatchBlocking(new Action(() =>
                {
                    windowWidth = mainWindow.Width;
                    windowHeight = mainWindow.Height;
                }));

                // Set random position
                Random random = new Random();
                Food food = new Food();
                int maxWidthGrid = Convert.ToInt32(Math.Floor(windowWidth / segmentSize)) - 1;
                int maxHeightGrid = Convert.ToInt32(Math.Floor(windowHeight / segmentSize)) - 1;
                food.pose.position = new Coordinates((random.Next(1, maxWidthGrid) * segmentSize) + 1,
                                                       (random.Next(1, maxHeightGrid) * segmentSize) + 1);

                segmentPose = food.pose;
                CreateEntityWith(ref food, out newEntity);
            }

            // Create rectangle in UI thread
            mainWindow.DispatchNonBlocking(new Action(() =>
            {
                Rectangle segmentRec = new Rectangle()
                {
                    Width = segmentSize,
                    Height = segmentSize,
                    Name = newEntity.ToString(),
                    Margin = new Thickness(segmentPose.position.x,
                                           segmentPose.position.y,
                                           0, 0),
                    Fill = isSnakeSegment ? Brushes.Black : Brushes.Green,
                };
                mainWindow.CreateRectangle(segmentRec);
            }));

            return segmentPose;
        }
    }
}
