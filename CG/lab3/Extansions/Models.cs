using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Globalization;
using System.IO;
using MeshClass;

namespace CG.Extansions{
    public class Ellipsoid : Mesh
    {
        public Ellipsoid(float r, int meridiansCount, int parallelsCount)
        {
            
            Vertices.Add(new Vertex(0, 0, r));
            TransformedVertices.Add(new Vertex());
            for (int i = 1; i < parallelsCount + 1; ++i)
            {
                float theta = (float) (i * Math.PI / ( 2 * (parallelsCount )));
                for (int j = 0; j < meridiansCount; ++j)
                {
                    float phi = (float) (j * 2 * Math.PI / meridiansCount);
                    Vertices.Add(new Vertex((float)(r * Math.Sin(theta) * Math.Cos(phi)), 
                                                (float)(r * Math.Sin(theta) * Math.Sin(phi)), 
                                                (float)(r * Math.Cos(theta))));
                    TransformedVertices.Add(new Vertex());
                }
            }
            Vertices.Add(new Vertex(0, 0, 0));
            TransformedVertices.Add(new Vertex());

            for (int i = 1; i < meridiansCount; ++i)
            {
                Polygons.Add(new Polygon(new List<Vertex>{
                    Vertices[0],
                    Vertices[i + 1],
                    Vertices[i]
                }));
                TransformedPolygons.Add(new Polygon(new List<Vertex>{
                    TransformedVertices[0],
                    TransformedVertices[i + 1],
                    TransformedVertices[i]
                }));
            }
            Polygons.Add(new Polygon(new List<Vertex>{Vertices[0], Vertices[1], Vertices[meridiansCount]}));
            TransformedPolygons.Add(new Polygon(new List<Vertex>{TransformedVertices[0], TransformedVertices[1], TransformedVertices[meridiansCount]}));
            
            for (int i = 0; i < parallelsCount - 1; ++i)
            {
                int shiftOnParralel = 1 + i * meridiansCount;
                for (int j = 0; j < meridiansCount - 1; ++j)
                {
                    Polygons.Add(new Polygon(new List<Vertex>{Vertices[shiftOnParralel + j], 
                        Vertices[shiftOnParralel + j + 1], 
                        Vertices[shiftOnParralel + meridiansCount + j + 1], 
                        Vertices[shiftOnParralel + meridiansCount + j]}));
                    TransformedPolygons.Add(new Polygon(new List<Vertex>{TransformedVertices[shiftOnParralel + j], 
                        TransformedVertices[shiftOnParralel + j + 1], 
                        TransformedVertices[shiftOnParralel + meridiansCount + j + 1], 
                        TransformedVertices[shiftOnParralel + meridiansCount + j]}));
                }
                Polygons.Add(new Polygon(new List<Vertex>{Vertices[shiftOnParralel + meridiansCount - 1], 
                    Vertices[shiftOnParralel], 
                    Vertices[shiftOnParralel + meridiansCount], 
                    Vertices[shiftOnParralel + meridiansCount + meridiansCount - 1]}));
                TransformedPolygons.Add(new Polygon(new List<Vertex>{TransformedVertices[shiftOnParralel + meridiansCount - 1], 
                    TransformedVertices[shiftOnParralel], 
                    TransformedVertices[shiftOnParralel + meridiansCount], 
                    TransformedVertices[shiftOnParralel + meridiansCount + meridiansCount - 1]}));
            }
            
            for (int i = Vertices.Count - meridiansCount - 1; i < Vertices.Count - 2; ++i)
            {
                Polygons.Add(new Polygon(new List<Vertex>{Vertices[Vertices.Count - 1], Vertices[i], Vertices[i + 1]}));
                TransformedPolygons.Add(new Polygon(new List<Vertex>{TransformedVertices[Vertices.Count - 1], TransformedVertices[i], TransformedVertices[i + 1]}));
            }
            Polygons.Add(new Polygon(new List<Vertex>{Vertices[Vertices.Count - 1], Vertices[Vertices.Count - 2], Vertices[Vertices.Count - meridiansCount - 1]}));
            TransformedPolygons.Add(new Polygon(new List<Vertex>{TransformedVertices[Vertices.Count - 1], TransformedVertices[Vertices.Count - 2], TransformedVertices[Vertices.Count - meridiansCount - 1]}));
        }
    }
}