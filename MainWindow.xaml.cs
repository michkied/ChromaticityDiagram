using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChromaticityDiagram
{
    public partial class MainWindow : Window
    {
        Bezier _bezier;

        public MainWindow()
        {
            InitializeComponent();

            _bezier = new Bezier(spectrumCanvas);

            using (var file = new StreamReader($"../../../data.csv"))
            {
                string? line = file.ReadLine();
                while (line != null)
                {
                    if (line == "WL;X;Y;Z")
                    {
                        line = file.ReadLine();
                        continue;
                    }
                    string[] parts = line.Split(';');
                    if (int.Parse(parts[0]) > 700)
                    {
                        line = file.ReadLine();
                        continue;
                    }
                    double x = double.Parse(parts[1]);
                    double y = double.Parse(parts[2]);
                    double z = double.Parse(parts[3]);

                    double x1 = x / (x + y + z);
                    double y1 = y / (x + y + z);
                    double z1 = z / (x + y + z);
                    double x2 = x1 / y1;
                    double y2 = 1;
                    double z2 = z1 / y1;
                    double r = x2 * 3.2406 + y2 * -1.5372 + z2 * -0.4986;
                    double g = x2 * -0.9689 + y2 * 1.8758 + z2 * 0.0415;
                    double b = x2 * 0.0557 + y2 * -0.2040 + z2 * 1.0570;
                    //if (r > 0.0031308)
                    //{
                    //    r = 1.055 * Math.Pow(r, 1 / 2.4) - 0.055;
                    //}
                    //else
                    //{
                    //    r = 12.92 * r;
                    //}
                    //if (g > 0.0031308)
                    //{
                    //    g = 1.055 * Math.Pow(g, 1 / 2.4) - 0.055;
                    //}
                    //else
                    //{
                    //    g = 12.92 * g;
                    //}
                    //if (b > 0.0031308)
                    //{
                    //    b = 1.055 * Math.Pow(b, 1 / 2.4) - 0.055;
                    //}
                    //else
                    //{
                    //    b = 12.92 * b;
                    //}
                    r = Math.Max(0, Math.Min(1, r));
                    g = Math.Max(0, Math.Min(1, g));
                    b = Math.Max(0, Math.Min(1, b));
                    Color color = Color.FromScRgb(1, (float)r, (float)g, (float)b);
                    Ellipse rect = new Ellipse
                    {
                        Width = 7,
                        Height = 7,
                        Fill = new SolidColorBrush(color)
                    };
                    diagramCanvas.Children.Add(rect);
                    var result = GetDisplayDiagramCoordinates(x1, y1);
                    Canvas.SetLeft(rect, result.x);
                    Canvas.SetTop(rect, result.y);
                    line = file.ReadLine();
                }
            }

            spectrumCanvas.Children.Add(new Line
            {
                X1 = 40,
                Y1 = 40,
                X2 = 40,
                Y2 = 440,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            });

            spectrumCanvas.Children.Add(new Line
            {
                X1 = 40,
                Y1 = 440,
                X2 = 440,
                Y2 = 440,
                Stroke = Brushes.Black,
                StrokeThickness = 3
            });

            spectrumCanvas.Children.Add(_bezier);
        }

        private (double x, double y) GetDisplayDiagramCoordinates(double x, double y)
        {
            double h = Height;
            double w = Width / 2;
            (double x, double y) result;
            result.x = x * 441 + 80;
            result.y = 440 - y * 441 - 15;

            return result;
        }

        private void spectrumCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(spectrumCanvas);
            bool outOfBounds = point.X < Bezier.DisplayBegin.x
                || point.X > Bezier.DisplayEnd.x
                || point.Y < Bezier.DisplayBegin.y
                || point.Y > Bezier.DisplayEnd.y;

            if (outOfBounds) return;
            _bezier.AddControlPoint(point);
        }
    }
}