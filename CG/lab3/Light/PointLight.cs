using System.Numerics;

namespace Light{
    public class PointLight
    {
        public Vector4 Position;
        public Vector4 TransformedPosition;
        public Vector3 Intensity;
        
        public PointLight(Vector4 position, Vector3 intensity)
        {
            Position = position;
            Intensity = intensity;
        }

        public PointLight(float x, float y, float z, float intensityR, float intensityG, float intensityB)
        {
            Position = new Vector4(x, y, z, 1);
            Intensity = new Vector3(intensityB, intensityG, intensityB);
        }
        
        public void ApplyTransformation(Matrix4x4 transformationMatrix)
        {
            TransformedPosition = Vector4.Transform(Position, transformationMatrix);
        }
    }
}