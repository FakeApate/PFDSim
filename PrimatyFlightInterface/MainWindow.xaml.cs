using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PrimatyFlightInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double rotation = 0;
        private Line horizon = new Line();
        private Polyline ground = new Polyline();
        private Point EdgeTL = new Point(0, 0);
        private Point EdgeTR = new Point(0, 0);
        private Point EdgeBL = new Point(0, 0);
        private Point EdgeBR = new Point(0, 0);

        
        public MainWindow()
        {
            InitializeComponent();          
        }

        private void drawingHorizon(Canvas canvas)
        {
            double radiant = (Math.PI / 180) * rotation;

            horizon.Stroke = Brushes.White;
            horizon.StrokeThickness = 5;
            horizon.X1 = 0;
            horizon.X2 = canvas.ActualWidth;

            EdgeTR.X = canvas.ActualWidth;
            EdgeBL.Y = canvas.ActualHeight;
            EdgeBR.X = canvas.ActualWidth;
            EdgeBR.Y = canvas.ActualHeight;

            Dictionary<Vector, List<Point>> groundRotation = new Dictionary<Vector, List<Point>> {
                { new Vector(45,90),        new List<Point>{ EdgeTL, EdgeBL } },
                { new Vector(91,134),       new List<Point>{ EdgeBL, EdgeTL } },
                { new Vector(-90,-45),      new List<Point>{ EdgeBR, EdgeTR } },
                { new Vector(-134,-91),     new List<Point>{ EdgeTR, EdgeBR } },
                { new Vector(135,180),      new List<Point>{ EdgeTL, EdgeTR } },
                { new Vector(-180,-135),    new List<Point>{ EdgeTL, EdgeTR } },
                { new Vector(-44,44),       new List<Point>{ EdgeBL, EdgeBR } },
            };

            switch (rotation)
            {
                case 90:
                    horizon.X1 = canvas.ActualWidth / 2;
                    horizon.X2 = canvas.ActualWidth / 2;
                    horizon.Y1 = 0;
                    horizon.Y2 = canvas.ActualHeight;
                    break;
                case -90:
                    horizon.X1 = canvas.ActualWidth / 2;
                    horizon.X2 = canvas.ActualWidth / 2;
                    horizon.Y1 = canvas.ActualHeight;
                    horizon.Y2 = 0;
                    break;

                case 180:
                    horizon.X1 = 0;
                    horizon.X2 = canvas.ActualWidth;
                    horizon.Y1 = canvas.ActualHeight / 2;
                    horizon.Y2 = canvas.ActualHeight/2;
                    break;
                case -180:
                    horizon.X1 = 0;
                    horizon.X2 = canvas.ActualWidth;
                    horizon.Y1 = canvas.ActualHeight / 2;
                    horizon.Y2 = canvas.ActualHeight / 2;
                    break;

                default:
                    horizon.Y1 = Math.Tan(radiant * -1) * (canvas.ActualWidth / 2) + (canvas.ActualHeight / 2);
                    horizon.Y2 = Math.Tan(radiant) * (canvas.ActualWidth / 2) + (canvas.ActualHeight / 2);
                    if (horizon.Y1 < 0)
                    {
                        double overflow = horizon.Y1;
                        horizon.Y1 = 0;
                        horizon.X1 = overflow / Math.Tan(radiant * -1);
                    }
                    else if (horizon.Y1 > canvas.ActualHeight)
                    {
                        double overflow = horizon.Y1 - canvas.ActualHeight;
                        horizon.Y1 = canvas.ActualHeight;
                        horizon.X1 = overflow / Math.Tan(radiant * -1);
                    }
                    if (horizon.Y2 < 0)
                    {
                        double overflow = horizon.Y2;
                        horizon.Y2 = 0;
                        horizon.X2 = canvas.ActualWidth - overflow / Math.Tan(radiant);
                    }
                    else if (horizon.Y2 > canvas.ActualHeight)
                    {
                        double overflow = horizon.Y2 - canvas.ActualHeight;
                        horizon.Y2 = canvas.ActualHeight;
                        horizon.X2 = canvas.ActualWidth - overflow / Math.Tan(radiant);
                    }

                   
                   
                    break;
            }

            ground.Points.Clear();
            ground.Fill = new SolidColorBrush(Color.FromRgb(0x7d, 0x52, 0x33));
            groundRotation[groundRotation.Keys.Single(x => rotation >= x.X && rotation <= x.Y)].ForEach(e => ground.Points.Add(e));
            ground.Points.Add(new Point(horizon.X2, horizon.Y2));
            ground.Points.Add(new Point(horizon.X1, horizon.Y1));








        }

        private void Horizon_Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            drawingHorizon(this.Horizon_Canvas);
            Horizon_Canvas.Children.Add(horizon);
            Horizon_Canvas.Children.Add(ground);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.rotation = e.NewValue;
            if(rotation > 180)
            {
                 this.rotation = -180 + (rotation - 180);
            }
            if (rotation < -180)
            {
                this.rotation = 180 + (rotation + 180);
            }
            drawingHorizon(this.Horizon_Canvas);
        }
    }

    
}
