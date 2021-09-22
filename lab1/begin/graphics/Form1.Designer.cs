using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Remoting.Channels;

namespace graphics{
    partial class Form1{
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing){
            if (disposing && (components != null)){
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent(){
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.settings = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.num_Angle = new System.Windows.Forms.NumericUpDown();
            this.labelScale = new System.Windows.Forms.Label();
            this.numScale = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.parameter = new System.Windows.Forms.NumericUpDown();
            this.step = new System.Windows.Forms.Label();
            this.panelGraph = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.settings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_Angle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.parameter)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(12, 36);
            this.numericUpDown1.Minimum = new decimal(new int[]{ 4, 0, 0, 0 });
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(74, 25);
            this.numericUpDown1.TabIndex = 0;
            this.numericUpDown1.Value = new decimal(new int[]{ 40, 0, 0, 0 });
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
            // 
            // settings
            // 
            this.settings.Controls.Add(this.label5);
            this.settings.Controls.Add(this.num_Angle);
            this.settings.Controls.Add(this.labelScale);
            this.settings.Controls.Add(this.numScale);
            this.settings.Controls.Add(this.label1);
            this.settings.Controls.Add(this.parameter);
            this.settings.Controls.Add(this.step);
            this.settings.Controls.Add(this.numericUpDown1);
            this.settings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.settings.Font = new System.Drawing.Font("Mongolian Baiti", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settings.Location = new System.Drawing.Point(31, 28);
            this.settings.Name = "settings";
            this.settings.Size = new System.Drawing.Size(197, 139);
            this.settings.TabIndex = 1;
            this.settings.TabStop = false;
            this.settings.Text = "Настройки";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(107, 83);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(73, 18);
            this.label5.TabIndex = 12;
            this.label5.Text = "angle";
            // 
            // num_Angle
            // 
            this.num_Angle.Location = new System.Drawing.Point(109, 102);
            this.num_Angle.Maximum = new decimal(new int[]{ 180, 0, 0, 0 });
            this.num_Angle.Minimum = new decimal(new int[]{ 180, 0, 0, -2147483648 });
            this.num_Angle.Name = "num_Angle";
            this.num_Angle.Size = new System.Drawing.Size(72, 25);
            this.num_Angle.TabIndex = 11;
            this.num_Angle.ValueChanged += new System.EventHandler(this.num_Angle_ValueChanged);
            // 
            // labelScale
            // 
            this.labelScale.Location = new System.Drawing.Point(12, 84);
            this.labelScale.Name = "labelScale";
            this.labelScale.Size = new System.Drawing.Size(54, 18);
            this.labelScale.TabIndex = 5;
            this.labelScale.Text = "scale";
            // 
            // numScale
            // 
            this.numScale.DecimalPlaces = 3;
            this.numScale.Increment = new decimal(new int[]{ 15, 0, 0, 0 });
            this.numScale.Location = new System.Drawing.Point(12, 102);
            this.numScale.Maximum = new decimal(new int[]{ 100000, 0, 0, 0 });
            this.numScale.Minimum = new decimal(new int[]{ 10, 0, 0, 0 });
            this.numScale.Name = "numScale";
            this.numScale.Size = new System.Drawing.Size(73, 25);
            this.numScale.TabIndex = 4;
            this.numScale.Value = new decimal(new int[]{ 10, 0, 0, 0 });
            this.numScale.ValueChanged += new System.EventHandler(this.numScale_ValueChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(109, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Параметр А";
            // 
            // parameter
            // 
            this.parameter.Location = new System.Drawing.Point(113, 36);
            this.parameter.Maximum = new decimal(new int[]{ 20, 0, 0, 0 });
            this.parameter.Minimum = new decimal(new int[]{ 20, 0, 0, -2147483648 });
            this.parameter.Name = "parameter";
            this.parameter.Size = new System.Drawing.Size(70, 25);
            this.parameter.TabIndex = 2;
            this.parameter.Value = new decimal(new int[]{ 5, 0, 0, 0 });
            this.parameter.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
            // 
            // step
            // 
            this.step.Location = new System.Drawing.Point(12, 16);
            this.step.Name = "step";
            this.step.Size = new System.Drawing.Size(31, 17);
            this.step.TabIndex = 1;
            this.step.Text = "Шаг";
            // 
            // panelGraph
            // 
            this.panelGraph.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(230)))), ((int)(((byte)(233)))));
            this.panelGraph.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelGraph.Location = new System.Drawing.Point(227, 64);
            this.panelGraph.Margin = new System.Windows.Forms.Padding(10);
            this.panelGraph.Name = "panelGraph";
            this.panelGraph.Size = new System.Drawing.Size(821, 648);
            this.panelGraph.TabIndex = 2;
            this.panelGraph.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel_Paint);
            this.panelGraph.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelGraph_MouseMove);
            this.panelGraph.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelGraph_MouseUp);
            this.panelGraph.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.panelGraph_WheelEvent);
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.Color.MistyRose;
            this.ClientSize = new System.Drawing.Size(1058, 720);
            this.Controls.Add(this.panelGraph);
            this.Controls.Add(this.settings);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "Form1";
            this.Text = "Визуализатор графика";
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.settings.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.num_Angle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.parameter)).EndInit();
            this.ResumeLayout(false);
        }


        private System.Windows.Forms.Label label5;

        private System.Windows.Forms.NumericUpDown num_Angle;
        private System.Windows.Forms.NumericUpDown numScale;

        private System.Windows.Forms.Label labelScale;

        private System.Windows.Forms.NumericUpDown parameter;
        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.GroupBox settings;

        private System.Windows.Forms.Panel panelGraph;

        private System.Windows.Forms.Label step;

        private System.Windows.Forms.NumericUpDown numericUpDown1;

        #endregion
    }
}