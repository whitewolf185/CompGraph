using System;
using System.Collections.Generic;

namespace Additions{
    public class ColorControl{
        public double R;
        public double G;
        public double B;

        public ColorControl(){
            R = 1;
            G = 1;
            B = 1;
        }
        
        public ColorControl(double r, double g, double b){
            R = r;
            G = g;
            B = b;
        }

        public void SetColor(double r, double g, double b){
            R = r;
            G = g;
            B = b;
        }

        public void GenerateColor(){
            var rand = new Random();
            R = (float)rand.Next(255) / 255;
            G = (float)rand.Next(255) / 255;
            B = (float)rand.Next(255) / 255;
        }
    }

    public class ListOfColorControl : ColorControl{
        
        public ListOfColorControl(){
            R = 1;
            G = 1;
            B = 1;
            colorList = new List<ColorControl>();
        }
        
        public List<ColorControl> colorList;
        
        public void createColor_toPolygonColor(int POLYGONS){
            for (int i = 0; i < POLYGONS; i++){
                colorList.Add(new ColorControl(0, .2, .2));
            }
        }

        public void SetColor_toPolygonColor(int POLYGONS){
            for (int i = 0; i < POLYGONS; i++){
                colorList[i].SetColor(0, .2, .2);
            }
        }

        public void Random_PolygonColor(int POLYGONS){
            for (int i = 0; i < POLYGONS; i++){
                colorList[i].GenerateColor();
            }
        }
    }
}