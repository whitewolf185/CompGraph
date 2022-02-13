using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Numerics;
using Cairo;
using Gdk;
using SharpGL;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using Window = Gtk.Window;
using System.IO;
using System.Linq;
using System.Reflection;

namespace lab7
{
    class MainWindow : Window
    {

        [UI] private GLArea _drawingArea = null;
        [UI] private Adjustment _tt = null;
        
        static OpenGL gl = new SharpGL.OpenGL();

        private List<Vector2> points = new List<Vector2>();
        private List<float> mas = new List<float>();
        private List<float> drawpoints = new List<float>();
        private bool motion = false;
        private float Oldx = -1;
        private float Oldy = -1;
        private float dX = 0;
        private float dY = 0;
        private double width = 0;
        private double height = 0;
        private bool setbuff = false;
        private int pointid;
        
        private const int DOTSIZE = 8;

        [UI] private CheckButton _points = null;
        [UI] private CheckButton _lines = null;

        [UI] private Button _clearBtn = null;
        
        uint[] VAO = new uint[3];
        uint[] VBO = new uint[4];
        
        
        public MainWindow() : this(new Builder("MainWindow.glade"))
        {
        }

        public static string ReadFromRes(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;
            var c = assembly.GetManifestResourceNames();

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))

            using (StreamReader reader = new StreamReader(stream)) 
            {
                return reader.ReadToEnd();
            }
        }

        void Figure()
        {
            drawpoints = new List<float>();
            
            for (int i = 0; i < points.Count; i += 1)
            {
                drawpoints.Add(points[i].X);
                drawpoints.Add(points[i].Y);
            }

            mas = new List<float>();
            
            if (points.Count >= 4){
                double dt = 1.0 / _tt.Value;
                for (int i = 0; i < points.Count - 4; i += 3)
                {
                    for (float t = 0; t < 1; t += (float)dt)
                    {
                        mas.Add(points[i].X * (1 - t)*(1 - t)*(1 - t) 
                                + 3 * t * (1 - t)*(1 - t) * points[i + 1].X 
                                + 3 * t*t * (1 - t) * points[i + 2].X
                                + t * t * t * points[i + 3].X);
                    
                        mas.Add(points[i].Y * (1 - t)*(1 - t)*(1 - t) 
                                + 3 * t * (1 - t)*(1 - t) * points[i + 1].Y 
                                + 3 * t*t * (1 - t) * points[i + 2].Y
                                + t * t * t * points[i + 3].Y);
                    }
                }

                for (float t = 0; t < 1; t += (float)dt)
                { 
                    mas.Add(points[^4].X * (1 - t)*(1 - t)*(1 - t) 
                            + 3 * t * (1 - t)*(1 - t) * points[^3].X 
                            + 3 * t*t * (1 - t) * points[^2].X
                            + t * t * t * points[^1].X);
                    
                    mas.Add(points[^4].Y * (1 - t)*(1 - t)*(1 - t) 
                            + 3 * t * (1 - t)*(1 - t) * points[^3].Y 
                            + 3 * t*t * (1 - t) * points[^2].Y
                            + t * t * t * points[^1].Y);
                }
            }
            setbuff = true;
        }


        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);

            points = new List<Vector2>();

            width = _drawingArea.Allocation.Width;
            height = _drawingArea.Allocation.Height;
            _drawingArea.Realized += DrawingAreaOnRealized; 
            _drawingArea.Events |= EventMask.ScrollMask;
            _drawingArea.Events |= EventMask.ButtonPressMask | EventMask.PointerMotionMask  | EventMask.ButtonReleaseMask;
            _drawingArea.Resize += DrawingAreaResizeEvent;
            _tt.ValueChanged += ValueChanged2;
            _points.Toggled += ValueChanged;
            _clearBtn.Clicked += (sender, args) => {
                points.Clear();
                Figure();
                _drawingArea.QueueRender();
            };
            _lines.Toggled += ValueChanged;
           
            _drawingArea.MotionNotifyEvent += DrawingAreaOnMotionNotifyEvent;
            _drawingArea.ButtonReleaseEvent += DrawingAreaOnButtonReleaseEvent;
            _drawingArea.ButtonPressEvent += DrawingAreaOnButtonPressEvent;
            
            DeleteEvent += Window_DeleteEvent;
        }

        private void DrawingAreaOnRealized(object? sender, EventArgs e)
        {
            _drawingArea.MakeCurrent();

            uint vertexShader;

            vertexShader = gl.CreateShader(OpenGL.GL_VERTEX_SHADER);
            string s = ReadFromRes("lab7.OpenGL.VertexShader.glsl");
            gl.ShaderSource(vertexShader, s);
            gl.CompileShader(vertexShader);

            System.Text.StringBuilder txt = new System.Text.StringBuilder(512);
            gl.GetShaderInfoLog(vertexShader, 512, (IntPtr) 0, txt);
            Console.WriteLine(txt);

            var glsl_tmp = new int[1];
            gl.GetShader(vertexShader, OpenGL.GL_COMPILE_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader compilation failed");

            uint fragmentShader;
            fragmentShader = gl.CreateShader(OpenGL.GL_FRAGMENT_SHADER);
            s = ReadFromRes("lab7.OpenGL.FragmentShader.glsl");
            gl.ShaderSource(fragmentShader, s);
            gl.CompileShader(fragmentShader);

            txt = new System.Text.StringBuilder(512);
            gl.GetShaderInfoLog(fragmentShader, 512, (IntPtr) 0, txt);
            Console.WriteLine(txt);
            gl.GetShader(fragmentShader, OpenGL.GL_COMPILE_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader compilation failed");

            uint shaderProgram;
            shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);

            gl.GetProgram(shaderProgram, OpenGL.GL_LINK_STATUS, glsl_tmp);
            Debug.Assert(glsl_tmp[0] == OpenGL.GL_TRUE, "Shader program link failed");
            
            gl.GenVertexArrays(2, VAO);
            gl.GenBuffers(2, VBO);
            
            Figure();
            
            _drawingArea.Render += (o, args) =>
            {

                if (setbuff)
                {
                    gl.BindVertexArray(VAO[0]);
                    gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[0]);
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, mas.ToArray(), OpenGL.GL_DYNAMIC_DRAW);
                    gl.VertexAttribPointer(0, 2, OpenGL.GL_FLOAT, false, 0, (IntPtr) 0);
                    gl.EnableVertexAttribArray(0);
                    gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
                    gl.BindVertexArray(0);
                    
                    
                    gl.BindVertexArray(VAO[1]);
                    gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VBO[1]);
                    gl.BufferData(OpenGL.GL_ARRAY_BUFFER, drawpoints.ToArray(), OpenGL.GL_DYNAMIC_DRAW);
                    gl.VertexAttribPointer(0, 2, OpenGL.GL_FLOAT, false, 0, (IntPtr) 0);
                    gl.EnableVertexAttribArray(0);
                    gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
                    gl.BindVertexArray(0);
                    
                    
                    setbuff = false;
                }

                gl.FrontFace(OpenGL.GL_CW);

                gl.Enable(OpenGL.GL_DEPTH_TEST);
                gl.DepthFunc(OpenGL.GL_LESS);

                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                gl.Enable(OpenGL.GL_CULL_FACE);

                gl.Enable(OpenGL.GL_LINE_SMOOTH);
                gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);

                gl.ClearColor(0.8f, 0.8f, 0.8f, 1);

                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                
                int loc = gl.GetUniformLocation(shaderProgram, "c");
                gl.Uniform4(loc, (float)0, (float) 0.5, (float)0.5, 1);
                
                gl.BindVertexArray(VAO[0]);
                gl.UseProgram(shaderProgram);
                
                gl.LineWidth(6);
                gl.DrawArrays(OpenGL.GL_LINE_STRIP, 0, mas.Count/2);
                gl.BindVertexArray(0);


                if (_points.Active)
                {
                    gl.BindVertexArray(VAO[1]);
                    gl.UseProgram(shaderProgram);
                    gl.Uniform4(loc, (float)1, (float) 0, (float)0, 1);
                    gl.PointSize(10);
                    gl.DrawArrays(OpenGL.GL_POINTS, 0, drawpoints.Count/2);
                    gl.BindVertexArray(0);
                }

                if (_lines.Active)
                {
                     gl.BindVertexArray(VAO[1]);
                    gl.UseProgram(shaderProgram);
                    gl.Uniform4(loc, (float)0, (float) 0, (float)0, 1);
                    gl.LineWidth(3);
                    gl.DrawArrays(OpenGL.GL_LINE_LOOP, 0, drawpoints.Count/2);
                    gl.BindVertexArray(0);
                }
                
            };
            
            
            _drawingArea.Unrealized += delegate(object? sender, EventArgs args)
            {
                gl.UseProgram(0);
                gl.DeleteShader(vertexShader);
                gl.DeleteShader(fragmentShader);
                gl.DeleteBuffers(2, new [] { VBO[0], VBO[1] });
                gl.DeleteVertexArrays(2, new [] { VAO[0], VAO[1] });
                gl.DeleteProgram(shaderProgram);
            };
        }
        void DrawingAreaResizeEvent(object sender, EventArgs args)
        {
            width = _drawingArea.Allocation.Width;
            height = _drawingArea.Allocation.Height;
            _drawingArea.QueueRender();
        }
        
        private void ValueChanged(object? sender, EventArgs e)
        {
            _drawingArea.QueueRender();
        }
        
        private void ValueChanged2(object? sender, EventArgs e)
        {
            Figure();
            _drawingArea.QueueRender();
        }
        
        private void DrawingAreaOnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
        {
            if (motion)
            {
                dX = (float) (args.Event.X / (0.5f * width)) - Oldx - 1;
                dY = (float) ((height - args.Event.Y)/(0.5f * height) - 1 - Oldy);
                
                Oldx = (float) (args.Event.X / (0.5f * width)) - 1;
                Oldy = (float) ((height - args.Event.Y) / (0.5f * height)) - 1;

                Vector2 v = new Vector2();
                v.X = points[pointid].X + dX;
                v.Y = points[pointid].Y + dY;
                
                points[pointid] = v;
                 Figure();
                _drawingArea.QueueRender();
            }
        }
        private void DrawingAreaOnButtonPressEvent(object o, ButtonPressEventArgs args){
            Oldx = (float) (args.Event.X / (0.5f * width)) - 1;
            Oldy = (float) ((height - args.Event.Y) / (0.5f * height)) - 1;

            for (int i = 0; i < points.Count; ++i)
            {
                if (points[i].X - DOTSIZE/(0.5f * width) < Oldx && Oldx < points[i].X + DOTSIZE/(0.5f * width) &&
                    points[i].Y - DOTSIZE/(0.5f * height) < Oldy && Oldy < points[i].Y + DOTSIZE/(0.5f * height))
                {
                    motion = true;
                    pointid = i;
                    return;
                }
            }

            if (points.Count != 0){
                float oldPointX = points[^1].X;
                float oldPointY = points[^1].Y;

                points.Add(new Vector2(1.0f/3.0f*(Oldx - oldPointX) + oldPointX, 1.0f/3.0f*(Oldy - oldPointY) + oldPointY));
                points.Add(new Vector2(2.0f/3.0f*(Oldx - oldPointX) + oldPointX, 2.0f/3.0f*(Oldy - oldPointY) + oldPointY));
                points.Add(new Vector2(Oldx, Oldy));
            }
            else{
                points.Add(new Vector2(Oldx, Oldy));
            }

            Figure();
            _drawingArea.QueueRender();
        }
        private void DrawingAreaOnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            motion = false;
        }
        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }
    }
}