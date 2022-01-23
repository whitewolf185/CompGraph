using System;
using System.Collections.Generic;
using System.Numerics;
using Gtk;
using Gdk;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using SharpGL;
using Application = Gtk.Application;
using Window = Gtk.Window;
using UI = Gtk.Builder.ObjectAttribute;

namespace CG
{
    class MainWindow : Window
    {
        [UI] private GLArea _glArea = null;

        #region UI спинбаттонов и чекбоксов
        //камера
        [UI] private Adjustment _xPosition = null;
        [UI] private Adjustment _yPosition = null;
        [UI] private Adjustment _zPosition = null;
        [UI] private Adjustment _xRotation = null;
        [UI] private Adjustment _yRotation = null;
        [UI] private Adjustment _zRotation = null;

        [UI] private Adjustment _aspectRatio = null;
        [UI] private Adjustment _FOV = null;
        [UI] private Adjustment _clipStart = null;
        [UI] private Adjustment _clipEnd = null;
        //параметры полуэлипсойда
        [UI] private Adjustment _r = null;

        [UI] private Adjustment _meridiansCount = null;
        [UI] private Adjustment _parallelsCount = null;
        // опции
        [UI] private CheckButton _allowZBuffer = null;
        [UI] private CheckButton _allowNormals = null;
        [UI] private CheckButton _allowWireframe = null;
        [UI] private CheckButton _allowInvisPoly = null;
        [UI] private CheckButton _animate = null;
        //материал
        [UI] private Adjustment _materialColorR = null;
        [UI] private Adjustment _materialColorG = null;
        [UI] private Adjustment _materialColorB = null;
        
        [UI] private Adjustment _k_aR = null;
        [UI] private Adjustment _k_aG = null;
        [UI] private Adjustment _k_aB = null;
        
        [UI] private Adjustment _k_dR = null;
        [UI] private Adjustment _k_dG = null;
        [UI] private Adjustment _k_dB = null;
        
        [UI] private Adjustment _k_sR = null;
        [UI] private Adjustment _k_sG = null;
        [UI] private Adjustment _k_sB = null;
        
        [UI] private Adjustment _p = null;
        //источник света
        [UI] private CheckButton _allowPointLightVisible = null;
            
        [UI] private Adjustment _pointLightIntensityR = null;
        [UI] private Adjustment _pointLightIntensityG = null;
        [UI] private Adjustment _pointLightIntensityB = null;
        
        [UI] private Adjustment _pointLightPositionX = null;
        [UI] private Adjustment _pointLightPositionY = null;
        [UI] private Adjustment _pointLightPositionZ = null;
        
        [UI] private Adjustment _attenuationСoefficient = null;
        #endregion

        #region UI матрицы

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
        
        #region Мышь

        private Vector3 _mousePosition;
        private uint _mousePressedButton;

        #endregion

        private float _mouseRotationSensitivity = 1f / 1000f;
        private Matrix4x4 _cameraTransformationMatrix;

        private bool _figureChanged = true;
        private bool _pointLightChanged = true;

        private uint _startTime;

        private enum FragmetShaderColorMode
        {
            DarkBlue,
            Green,
            Cyan,
            LightPaint,
            None
        }

        #region выстраиваивание сцены

        private PointLight _pointLight = new PointLight(3, 0, 0);
        private Camera _camera = new Camera(new Vector3(0f, -2.3f, 0f), new Vector3(90f, 0f, 0f), 
            1, 60, (float)0.01, (float)1000);
        
        private Mesh _figure = new Ellipsoid(1, 16, 8);

        #endregion

        public MainWindow() : this(new Builder("lab456.glade"))
        {
            _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
            _figure.TriangulateSquares();
        }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += (o, args) => Application.Quit();
            
            _glArea.Realized += GLInit;

            _glArea.SizeAllocated += (o, args) =>
            {
                _aspectRatio.Value = (float)args.Allocation.Width / (float)args.Allocation.Height;
                _camera.AspectRatio = (float)_aspectRatio.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };

            #region Обработка спинбатоннов, чекбоксов и комботекстбоксов

            _xPosition.ValueChanged += (o, args) =>
            {
                _camera.Position.X = (float)_xPosition.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };
            _yPosition.ValueChanged += (o, args) =>
            {
                _camera.Position.Y = (float)_yPosition.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };
            _zPosition.ValueChanged += (o, args) =>
            {
                _camera.Position.Z = (float)_zPosition.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };
            
