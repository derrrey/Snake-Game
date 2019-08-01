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
            segmentSize = 25;

            // Create new ecs world instance
            Logger.Info("Creating world instance.");
            world = new EcsWorld();

            // Create the ecs systems
            Logger.Info("Creating systems.");
            systems = new EcsSystems(world)
                .Add(new SnakeSystem(mainWindow, this));

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
        void CreateEntityWith<T>(ref T component, out EcsEntity newEntity) where T : class
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
            // Create snake
            Snake snake = new Snake();
            EcsEntity snakeEntity;
            CreateEntityWith(ref snake, out snakeEntity);

            // Create snake head and add
            Pose snakeHead = CreateSegment();
            world.GetComponent<Snake>(snakeEntity).segments.Add(snakeHead);
        }

        public Pose CreateSegment()
        {
            // Create segment pose
            Pose segmentPose = new Pose();

            // Check it it's the first segment
            if(entities.Count == 1)
            {
                // If yes, set the initial movement direction
                segmentPose.direction = Coordinates.Right;

                // Set position to be the origin
                segmentPose.position = new Coordinates(0, 0);
            }
            else if(entities.Count == 2)
            {
                // Place in opposite direction of snake head
                Pose headPose = world.GetComponent<Pose>(entities[1].entity);
                Coordinates oppositeDir = Coordinates.GetOppositeDirection(headPose.direction);
                segmentPose.position = headPose.position - (oppositeDir * segmentSize);
            }
            else
            {
                // Place in opposite direction of last snake segment
                Pose lastPose = world.GetComponent<Pose>(entities[entities.Count - 1].entity);
                Pose preLastPose = world.GetComponent<Pose>(entities[entities.Count - 2].entity);
                Coordinates oppositeDir = Coordinates.GetOppositeDirection((lastPose.position - preLastPose.position) / segmentSize);
                segmentPose.position = lastPose.position - (oppositeDir * segmentSize);
            }

            EcsEntity segmentEntity;
            CreateEntityWith(ref segmentPose, out segmentEntity);

            // Create rectangle in UI thread
            mainWindow.DispatchNonBlocking(new Action(() =>
            {
                Rectangle segmentRec = new Rectangle()
                {
                    Width = segmentSize,
                    Height = segmentSize,
                    Name = segmentEntity.ToString(),
                    Margin = new Thickness(segmentPose.position.x,
                                           segmentPose.position.y,
                                           0, 0),
                    Fill = Brushes.Green,
                };
                mainWindow.CreateRectangle(segmentRec);
            }));

            return segmentPose;
        }
    }
}
