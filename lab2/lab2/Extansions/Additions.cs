using System;

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
}