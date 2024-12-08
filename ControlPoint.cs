using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
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

        private bool _isDragging = false;
        private Bezier _bezier;

        private ControlPoint(Point point, Bezier bezier)
        {
            _bezier = bezier;
            DisplayPoint = point;
            this.Point = ConvertFromDisplayPoint(point);
            Canvas.SetZIndex(this, 2);

            Fill = Brushes.Black;

            MouseLeftButtonDown += ControlPoint_MouseLeftButtonDown;
            MouseLeftButtonUp += ControlPoint_MouseLeftButtonUp;
            MouseMove += ControlPoint_MouseMove;
        }

        public static ControlPoint FromPoint(Point point, Bezier bezier)
        {
            double x = (point.X - Bezier.Begin.x) 
                / (Bezier.End.x - Bezier.Begin.x) 
                * (Bezier.DisplayEnd.x - Bezier.DisplayBegin.x) 
                + Bezier.DisplayBegin.x;

            double y = (point.Y - Bezier.Begin.y)
                / (Bezier.End.y - Bezier.Begin.y)
                * (Bezier.DisplayEnd.y - Bezier.DisplayBegin.y)
                + Bezier.DisplayBegin.y;

            return new ControlPoint(new Point(x, y), bezier);
        }

        public static ControlPoint FromDisplayPoint(Point point, Bezier bezier)
        {
            return new ControlPoint(point, bezier);
        }

        private Point ConvertFromDisplayPoint(Point point)
        {
            double x = (point.X - Bezier.DisplayBegin.x)
                / (Bezier.DisplayEnd.x - Bezier.DisplayBegin.x)
                * (Bezier.End.x - Bezier.Begin.x)
                + Bezier.Begin.x;
            double y = (point.Y - Bezier.DisplayBegin.y)
                / (Bezier.DisplayEnd.y - Bezier.DisplayBegin.y)
                * (Bezier.End.y - Bezier.Begin.y)
                + Bezier.Begin.y;
            return new Point(x, y);
        }

        private void ControlPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            CaptureMouse();
        }

        private void ControlPoint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();
        }

        private void ControlPoint_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point position = e.GetPosition(this.Parent as UIElement);

                var bounds = _bezier.GetControlPointMoveBounds(this);

                double resultX = position.X;
                if (position.X < bounds.x1)
                {
                    resultX = bounds.x1;
                }
                else if (position.X > bounds.x2)
                {
                    resultX = bounds.x2;
                }

                double resultY = position.Y;
                if (position.Y < Bezier.DisplayBegin.y)
                {
                    resultY = Bezier.DisplayBegin.y;
                }
                else if (position.Y > Bezier.DisplayEnd.y)
                {
                    resultY = Bezier.DisplayEnd.y;
                }

                DisplayPoint = new Point(resultX, resultY);
                Point = ConvertFromDisplayPoint(DisplayPoint);
                InvalidateVisual();
                _bezier.Redraw(); 
            }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup group = new();
                group.Children.Add(new EllipseGeometry(DisplayPoint, 5, 5));

                FormattedText text = new FormattedText($"({X.ToString("0.#")},{Y.ToString("0.#")})",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Calibri"),
                    9,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                group.Children.Add(text.BuildGeometry(new Point(DisplayPoint.X - 5, DisplayPoint.Y - 15)));

                return group;
            }
        }
    }
}
