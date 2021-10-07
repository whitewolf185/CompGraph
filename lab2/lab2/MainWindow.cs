using System;
using System.Collections.Generic;
using System.Numerics;
using Gtk;
using Cairo;
using UI = Gtk.Builder.ObjectAttribute;
using MeshClass;
using Additions;

namespace CG
{
    class MainWindow : Gtk.Window{
        public const int POLYGONS = 5;
        #region UI

        [UI] private DrawingArea _canvas = null;

        [UI] private CheckButton _allowZBuffer = null;
        [UI] private CheckButton _allowNormals = null;
        [UI] private CheckButton _allowWireframe = null;
        [UI] private CheckButton _allowInvisPoly = null;
        [UI] private CheckButton _allowRandom = null;

        [UI] private Adjustment _xRotation = null;
        [UI] private Adjustment _yRotation = null;
        [UI] private Adjustment _zRotation = null;
        [UI] private Adjustment _xScale = null;
        [UI] private Adjustment _yScale = null;
        [UI] private Adjustment _zScale = null;
        [UI] private Adjustment _xShift = null;
        [UI] private Adjustment _yShift = null;
        [UI] private Adjustment _zShift = null;

        // матрица
        [UI] private Adjustment _m11 = null;
        [UI] private Adjustment _m12 = null;
        [UI] private Adjustment _m13 = null;
        [UI] private Adjustment _m14 = null;
        [UI] private Adjustment _m21 = null;
        [UI] private Adjustment _m22 = null;
        [UI] private Adjustment _m23 = null;
        [UI] private Adjustment _m24 = null;
        [UI] private Adjustment _m31 = null;
        [UI] private Adjustment _m32 = null;
        [UI] private Adjustment _m33 = null;
        [UI] private Adjustment _m34 = null;
        [UI] private Adjustment _m41 = null;
        [UI] private Adjustment _m42 = null;
        [UI] private Adjustment _m43 = null;
        [UI] private Adjustment _m44 = null;

        #endregion

        private Matrix4x4 _defaultTransformationMatrix;
        private Matrix4x4 _transformationMatrix = Matrix4x4.Identity;

        #region Colors

        private ColorControl _canvasColor = new ColorControl(.2, .2, .2);
        private List<ColorControl> _polygonColor = new List<ColorControl>();
        private ColorControl _lineColor = new ColorControl(.5, 1, .5);
        private ColorControl _normalColor = new ColorControl(.0, 1, 1);

        #endregion
        
        private Mesh _myFigure = new Mesh(new List<Vertex> 
            {
                new Vertex (new Vector4(-.5f, -.5f, -.5f, 1)),// 0
                new Vertex (new Vector4(.5f, -.5f, -.5f, 1)), // 1
                new Vertex (new Vector4(.5f, .5f, -.5f, 1)),  // 2
                new Vertex (new Vector4(-.5f, .5f, -.5f, 1)), // 3
                new Vertex (new Vector4(0f, -.5f, .5f, 1)),   // 4
                new Vertex (new Vector4(0f, .5f, .5f, 1)),    // 5
            }, 
            new List<List<int>>
            {
                new List<int> {0, 1, 2, 3},
                new List<int> {3, 5, 4, 0},
                new List<int> {1, 4, 5, 2},
                new List<int> {4,1,0},
                new List<int> {5,3,2}
            }
        );

        public MainWindow() : this(new Builder("CGLab2.glade"))
        {
            _transformationMatrix = new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
            
            CalculateTranformationMatrix();
        }

        private void createColor_toPolygonColor(){
            for (int i = 0; i < POLYGONS; i++){
                _polygonColor.Add(new ColorControl(0, .2, .2));
            }
        }

        private void SetColor_toPolygonColor(){
            for (int i = 0; i < POLYGONS; i++){
                _polygonColor[i].SetColor(0, .2, .2);
            }
        }

        private void Random_PolygonColor(){
            for (int i = 0; i < POLYGONS; i++){
                _polygonColor[i].GenerateColor();
            }
        }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += (o, args) => Application.Quit();
            
            // нужна для инициализации list
            createColor_toPolygonColor();

            if (_allowRandom != null &&_allowRandom.Active){
                
            }

            _canvas.Drawn += (o, args) => {
                var context = args.Cr;

                context.SetSourceRGB(_canvasColor.R, _canvasColor.G, _canvasColor.B);
                context.Paint();

                context.Antialias = Antialias.Subpixel;      // Сглаживание для более гладких линий     
                context.LineWidth = 2d;                      // ширина линий
                      // цвет линий
                
                _myFigure.ApplyTransformation(_transformationMatrix * _defaultTransformationMatrix);
                
                DrawMesh(context, _myFigure);

                if (_allowNormals.Active)
                {
                    DrawNormals(context, _myFigure);
                }
            };

            _canvas.SizeAllocated += (o, args) =>
            {
                _defaultTransformationMatrix = Matrix4x4.CreateScale(200, 200, 200);
                _defaultTransformationMatrix *= Matrix4x4.CreateRotationZ(35);
                _defaultTransformationMatrix *= Matrix4x4.CreateRotationX(35);
                _defaultTransformationMatrix *= Matrix4x4.CreateTranslation(args.Allocation.Width / 2, args.Allocation.Height / 2, 0);
            };

            #region Обработка спинбатоннов со свичей

