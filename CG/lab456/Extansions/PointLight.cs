using System.Numerics;
using System.Collections.Generic;

namespace CG
{
    public class PointLight
    {
        public Vector4 Position;

        public PointLight(Vector4 position, Vector3 intensity)
        {
            Position = position;
        }

        public PointLight(float x, float y, float z)
        {
            Position = new Vector4(x, y, z, 1);
        }
    }
}