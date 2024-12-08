using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace ChromaticityDiagram
{
    class Diagram
    {
        private Bezier _bezier;
        private Canvas _canvas;
        private DiagramPoint _point;

        public Diagram(Canvas diagramCanvas, Bezier bezier, Dictionary<int, (double x, double y, double z)> data)
        {

            foreach (var item in data) 
            {
                double CIE_X = item.Value.x;
                double CIE_Y = item.Value.y;
                double CIE_Z = item.Value.z;

                if (CIE_X + CIE_Y + CIE_Z == 0) continue;
                if (item.Key < Bezier.Begin.y || item.Key > 738) continue;

                double x = CIE_X / (CIE_X + CIE_Y + CIE_Z);
                double y = CIE_Y / (CIE_X + CIE_Y + CIE_Z);
                double z = CIE_Z / (CIE_X + CIE_Y + CIE_Z);

                var color = GetRGBFromXYZ(x, y, z);
                Ellipse rect = new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    Fill = new SolidColorBrush(color)
                };
                diagramCanvas.Children.Add(rect);
                var disp = GetDisplayDiagramCoordinates(x, y);
                Canvas.SetLeft(rect, disp.X);
                Canvas.SetTop(rect, disp.Y);
            }

            _canvas = diagramCanvas;
            _bezier = bezier;
            _point = new DiagramPoint(bezier);
            diagramCanvas.Children.Add(_point);
        }

        public static Point GetDisplayDiagramCoordinates(double x, double y)
        {
            double dispX = x * 433 + 80;
            double dispY = 433 - y * 433 - 16;

            return new Point(dispX, dispY);
        }

        public static Color GetRGBFromXYZ(double x, double y, double z)
        {
            double r = x * 3.2406 + y * -1.5372 + z * -0.4986;
            double g = x * -0.9689 + y * 1.8758 + z * 0.0415;
            double b = x * 0.0557 + y * -0.2040 + z * 1.0570;

            r = (r > 0.0031308) ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r;
            g = (g > 0.0031308) ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g;
            b = (b > 0.0031308) ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b;
            r = Math.Max(0, Math.Min(1, r));
            g = Math.Max(0, Math.Min(1, g));
            b = Math.Max(0, Math.Min(1, b));
            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }
    }

    class DiagramPoint : Shape
    {
        private Bezier _bezier;
        public double X { get; private set; }
        public double Y { get; private set; }

        private Point? _displayPoint;

        public DiagramPoint(Bezier bezier)
        {
            Fill = Brushes.Black;

            _bezier = bezier;
            bezier.CIERecalculated += Redraw;
            Canvas.SetZIndex(this, 2);
            CalculatePoints();
            if (_displayPoint == null) return;
        }

        private void Redraw(object? sender, EventArgs e)
        {
            CalculatePoints();
            InvalidateVisual();
        }

        private void CalculatePoints()
        {
            double denom = _bezier.CIE_Coords.X + _bezier.CIE_Coords.Y + _bezier.CIE_Coords.Z;
            if (denom == 0) 
            {
                _displayPoint = null;
                return;
            }

            X = _bezier.CIE_Coords.X / denom;
            Y = _bezier.CIE_Coords.Y / denom;
            _displayPoint = Diagram.GetDisplayDiagramCoordinates(X, Y);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (_displayPoint == null) return Geometry.Empty;

                GeometryGroup group = new();
                group.Children.Add(new EllipseGeometry(_displayPoint.Value, 5, 5));

                FormattedText text = new FormattedText($"({X.ToString("0.##")},{Y.ToString("0.##")})",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Calibri"),
                    10,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                group.Children.Add(text.BuildGeometry(new Point(_displayPoint.Value.X - 5, _displayPoint.Value.Y - 15)));

                return group;
            }
        }
    }
}