            _xShift.ValueChanged += (o, args) => { CalculateTranformationMatrix(); _canvas.QueueDraw();};
            _yShift.ValueChanged += (o, args) => { CalculateTranformationMatrix(); _canvas.QueueDraw();};
            _zShift.ValueChanged += (o, args) => { CalculateTranformationMatrix(); _canvas.QueueDraw();};

            _xScale.ValueChanged += (o, args) => { CalculateTranformationMatrix(); _canvas.QueueDraw();};
            _yScale.ValueChanged += (o, args) => { CalculateTranformationMatrix(); _canvas.QueueDraw();};
            _zScale.ValueChanged += (o, args) => { CalculateTranformationMatrix(); _canvas.QueueDraw();};
            
            _xRotation.ValueChanged += (o, args) => { CalculateTranformationMatrix(); _canvas.QueueDraw();};
            _yRotation.ValueChanged += (o, args) => { CalculateTranformationMatrix(); _canvas.QueueDraw();};
            _zRotation.ValueChanged += (o, args) => { CalculateTranformationMatrix(); _canvas.QueueDraw();};
            
            _allowNormals.Toggled += (o, args) => { _canvas.QueueDraw();};
            _allowWireframe.Toggled += (o, args) => { _canvas.QueueDraw();};
            _allowInvisPoly.Toggled += (o, args) => { _canvas.QueueDraw();};
            _allowZBuffer.Toggled += (o, args) => { _canvas.QueueDraw();};
            _allowRandom.Toggled += (o, args) => {
                if (_allowRandom.Active){
                    Random_PolygonColor();
                }
                else{
                    SetColor_toPolygonColor();
                }
                _canvas.QueueDraw();
            };

            #endregion
        }

        #region Отрисовка фигруы
        
        // Id нужен для сохранения рандомного цвета
        private void DrawPolygon(Context context, Polygon polygon, int Id){
            if (polygon.Vertexes.Count == 0)
                return;
            if (_allowInvisPoly.Active && polygon.CalculateNormal().Z < 0)
                return;

            context.MoveTo(polygon.Vertexes[0].Point.X, polygon.Vertexes[0].Point.Y);
            
            for (int i = 1; i < polygon.Vertexes.Count; ++i)
            {
                context.LineTo(polygon.Vertexes[i].Point.X, polygon.Vertexes[i].Point.Y);
            }
            context.ClosePath();
            
            if (_allowWireframe.Active == false)
            {
                context.SetSourceRGB(_polygonColor[Id].R, _polygonColor[Id].G, _polygonColor[Id].B);
                context.FillPreserve();
            }
            
            context.SetSourceRGB(_lineColor.R, _lineColor.G, _lineColor.B);
            context.Stroke();
        }

        private void DrawMesh(Context context, Mesh mesh){
            int i = 0;
            foreach (var polygon in mesh.TransformedPolygons)
            {
                DrawPolygon(context, polygon, i);
                i++;
            }
        }

        private void DrawNormal(Context context, Polygon polygon){
            if (_allowInvisPoly.Active && polygon.CalculateNormal().Z < 0)
                return;
            
            Vector4 normal = polygon.CalculateNormal();
            normal *= 100;
            Vector4 polygonCenter = new Vector4(0, 0, 0, 0);
            for (int i = 0; i < polygon.Vertexes.Count; ++i)
            {
                polygonCenter += polygon.Vertexes[i].Point;
            }
            polygonCenter /= polygon.Vertexes.Count;

            context.MoveTo(polygonCenter.X, polygonCenter.Y);
            context.LineTo(polygonCenter.X + normal.X, polygonCenter.Y + normal.Y);
            
            context.SetSourceRGB(_normalColor.R, _normalColor.G, _normalColor.B);
            context.Stroke();
        }
        
        private void DrawNormals(Context context, Mesh mesh)
        {
            foreach (var polygon in mesh.TransformedPolygons)
            {
                DrawNormal(context, polygon);
            }
        }
        
        #endregion

        private void CalculateTranformationMatrix()
        {
            _transformationMatrix = Matrix4x4.CreateScale((float) _xScale.Value, (float) _yScale.Value, (float) _zScale.Value);
            
            _transformationMatrix *= Matrix4x4.CreateRotationX((float)(_xRotation.Value * Math.PI / 180)) *
                                    Matrix4x4.CreateRotationY((float)(_yRotation.Value * Math.PI / 180)) *
                                    Matrix4x4.CreateRotationZ((float)(_zRotation.Value * Math.PI / 180));
            
            _transformationMatrix *= Matrix4x4.CreateTranslation((float)_xShift.Value, (float)_yShift.Value, (float)_zShift.Value);
            
            UpdateSpinButtons();
        }

        private void UpdateSpinButtons(){
            _m11.Value = _transformationMatrix.M11;
            _m12.Value = _transformationMatrix.M12;
            _m13.Value = _transformationMatrix.M13;
            _m14.Value = _transformationMatrix.M14;
            _m21.Value = _transformationMatrix.M21;
            _m22.Value = _transformationMatrix.M22;
            _m23.Value = _transformationMatrix.M23;
            _m24.Value = _transformationMatrix.M24;
            _m31.Value = _transformationMatrix.M31;
            _m32.Value = _transformationMatrix.M32;
            _m33.Value = _transformationMatrix.M33;
            _m34.Value = _transformationMatrix.M34;
            _m41.Value = _transformationMatrix.M41;
            _m42.Value = _transformationMatrix.M42;
            _m43.Value = _transformationMatrix.M43;
            _m44.Value = _transformationMatrix.M44;
        }
    }
}