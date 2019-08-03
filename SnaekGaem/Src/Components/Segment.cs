using SnaekGaem.Src.Tools;

namespace SnaekGaem.Src.Components
{
    // A snake segment is represented as a pose
    class Segment
    {
        public Pose pose { get; set; }

        public Segment()
        {
            pose = new Pose();
        }
    }
}
