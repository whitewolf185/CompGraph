using System;
using System.Numerics;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Additions;
using GLib;

using Gtk;

namespace MeshClass
{
    public struct RGB{
        public double R;
        public double G;
        public double B;
    }
    public class Vertex
    {
        public Vector4 Point = new Vector4();
        public List<Polygon> Polygons = new List<Polygon>();
        public Vector3 Color = new Vector3(0.57f, 0.81f, 0.21f);

        public Vertex(float x, float y, float z)
        {
            Point = new Vector4(x, y, z, 1);
        }

        public Vertex()
        {
            Point = new Vector4(0, 0, 0, 1);
        }
        
        public Vertex(Vector4 vector4)
        {
            Point = vector4;
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
            return new Vector3(vertex.Point.X, vertex.Point.Y, vertex.Point.Z);
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
                result += (1 / (float)Vertexes.Count) * Vertexes[i].Point;
            }

            return result;
        }
    }

    public class Mesh
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public List<Vertex> TransformedVertices = new List<Vertex>();
        public List<Polygon> Polygons = new List<Polygon>();
        public List<Polygon> TransformedPolygons = new List<Polygon>(); //связывают преобразованные вершины

        public Mesh()
        {
            Vertices = new List<Vertex>();
            TransformedVertices = new List<Vertex>();
            Polygons = new List<Polygon>();
            TransformedPolygons = new List<Polygon>();
        }

        public Mesh(List<Vertex> vertices, List<Polygon> polygons)
        {
            Vertices = vertices;
            TransformedVertices = new List<Vertex>(Vertices.Count);
            Polygons = polygons;
            TransformedPolygons = new List<Polygon>(Polygons.Count);
        }
        
        public Mesh(List<Vertex> vertices, List<List<int>> polygons)
        {
            Vertices = new List<Vertex>(vertices);
            TransformedVertices = new List<Vertex>(vertices.Count);
            for (int i = 0; i < TransformedVertices.Capacity; ++i)
            {
                TransformedVertices.Add(new Vertex(0, 0, 0));
            }
            
            Polygons = new List<Polygon>();
            TransformedPolygons = new List<Polygon>();
            
            foreach (var polygon in polygons)
            {
                List<Vertex> polygonVertices = new List<Vertex>();
                List<Vertex> transformedPolygonVertices = new List<Vertex>();
                foreach (var vertexIndex in polygon)
                {
                    polygonVertices.Add(vertices[vertexIndex]);
                    transformedPolygonVertices.Add(TransformedVertices[vertexIndex]);
                }
                Polygons.Add(new Polygon(polygonVertices));
                TransformedPolygons.Add(new Polygon(transformedPolygonVertices));
            }
        }
        
        public void ApplyTransformation(Matrix4x4 transformationMatrix)
        {
            for (int i = 0; i < Vertices.Count(); ++i)
            {
                TransformedVertices[i].Point = Vector4.Transform(Vertices[i].Point, transformationMatrix);
            }
        }

        public void SetColor(Vector3 color)
        {
            foreach (Polygon polygon in TransformedPolygons)
            {
                polygon.Color = color;
            }
            
            foreach (Vertex vertex in TransformedVertices)
            {
                vertex.Color = color;
            }
        }
        
        public void SetColor(float r, float g, float b)
        {
            foreach (Polygon polygon in TransformedPolygons)
            {
                polygon.Color = new Vector3(r, g, b);
            }
            
            foreach (Vertex vertex in TransformedVertices)
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
                    
                    first = new Polygon(new List<Vertex>{TransformedPolygons[i].Vertexes[0], TransformedPolygons[i].Vertexes[1], TransformedPolygons[i].Vertexes[2]});
                    first.Color = Polygons[i].Color;
                    second = new Polygon(new List<Vertex>{TransformedPolygons[i].Vertexes[2], TransformedPolygons[i].Vertexes[3], TransformedPolygons[i].Vertexes[0]});
                    second.Color = Polygons[i].Color;
                    
                    TransformedPolygons.RemoveAt(i);
                    TransformedPolygons.Insert(i, second);
                    TransformedPolygons.Insert(i, first);
                    // вставка с сохранением порядка (может пригодиться?)
                }
            }
        }
    }
}