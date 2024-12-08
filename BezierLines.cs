using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Controls;

namespace ChromaticityDiagram
{
    class BezierLines : Shape
    {
        private List<ControlPoint> ControlPoints { get { return _bezier.ControlPoints; } }
        private Bezier _bezier;
        public BezierLines(Bezier bezier)
        {
            _bezier = bezier;
            StrokeThickness = 1;
            Stroke = Brushes.Gray;
            StrokeDashArray = new DoubleCollection(new double[] { 5 });
            Canvas.SetZIndex(this, 0);

            _bezier.RedrawEvent += Redraw;
        }

        private void Redraw(object? sender, EventArgs e)
        {
            InvalidateVisual();
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup geometryGroup = new();

                for (int i = 0; i < ControlPoints.Count - 1; i++)
                {
                    ControlPoint p0 = ControlPoints[i];
                    ControlPoint p1 = ControlPoints[i + 1];
                    geometryGroup.Children.Add(new LineGeometry(p0.DisplayPoint, p1.DisplayPoint));
                }

                ControlPoint start = ControlPoints[0];
                geometryGroup.Children.Add(new LineGeometry(start.DisplayPoint, new Point(start.DisplayPoint.X, Bezier.DisplayEnd.y)));

                ControlPoint end = ControlPoints[^1];
                geometryGroup.Children.Add(new LineGeometry(end.DisplayPoint, new Point(end.DisplayPoint.X, Bezier.DisplayEnd.y)));

                return geometryGroup;
            }
        }
    }
}
