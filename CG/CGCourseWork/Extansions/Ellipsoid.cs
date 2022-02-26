using System;
using System.Collections.Generic;
using System.Linq;
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
        private float stepX, stepY;

        // public void Update(){
        //     for (int i = 0; i < controlPoints.Count-1; i++){
        //         for (float t = 0; t < 1; t +=stepY){
        //             List<Vector3> interpolitedControlPoints = new List<Vector3>();
        //             for (int j = 0; j < controlPoints[i].Count; j++){
        //                 interpolitedControlPoints.Add(controlPoints[i][j] + t*controlPoints[i+1][j]);
        //             }
        //
        //             uint count = 0;
        //             for (int u = 0; u < interpolitedControlPoints.Count-4; u += 3){
        //                 for (float v = 0; v < 1; v += stepX)
        //                 {
        //                     Vertices[(int)count].Position = new Vector4(
        //                         interpolitedControlPoints[u].X * (1 - v)*(1 - v)*(1 - v) 
        //                                         + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[u + 1].X 
        //                                         + 3 * v*v * (1 - v) * interpolitedControlPoints[u + 2].X
        //                                         + v * v * v * interpolitedControlPoints[u + 3].X,
        //                         interpolitedControlPoints[u].Y * (1 - v)*(1 - v)*(1 - v) 
        //                         + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[u + 1].Y 
        //                         + 3 * v*v * (1 - v) * interpolitedControlPoints[u + 2].Y
        //                         + v * v * v * interpolitedControlPoints[u + 3].Y,
        //                         interpolitedControlPoints[u].Z * (1 - v)*(1 - v)*(1 - v) 
        //                         + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[u + 1].Z 
        //                         + 3 * v*v * (1 - v) * interpolitedControlPoints[u + 2].Z
        //                         + v * v * v * interpolitedControlPoints[u + 3].Z,
        //                         1);
        //                     ++count;
        //                 }
        //             }
        //             
        //             for (float v = 0; v < 1; v += stepX)
        //             {
        //                 Vertices[(int)count].Position = new Vector4(
        //                     interpolitedControlPoints[^4].X * (1 - v)*(1 - v)*(1 - v) 
        //                     + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[^3].X 
        //                     + 3 * v*v * (1 - v) * interpolitedControlPoints[^2].X
        //                     + v * v * v * interpolitedControlPoints[^1].X,
        //                     interpolitedControlPoints[^4].Y * (1 - v)*(1 - v)*(1 - v) 
        //                     + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[^3].Y 
        //                     + 3 * v*v * (1 - v) * interpolitedControlPoints[^2].Y
        //                     + v * v * v * interpolitedControlPoints[^1].Y,
        //                     interpolitedControlPoints[^4].Z * (1 - v)*(1 - v)*(1 - v) 
        //                     + 3 * v * (1 - v)*(1 - v) * interpolitedControlPoints[^3].Z 
        //                     + 3 * v*v * (1 - v) * interpolitedControlPoints[^2].Z
        //                     + v * v * v * interpolitedControlPoints[^1].Z
        //                     ,1);
        //                 ++count;
        //             }
        //         }
        //     }
        // }

        public Busie(List<List<Vector3>> _controlPoints, float stepX, float stepY)
        {
            controlPoints = _controlPoints;
            this.stepX = stepX;
            this.stepY = stepY;

            uint cnt = 0;
            List<List<uint>> indexes = new List<List<uint>>();

            List<List<Vector3>> interpolatedCP = new List<List<Vector3>>();
            List<List<Vector3>> fin = new List<List<Vector3>>();
            for (int i = 0; i < controlPoints.Count; ++i)
            {
                interpolatedCP.Add(new List<Vector3>());
                for (float t = 0; t < 1; t += stepX)
                {
                    interpolatedCP.Last().Add(controlPoints[i][0] * (1 - t)*(1 - t)*(1 - t) 
                            + 3 * t * (1 - t)*(1 - t) * controlPoints[i][1] 
                            + 3 * t*t * (1 - t) * controlPoints[i][2]
                            + t * t * t * controlPoints[i][3]);
                }
                interpolatedCP.Last().Add(controlPoints[i][3]);
            }

            for (int i = 0; i < interpolatedCP[0].Count; ++i)
            {
                fin.Add(new List<Vector3>());
                for (float t = 0; t < 1; t += stepY)
                {
                    fin.Last().Add(interpolatedCP[0][i] * (1 - t)*(1 - t)*(1 - t) 
                                              + 3 * t * (1 - t)*(1 - t) * interpolatedCP[1][i] 
                                              + 3 * t*t * (1 - t) * interpolatedCP[2][i]
                                              + t * t * t * interpolatedCP[3][i]);
                    Vertices.Add(new Vertex(new Vector4(fin.Last().Last().X, fin.Last().Last().Y, fin.Last().Last().Z, 1),
                                                cnt));
                    ++cnt;
                }
                fin.Last().Add(interpolatedCP[3][i]);
                Vertices.Add(new Vertex(new Vector4(fin.Last().Last().X, fin.Last().Last().Y, fin.Last().Last().Z, 1),
                    cnt));
                ++cnt;
            }

            for (int i = 0; i < fin.Count-1; ++i)
            {
                for (int j = 0; j < fin[0].Count-1; ++j)
                {
                    Polygons.Add(new Polygon(new List<Vertex>
                    {
                        Vertices[fin[0].Count * i + j],
                        Vertices[fin[0].Count * (i+1) + j],
                        Vertices[fin[0].Count * i + j+1],
                    }));
                    Polygons.Add(new Polygon(new List<Vertex>
                    {
                        Vertices[fin[0].Count * i + j+1],
                        Vertices[fin[0].Count * (i+1) + j],
                        Vertices[fin[0].Count * (i+1) + j+1],
                    }));
                }
            }
        }

        public List<Vector3> GetControlPoints(){
            List<Vector3> result = new List<Vector3>();

            for (int i = 0; i < controlPoints.Count; i++){
                for (int j = 0; j < controlPoints[i].Count; j++){
                    result.Add(controlPoints[i][j]);
                }
            }
            
            return result;
        }
    }
}