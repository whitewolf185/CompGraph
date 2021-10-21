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
        public Vector4 Point;

        public Vertex(float x, float y, float z)
        {
            Point = new Vector4(x, y, z, 1);
        }

        public Vertex(Vector4 vector4)
        {
            Point = vector4;
        }
    }
    
    public class Polygon
    {
        public List<Vertex> Vertexes;
        public RGB Color;
        
        public Polygon(List<Vertex> vertexes)
        {
            Vertexes = vertexes;
        }
        
        private Vector3 toVector3(Vertex vertex)
        { 
            return new Vector3(vertex.Point.X, vertex.Point.Y, vertex.Point.Z);
        }   

        public Vector4 CalculateNormal()
        {
            if (Vertexes.Count() >= 3)
            {
                Vector3 a = toVector3(Vertexes[1]) - toVector3(Vertexes[0]);
                Vector3 b = toVector3(Vertexes[2]) - toVector3(Vertexes[0]);
                Vector3 normal = Vector3.Cross(b, a);
                normal = normal / normal.Length();
                return new Vector4(normal.X, normal.Y, normal.Z, 0);
            }
        
            return new Vector4(0, 0, 0, 0);
        }
    }

    public class Mesh
    {
        private List<Vertex> Vertices;
        private List<Vertex> TransformedVertices;
        private List<Polygon> Polygons;
        public List<Polygon> TransformedPolygons;

        public Mesh(List<Vertex> vertices, List<List<int>> polygons){
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
    }
}