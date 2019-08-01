using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnaekGaem.Src.Components
{
    // Food is represented as a pose
    class Food
    {
        public Pose pose { get; set; }

        public Food()
        {
            pose = new Pose();
        }
    }
}
