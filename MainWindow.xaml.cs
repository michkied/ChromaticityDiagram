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
        Diagram _diagram;
        Bezier _bezier;

        public MainWindow()
        {
            InitializeComponent();

            Dictionary<int, (double x, double y, double z)> data = new();
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
                    int wl = int.Parse(parts[0]);
                    double x = double.Parse(parts[1]);
                    double y = double.Parse(parts[2]);
                    double z = double.Parse(parts[3]);
                    data[wl] = (x, y, z);
                    line = file.ReadLine();
                }
            }

            _bezier = new Bezier(spectrumCanvas, data);
            _diagram = new Diagram(diagramCanvas, _bezier, data);

            spectrumCanvas.Children.Add(_bezier);
            spectrumCanvas.Children.Add(new BezierLines(_bezier));
            DrawScales();
        }

        private void spectrumCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ControlPoint) return;

            Point position = e.GetPosition(spectrumCanvas);

            double resultX = position.X;
            if (position.X < Bezier.DisplayBegin.x) resultX = Bezier.DisplayBegin.x;
            else if (position.X > Bezier.DisplayEnd.x) resultX = Bezier.DisplayEnd.x;

            double resultY = position.Y;
            if (position.Y < Bezier.DisplayBegin.y) resultY = Bezier.DisplayBegin.y;
            else if (position.Y > Bezier.DisplayEnd.y) resultY = Bezier.DisplayEnd.y;

            _bezier.AddControlPoint(new Point(resultX, resultY));
        }

        private void DrawScales()
        {
            int numOfSteps = 9;
            double length = 10;
            double stepAX = (Bezier.DisplayEnd.x - Bezier.DisplayBegin.x) / numOfSteps;
            double stepAY = (Bezier.DisplayEnd.y - Bezier.DisplayBegin.y) / numOfSteps;

            for (int i = 0; i <= numOfSteps; i++) {

                Line lineX = new Line
                {
                    X1 = i * stepAX + Bezier.DisplayBegin.x,
                    Y1 = Bezier.DisplayEnd.y,
                    X2 = i * stepAX + Bezier.DisplayBegin.x,
                    Y2 = Bezier.DisplayEnd.y + length / 2,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Line lineY = new Line
                {
                    X1 = 40,
                    Y1 = i * stepAY + Bezier.DisplayBegin.y,
                    X2 = 40 - length / 2,
                    Y2 = i * stepAY + Bezier.DisplayBegin.y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                TextBlock textX = new TextBlock
                {
                    Text = $"{Bezier.Begin.x + i * (Bezier.End.x - Bezier.Begin.x) / numOfSteps}",
                    FontSize = 9,
                };

                TextBlock textY = new TextBlock
                {
                    Text = $"{(Bezier.Begin.y + i * (Bezier.End.y - Bezier.Begin.y) / numOfSteps).ToString("0.#")}",
                    FontSize = 9,
                };

                spectrumCanvas.Children.Add(lineX);
                spectrumCanvas.Children.Add(lineY);

                spectrumCanvas.Children.Add(textX);
                Canvas.SetLeft(textX, i * stepAX + Bezier.DisplayBegin.x - 8);
                Canvas.SetTop(textX, Bezier.DisplayEnd.y + length / 2 + 5);

                spectrumCanvas.Children.Add(textY);
                Canvas.SetLeft(textY, 40 - length / 2 - 20);
                Canvas.SetTop(textY, i * stepAY + Bezier.DisplayBegin.y - 5);
            }
        }
    }
}