            _xRotation.ValueChanged += (o, args) =>
            {
                _camera.Rotation.X = (float)_xRotation.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };
            _yRotation.ValueChanged += (o, args) =>
            {
                _camera.Rotation.Y = (float)_yRotation.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };
            _zRotation.ValueChanged += (o, args) =>
            {
                _camera.Rotation.Z = (float)_zRotation.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };
            
            _aspectRatio.ValueChanged += (o, args) =>
            {
                _camera.AspectRatio = (float) _aspectRatio.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };
            
            _FOV.ValueChanged += (o, args) =>
            {
                _camera.FOV = (float) _FOV.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };

            _clipStart.ValueChanged += (o, args) =>
            {
                _camera.ClipStart = (float) _clipStart.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };

            _clipEnd.ValueChanged += (o, args) =>
            {
                _camera.ClipEnd = (float) _clipEnd.Value;
                _cameraTransformationMatrix = _camera.CalculateProjectionMatrix() * _camera.CalculateViewMatrix();
                updateTranformationMatrixAdjustments();
            };

            _r.ValueChanged += (o, args) => {_figureChanged = true;};
            _meridiansCount.ValueChanged += (o, args) => {_figureChanged = true;};
            _parallelsCount.ValueChanged += (o, args) => {_figureChanged = true;};
            
            _pointLightPositionX.ValueChanged += (o, args) => {_pointLightChanged = true;};
            _pointLightPositionY.ValueChanged += (o, args) => {_pointLightChanged = true;};
            _pointLightPositionZ.ValueChanged += (o, args) => {_pointLightChanged = true;};

            _materialColorR.ValueChanged += (o, args) => {_figureChanged = true;};
            _materialColorG.ValueChanged += (o, args) => {_figureChanged = true;};
            _materialColorB.ValueChanged += (o, args) => {_figureChanged = true;};
            
            #endregion

            #region Обработка матрицы

