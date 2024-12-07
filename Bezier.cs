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
            ControlPoint p0 = ControlPoint.FromPoint(new Point(380, 1));
            ControlPoint p1 = ControlPoint.FromPoint(new Point(780, 1));
            _controlPoints.Add(p0);
            _controlPoints.Add(p1);
            canvas.Children.Add(p0);
            canvas.Children.Add(p1);
            _canvas = canvas;
        }

        public void AddControlPoint(Point point)
        {
            var controlPoint = ControlPoint.FromDisplayPoint(point);
            _controlPoints.Add(controlPoint);
            _controlPoints = _controlPoints.OrderBy(p => p.X).ToList();
            _canvas.Children.Add(controlPoint);
            InvalidateVisual();
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
                geometryGroup.Children.Add(new LineGeometry(start.DisplayPoint, new Point(DisplayBegin.x, DisplayEnd.y)));

                ControlPoint end = _controlPoints[^1];
                geometryGroup.Children.Add(new LineGeometry(end.DisplayPoint, new Point(DisplayEnd.x, DisplayEnd.y)));

                return geometryGroup;
            }
        }
    }
}
