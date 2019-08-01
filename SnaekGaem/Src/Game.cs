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
        // The ecs world instance
        public EcsWorld world { get; set; }

        // All the ecs systems
        public EcsSystems systems { get; set; }

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

        // All the entities that were created
        static List<EntityWithFlag> entities = new List<EntityWithFlag>();

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
                .Add(new MovementSystem(mainWindow));

            // Initialize systems
            Logger.Info("Initializing systems.");
            systems.Initialize();
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
        public void CreateEntityWith<T>(ref T component, out EcsEntity newEntity) where T : class
        {
            // Check component type
            if (typeof(T).Equals(typeof(Pose)))
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
            // Create snake head
            Pose snakeHead = new Pose();
            // Snake head is pointing to the right
            snakeHead.direction = Coordinates.Right;
            EcsEntity headEntity;
            CreateEntityWith(ref snakeHead, out headEntity);

            // Create rectangle in UI thread
            mainWindow.Dispatcher.Invoke(() =>
            {
                Rectangle snakeHeadRec = new Rectangle()
                {
                    Width = 100,
                    Height = 100,
                    Name = "TestRect",
                    Margin = new Thickness(0, 0, 0, 0),
                    Fill = Brushes.Green,
                };
                mainWindow.CreateRectangle(snakeHeadRec);
            });

            // Create snake with head
            Snake snake = new Snake();
            snake.segments.Add(snakeHead);
            EcsEntity snakeEntity;
            CreateEntityWith(ref snake, out snakeEntity);
        }
    }
}
