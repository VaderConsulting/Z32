namespace Graphing
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Gauge1 = new AGaugeApp.AGauge();
            this.tmrMain = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // Gauge1
            // 
            this.Gauge1.BaseArcColor = System.Drawing.Color.Gray;
            this.Gauge1.BaseArcRadius = 80;
            this.Gauge1.BaseArcStart = 150;
            this.Gauge1.BaseArcSweep = 75;
            this.Gauge1.BaseArcWidth = 2;
            this.Gauge1.Cap_Idx = ((byte)(1));
            this.Gauge1.CapColors = new System.Drawing.Color[] {
        System.Drawing.Color.Black,
        System.Drawing.Color.Black,
        System.Drawing.Color.Black,
        System.Drawing.Color.Black,
        System.Drawing.Color.Black};
            this.Gauge1.CapPosition = new System.Drawing.Point(10, 10);
            this.Gauge1.CapsPosition = new System.Drawing.Point[] {
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10),
        new System.Drawing.Point(10, 10)};
            this.Gauge1.CapsText = new string[] {
        "",
        "",
        "",
        "",
        ""};
            this.Gauge1.CapText = "";
            this.Gauge1.Center = new System.Drawing.Point(100, 100);
            this.Gauge1.Location = new System.Drawing.Point(12, 12);
            this.Gauge1.MaxValue = 120F;
            this.Gauge1.MinValue = 30F;
            this.Gauge1.Name = "Gauge1";
            this.Gauge1.NeedleColor1 = AGaugeApp.AGauge.NeedleColorEnum.Gray;
            this.Gauge1.NeedleColor2 = System.Drawing.Color.DimGray;
            this.Gauge1.NeedleRadius = 80;
            this.Gauge1.NeedleType = 0;
            this.Gauge1.NeedleWidth = 2;
            this.Gauge1.Range_Idx = ((byte)(2));
            this.Gauge1.RangeColor = System.Drawing.Color.Red;
            this.Gauge1.RangeEnabled = true;
            this.Gauge1.RangeEndValue = 120F;
            this.Gauge1.RangeInnerRadius = 70;
            this.Gauge1.RangeOuterRadius = 80;
            this.Gauge1.RangesColor = new System.Drawing.Color[] {
        System.Drawing.Color.LightGreen,
        System.Drawing.Color.ForestGreen,
        System.Drawing.Color.Red,
        System.Drawing.SystemColors.Control,
        System.Drawing.SystemColors.Control};
            this.Gauge1.RangesEnabled = new bool[] {
        true,
        true,
        true,
        false,
        false};
            this.Gauge1.RangesEndValue = new float[] {
        80F,
        95F,
        120F,
        0F,
        0F};
            this.Gauge1.RangesInnerRadius = new int[] {
        70,
        70,
        70,
        70,
        70};
            this.Gauge1.RangesOuterRadius = new int[] {
        80,
        80,
        80,
        80,
        80};
            this.Gauge1.RangesStartValue = new float[] {
        -10F,
        80F,
        95F,
        0F,
        0F};
            this.Gauge1.RangeStartValue = 95F;
            this.Gauge1.ScaleLinesInterColor = System.Drawing.Color.Black;
            this.Gauge1.ScaleLinesInterInnerRadius = 73;
            this.Gauge1.ScaleLinesInterOuterRadius = 80;
            this.Gauge1.ScaleLinesInterWidth = 1;
            this.Gauge1.ScaleLinesMajorColor = System.Drawing.Color.Black;
            this.Gauge1.ScaleLinesMajorInnerRadius = 70;
            this.Gauge1.ScaleLinesMajorOuterRadius = 80;
            this.Gauge1.ScaleLinesMajorStepValue = 50F;
            this.Gauge1.ScaleLinesMajorWidth = 2;
            this.Gauge1.ScaleLinesMinorColor = System.Drawing.Color.Gray;
            this.Gauge1.ScaleLinesMinorInnerRadius = 75;
            this.Gauge1.ScaleLinesMinorNumOf = 9;
            this.Gauge1.ScaleLinesMinorOuterRadius = 80;
            this.Gauge1.ScaleLinesMinorWidth = 1;
            this.Gauge1.ScaleNumbersColor = System.Drawing.Color.Black;
            this.Gauge1.ScaleNumbersFormat = null;
            this.Gauge1.ScaleNumbersRadius = 95;
            this.Gauge1.ScaleNumbersRotation = 0;
            this.Gauge1.ScaleNumbersStartScaleLine = 0;
            this.Gauge1.ScaleNumbersStepScaleLines = 1;
            this.Gauge1.Size = new System.Drawing.Size(121, 180);
            this.Gauge1.TabIndex = 0;
            this.Gauge1.Value = 0F;
            // 
            // tmrMain
            // 
            this.tmrMain.Interval = 500;
            this.tmrMain.Tick += new System.EventHandler(this.tmrMain_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.Gauge1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private AGaugeApp.AGauge Gauge1;
        private System.Windows.Forms.Timer tmrMain;
    }
}

