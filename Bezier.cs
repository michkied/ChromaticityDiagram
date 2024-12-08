using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Reflection;
using System.Numerics;
using System.Windows.Input;
using System.Diagnostics;

namespace ChromaticityDiagram
{
    class Bezier : Shape
    {
        public static readonly (double x, double y) DisplayBegin = (70, 70);
        public static readonly (double x, double y) DisplayEnd = (410, 440);
        public static readonly (double x, double y) Begin = (380, 1.8);
        public static readonly (double x, double y) End = (740, 0);

        public (double X, double Y, double Z) CIE_Coords { get; private set; }

        public List<ControlPoint> ControlPoints { get; private set; }
        private Canvas _canvas;
        private Dictionary<int, (double x, double y, double z)> _data;

        public event EventHandler? RedrawEvent;
        public event EventHandler? CIERecalculated;

        public Bezier(Canvas canvas, Dictionary<int, (double x, double y, double z)> data)
        {
            ControlPoints = new List<ControlPoint>();
            ControlPoint p0 = ControlPoint.FromPoint(new Point(Begin.x, 1), this);
            ControlPoint p1 = ControlPoint.FromPoint(new Point(End.x, 1), this);
            p0.MouseRightButtonDown += ControlPoint_MouseRightButtonDown;
            p1.MouseRightButtonDown += ControlPoint_MouseRightButtonDown;
            ControlPoints.Add(p0);
            ControlPoints.Add(p1);
            canvas.Children.Add(p0);
            canvas.Children.Add(p1);
            _canvas = canvas;
            Canvas.SetZIndex(this, 1);

            StrokeThickness = 2;
            Stroke = Brushes.Black;

            _data = data;
            CIE_Coords = (1, 1, 1);
        }

        public void Redraw()
        {
            InvalidateVisual();
            RedrawEvent?.Invoke(this, EventArgs.Empty);
        }

        public void AddControlPoint(Point point)
        {
            var controlPoint = ControlPoint.FromDisplayPoint(point, this);
            controlPoint.MouseRightButtonDown += ControlPoint_MouseRightButtonDown;
            ControlPoints.Add(controlPoint);
            ControlPoints = ControlPoints.OrderBy(p => p.X).ToList();
            _canvas.Children.Add(controlPoint);
            Redraw();
        }

        private void ControlPoint_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ControlPoints.Count <= 2) return;
            ControlPoint control = (ControlPoint)sender;
            ControlPoints.Remove(control);
            _canvas.Children.Remove(control);
            Redraw();
        }

        public (double x1, double x2) GetControlPointMoveBounds(ControlPoint control)
        {
            int index = ControlPoints.IndexOf(control);
            if (index == 0)
            {
                return (DisplayBegin.x , ControlPoints[1].DisplayPoint.X - 1);
            }
            else if (index == ControlPoints.Count - 1)
            {
                return (ControlPoints[^2].DisplayPoint.X + 1, DisplayEnd.x);
            }
            else
            {
                return (ControlPoints[index - 1].DisplayPoint.X + 1, ControlPoints[index + 1].DisplayPoint.X - 1);
            }
        }

        private static double GetBinCoeff(long N, long K)
        {
            long r = 1;
            long d;
            if (K > N) return 0;
            for (d = 1; d <= K; d++)
            {
                r *= N--;
                r /= d;
            }
            return r;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup geometryGroup = new();

                Point lastDisplayPoint = ControlPoints[0].DisplayPoint;
                Point lastPoint = ControlPoints[0].Point;

                double d = 1.0 / 500.0;
                double CIE_X = 0, CIE_Y = 0, CIE_Z = 0;

                for (double t = d; t < 1; t += d)
                {
                    double x = 0;
                    double y = 0;
                    double displayX = 0;
                    double displayY = 0;
                    for (int i = 0; i < ControlPoints.Count; i++)
                    {
                        double coeff = GetBinCoeff(ControlPoints.Count - 1, i) * Math.Pow(1 - t, ControlPoints.Count - 1 - i) * Math.Pow(t, i);
                        displayX += coeff * ControlPoints[i].DisplayPoint.X;
                        displayY += coeff * ControlPoints[i].DisplayPoint.Y;

                        x += coeff * ControlPoints[i].Point.X;
                        y += coeff * ControlPoints[i].Point.Y; 
                    }

                    double diff = x - lastPoint.X;

                    CIE_X += y * _data[(int)Math.Round(x)].x * diff;
                    CIE_Y += y * _data[(int)Math.Round(x)].y * diff;
                    CIE_Z += y * _data[(int)Math.Round(x)].z * diff;

                    Point newDisplayPoint = new Point(displayX, displayY);
                    geometryGroup.Children.Add(new LineGeometry(newDisplayPoint, lastDisplayPoint));
                    lastDisplayPoint = newDisplayPoint;
                    lastPoint = new Point(x, y);
                }

                double Px = ControlPoints[^1].Point.X;
                double Py = ControlPoints[^1].Point.Y;
                CIE_X += Py * _data[(int)Math.Round(Px)].x * (Px - lastPoint.X);
                CIE_Y += Py * _data[(int)Math.Round(Px)].y * (Px - lastPoint.X);
                CIE_Z += Py * _data[(int)Math.Round(Px)].z * (Px - lastPoint.X);
                CIE_Coords = (CIE_X, CIE_Y, CIE_Z);
                CIERecalculated?.Invoke(this, EventArgs.Empty);

                geometryGroup.Children.Add(new LineGeometry(ControlPoints[^1].DisplayPoint, lastDisplayPoint));

                return geometryGroup;
            }
        }
    }
}
