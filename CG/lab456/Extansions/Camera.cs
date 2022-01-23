using System;
using System.Numerics;


namespace CG
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 Rotation; // в градусах
        public float AspectRatio;
        public float FOV; // в градусах
        public float ClipStart;
        public float ClipEnd;
        
        public Camera(Vector3 position, Vector3 rotation, float aspectRatio, float fov, float clipStart, float clipEnd)
        {
            Position = position;
            Rotation = rotation; // в градусах
            AspectRatio = aspectRatio;
            FOV = fov; // в градусах
            ClipStart = clipStart;
            ClipEnd = clipEnd;
        }

        public Matrix4x4 CalculateRotationMatrix()
        {
            Matrix4x4 result = Matrix4x4.CreateRotationX((float)(Rotation.X * Math.PI / 180));
            result *= Matrix4x4.CreateRotationY((float)(Rotation.Y * Math.PI / 180));
            result *= Matrix4x4.CreateRotationZ((float)(Rotation.Z * Math.PI / 180));
            return result;
        }

        // объектно-видовая матрица камеры
        public Matrix4x4 CalculateViewMatrix()
        {
            Matrix4x4 rotationTransforamtion = CalculateRotationMatrix();

            // var cameraDirection = Vector3.Normalize(Position - Target);
            // var cameraRight = Vector3.Cross(Up, cameraDirection);
            Vector4 cameraDirection = Vector4.Transform(new Vector4(0, 0, 1, 0), rotationTransforamtion);
            Vector4 Up = Vector4.Transform(new Vector4(0, 1, 0, 0), rotationTransforamtion);
            var cameraRight = Vector3.Cross(new Vector3(Up.X, Up.Y, Up.Z), 
                                                  new Vector3(cameraDirection.X, cameraDirection.Y, cameraDirection.Z));
            var matrix1 = new Matrix4x4(
                cameraRight.X, cameraRight.Y, cameraRight.Z, 0,
                Up.X, Up.Y, Up.Z, 0,
                cameraDirection.X, cameraDirection.Y, cameraDirection.Z, 0,
                0, 0, 0, 1
            );
            var matrix2 = new Matrix4x4(
                1, 0, 0, -Position.X,
                0, 1, 0, -Position.Y,
                0, 0, 1, -Position.Z,
                0, 0, 0, 1
            );
            return matrix1 * matrix2;
        }

        // матрица проекции (применить после перехода в базис камеры)
        public Matrix4x4 CalculateProjectionMatrix()
        {
            return Matrix4x4.Transpose(
                Matrix4x4.CreatePerspectiveFieldOfView((float)(FOV * Math.PI / 180), 
                                                        AspectRatio, 
                                                        ClipStart, 
                                                        ClipEnd));
        }
    }
}