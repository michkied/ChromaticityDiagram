using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChromaticityDiagram
{
    class ControlPoint : Shape
    {
        public Point Point { get; private set; }
        public double X => Point.X;
        public double Y => Point.Y;

        public Point DisplayPoint { get; private set; }

        private ControlPoint(Point point)
        {
            DisplayPoint = point;
            
            double x = (point.X - Bezier.DisplayBegin.x) 
                / (Bezier.DisplayEnd.x - Bezier.DisplayBegin.x) 
                * (Bezier.End.x - Bezier.Begin.x) 
                + Bezier.Begin.x;

            double y = (point.Y - Bezier.DisplayBegin.y)
                / (Bezier.DisplayEnd.y - Bezier.DisplayBegin.y)
                * (Bezier.End.y - Bezier.Begin.y)
                + Bezier.Begin.y;

            this.Point = new Point(x, y);

            Fill = Brushes.Black;
        }

        public static ControlPoint FromPoint(Point point)
        {
            double x = (point.X - Bezier.Begin.x) 
                / (Bezier.End.x - Bezier.Begin.x) 
                * (Bezier.DisplayEnd.x - Bezier.DisplayBegin.x) 
                + Bezier.DisplayBegin.x;

            double y = (point.Y - Bezier.Begin.y)
                / (Bezier.End.y - Bezier.Begin.y)
                * (Bezier.DisplayEnd.y - Bezier.DisplayBegin.y)
                + Bezier.DisplayBegin.y;

            return new ControlPoint(new Point(x, y));
        }

        public static ControlPoint FromDisplayPoint(Point point)
        {
            return new ControlPoint(point);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup group = new();
                group.Children.Add(new EllipseGeometry(DisplayPoint, 5, 5));

                FormattedText text = new FormattedText($"({X.ToString("0.##")},{Y.ToString("0.##")})",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Calibri"),
                    14,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                group.Children.Add(text.BuildGeometry(DisplayPoint));

                return group;
            }
        }
    }
}
