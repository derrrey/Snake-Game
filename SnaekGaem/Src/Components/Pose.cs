/*
 * This component specifies a pose of an entity.
 * The pose is defined by a position and a rotation.
 */

using SnaekGaem.Src.Tools;

namespace SnaekGaem.Src.Components
{
    class Pose
    {
        // A pose is defined as a position and a rotation in 2-dimensional space.
        public Coordinates position { get; set; }
        public Coordinates direction { get; set; }

        // Initialize the fields.
        public Pose()
        {
            position = new Coordinates();
            direction = new Coordinates();
        }

        // Constructor with parameters.
        public Pose(Coordinates position, Coordinates direction)
        {
            this.position = position;
            this.direction = direction;
        }

        // Copies the values from the other pose into this pose.
        public void Set(ref Pose copyFrom)
        {
            this.position = copyFrom.position;
            this.direction = copyFrom.direction;
        }
    }
}
