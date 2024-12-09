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

        private static readonly Point[] sRGBTriangle = new Point[3]
        {
            new Point(0.64, 0.33),
            new Point(0.30, 0.60),
            new Point(0.15, 0.06)
        };

        private static readonly Point[] WideGamutTriangle = new Point[3]
        {
            new Point(0.7347, 0.2653),
            new Point(0.1152, 0.8264),
            new Point(0.1566, 0.0177)
        };

        public Diagram(Canvas diagramCanvas, Bezier bezier, Dictionary<int, (double x, double y, double z)> data, Rectangle sRGBRect, Rectangle WideGamutRect)
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

                var color = GetsRGBFromXYZ(x, y, z);
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
            _point = new DiagramPoint(bezier, sRGBRect, WideGamutRect);
            diagramCanvas.Children.Add(_point);

            DrawTriangleSRGB();
            DrawTriangleWideGamut();
        }

        private void DrawTriangleSRGB()
        {
            var sRGB_R = GetDisplayDiagramCoordinates(sRGBTriangle[0].X, sRGBTriangle[0].Y);
            var sRGB_G = GetDisplayDiagramCoordinates(sRGBTriangle[1].X, sRGBTriangle[1].Y);
            var sRGB_B = GetDisplayDiagramCoordinates(sRGBTriangle[2].X, sRGBTriangle[2].Y);

            Line line_sRGB1 = new Line
            {
                X1 = sRGB_R.X,
                Y1 = sRGB_R.Y,
                X2 = sRGB_G.X,
                Y2 = sRGB_G.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Line line_sRGB2 = new Line
            {
                X1 = sRGB_G.X,
                Y1 = sRGB_G.Y,
                X2 = sRGB_B.X,
                Y2 = sRGB_B.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Line line_sRGB3 = new Line
            {
                X1 = sRGB_B.X,
                Y1 = sRGB_B.Y,
                X2 = sRGB_R.X,
                Y2 = sRGB_R.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            _canvas.Children.Add(line_sRGB1);
            _canvas.Children.Add(line_sRGB2);
            _canvas.Children.Add(line_sRGB3);
        }

        private void DrawTriangleWideGamut()
        {
            var wideGamut_R = GetDisplayDiagramCoordinates(WideGamutTriangle[0].X, WideGamutTriangle[0].Y);
            var wideGamut_G = GetDisplayDiagramCoordinates(WideGamutTriangle[1].X, WideGamutTriangle[1].Y);
            var wideGamut_B = GetDisplayDiagramCoordinates(WideGamutTriangle[2].X, WideGamutTriangle[2].Y);

            Line line_wideGamut1 = new Line
            {
                X1 = wideGamut_R.X,
                Y1 = wideGamut_R.Y,
                X2 = wideGamut_G.X,
                Y2 = wideGamut_G.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Line line_wideGamut2 = new Line
            {
                X1 = wideGamut_G.X,
                Y1 = wideGamut_G.Y,
                X2 = wideGamut_B.X,
                Y2 = wideGamut_B.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Line line_wideGamut3 = new Line
            {
                X1 = wideGamut_B.X,
                Y1 = wideGamut_B.Y,
                X2 = wideGamut_R.X,
                Y2 = wideGamut_R.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            _canvas.Children.Add(line_wideGamut1);
            _canvas.Children.Add(line_wideGamut2);
            _canvas.Children.Add(line_wideGamut3);
        }

        public static Point GetDisplayDiagramCoordinates(double x, double y)
        {
            double dispX = x * 433 + 80;
            double dispY = 433 - y * 433 - 16;

            return new Point(dispX, dispY);
        }

        public static Color GetsRGBFromXYZ(double x, double y, double z)
        {
            double r = x * 3.2404542 + y * -1.5371385 + z * -0.4985314;
            double g = x * -0.9692660 + y * 1.8760108 + z * 0.0415560;
            double b = x * 0.0556434 + y * -0.2040259 + z * 1.0572252;

            r = (r > 0.0031308) ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r;
            g = (g > 0.0031308) ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g;
            b = (b > 0.0031308) ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b;

            r = Math.Max(0, Math.Min(1, r));
            g = Math.Max(0, Math.Min(1, g));
            b = Math.Max(0, Math.Min(1, b));
            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        public static Color GetWideGamutFromXYZ(double x, double y, double z)
        {
            double r = x * 1.4628067 + y * -0.1840623 + z * -0.2743606;
            double g = x * -0.5217933 + y * 1.4472381 + z * 0.0677227;
            double b = x * 0.0349342 + y * -0.0968930 + z * 1.2884099;

            r = (r > 0.0031308) ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r;
            g = (g > 0.0031308) ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g;
            b = (b > 0.0031308) ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b;

            r = Math.Max(0, Math.Min(1, r));
            g = Math.Max(0, Math.Min(1, g));
            b = Math.Max(0, Math.Min(1, b));
            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        public static (double X, double Y, double Z) GetXYZFromxyY(double x, double y, double Y)
        {
            double X = x * Y / y;
            double Z = (1 - x - y) * Y / y;
            return (X, Y, Z);
        }

        public static bool IsPointInTriangle(Point p, Point[] t)
        {
            double as_x = p.X - t[0].X;
            double as_y = p.Y - t[0].Y;
            bool s_ab = (t[1].X - t[0].X) * as_y - (t[1].Y - t[0].Y) * as_x > 0;
            if ((t[2].X - t[0].X) * as_y - (t[2].Y - t[0].Y) * as_x > 0 == s_ab) return false;
            if ((t[2].X - t[1].X) * (p.Y - t[1].Y) - (t[2].Y - t[1].Y) * (p.X - t[1].X) > 0 != s_ab) return false;
            return true;
        }

        class DiagramPoint : Shape
        {
            private Bezier _bezier;
            private Rectangle _sRGBRect, _WideGamutRect;
            public double x { get; private set; }
            public double y { get; private set; }

            private Point? _displayPoint;

            public DiagramPoint(Bezier bezier, Rectangle sRGBRect, Rectangle WideGamutRect)
            {
                Fill = Brushes.Black;

                _bezier = bezier;
                bezier.CIERecalculated += Redraw;
                Canvas.SetZIndex(this, 2);
                _sRGBRect = sRGBRect;
                _WideGamutRect = WideGamutRect;
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

                x = _bezier.CIE_Coords.X / denom;
                y = _bezier.CIE_Coords.Y / denom;
                _displayPoint = Diagram.GetDisplayDiagramCoordinates(x, y);

                var XYZ = Diagram.GetXYZFromxyY(x, y, 0.5);
                if (Diagram.IsPointInTriangle(new Point(x, y), Diagram.sRGBTriangle))
                {
                    _sRGBRect.Fill = new SolidColorBrush(Diagram.GetsRGBFromXYZ(XYZ.X, XYZ.Y, XYZ.Z));
                }
                else
                {
                    _sRGBRect.Fill = Brushes.Transparent;
                }
                if (Diagram.IsPointInTriangle(new Point(x, y), Diagram.WideGamutTriangle))
                {
                    _WideGamutRect.Fill = new SolidColorBrush(Diagram.GetWideGamutFromXYZ(XYZ.X, XYZ.Y, XYZ.Z));
                }
                else
                {
                    _WideGamutRect.Fill = Brushes.Transparent;
                }

                _sRGBRect.InvalidateVisual();
                _WideGamutRect.InvalidateVisual();
            }

            protected override Geometry DefiningGeometry
            {
                get
                {
                    if (_displayPoint == null) return Geometry.Empty;

                    GeometryGroup group = new();
                    group.Children.Add(new EllipseGeometry(_displayPoint.Value, 5, 5));

                    FormattedText text = new FormattedText($"({x.ToString("0.##")},{y.ToString("0.##")})",
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
}
