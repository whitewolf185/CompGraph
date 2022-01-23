using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CG
{
    public class Ellipsoid : Mesh
    {
        public Ellipsoid(float r, int meridiansCount, int parallelsCount)
        {
            uint curretnVertexId = 0;
            
            Vertices.Add(new Vertex(0, 0, r, curretnVertexId));
            ++curretnVertexId;
            
            for (int i = 1; i < parallelsCount + 1; ++i)
            {
                float theta = (float) (i * Math.PI / (2 * parallelsCount));
                for (int j = 0; j < meridiansCount; ++j)
                {
                    float phi = (float) (j * 2 * Math.PI / meridiansCount);
                    Vertices.Add(new Vertex((float)(r * Math.Sin(theta) * Math.Cos(phi)), 
                                                (float)(r * Math.Sin(theta) * Math.Sin(phi)), 
                                                (float)(r * Math.Cos(theta)), curretnVertexId));
                    ++curretnVertexId;
                }
            }
            Vertices.Add(new Vertex(0, 0, 0, curretnVertexId));
            ++curretnVertexId;

            for (int i = 1; i < meridiansCount; ++i)
            {
                Polygons.Add(new Polygon(new List<Vertex>{Vertices[0], Vertices[i + 1], Vertices[i]}));
            }
            Polygons.Add(new Polygon(new List<Vertex>{Vertices[0], Vertices[1], Vertices[meridiansCount]}));
            
            for (int i = 0; i < parallelsCount - 1; ++i)
            {
                int shiftOnParralel = 1 + i * meridiansCount;
                for (int j = 0; j < meridiansCount - 1; ++j)
                {
                    Polygons.Add(new Polygon(new List<Vertex>{Vertices[shiftOnParralel + j], 
                        Vertices[shiftOnParralel + j + 1], 
                        Vertices[shiftOnParralel + meridiansCount + j + 1], 
                        Vertices[shiftOnParralel + meridiansCount + j]}));
                }
                Polygons.Add(new Polygon(new List<Vertex>{Vertices[shiftOnParralel + meridiansCount - 1], 
                Vertices[shiftOnParralel], 
                Vertices[shiftOnParralel + meridiansCount], 
                Vertices[shiftOnParralel + meridiansCount + meridiansCount - 1]}));
            }
            
            for (int i = Vertices.Count - meridiansCount - 1; i < Vertices.Count - 2; ++i)
            {
                Polygons.Add(new Polygon(new List<Vertex>{Vertices[Vertices.Count - 1], Vertices[i], Vertices[i + 1]}));
            }
            Polygons.Add(new Polygon(new List<Vertex>{Vertices[Vertices.Count - 1], Vertices[Vertices.Count - 2], Vertices[Vertices.Count - meridiansCount - 1]}));
        }
    }
}