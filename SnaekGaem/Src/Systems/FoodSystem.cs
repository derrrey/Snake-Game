﻿/*
 *  This system manages the snake food.
 */

using Leopotam.Ecs;
using SnaekGaem.Src.Components;

namespace SnaekGaem.Src.Systems
{
    // Use Code-Injection for filter
    [EcsInject]
    class FoodSystem : IEcsRunSystem
    {
        // Filter for the snake entity
        EcsFilter<Snake> snakeFilter = null;

        // Filter for the food entities
        EcsFilter<Food> foodFilter = null;

        // The number of food that was eaten
        int foodEaten = 0;

        // The number of points food gives
        const int FOODSCORE = 5;

        // Reference to the main app
        Game game = null;

        // Set references
        public FoodSystem(Game game)
        {
            this.game = game;
        }

        // Nothing to do on destroy
        public void Destroy() { }

        // Checks if new food has to be spawned
        public void Run()
        {
            // Check if the snakes head intercepts with the food
            if (snakeFilter.Components1[0] != null && foodFilter.Components1[0] != null
                && snakeFilter.Components1[0].segments[0].pose.position == foodFilter.Components1[0].pose.position)
            {
                // Mark the food for removal in ecs
                game.SetDeletionFlag(foodFilter.Entities[0].Id);

                // Snake has to grow
                snakeFilter.Components1[0].shouldGrow = true;

                // Tell the score change to the game instance
                ++foodEaten;
                game.SetScore(foodEaten * FOODSCORE);

                // Spawn new food
                game.CreateOnCanvas(false);
            }
        }
    }
}
