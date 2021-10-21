using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Gtk;
using Cairo;
using UI = Gtk.Builder.ObjectAttribute;
using MeshClass;
using Additions;
using Gdk;

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
        
        [UI] private ComboBoxText _projection = null;

        #endregion

        private Matrix4x4 _defaultTransformationMatrix;
        private Matrix4x4 _transformationMatrix = Matrix4x4.Identity;
        
        private float _defaultScale = 200;
        private float _compressedScale = 1;
        
        private float _axisSize = 40;
        private Matrix4x4 _axisTransformMatrix = Matrix4x4.Identity;
        
        private enum Projection
        {
            None,
            Front,
            Right,
            Top,
            Isometric
        }

        //мышь
        private Vector3 _mousePosition = new Vector3(0, 0, 0);
        private uint _mousePressedButton = 0;

        #region Colors

        private ColorControl _canvasColor = new ColorControl(.2, .2, .2);
        private ListOfColorControl _polygonColor = new ListOfColorControl();
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

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += (o, args) => Application.Quit();
            
            // нужна для инициализации list
            _polygonColor.createColor_toPolygonColor(POLYGONS);

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

                DrawAxis(context);
            };

            _canvas.SizeAllocated += (o, args) =>
            {
                if (_defaultScale > Math.Min(args.Allocation.Width, args.Allocation.Height)){
                    _compressedScale = Math.Min(args.Allocation.Width, args.Allocation.Height) / _defaultScale;
                }
                if (_defaultScale < Math.Min(args.Allocation.Width, args.Allocation.Height)){
                    _compressedScale = Math.Min(args.Allocation.Width, args.Allocation.Height) / _defaultScale;
                    if (_compressedScale > 1){
                        _compressedScale = 1;
                    }
                }

                float trueScale = _defaultScale * _compressedScale;

                _defaultTransformationMatrix = Matrix4x4.CreateScale(trueScale, trueScale, trueScale);
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
                    _polygonColor.Random_PolygonColor(POLYGONS);
                }
                else{
                    _polygonColor.SetColor_toPolygonColor(POLYGONS);
                }
                _canvas.QueueDraw();
            };

            _projection.Changed += (o, args) => {SetProjection(); CalculateTranformationMatrix(); _canvas.QueueDraw(); };
            #endregion
            
            #region Обработка мыши

            _canvas.Events |= EventMask.ScrollMask | EventMask.PointerMotionMask | EventMask.ButtonPressMask |
                              EventMask.ButtonReleaseMask;

            _canvas.ButtonPressEvent += (o, args) =>
            {
                _mousePressedButton = args.Event.Button;
                _mousePosition.X = (float)args.Event.X;
                _mousePosition.Y = (float)args.Event.Y;
            };

            _canvas.MotionNotifyEvent += (o, args) =>
            {
                Vector3 _currentMousePosition = new Vector3((float)args.Event.X, (float)args.Event.Y, 0);

                if (_mousePressedButton == -1) return;

                if (_mousePressedButton == 1)
                {
                    _xShift.Value += (double)(_currentMousePosition.X - _mousePosition.X) / (double)_defaultTransformationMatrix.M11;
                    _yShift.Value += (double)(_currentMousePosition.Y - _mousePosition.Y) / (double)_defaultTransformationMatrix.M22;
                }
                if (_mousePressedButton == 3)
                {
                    if (_xRotation.Value + _currentMousePosition.Y - _mousePosition.Y < 0)
                        _xRotation.Value += 360 + _currentMousePosition.Y - _mousePosition.Y;
                    else if (_xRotation.Value + _currentMousePosition.Y - _mousePosition.Y > 360)
                        _xRotation.Value += -360 + _currentMousePosition.Y - _mousePosition.Y;
                    else
                        _xRotation.Value += _currentMousePosition.Y - _mousePosition.Y;

                    if (_yRotation.Value + _currentMousePosition.X - _mousePosition.X < 0)
                        _yRotation.Value += 360 + _currentMousePosition.X - _mousePosition.X;
                    else if (_yRotation.Value + _currentMousePosition.X - _mousePosition.X > 360)
                        _yRotation.Value += -360 + _currentMousePosition.X - _mousePosition.X;
                    else
                        _yRotation.Value += _currentMousePosition.X - _mousePosition.X;
                }

                _mousePosition = _currentMousePosition;

                // _canvas.QueueDraw();
            };

            _canvas.ButtonReleaseEvent += (o, args) => _mousePressedButton = 0;
            
            _canvas.ScrollEvent += (o, args) =>
            {
                if (args.Event.Direction == ScrollDirection.Down)
                {
                    _xScale.Value -= _xScale.StepIncrement;
                    _yScale.Value -= _yScale.StepIncrement;
                    _zScale.Value -= _zScale.StepIncrement;
                }
                else if (args.Event.Direction == ScrollDirection.Up)
                {
                    _xScale.Value += _xScale.StepIncrement;
                    _yScale.Value += _yScale.StepIncrement;
                    _zScale.Value += _zScale.StepIncrement;
                }
                _canvas.QueueDraw();
            };

            #endregion
            
            #region Обработка матрицы

            // реагирую на изменения
            _m11.ValueChanged += (o, args) => { _transformationMatrix.M11 = (float)_m11.Value; _canvas.QueueDraw();};
            _m12.ValueChanged += (o, args) => { _transformationMatrix.M12 = (float)_m12.Value; _canvas.QueueDraw();};
            _m13.ValueChanged += (o, args) => { _transformationMatrix.M13 = (float)_m13.Value; _canvas.QueueDraw();};
            _m14.ValueChanged += (o, args) => { _transformationMatrix.M14 = (float)_m14.Value; _canvas.QueueDraw();};
            _m21.ValueChanged += (o, args) => { _transformationMatrix.M21 = (float)_m21.Value; _canvas.QueueDraw();};
            _m22.ValueChanged += (o, args) => { _transformationMatrix.M22 = (float)_m22.Value; _canvas.QueueDraw();};
            _m23.ValueChanged += (o, args) => { _transformationMatrix.M23 = (float)_m23.Value; _canvas.QueueDraw();};
            _m24.ValueChanged += (o, args) => { _transformationMatrix.M24 = (float)_m24.Value; _canvas.QueueDraw();};
            _m31.ValueChanged += (o, args) => { _transformationMatrix.M31 = (float)_m31.Value; _canvas.QueueDraw();};
            _m32.ValueChanged += (o, args) => { _transformationMatrix.M32 = (float)_m32.Value; _canvas.QueueDraw();};
            _m33.ValueChanged += (o, args) => { _transformationMatrix.M33 = (float)_m33.Value; _canvas.QueueDraw();};
            _m34.ValueChanged += (o, args) => { _transformationMatrix.M34 = (float)_m34.Value; _canvas.QueueDraw();};
            _m41.ValueChanged += (o, args) => { _transformationMatrix.M41 = (float)_m41.Value; _canvas.QueueDraw();};
            _m42.ValueChanged += (o, args) => { _transformationMatrix.M42 = (float)_m42.Value; _canvas.QueueDraw();};
            _m43.ValueChanged += (o, args) => { _transformationMatrix.M43 = (float)_m43.Value; _canvas.QueueDraw();};
            _m44.ValueChanged += (o, args) => { _transformationMatrix.M44 = (float)_m44.Value; _canvas.QueueDraw();};

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
                context.SetSourceRGB(_polygonColor.colorList[Id].R, _polygonColor.colorList[Id].G, _polygonColor.colorList[Id].B);
                context.FillPreserve();
            }
            
            context.SetSourceRGB(_lineColor.R, _lineColor.G, _lineColor.B);
            context.Stroke();
        }

        private void DrawMesh(Context context, Mesh mesh){
            
            if (_allowZBuffer.Active)
            {
                mesh.TransformedPolygons = mesh.TransformedPolygons.OrderBy(polygon => (polygon.Vertexes.Select(vertex => vertex.Point.Z)).Sum()/polygon.Vertexes.Count).ToList();
            }
            
            for (int i = 0; i < mesh.TransformedPolygons.Count; ++i)
            {
                DrawPolygon(context, mesh.TransformedPolygons[i], i);
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
        
        private void DrawAxis(Context context)
        {
            Vector4 o = Vector4.Transform(new Vector4(1, 1, 1, 1), _axisTransformMatrix);
            Vector4 x = Vector4.Transform( new Vector4(_axisSize, 1, 1, 1), _axisTransformMatrix);
            Vector4 y = Vector4.Transform( new Vector4(1, _axisSize, 1, 1), _axisTransformMatrix);
            Vector4 z = Vector4.Transform( new Vector4(1, 1, _axisSize, 1), _axisTransformMatrix);

            context.MoveTo(o.X, o.Y);
            context.LineTo(x.X, x.Y);
            context.SetSourceRGB(1, .0, .0);
            context.Stroke();

            context.MoveTo(o.X, o.Y);
            context.LineTo(y.X, y.Y);
            context.SetSourceRGB(0, 1, .0);
            context.Stroke();

            context.MoveTo(o.X, o.Y);
            context.LineTo(z.X, z.Y);
            context.SetSourceRGB(.0, .0, 1);
            context.Stroke();

        }
        
        #endregion

        private void CalculateTranformationMatrix()
        {
            CalculateAxisTransformationMatrix();
            _transformationMatrix = Matrix4x4.CreateScale((float) _xScale.Value, (float) _yScale.Value, (float) _zScale.Value);
            
            _transformationMatrix *= Matrix4x4.CreateRotationX((float)(_xRotation.Value * Math.PI / 180)) *
                                    Matrix4x4.CreateRotationY((float)(_yRotation.Value * Math.PI / 180)) *
                                    Matrix4x4.CreateRotationZ((float)(_zRotation.Value * Math.PI / 180));
            
            _transformationMatrix *= Matrix4x4.CreateTranslation((float)_xShift.Value, (float)_yShift.Value, (float)_zShift.Value);
            
            UpdateSpinButtons();
        }
        
        private void CalculateAxisTransformationMatrix()
        {
            _axisTransformMatrix = Matrix4x4.CreateRotationX((float) (_xRotation.Value * Math.PI / 180)) *
                                   Matrix4x4.CreateRotationY((float) (_yRotation.Value * Math.PI / 180)) *
                                   Matrix4x4.CreateRotationZ((float) (_zRotation.Value * Math.PI / 180));
            _axisTransformMatrix *= Matrix4x4.CreateTranslation(45, 45, 0);
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
        
                private void SetProjection()
        {
            if (_projection.Active == (int)Projection.Isometric)
            {
                _xScale.Value = 1;
                _yScale.Value = 1;
                _zScale.Value = 1;
                _xRotation.Value = 35;
                _yRotation.Value = 45;
            }
            else if (_projection.Active == (int) Projection.Front)
            {
                _xScale.Value = 1;
                _yScale.Value = 1;
                _zScale.Value = 0;
            }
            else if (_projection.Active == (int) Projection.Top)
            {
                _xScale.Value = 1;
                _yScale.Value = 0;
                _zScale.Value = 1;
            }
            else if (_projection.Active == (int) Projection.Right)
            {
                _xScale.Value = 0;
                _yScale.Value = 1;
                _zScale.Value = 1;
            }
            else if (_projection.Active == (int) Projection.None)
            {
                _xScale.Value = 1;
                _yScale.Value = 1;
                _zScale.Value = 1;
                _xRotation.Value = 0;
                _yRotation.Value = 0;
            }
        }

        private void CalculateTranformationPapameters()
        {
            #region Обновление матрицы и вектора сдига

            _transformationMatrix.M11 = (float)_m11.Value;
            _transformationMatrix.M12 = (float)_m12.Value;
            _transformationMatrix.M13 = (float)_m13.Value;
            _transformationMatrix.M14 = (float)_m14.Value;
            _transformationMatrix.M21 = (float)_m21.Value;
            _transformationMatrix.M22 = (float)_m22.Value;
            _transformationMatrix.M23 = (float)_m23.Value;
            _transformationMatrix.M24 = (float)_m24.Value;
            _transformationMatrix.M31 = (float)_m31.Value;
            _transformationMatrix.M32 = (float)_m32.Value;
            _transformationMatrix.M33 = (float)_m33.Value;
            _transformationMatrix.M34 = (float)_m34.Value;
            _transformationMatrix.M41 = (float)_m41.Value;
            _transformationMatrix.M42 = (float)_m42.Value;
            _transformationMatrix.M43 = (float)_m43.Value;
            _transformationMatrix.M44 = (float)_m44.Value;

            #endregion
        }
    }
}