using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using Gtk;

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


    public class Busie : Mesh{
        public List<List<Vector3>> controlPoints = new List<List<Vector3>>();
        private int stepX, stepY;

        public void Update(){
            for (int i = 0; i < controlPoints.Count-1; i++){
                for (int t = 0; t < 1; t +=stepY){
                    List<Vector3> interpolitedControlPoints = new List<Vector3>();
                    for (int j = 0; j < controlPoints[i].Count; j++){
                        interpolitedControlPoints.Add(controlPoints[i][j] + t*controlPoints[i+1][j]);
                    }

                    uint count = 0;
                    for (int u = 0; u < interpolitedControlPoints.Count-4; u += 3){
                        for (float v = 0; v < 1; v += stepX)
                        {
                            Vertices[(int)count].Position = new Vector4(
                                interpolitedControlPoints[u].X * (1 - v)*(1 - v)*(1 - v) 
                                                + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[u + 1].X 
                                                + 3 * v*v * (1 - v) * interpolitedControlPoints[u + 2].X
                                                + v * v * v * interpolitedControlPoints[u + 3].X,
                                interpolitedControlPoints[u].Y * (1 - v)*(1 - v)*(1 - v) 
                                + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[u + 1].Y 
                                + 3 * v*v * (1 - v) * interpolitedControlPoints[u + 2].Y
                                + v * v * v * interpolitedControlPoints[u + 3].Y,
                                interpolitedControlPoints[u].Z * (1 - v)*(1 - v)*(1 - v) 
                                + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[u + 1].Z 
                                + 3 * v*v * (1 - v) * interpolitedControlPoints[u + 2].Z
                                + v * v * v * interpolitedControlPoints[u + 3].Z,
                                1);
                            ++count;
                        }
                    }
                    
                    for (float v = 0; v < 1; v += stepX)
                    {
                        Vertices[(int)count].Position = new Vector4(
                            interpolitedControlPoints[^4].X * (1 - v)*(1 - v)*(1 - v) 
                            + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[^3].X 
                            + 3 * v*v * (1 - v) * interpolitedControlPoints[^2].X
                            + v * v * v * interpolitedControlPoints[^1].X,
                            interpolitedControlPoints[^4].Y * (1 - v)*(1 - v)*(1 - v) 
                            + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[^3].Y 
                            + 3 * v*v * (1 - v) * interpolitedControlPoints[^2].Y
                            + v * v * v * interpolitedControlPoints[^1].Y,
                            interpolitedControlPoints[^4].Z * (1 - v)*(1 - v)*(1 - v) 
                            + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[^3].Z 
                            + 3 * v*v * (1 - v) * interpolitedControlPoints[^2].Z
                            + v * v * v * interpolitedControlPoints[^1].Z
                            ,1);
                        ++count;
                    }
                }
            }
        }

        public Busie(List<List<Vector3>> _contralPoints, int stepX, int stepY){
            controlPoints = _contralPoints;
            this.stepX = stepX;
            this.stepY = stepY;
            
            uint cmt = 0;
            List<List<uint>> indexes = new List<List<uint>>();
            
            for (int i = 0; i < controlPoints.Count-1; i++){
                for (int t = 0; t < 1; t +=stepY){
                    List<Vector3> interpolitedControlPoints = new List<Vector3>();
                    for (int j = 0; j < controlPoints[i].Count; j++){
                        interpolitedControlPoints.Add(controlPoints[i][j] + t*controlPoints[i+1][j]);
                    }

                    List<Vector4> mas = new List<Vector4>();
                    for (int u = 0; u < interpolitedControlPoints.Count - 4; u += 3){
                        for (float v = 0; v < 1; v += stepX)
                        {
                            mas.Add(new Vector4(
                                interpolitedControlPoints[u].X * (1 - v)*(1 - v)*(1 - v) 
                                                + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[u + 1].X 
                                                + 3 * v*v * (1 - v) * interpolitedControlPoints[u + 2].X
                                                + v * v * v * interpolitedControlPoints[u + 3].X,
                                interpolitedControlPoints[u].Y * (1 - v)*(1 - v)*(1 - v) 
                                + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[u + 1].Y 
                                + 3 * v*v * (1 - v) * interpolitedControlPoints[u + 2].Y
                                + v * v * v * interpolitedControlPoints[u + 3].Y,
                                interpolitedControlPoints[u].Z * (1 - v)*(1 - v)*(1 - v) 
                                + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[u + 1].Z 
                                + 3 * v*v * (1 - v) * interpolitedControlPoints[u + 2].Z
                                + v * v * v * interpolitedControlPoints[u + 3].Z,
                                1));
                        }
                    }
                    
                    for (float v = 0; v < 1; v += stepX)
                    {
                        mas.Add(new Vector4(
                            interpolitedControlPoints[^4].X * (1 - v)*(1 - v)*(1 - v) 
                            + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[^3].X 
                            + 3 * v*v * (1 - v) * interpolitedControlPoints[^2].X
                            + v * v * v * interpolitedControlPoints[^1].X,
                            interpolitedControlPoints[^4].Y * (1 - v)*(1 - v)*(1 - v) 
                            + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[^3].Y 
                            + 3 * v*v * (1 - v) * interpolitedControlPoints[^2].Y
                            + v * v * v * interpolitedControlPoints[^1].Y,
                            interpolitedControlPoints[^4].Z * (1 - v)*(1 - v)*(1 - v) 
                            + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[^3].Z 
                            + 3 * v*v * (1 - v) * interpolitedControlPoints[^2].Z
                            + v * v * v * interpolitedControlPoints[^1].Z
                            ,1));
                    }

                    indexes.Add(new List<uint>());
                    for (int k = 0; k < mas.Count; k++){
                        Vertices.Add(new Vertex(mas[k], cmt));
                        indexes[indexes.Count - 1].Add(cmt);
                        cmt++;
                    }
                    
                    
                }
            }
            
            for (int i = 0; i < indexes.Count - 1; i++){
                for (int j = 0; j < indexes[i].Count - 1; j++){
                    Polygons.Add(new Polygon(new List<Vertex>{ 
                            Vertices[indexes[0].Count * i + j],
                            Vertices[indexes[0].Count * (i + 1) + j],
                            Vertices[indexes[0].Count * i + j+1]
                        })
                    );
                    
                    Polygons.Add(new Polygon(new List<Vertex>{
                            Vertices[indexes[0].Count * (i + 1) + j + 1],
                            Vertices[indexes[0].Count * i + j+1],
                            Vertices[indexes[0].Count * (i+1) + j]
                        })
                    );
                }
            }
        }
    }
}