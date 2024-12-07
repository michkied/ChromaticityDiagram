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

namespace ChromaticityDiagram
{
    class Bezier : Shape
    {
        public static readonly (double x, double y) DisplayBegin = (70, 70);
        public static readonly (double x, double y) DisplayEnd = (410, 440);
        public static readonly (double x, double y) Begin = (380, 1.8);
        public static readonly (double x, double y) End = (780, 0);

        private List<ControlPoint> _controlPoints;
        private Canvas _canvas;

        public Bezier(Canvas canvas)
        {
            _controlPoints = new List<ControlPoint>();
            ControlPoint p0 = ControlPoint.FromPoint(new Point(380, 1), this);
            ControlPoint p1 = ControlPoint.FromPoint(new Point(780, 1), this);
            p0.MouseRightButtonDown += ControlPoint_MouseRightButtonDown;
            p1.MouseRightButtonDown += ControlPoint_MouseRightButtonDown;
            _controlPoints.Add(p0);
            _controlPoints.Add(p1);
            canvas.Children.Add(p0);
            canvas.Children.Add(p1);
            _canvas = canvas;
        }

        public void AddControlPoint(Point point)
        {
            var controlPoint = ControlPoint.FromDisplayPoint(point, this);
            controlPoint.MouseRightButtonDown += ControlPoint_MouseRightButtonDown;
            _controlPoints.Add(controlPoint);
            _controlPoints = _controlPoints.OrderBy(p => p.X).ToList();
            _canvas.Children.Add(controlPoint);
            InvalidateVisual();
        }

        private void ControlPoint_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_controlPoints.Count <= 2) return;
            ControlPoint control = (ControlPoint)sender;
            _controlPoints.Remove(control);
            _canvas.Children.Remove(control);
            InvalidateVisual();
        }

        public (double x1, double x2) GetControlPointMoveBounds(ControlPoint control)
        {
            int index = _controlPoints.IndexOf(control);
            if (index == 0)
            {
                return (DisplayBegin.x , _controlPoints[1].DisplayPoint.X);
            }
            else if (index == _controlPoints.Count - 1)
            {
                return (_controlPoints[^2].DisplayPoint.X, DisplayEnd.x);
            }
            else
            {
                return (_controlPoints[index - 1].DisplayPoint.X, _controlPoints[index + 1].DisplayPoint.X);
            }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup geometryGroup = new();
                StrokeThickness = 2;
                Stroke = Brushes.Black;

                for (int i = 0; i < _controlPoints.Count - 1; i++)
                {
                    ControlPoint p0 = _controlPoints[i];
                    ControlPoint p1 = _controlPoints[i + 1];
                    geometryGroup.Children.Add(new LineGeometry(p0.DisplayPoint, p1.DisplayPoint));
                }

                ControlPoint start = _controlPoints[0];
                geometryGroup.Children.Add(new LineGeometry(start.DisplayPoint, new Point(start.DisplayPoint.X, DisplayEnd.y)));

                ControlPoint end = _controlPoints[^1];
                geometryGroup.Children.Add(new LineGeometry(end.DisplayPoint, new Point(end.DisplayPoint.X, DisplayEnd.y)));

                return geometryGroup;
            }
        }
    }
}