            _m11.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M11 = (float) _m11.Value;
            };
            _m12.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M12 = (float) _m12.Value;
            };
            _m13.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M13 = (float) _m13.Value;
            };
            _m14.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M14 = (float) _m14.Value;
            };
            _m21.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M21 = (float) _m21.Value;
            };
            _m22.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M22 = (float) _m22.Value;
            };
            _m23.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M23 = (float) _m23.Value;
            };
            _m24.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M24 = (float) _m24.Value;
            };
            _m31.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M31 = (float) _m31.Value;
            };
            _m32.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M32 = (float) _m32.Value;
            };
            _m33.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M33 = (float) _m33.Value;
            };
            _m34.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M34 = (float) _m34.Value;
            };
            _m41.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M41 = (float) _m41.Value;
            };
            _m42.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M42 = (float) _m42.Value;
            };
            _m43.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M43 = (float) _m43.Value;
            };
            _m44.ValueChanged += (o, args) =>
            {
                _cameraTransformationMatrix.M44 = (float) _m44.Value;
            };

            #endregion

            #region Обработка мыши

            _glArea.Events |= EventMask.ScrollMask | EventMask.PointerMotionMask | EventMask.ButtonPressMask |
                              EventMask.ButtonReleaseMask;

            _glArea.ButtonPressEvent += (o, args) =>
            {
                _mousePressedButton = args.Event.Button;
                _mousePosition.X = (float) args.Event.X;
                _mousePosition.Y = (float) args.Event.Y;
            };

            _glArea.MotionNotifyEvent += (o, args) =>
            {
                Vector3 _currentMousePosition = new Vector3((float) args.Event.X, (float) args.Event.Y, 0);
                Matrix4x4 rotationtransformation = _camera.CalculateRotationMatrix();
                
                if (_mousePressedButton == 1)
                {
                    Vector4 shift = Vector4.Transform(new Vector4(-(_currentMousePosition.X - _mousePosition.X) / 200, 
                        (_currentMousePosition.Y - _mousePosition.Y) / 200, 0, 0), rotationtransformation);

                    _xPosition.Value += shift.X;
                    _yPosition.Value += shift.Y;
                    _zPosition.Value += shift.Z;
                }

                if (_mousePressedButton == 3)
                {
                    Matrix4x4 mouseRotation = Matrix4x4.CreateRotationX((_currentMousePosition.Y - _mousePosition.Y) * _mouseRotationSensitivity);
                    mouseRotation *= Matrix4x4.CreateRotationY((_currentMousePosition.X - _mousePosition.X) * _mouseRotationSensitivity);

                    Matrix4x4 currnetRotation = mouseRotation * 
                                                Matrix4x4.CreateRotationX((float) (_xRotation.Value * Math.PI / 180)) *
                                                Matrix4x4.CreateRotationY((float) (_yRotation.Value * Math.PI / 180)) *
                                                Matrix4x4.CreateRotationZ((float) (_zRotation.Value * Math.PI / 180));

                    //для углов Эйлера
                    MatrixToAngles(currnetRotation, out var x, out var y, out var z);
                    _xRotation.Value = x;
                    _yRotation.Value = y;
                    _zRotation.Value = z;
                }

                _mousePosition = _currentMousePosition;
            };

            _glArea.ButtonReleaseEvent += (o, args) => _mousePressedButton = 0;

            _glArea.ScrollEvent += (o, args) =>
            {
                Vector4 shift = new Vector4(0, 0, (float)0.2, 0);
                shift = Vector4.Transform(shift, _camera.CalculateRotationMatrix());
                
                if (args.Event.Direction == ScrollDirection.Up)
                {
                    _xPosition.Value -= shift.X;
                    _yPosition.Value -= shift.Y;
                    _zPosition.Value -= shift.Z;
                }
                else if (args.Event.Direction == ScrollDirection.Down)
                {
                    _xPosition.Value += shift.X;
                    _yPosition.Value += shift.Y;
                    _zPosition.Value += shift.Z;
                }
            };

            #endregion
        }

        private static void MatrixToAngles(Matrix4x4 matrix, out double x, out double y, out double z)
        {
            //область определения аркстангенса от pi/2 до -pi/2
            x = Math.Atan2(matrix.M23, matrix.M33) / Math.PI * 180;
            y = Math.Atan2(-matrix.M13, Math.Sqrt(1 - matrix.M13 * matrix.M13)) / Math.PI * 180;
            z = Math.Atan2(matrix.M12, matrix.M11) / Math.PI * 180;
        }

        private void GLInit(object sender, EventArgs args)
        {
            var glArea = sender as GLArea;
            glArea.MakeCurrent();
            var gl = new OpenGL();
            
            var frame_clock = glArea.Context.Window.FrameClock;
            frame_clock.Update += (_, _) => glArea.QueueRender();
            frame_clock.BeginUpdating();

            #region сборка шейдоров и шейдерной программы

            uint vertexShader;
            vertexShader = gl.CreateShader(OpenGL.GL_VERTEX_SHADER);
            string vertexShaderSource = ReadFromRes(@"lab456.Shaders.shader.vert");
            gl.ShaderSource(vertexShader, vertexShaderSource);
            gl.CompileShader(vertexShader);
            
            var txt = new StringBuilder(512);
            int[] tmp = new int[1];
            gl.GetShaderInfoLog(vertexShader, 512, IntPtr.Zero, txt);
            gl.GetShader(vertexShader, OpenGL.GL_COMPILE_STATUS, tmp);
            if (tmp[0] != OpenGL.GL_TRUE) Debug.WriteLine(txt);
            Debug.Assert(tmp[0] == OpenGL.GL_TRUE, "Vertexes Shader compilation failed");
            
            uint polygonNormalsGeometryShader;
            polygonNormalsGeometryShader = gl.CreateShader(OpenGL.GL_GEOMETRY_SHADER);
            string polygonNormalsGeometryShaderSource = ReadFromRes(@"lab456.Extansions.polygon_normals.geom");
            gl.ShaderSource(polygonNormalsGeometryShader, polygonNormalsGeometryShaderSource);
            gl.CompileShader(polygonNormalsGeometryShader);
            
            gl.GetShaderInfoLog(polygonNormalsGeometryShader, 512, IntPtr.Zero, txt);
            gl.GetShader(polygonNormalsGeometryShader, OpenGL.GL_COMPILE_STATUS, tmp);
            if (tmp[0] != OpenGL.GL_TRUE) Debug.WriteLine(txt);
            Debug.Assert(tmp[0] == OpenGL.GL_TRUE, "Polygon Normals Geometry Shader compilation failed");
            
            uint fragmentShader;
            fragmentShader = gl.CreateShader(OpenGL.GL_FRAGMENT_SHADER);
            string fragmentShaderSource = ReadFromRes(@"lab456.Shaders.shader.frag");
            gl.ShaderSource(fragmentShader, fragmentShaderSource);
            gl.CompileShader(fragmentShader);
            
            gl.GetShaderInfoLog(fragmentShader, 512, IntPtr.Zero, txt);
            gl.GetShader(fragmentShader, OpenGL.GL_COMPILE_STATUS, tmp);
            if (tmp[0] != OpenGL.GL_TRUE) Debug.WriteLine(txt);
            Debug.Assert(tmp[0] == OpenGL.GL_TRUE, "Fragmet Shader compilation failed");
            
            uint shaderProgram, polygonNormalsShaderProgram;
            shaderProgram = gl.CreateProgram();
            polygonNormalsShaderProgram = gl.CreateProgram();

            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);

            gl.GetProgram(shaderProgram, OpenGL.GL_LINK_STATUS, tmp);
            Debug.Assert(tmp[0] == OpenGL.GL_TRUE, "Shader program link failed");

            gl.AttachShader(polygonNormalsShaderProgram, vertexShader);
            gl.AttachShader(polygonNormalsShaderProgram, polygonNormalsGeometryShader);
            gl.AttachShader(polygonNormalsShaderProgram, fragmentShader);
            gl.LinkProgram(polygonNormalsShaderProgram);
            
            gl.GetProgram(polygonNormalsShaderProgram, OpenGL.GL_LINK_STATUS, tmp);
            Debug.Assert(tmp[0] == OpenGL.GL_TRUE, "Normals program link failed");
            
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(polygonNormalsGeometryShader);
            gl.DeleteShader(fragmentShader);
            
            #endregion

            //перевожу координаты верши в массив float
            List<float> vertices = new List<float>();
            for (int i = 0; i < _figure.Vertices.Count; ++i)
            {
                vertices.Add(_figure.Vertices[i].Position.X);
                vertices.Add(_figure.Vertices[i].Position.Y);
                vertices.Add(_figure.Vertices[i].Position.Z);
            }
            
            // создать объект вершинного массива
            uint[] arrays = new uint[3];
            gl.GenVertexArrays(3, arrays);
            uint mainVAO = arrays[0];
            uint normalsVAO = arrays[1];
            uint pointLightVAO = arrays[2];
            
            // создать буффер вершин
            uint[] buffers = new uint[5];
            gl.GenBuffers(5, buffers);
            uint VBO = buffers[0];
            uint VIO = buffers[1];
            uint normalsVBO = buffers[2];
            uint normalsVIO = buffers[3];
            uint pointLightVBO = buffers[4];

            uint[] indexes = new uint[] { };
            List<uint> normalIndexes = new List<uint>();
            
            gl.BindVertexArray(mainVAO);
                gl.UseProgram(shaderProgram);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO);
                gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, VIO);
                gl.VertexAttribPointer((uint)gl.GetAttribLocation(shaderProgram, "position"), 3, OpenGL.GL_FLOAT, false, 9 * sizeof(float), IntPtr.Zero);
                gl.VertexAttribPointer((uint)gl.GetAttribLocation(shaderProgram, "inNormal"), 3, OpenGL.GL_FLOAT, false, 9 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                gl.VertexAttribPointer((uint)gl.GetAttribLocation(shaderProgram, "inColor"), 3, OpenGL.GL_FLOAT, false, 9 * sizeof(float), (IntPtr)(6 * sizeof(float)));
                gl.EnableVertexAttribArray((uint)gl.GetAttribLocation(shaderProgram, "position"));
                gl.EnableVertexAttribArray((uint)gl.GetAttribLocation(shaderProgram, "inNormal"));
                gl.EnableVertexAttribArray((uint)gl.GetAttribLocation(shaderProgram, "inColor"));
            gl.BindVertexArray(0);
            gl.BindVertexArray(normalsVAO);
                gl.UseProgram(polygonNormalsShaderProgram);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, normalsVBO);
                gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, normalsVIO);
                gl.VertexAttribPointer((uint)gl.GetAttribLocation(polygonNormalsShaderProgram, "position"), 3, OpenGL.GL_FLOAT, false, 3 * sizeof(float), IntPtr.Zero);
                gl.EnableVertexAttribArray((uint)gl.GetAttribLocation(polygonNormalsShaderProgram, "position"));
            gl.BindVertexArray(0);
            gl.BindVertexArray(pointLightVAO);
                gl.UseProgram(shaderProgram);
                gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, pointLightVBO);
                gl.VertexAttribPointer((uint)gl.GetAttribLocation(shaderProgram, "position"), 3, OpenGL.GL_FLOAT, false, 3 * sizeof(float), IntPtr.Zero);
                gl.EnableVertexAttribArray((uint)gl.GetAttribLocation(shaderProgram, "position"));
            gl.BindVertexArray(0);

            #region настройка параметров

            gl.FrontFace(OpenGL.GL_CW);
            
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.DepthFunc(OpenGL.GL_LESS);
            
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.CullFace(OpenGL.GL_BACK);

            gl.ClearColor(0.2f, 0.2f, 0.2f, 1);

            #endregion

            glArea.Render += (o, args) =>
            {
                gl.UseProgram(shaderProgram);
                //отчистить буферы
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                gl.ClearDepth(1.0f); // 0 - ближе, 1 - далеко 
                gl.ClearStencil(0);

                #region отрисовка с разными опциями
                
                if (_figureChanged)
                {
                    _figureChanged = false;
                    _figure = new Ellipsoid((float) _r.Value,
                        (int) _meridiansCount.Value,
                        (int) _parallelsCount.Value);
                    _figure.TriangulateSquares();
                    _figure.SetColor((float) _materialColorR.Value,
                                     (float) _materialColorG.Value,
                                     (float) _materialColorB.Value);
                    
                    #region обновить буферы

                    //перевожу координаты верши в массив float
                    vertices = new List<float>();
                    for (int i = 0; i < _figure.Vertices.Count; ++i)
                    {
                        vertices.Add(_figure.Vertices[i].Position.X);
                        vertices.Add(_figure.Vertices[i].Position.Y);
                        vertices.Add(_figure.Vertices[i].Position.Z);

                        Vector4 normal = _figure.Vertices[i].CalculateNormal();
                        vertices.Add(normal.X);
                        vertices.Add(normal.Y);
                        vertices.Add(normal.Z);
                        
                        vertices.Add(_figure.Vertices[i].Color.X);
                        vertices.Add(_figure.Vertices[i].Color.Y);
                        vertices.Add(_figure.Vertices[i].Color.Z);
                    }
                    gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO);
                    gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, VIO);
                    // данные о вершинах 
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, vertices.ToArray(), OpenGL.GL_DYNAMIC_DRAW);
                    // массив индексов
                    indexes = _figure.GetEnumerationOfVertexes().ToArray();
                    gl.BufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, indexes, OpenGL.GL_DYNAMIC_DRAW);
                    
                    List<float> normalVertices = new List<float>();
                    normalIndexes = new List<uint>();
                    uint cnt = 0;
                    for (int i = 0; i < _figure.Polygons.Count; ++i)
                    {
                        Vector4 center = _figure.Polygons[i].CalculateCenter();
                        Vector4 normal = _figure.Polygons[i].CalculateNormal();
                        
                        normalVertices.Add(center.X);
                        normalVertices.Add(center.Y);
                        normalVertices.Add(center.Z);
                    
                        normalIndexes.Add(cnt);
                        ++cnt;

                        normalVertices.Add(center.X + 0.2f * normal.X);
                        normalVertices.Add(center.Y + 0.2f * normal.Y);
                        normalVertices.Add(center.Z + 0.2f * normal.Z);
                    
                        normalIndexes.Add(cnt);
                        ++cnt;
                    }
                
                    gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, normalsVBO);
                    gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, normalsVIO);
                
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, normalVertices.ToArray(), OpenGL.GL_DYNAMIC_DRAW);
                    gl.BufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, normalIndexes.ToArray(), OpenGL.GL_DYNAMIC_DRAW);
                    gl.VertexAttribPointer((uint)gl.GetAttribLocation(polygonNormalsShaderProgram, "position"), 3, OpenGL.GL_FLOAT, false, 3 * sizeof(float), IntPtr.Zero);
                    gl.EnableVertexAttribArray((uint)gl.GetAttribLocation(polygonNormalsShaderProgram, "position"));

                    #endregion
                }
                if (_pointLightChanged)
                {
                    _pointLightChanged = false;
                    _pointLight = new PointLight((float) _pointLightPositionX.Value,
                                                 (float) _pointLightPositionY.Value,
                                                 (float) _pointLightPositionZ.Value);

                    #region обновить буферы

                    gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, pointLightVBO);
                    float[] position = {_pointLight.Position.X, _pointLight.Position.Y, _pointLight.Position.Z};
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, position, OpenGL.GL_DYNAMIC_DRAW);
                    
                    #endregion
                }

                int animateLocation = gl.GetUniformLocation(shaderProgram, "animate");
                int curTimeLocation = gl.GetUniformLocation(shaderProgram, "curTime");
                if (_animate.Active)
                {
                    gl.Uniform1(animateLocation, 1);
                    gl.Uniform1(curTimeLocation, (uint) frame_clock.FrameTime - _startTime);
                }
                else
                {
                    gl.Uniform1(animateLocation, 0);
                    _startTime = (uint) frame_clock.FrameTime;
                }
                
                int transformationMatrixLocation = gl.GetUniformLocation(shaderProgram, "transformation");
                int scaleLocation = gl.GetUniformLocation(shaderProgram, "scale");
                int materialK_aLocation = gl.GetUniformLocation(shaderProgram, "material.k_a");
                int materialK_dLocation = gl.GetUniformLocation(shaderProgram, "material.k_d");
                int materialK_sLocation = gl.GetUniformLocation(shaderProgram, "material.k_s");
                int materialPLocation = gl.GetUniformLocation(shaderProgram, "material.p");
                int lightIntensityLocation = gl.GetUniformLocation(shaderProgram, "light.intensity");
                int lightPositionLocation = gl.GetUniformLocation(shaderProgram, "light.position");
                int lightAttenuationLocation = gl.GetUniformLocation(shaderProgram, "light.attenuation");
                int cameraPositionLocation = gl.GetUniformLocation(shaderProgram, "cameraPosition");
                
                gl.UniformMatrix4(transformationMatrixLocation, 1, false,  ToArray(_cameraTransformationMatrix));
                gl.Uniform3(scaleLocation, (float) _r.Value,(float) _r.Value,(float) _r.Value);
                gl.Uniform3(materialK_aLocation, (float)_k_aR.Value, (float)_k_aG.Value, (float)_k_aB.Value);
                gl.Uniform3(materialK_dLocation, (float)_k_dR.Value, (float)_k_dG.Value, (float)_k_dB.Value);
                gl.Uniform3(materialK_sLocation, (float)_k_sR.Value, (float)_k_sG.Value, (float)_k_sB.Value);
                gl.Uniform1(materialPLocation, (float)_p.Value);
                gl.Uniform3(lightIntensityLocation, (float)_pointLightIntensityR.Value, (float)_pointLightIntensityG.Value, (float)_pointLightIntensityB.Value);
                gl.Uniform3(lightPositionLocation, (float)_pointLightPositionX.Value, (float)_pointLightPositionY.Value, (float)_pointLightPositionZ.Value);
                gl.Uniform1(lightAttenuationLocation, (float)_attenuationСoefficient.Value);
                gl.Uniform3(cameraPositionLocation, _camera.Position.X, _camera.Position.Y, _camera.Position.Z);

                gl.BindVertexArray(mainVAO);
                    if (_allowZBuffer.Active)
                    {
                        gl.Enable(OpenGL.GL_DEPTH_TEST);
                        gl.CullFace(OpenGL.GL_BACK);
                    }
                    else if (!_allowZBuffer.Active)
                        gl.Disable(OpenGL.GL_DEPTH_TEST);
                    
                    if (_allowWireframe.Active && !_allowInvisPoly.Active)
                    {
                        gl.Disable(OpenGL.GL_CULL_FACE);
                        
                        gl.Uniform1(gl.GetUniformLocation(shaderProgram, "colorMode"), 1, new int[] {(int)FragmetShaderColorMode.Green});
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                        gl.DrawElements(OpenGL.GL_TRIANGLES, indexes.Length, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    }
                    else if (_allowWireframe.Active && _allowInvisPoly.Active)
                    {
                        gl.Enable(OpenGL.GL_CULL_FACE);
                        
                        gl.Uniform1(gl.GetUniformLocation(shaderProgram, "colorMode"), 1, new int[] {(int)FragmetShaderColorMode.Green});
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                        gl.DrawElements(OpenGL.GL_TRIANGLES, indexes.Length, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    }
                    else if (!_allowWireframe.Active && !_allowInvisPoly.Active)
                    {
                        gl.Disable(OpenGL.GL_CULL_FACE);
                    
                        gl.Uniform1(gl.GetUniformLocation(shaderProgram, "colorMode"), 1, new int[] {(int)FragmetShaderColorMode.None});
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                        gl.DrawElements(OpenGL.GL_TRIANGLES, indexes.Length, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    }
                    else if (!_allowWireframe.Active && _allowInvisPoly.Active)
                    {
                        gl.Enable(OpenGL.GL_CULL_FACE);
                        
                        gl.Uniform1(gl.GetUniformLocation(shaderProgram, "colorMode"), 1, new int[] {(int)FragmetShaderColorMode.None});
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                        gl.DrawElements(OpenGL.GL_TRIANGLES, indexes.Length, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    }
                gl.BindVertexArray(0);
                
                if (_allowPointLightVisible.Active)
                {
                    gl.UseProgram(shaderProgram);
                    gl.BindVertexArray(pointLightVAO);
                    gl.Uniform1(gl.GetUniformLocation(polygonNormalsShaderProgram, "colorMode"), 1, new int[] {(int)FragmetShaderColorMode.LightPaint});
                    gl.Uniform1(gl.GetUniformLocation(shaderProgram,"animate"), 1, new int[]{(int)0});
                    gl.PointSize(10);
                    gl.DrawArrays(OpenGL.GL_POINTS, 0, 1);
                    gl.BindVertexArray(0);
                }

                if (_allowNormals.Active)
                {
                    gl.UseProgram(shaderProgram);
                    gl.BindVertexArray(normalsVAO); 
                        transformationMatrixLocation = gl.GetUniformLocation(polygonNormalsShaderProgram, "transformation");
                        gl.UniformMatrix4(transformationMatrixLocation, 1, false,  ToArray(_cameraTransformationMatrix));
                        gl.Uniform1(gl.GetUniformLocation(polygonNormalsShaderProgram, "colorMode"), 1, new int[] {(int)FragmetShaderColorMode.Cyan});
                        gl.Uniform1(gl.GetUniformLocation(shaderProgram,"animate"), 1, new int[]{(int)1});
                        gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                        gl.DrawElements(OpenGL.GL_LINES, normalIndexes.Count, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    gl.BindVertexArray(0);
                }

                #endregion
            };
        }
        
        public static float[] ToArray(Matrix4x4 m)
        {
            return new float[]
            {
                m.M11, m.M21, m.M31, m.M41,
                m.M12, m.M22, m.M32, m.M42,
                m.M13, m.M23, m.M33, m.M43,
                m.M14, m.M24, m.M34, m.M44
            };
        }
        
        // считать исходин шейдера по названию файла
        public static string ReadFromRes(string name) {
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            using (Stream stream = assembly.GetManifestResourceStream(name))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        
        private void updateTranformationMatrixAdjustments()
        {
            #region Обновление спинбатоннов для матрицы

            _m11.Value = _cameraTransformationMatrix.M11;
            _m12.Value = _cameraTransformationMatrix.M12;
            _m13.Value = _cameraTransformationMatrix.M13;
            _m14.Value = _cameraTransformationMatrix.M14;
            _m21.Value = _cameraTransformationMatrix.M21;
            _m22.Value = _cameraTransformationMatrix.M22;
            _m23.Value = _cameraTransformationMatrix.M23;
            _m24.Value = _cameraTransformationMatrix.M24;
            _m31.Value = _cameraTransformationMatrix.M31;
            _m32.Value = _cameraTransformationMatrix.M32;
            _m33.Value = _cameraTransformationMatrix.M33;
            _m34.Value = _cameraTransformationMatrix.M34;
            _m41.Value = _cameraTransformationMatrix.M41;
            _m42.Value = _cameraTransformationMatrix.M42;
            _m43.Value = _cameraTransformationMatrix.M43;
            _m44.Value = _cameraTransformationMatrix.M44;
            
            #endregion
        }
    }
}