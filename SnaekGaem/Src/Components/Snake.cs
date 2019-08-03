using System.Collections.Generic;

namespace SnaekGaem.Src.Components
{
    // This class represents the snake in the game.
    class Snake
    {
        // A snake is a list of snake segments (represented as pose)
        public List<Segment> segments { get; set; }

        // Should the snake grow in the next update cycle?
        public bool shouldGrow { get; set; }

        // Constructor sets fields
        public Snake()
        {
            segments = new List<Segment>(256);
            shouldGrow = false;
        }
    }
}
