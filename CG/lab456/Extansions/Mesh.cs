using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace CG
{
    public class Vertex
    {
        public uint Id;
        public Vector4 Position = new Vector4();
        public List<Polygon> Polygons = new List<Polygon>();
        // public Vector3 Color = new Vector3(0.57f, 0.81f, 0.21f);
        public Vector3 Color = new Vector3(1, 1, 1);

        public Vertex(float x, float y, float z, uint id)
        {
            Id = id;
            Position = new Vector4(x, y, z, 1);
        }

        public Vertex(uint id)
        {
            Id = id;
            Position = new Vector4(0, 0, 0, 1);
        }
        
        public Vertex(Vector4 vector4, uint id)
        {
            Id = id;
            Position = vector4;
        }

        public Vector4 CalculateNormal()
        {
            Vector4 normal = Vector4.Zero;
            
            foreach (Polygon polygon in Polygons)
            {
                normal += polygon.CalculateNormal() / Polygons.Count;
            }

            return normal;
        }
    }
    
    public class Polygon
    {
        public List<Vertex> Vertexes = new List<Vertex>();
        public Vector3 Color = new Vector3(0.57f, 0.81f, 0.21f);

        public Polygon(List<Vertex> vertexes)
        {
            Vertexes = vertexes;

            for (int i = 0; i < vertexes.Count; ++i)
            {
                vertexes[i].Polygons.Add(this);
            }
        }
        
        public static Vector3 ToVector3(Vertex vertex)
        { 
            return new Vector3(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
        }
        
        public Vector4 CalculateNormal()
        {
            if (Vertexes.Count() >= 3)
            {
                Vector3 a = ToVector3(Vertexes[1]) - ToVector3(Vertexes[0]);
                Vector3 b = ToVector3(Vertexes[2]) - ToVector3(Vertexes[0]);
                Vector3 normal = Vector3.Cross(b, a);
                normal = normal / normal.Length();
                return new Vector4(normal.X, normal.Y, normal.Z, 0);
            }
        
            return new Vector4(0, 0, 0, 0);
        }

        public Vector4 CalculateCenter()
        {
            Vector4 result = new Vector4(0, 0, 0, 0);
            for (int i = 0; i < Vertexes.Count; ++i)
            {
                result += (1 / (float)Vertexes.Count) * Vertexes[i].Position;
            }

            return result;
        }
    }

    public class Mesh
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public List<Polygon> Polygons = new List<Polygon>();

        public Mesh()
        {
            Vertices = new List<Vertex>();
            Polygons = new List<Polygon>();
        }

        public Mesh(List<Vertex> vertices, List<Polygon> polygons)
        {
            Vertices = vertices;
            Polygons = polygons;
        }
        
        public Mesh(List<Vertex> vertices, List<List<int>> polygons)
        {
            Vertices = new List<Vertex>(vertices);

            Polygons = new List<Polygon>();
            
            foreach (var polygon in polygons)
            {
                List<Vertex> polygonVertices = new List<Vertex>();
                List<Vertex> transformedPolygonVertices = new List<Vertex>();
                foreach (var vertexIndex in polygon)
                {
                    polygonVertices.Add(vertices[vertexIndex]);
                }
                Polygons.Add(new Polygon(polygonVertices));
            }
        }

        public void SetColor(Vector3 color)
        {
            foreach (Polygon polygon in Polygons)
            {
                polygon.Color = color;
            }
            
            foreach (Vertex vertex in Vertices)
            {
                vertex.Color = color;
            }
        }
        
        public void SetColor(float r, float g, float b)
        {
            foreach (Polygon polygon in Polygons)
            {
                polygon.Color = new Vector3(r, g, b);
            }
            
            foreach (Vertex vertex in Vertices)
            {
                vertex.Color = new Vector3(r, g, b);
            }
        }

        public void TriangulateSquares()
        {
            for (int i = 0; i < Polygons.Count; ++i)
            {
                if (Polygons[i].Vertexes.Count == 4)
                {
                    Polygon first = new Polygon(new List<Vertex>{Polygons[i].Vertexes[0], Polygons[i].Vertexes[1], Polygons[i].Vertexes[2]});
                    first.Color = Polygons[i].Color;
                    Polygon second = new Polygon(new List<Vertex>{Polygons[i].Vertexes[2], Polygons[i].Vertexes[3], Polygons[i].Vertexes[0]});
                    second.Color = Polygons[i].Color;
                    
                    Polygons.RemoveAt(i);
                    Polygons.Insert(i, second);
                    Polygons.Insert(i, first);
                    // вставка с сохранением порядка (может пригодиться?)
                }
            }
        }

        public List<uint> GetEnumerationOfVertexes()
        {
            List<uint> result = new List<uint>();

            for (int i = 0; i < Polygons.Count; ++i)
            {
                for (int j = 0; j < Polygons[i].Vertexes.Count; ++j)
                {
                    result.Add(Polygons[i].Vertexes[j].Id);
                }
            }

            return result;
        }
    }
}