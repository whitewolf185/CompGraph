using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace graphics{
    public partial class Form1 : Form{
        public Form1(){
            InitializeComponent();
            Set_parameter((float) parameter.Value);
            _step = 100;
            Change_shifts();
            Change_border();
            diffX = 0;
            diffY = 0;
            compressingScale = 1;
            originalScale = 50;
            Set_scale();
        }

        #region MyLogic

        #region Atributs

        private float _a;
        private PointF[] pointFs;
        private int _step;
        private int shiftX;
        private int shiftY;
        private float scale;
        private int borderHeight;
        private int borderWidth;
        private bool wasMaximized = false;
        private int diffX;
        private int diffY;
        private int absX;
        private int absY;
        private float sizeX;
        private float sizeY;
        private float compressingScale;
        private float originalScale;
        private bool beginDown = false;

        #endregion
        
        private float X(float t){
            return (_a * (float)Math.Cos(t)) / (1 + (float)Math.Sin(t) * (float)Math.Sin(t));
        }

        private float Y(float t){
            return (_a * (float)Math.Cos(t) * (float)Math.Sin(t)) / (1 + (float)Math.Sin(t) * (float)Math.Sin(t));
        }

        #region gets

        public void SetA(float _A){
            _a = _A;
        }

        #endregion

        #region sets

        private void Change_shifts(){
            shiftX = this.panelGraph.Width / 2 + absX;
            shiftY = this.panelGraph.Height / 2 + absY;
        }

        private void Change_border(){
            borderHeight = panelGraph.Height;
            borderWidth = panelGraph.Width;
        }

        private void Set_parameter(float value){
            _a = value;
        }
        
        private void Set_MaxAndMin(){
            float maxX = -10000;
            float maxY = -10000;
            float minX = 10000;
            float minY = 10000;
            
            foreach (PointF pointF in pointFs){
                minX = Math.Min(minX, pointF.X);
                minY = Math.Min(minY, pointF.Y);
                maxX = Math.Max(maxX, pointF.X);
                maxY = Math.Max(maxY, pointF.Y);
            }

            sizeX = maxX - minX;
            sizeY = maxY - minY;
        }

        private void Set_scale(){
            scale = originalScale * compressingScale;
        }

        #endregion
        
        public void Generate_points(){
            List<PointF> pointS = new List<PointF>();
            float t;
            float l = 0;
            float r = 2 * (float)Math.PI;
            
            for (t = 0; t < r; t += (r - l)/_step){
                pointS.Add(new PointF(X(t),Y(t)) );
            }

            Matrix transformMatrix = new Matrix();
            var angle = (int)num_Angle.Value;
            transformMatrix.Scale(scale, scale, MatrixOrder.Append);
            transformMatrix.Rotate(angle);
            transformMatrix.Translate(shiftX, shiftY, MatrixOrder.Append);

            pointFs = pointS.ToArray();

            transformMatrix.TransformPoints(pointFs);
        }
        
        private void Pait_graph(PaintEventArgs e){
            Change_shifts();
            
            _step = (int)this.numericUpDown1.Value;
            Set_parameter((float) parameter.Value);
            Generate_points();
            PointF pref = pointFs[pointFs.Length - 1];

            Pen pen = new Pen(Color.Tomato, 3);
            
            foreach (PointF pointF in pointFs){
                e.Graphics.DrawLine(pen,pointF, pref);
                pref = pointF;
            }
        }
        
        private void Create_Coordinates(PaintEventArgs e){
            Pen coordinatePen = new Pen(Color.Black, 2);
            Change_border();

            PointF y_line1 = new PointF(shiftX, 0);
            PointF y_line2 = new PointF(shiftX, borderHeight);

            PointF x_line1 = new PointF(0, shiftY);
            PointF x_line2 = new PointF(borderWidth, shiftY);

            List<PointF> Singlers = new List<PointF>();

            for (float i = shiftY; i >= 0; i -= scale){
                Singlers.Add(new PointF(shiftX - 5,i));
                Singlers.Add(new PointF(shiftX + 5,i));
            }
            
            for (float i = shiftY; i < borderHeight ; i += scale){
                Singlers.Add(new PointF(shiftX - 5,i));
                Singlers.Add(new PointF(shiftX + 5,i));
            }
            
            for (float i = shiftX; i >= 0; i -= scale){
                Singlers.Add(new PointF(i,shiftY - 5));
                Singlers.Add(new PointF(i,shiftY + 5));
            }
            
            for (float i = shiftX; i < borderWidth ; i += scale){
                Singlers.Add(new PointF(i,shiftY - 5));
                Singlers.Add(new PointF(i,shiftY + 5));
            }

            e.Graphics.DrawLine(coordinatePen, y_line1, y_line2);
            e.Graphics.DrawLine(coordinatePen, x_line1, x_line2);

            for (int i = 0; i < Singlers.Count; i += 2){
                e.Graphics.DrawLine(coordinatePen, Singlers[i], Singlers[i+1]);
            }
        }

        

        #endregion

        private void Panel_Paint(object sender, PaintEventArgs e){
            // throw new System.NotImplementedException();
            Create_Coordinates(e);

            Pait_graph(e);
        }


        private void Form1_Resize(object sender, EventArgs e){
            this.panelGraph.Height = this.ClientSize.Height - panelGraph.Location.Y - panelGraph.Margin.All;
            this.panelGraph.Width = this.ClientSize.Width - panelGraph.Location.X - panelGraph.Margin.All;
            
            Set_MaxAndMin();

            if (sizeX > panelGraph.Width){
                compressingScale *= panelGraph.Width  / (sizeX + 10);
            }
            
            if (sizeY > panelGraph.Height){
                compressingScale *= panelGraph.Height  / (sizeY + 10);
            }

            if ((sizeX < panelGraph.Width && originalScale < panelGraph.Width) || (sizeY < panelGraph.Height && originalScale < panelGraph.Height)){
                compressingScale *= Math.Min(panelGraph.Width/ (sizeX + 10),panelGraph.Height/ (sizeY + 10) );
                if (compressingScale > 1){
                    compressingScale = 1;
                }
            }
            
            /*if (sizeY < panelGraph.Height && originalScale < panelGraph.Height){
                compressingScale *= panelGraph.Height / (sizeY + 30);
                if (compressingScale > 1){
                    compressingScale = 1;
                }
            }*/
            
            Set_scale();

            if (WindowState == FormWindowState.Maximized){
                if (!wasMaximized){
                    Refresh();
                }

                wasMaximized = true;
            }

            if (WindowState == FormWindowState.Normal ){
                if (wasMaximized){
                    Refresh();
                }
                wasMaximized = false;
            }

            Refresh();
        }

        private void panelGraph_WheelEvent(object sender, System.Windows.Forms.MouseEventArgs e){
            var prev = originalScale;
            var wheel = e.Delta * SystemInformation.MouseWheelScrollLines / 90;
            var tmp = originalScale + wheel;

            if (tmp >= 10){
                originalScale = tmp;
                Set_MaxAndMin();
                if (sizeX > panelGraph.Width || sizeY > panelGraph.Height ){
                    originalScale = prev - 10;
                }
                Set_scale();
                
                Refresh();
            }
        }

        private void Form1_ResizeEnd(object sender, EventArgs e){
            Refresh();
        }


        private void numeric_ValueChanged(object sender, EventArgs e){
            Refresh();
        }

        private void numScale_ValueChanged(object sender, EventArgs e){
            originalScale = float.Parse(numScale.Text);
            Set_scale();
            Refresh();
        }


        private void panelGraph_MouseMove(object sender, MouseEventArgs e){

            if (e.Button == MouseButtons.Left){
                if (!beginDown){
                    diffX = e.Location.X;
                    diffY = e.Location.Y;
                    beginDown = true;
                }
                else{
                    absX += e.Location.X - diffX;
                    absY += e.Location.Y - diffY;
                    diffX = e.Location.X;
                    diffY = e.Location.Y;
                    Refresh();
                }
            }
            else{
                beginDown = false;
            }
        }

        private void num_Angle_ValueChanged(object sender, EventArgs e){
            Refresh();
        }

        private void panelGraph_MouseUp(object sender, MouseEventArgs e){
            Refresh();
        }
    }
}