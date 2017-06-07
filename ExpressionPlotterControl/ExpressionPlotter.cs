/*
 * Expression Plotter Control
 * Copyright 2007 by Syed Mehroz Alam <smehrozalam@yahoo.com>
 * This code can be used freely as long as you keep these comments
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace ExpressionPlotterControl
{
    public enum GraphMode
    { Rectangular, Polar };

    [ToolboxBitmap("graph.bmp")]
    public partial class ExpressionPlotter : Control
    {

        private double dScaleX = 10, dScaleY = 10;  //base scale for graph
        private readonly double EPSILON = 0.000000000001;
        private readonly List<Color> expColors;

        private readonly List<IEvaluatable> expressions;
        private readonly List<bool> expVisible;

        private int iDivisionsX = 5, iDivisionsY = 5;

        private int iLengthBoxX;
        private int iLengthBoxY;

        #region MemberVariables

        private int iLengthScaleX; //represents no. of pixels for x-axis
        private int iLengthScaleY; //represents no. of pixels for y-axis
        private int iOriginX, iOriginY; //represents the location of origin

        private int iPolarSensitivity = 100;
        private int iPrintStepX = 1;
        private int iPrintStepY = 1;
        private PrintDocument printDoc;

        #endregion MemberVariables

        #region Control specific functions

        public ExpressionPlotter()
        {
            this.expressions = new List<IEvaluatable>();
            this.expColors = new List<Color>();
            this.expVisible = new List<bool>();
            InitializeComponent();
        }

        public bool DisplayText { get; set; } = true;

        #endregion Control specific functions

        #region Properties

        public int DivisionsX
        {
            get { return this.iDivisionsX; }
            set { if (value > 0) this.iDivisionsX = value; }
        }

        public int DivisionsY
        {
            get { return this.iDivisionsY; }
            set { if (value > 0) this.iDivisionsY = value; }
        }

        public double ForwardX { get; set; }

        public double ForwardY { get; set; }

        public GraphMode GraphMode { get; set; } = GraphMode.Rectangular;

        public bool Grids { get; set; }

        public int PenWidth { get; set; } = 1;

        public int PolarSensitivity
        {
            get { return this.iPolarSensitivity; }
            set { if (value > 0) this.iPolarSensitivity = value; }
        }

        public int PrintStepX
        {
            get { return this.iPrintStepX; }
            set { if (value > 0) this.iPrintStepX = value; }
        }

        public int PrintStepY
        {
            get { return this.iPrintStepY; }
            set { if (value > 0) this.iPrintStepY = value; }
        }

        public double ScaleX
        {
            get { return this.dScaleX; }
            set { if (Math.Abs(value) > EPSILON) this.dScaleX = value; }
        }

        public double ScaleY
        {
            get { return this.dScaleY; }
            set { if (Math.Abs(value) > EPSILON) this.dScaleY = value; }
        }

        #endregion Properties

        #region Public functions for expression management

        public void AddExpression(IEvaluatable expression, Color color, bool visible)
        {
            expressions.Add(expression);
            expColors.Add(color);
            expVisible.Add(visible);
        }

        public void CopyToClipboard()
        {
            Clipboard.SetImage(GetGraphBitmap());
        }

        public IEvaluatable GetExpression(int index) => expressions[index];

        public Color GetExpressionColor(int index) => expColors[index];

        public bool GetExpressionVisibility(int index) => expVisible[index];

        public Bitmap GetGraphBitmap()
        {
            var bmpSnap = new Bitmap(this.Width, this.Height);
            DrawToBitmap(bmpSnap, new Rectangle(0, 0, this.Width, this.Height));
            return bmpSnap;
        }

        public double[] GetValues(double x)
        {
            var result = new double[expressions.Count];
            for (int i = 0; i < this.expressions.Count; i++)
            {
                if (this.expressions[i].IsValid)
                {
                    result[i] = this.expressions[i].Evaluate(x);
                }
            }

            return result;
        }

        public void MoveDown(int divisions)
        {
            this.ForwardY -= divisions * this.dScaleY / this.iDivisionsY;
        }

        public void MoveLeft(int divisions)
        {
            this.ForwardX -= divisions * this.dScaleX / this.iDivisionsX;
        }

        public void MoveRight(int divisions)
        {
            this.ForwardX += divisions * this.dScaleX / this.iDivisionsX;
        }

        public void MoveUp(int divisions)
        {
            this.ForwardY += divisions * this.dScaleY / this.iDivisionsY;
        }

        public void RemoveAllExpressions()
        {
            this.expressions.Clear();
            this.expColors.Clear();
            this.expVisible.Clear();
        }

        public bool RemoveExpression(IEvaluatable expression)
        {
            var index = expressions.IndexOf(expression);
            if (index == -1)
            {
                return false;
            }

            expressions.RemoveAt(index);
            expColors.RemoveAt(index);
            expVisible.RemoveAt(index);
            return true;
        }

        public void RemoveExpressionAt(int index)
        {
            // can throw OutOfRangeException
            expressions.RemoveAt(index);
            expColors.RemoveAt(index);
            expVisible.RemoveAt(index);
        }

        public void RestoreDefaults()
        {
            this.dScaleX = this.dScaleY = 10;
            this.ForwardX = this.ForwardY = 0;
            this.iDivisionsX = this.iDivisionsY = 5;
            this.iPrintStepX = this.iPrintStepY = 1;
            this.Grids = false;
            this.iPolarSensitivity = 100;
        }

        public void SetExpression(int index, IEvaluatable expression)
        {
            // can throw OutOfRangeException
            this.expressions[index] = expression;
        }

        public void SetExpressionColor(int index, Color color)
        {
            // can throw OutOfRangeException
            this.expColors[index] = color;
        }

        public void SetExpressionVisibility(int index, bool visibility)
        {
            // can throw OutOfRangeException
            this.expVisible[index] = visibility;
        }

        #endregion Public functions for expression management

        #region Public functions for graph management

        public void SetRangeX(double startX, double endX)
        {
            this.dScaleX = (endX - startX) / 2;
            this.ForwardX = (endX + startX) / 2;
        }

        public void SetRangeY(double startY, double endY)
        {
            this.dScaleY = (endY - startY) / 2;
            this.ForwardY = (endY + startY) / 2;
        }

        public void ToggleGrids()
        {
            this.Grids = (!Grids);
        }

        public void ZoomIn()
        {
            ZoomInX();
            ZoomInY();
        }

        public void ZoomInX()
        {
            this.dScaleX = DecreaseScale(this.dScaleX);
        }

        public void ZoomInY()
        {
            this.dScaleY = DecreaseScale(this.dScaleY);
        }

        public void ZoomOut()
        {
            ZoomOutX();
            ZoomOutY();
        }

        public void ZoomOutX()
        {
            this.dScaleX = IncreaseScale(this.dScaleX);
        }

        public void ZoomOutY()
        {
            this.dScaleY = IncreaseScale(this.dScaleY);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // update internal variables
            UpdateVariables();

            e.Graphics.Clear(Color.White);
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            PlotGraph(e.Graphics);

            // Calling the base class OnPaint
            base.OnPaint(e);
        }

        private static double DecreaseScale(double scale)
        {
            var absScale = Math.Round(Math.Abs(scale), 3);
            double newScale;
            newScale = absScale > 100 ? (absScale - 100) : absScale > 10 ? (absScale - 10) : absScale > 1 ? (absScale - 1) : absScale > .1 ? (absScale - .1) : absScale > .01 ? (absScale - .01) : absScale;
            return newScale * Math.Sign(scale);
        }

        private static double IncreaseScale(double scale)
        {
            var absScale = Math.Round(Math.Abs(scale), 3);
            double newScale;
            newScale = absScale >= 100 ? (absScale + 100) : absScale >= 10 ? (absScale + 10) : absScale >= 1 ? (absScale + 1) : absScale >= .10 ? (absScale + .10) : (absScale + .010);
            return newScale * Math.Sign(scale);
        }

        private void DisplayExpressionsText(Graphics g)
        {
            var line = 0;
            for (int i = 0; i < this.expressions.Count; i++)
            {
                if (expVisible[i])
                {
                    if (expressions[i].IsValid)
                    {
                        using (var solidBrush = new SolidBrush(expColors[i]))
                        {
                            using (var font = new Font("Arial", 8))
                            {
                                g.DrawString(expressions[i].ExpressionText, font, solidBrush, 10, 10 + 10 * line);
                            }
                        }
                    }
                    else
                    {
                        using (var solidBrush = new SolidBrush(expColors[i]))
                        {
                            using (var font = new Font("Arial", 8))
                            {
                                g.DrawString($"ERROR: {expressions[i].ExpressionText}", font, solidBrush, 10, 10 + 10 * line);
                            }
                        }
                    }

                    line++;
                }
            }
        }

        private void DisplayScale(Graphics g)
        {
            //axes lines
            using (var pen = new Pen(Color.Black, 1))
            {
                //axes lines
                g.DrawLine(pen, new Point(iOriginX - iLengthScaleX, iOriginY), new Point(iOriginX + iLengthScaleX, iOriginY));
            }

            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, new Point(iOriginX, iOriginY - iLengthScaleY), new Point(iOriginX, iOriginY + iLengthScaleY));
            }

            int i;
            double dValue;
            string strValue;

            float cordX, cordY;

            //X-axis values
            dValue = -dScaleX + ForwardX;
            for (i = -iDivisionsX; i <= iDivisionsX; i++)
            {
                using (var pen = new Pen(Color.Gray, 1))
                {
                    g.DrawLine(pen, new PointF((float)(iOriginX + (dValue - ForwardX) * iLengthScaleX / dScaleX), iOriginY - iLengthBoxY), new PointF((float)(iOriginX + (dValue - ForwardX) * iLengthScaleX / dScaleX), iOriginY + iLengthBoxY));
                }
                if (i % iPrintStepX == 0 && i != 0)
                {
                    strValue = Math.Round(dValue, 3).ToString();
                    cordX = (float)(iOriginX + (dValue - ForwardX) * iLengthScaleX / dScaleX - 6 - (strValue.Length - 2) * 5);
                    cordY = (float)(iOriginY + 10);
                    using (var solidBrush = new SolidBrush(Color.Black))
                    {
                        using (var font = new Font("Arial", 8))
                        {
                            g.DrawString(strValue, font, solidBrush, cordX, cordY);
                        }
                    }
                }
                dValue = dValue + dScaleX / iDivisionsX;
            }

            //Y-axis values
            dValue = -dScaleY + ForwardY;
            for (i = -iDivisionsY; i <= iDivisionsY; i++)
            {
                using (var pen = new Pen(Color.Gray, 1))
                {
                    g.DrawLine(pen, new PointF(iOriginX - iLengthBoxX, (float)(iOriginY + (dValue - ForwardY) * iLengthScaleY / dScaleY)), new PointF(iOriginX + iLengthBoxX, (float)(iOriginY + (dValue - ForwardY) * iLengthScaleY / dScaleY)));
                }
                if (i % iPrintStepY == 0 && i != 0)
                {
                    strValue = Math.Round(dValue, 3).ToString();
                    cordX = (float)(iOriginX - 20 - (strValue.Length) * 4);
                    cordY = (float)(iOriginY - (dValue - ForwardY) * iLengthScaleY / dScaleY - 7);
                    if (this.iLengthBoxY == this.iLengthScaleY)
                    {
                        cordY += 6;
                    }

                    using (var solidBrush = new SolidBrush(Color.Black))
                    {
                        using (var font = new Font("Arial", 8))
                        {
                            g.DrawString(strValue, font, solidBrush, cordX, cordY);
                        }
                    }
                }
                dValue = dValue + dScaleY / iDivisionsY;
            }

            if (GraphMode == GraphMode.Polar)
            {
                using (var pen = new Pen(Color.Black, 1))
                {
                    g.DrawEllipse(pen, iOriginX - iLengthScaleX, iOriginY - (float)(iLengthScaleY * dScaleX / dScaleY), iLengthScaleX * 2, (float)(iLengthScaleY * dScaleX / dScaleY) * 2);
                }
                for (dValue = 0; dValue <= 2 * Math.PI; dValue += Math.PI / 6)
                {
                    using (var pen = new Pen(Color.Gray, 1))
                    {
                        g.DrawLine(pen, new Point(iOriginX, iOriginY), new PointF((float)(iOriginX + iLengthScaleX * Math.Cos(dValue)), (float)(iOriginY + iLengthScaleY * Math.Sin(dValue))));
                    }
                }
            }
        }

        private void ExpressionPlotter_Resize(object sender, EventArgs e)
        {
            this.Refresh();
            //removed code that was keeping the control's height and width same
        }

        private void InitializePrintDoc()
        {
            this.printDoc = new PrintDocument();
            this.printDoc.OriginAtMargins = true;
            this.printDoc.DefaultPageSettings.Margins.Left = 200;
            this.printDoc.DefaultPageSettings.Margins.Top = 100;
            this.printDoc.DocumentName = "Graph Plotter by Syed Mehroz Alam";
            this.printDoc.PrintPage += delegate (object sender, PrintPageEventArgs e) { PlotGraph(e.Graphics); };
        }

        #endregion Public functions for graph management

        #region Plotting Functions

        private void PlotGraph(Graphics g)
        {
            DisplayScale(g);
            if (this.DisplayText)
            {
                DisplayExpressionsText(g);
            }

            double X, Y;
            double dPointX, dPointY;
            double dLeastStepX, dLeastStepY;
            double dMin, dMax, dStep;
            int i;

            //(X1,Y1) is the previous point ploted, (X2,Y2) is the current point to plot. (we will join both to have our
            // graph continuous).
            float X1 = 0, Y1 = 0, X2 = 0, Y2 = 0;
            //This variable controls whether our graph should be continuous or not
            var bContinuity = false;

            //divide scale with its length(pixels) to get increment per pixel
            dLeastStepX = dScaleX / iLengthScaleX;
            dLeastStepY = dScaleY / iLengthScaleY;

            //prepare variables for loop
            if (GraphMode == GraphMode.Polar)
            {
                dMin = -Math.PI;
                dMax = Math.PI;
                dStep = dScaleX / iPolarSensitivity;
            }
            else //if (Rectangular Mode)
            {
                dStep = dLeastStepX;
                dMin = -dScaleX + ForwardX;
                dMax = dScaleX + ForwardX;
            }

            for (i = 0; i < this.expressions.Count; i++)
            {
                //check if expression needs to be drawn and is valid
                if (expVisible[i] && expressions[i].IsValid)
                {
                    bContinuity = false;
                    for (X = dMin; Math.Abs(X - dMax) > EPSILON; X += dStep)
                    {
                        if (dScaleX < 0 && X < dMax)
                        {
                            break;
                        }

                        if (dScaleX > 0 && X > dMax)
                        {
                            break;
                        }

                        try
                        {
                            //evaluate expression[i] at point: X
                            Y = expressions[i].Evaluate(X);
                            if (double.IsNaN(Y))
                            {
                                //break continuity in graph if expression returned a NaN
                                bContinuity = false;
                                continue;
                            }

                            //get points to plot
                            if (GraphMode == GraphMode.Polar)
                            {
                                dPointX = Y * Math.Cos(X) / dLeastStepX;
                                dPointY = Y * Math.Sin(X) / dLeastStepY;
                            }
                            else // if (Rectangular mode
                            {
                                dPointX = X / dLeastStepX;
                                dPointY = Y / dLeastStepY;
                            }

                            //check if the point to be plotted lies inside our visible area(i.e. inside our current axes ranges)
                            if ((iOriginY - dPointY + ForwardY / dLeastStepY) < iOriginY - iLengthScaleY
                                || (iOriginY - dPointY + ForwardY / dLeastStepY) > iOriginY + iLengthScaleY
                                || (iOriginX + dPointX - ForwardX / dLeastStepX) < iOriginX - iLengthScaleX
                                || (iOriginX + dPointX - ForwardX / dLeastStepX) > iOriginX + iLengthScaleX)
                            {
                                //the point lies outside our current scale so break continuity
                                bContinuity = false;
                                continue;
                            }

                            //get coordinates for currently evaluated point
                            X2 = (float)(iOriginX + dPointX - ForwardX / dLeastStepX);
                            Y2 = (float)(iOriginY - dPointY + ForwardY / dLeastStepY);

                            //if graph should not be continuous
                            if (!bContinuity)
                            {
                                X1 = X2;
                                Y1 = Y2;
                                // the graph should be continuous afterwards since the current evaluated value is valid
                                //  and can be plot within our axes range
                                bContinuity = true;
                            }

                            //join points (X1,Y1) and (X2,Y2)
                            using (var pen = new Pen(expColors[i], PenWidth))
                            {
                                //join points (X1,Y1) and (X2,Y2)
                                g.DrawLine(pen, new PointF(X1, Y1), new PointF(X2, Y2));
                            }

                            //get current values into X1,Y1
                            X1 = X2;
                            Y1 = Y2;
                        }
                        catch (Exception)
                        {
                            bContinuity = false;
                            continue;
                        }
                    }
                }
            }
        }

        private void UpdateVariables()
        {
            iLengthScaleX = (int)(this.Width / 2.25);
            iLengthScaleY = (int)(this.Height / 2.25);
            iOriginX = (this.Width) / 2;
            iOriginY = (this.Height) / 2;
            if (Grids)
            {
                this.iLengthBoxX = this.iLengthScaleX;
                this.iLengthBoxY = this.iLengthScaleY;
            }
            else
            {
                this.iLengthBoxX = (int)(this.iLengthScaleX * 0.025);
                this.iLengthBoxY = (int)(this.iLengthScaleY * 0.025);
            }
        }

        #endregion Plotting Functions
    }
}