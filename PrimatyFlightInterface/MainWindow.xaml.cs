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
        private Polygon ground = new Polygon();
        private Polygon indicator = new Polygon();
        private Line[] indicatorLines = new Line[11];
        private Point EdgeTL = new Point(0, 0);
        private Point EdgeTR = new Point(0, 0);
        private Point EdgeBL = new Point(0, 0);
        private Point EdgeBR = new Point(0, 0);
        private Dictionary<Vector, List<Point>> groundRotation;


        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < indicatorLines.Length; i++)
            {
                indicatorLines[i] = new Line();
                indicatorLines[i].Stroke = Brushes.White;
                Horizon_Canvas.Children.Add(indicatorLines[i]);
            }
        }
        private void updateEdges()
        {
            EdgeTR.X = this.Horizon_Canvas.ActualWidth;
            EdgeBL.Y = this.Horizon_Canvas.ActualHeight;
            EdgeBR.X = this.Horizon_Canvas.ActualWidth;
            EdgeBR.Y = this.Horizon_Canvas.ActualHeight;

            groundRotation = new Dictionary<Vector, List<Point>> {
                { new Vector(45,90),        new List<Point>{ EdgeTL, EdgeBL } },
                { new Vector(91,134),       new List<Point>{ EdgeBL, EdgeTL } },
                { new Vector(-90,-45),      new List<Point>{ EdgeBR, EdgeTR } },
                { new Vector(-134,-91),     new List<Point>{ EdgeTR, EdgeBR } },
                { new Vector(135,180),      new List<Point>{ EdgeTL, EdgeTR } },
                { new Vector(-180,-135),    new List<Point>{ EdgeTL, EdgeTR } },
                { new Vector(-44,44),       new List<Point>{ EdgeBL, EdgeBR } },
            };
        }

        private void updateCanvasSize()
        {
            double maxWidth  = this.MainGrid.ColumnDefinitions[1].ActualWidth;
            double maxHeight = this.MainGrid.RowDefinitions[0].ActualHeight;

            if(maxHeight > maxWidth)
            {
                this.Horizon_Canvas.Width = maxWidth;
                this.Horizon_Canvas.Height = maxWidth;
            }
            else
            {
                this.Horizon_Canvas.Width = maxHeight;
                this.Horizon_Canvas.Height = maxHeight;
            }           
        }
        private void drawingHorizon()
        {
            Canvas canvas = this.Horizon_Canvas;
            horizon.X1 = 0;
            horizon.X2 = canvas.ActualWidth;
            if (Math.Abs(rotation) == 90)
            {
                horizon.X1 = canvas.ActualWidth / 2;
                horizon.X2 = canvas.ActualWidth / 2;
                horizon.Y1 = rotation == 90 ? 0: canvas.ActualHeight;
                horizon.Y2 = rotation == 90 ? canvas.ActualHeight : 0;
            }
            else if( Math.Abs(rotation) == 180)
            {
                horizon.X1 = 0;
                horizon.X2 = canvas.ActualWidth;
                horizon.Y1 = canvas.ActualHeight / 2;
                horizon.Y2 = canvas.ActualHeight / 2;
            }
            else
            {
                double radiant = (Math.PI / 180) * rotation;
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
            }

            updateEdges();
            ground.Points.Clear();            
            groundRotation[groundRotation.Keys.Single(x => rotation >= x.X && rotation <= x.Y)].ForEach(e => ground.Points.Add(e));
            ground.Points.Add(new Point(horizon.X2, horizon.Y2));
            ground.Points.Add(new Point(horizon.X1, horizon.Y1));
        }

        private void drawingTurnIndicator()
        {
            if (this.Horizon_Canvas.Width != this.Horizon_Canvas.Height) throw new Exception("Canvas is not a square!");

            int[] angels = { -70, -50, -30, -20, -10, 0, 10, 20, 30, 50, 70 };
            bool[] bigIndicator = { true, false, true, false, false, true, false, false, true, false, true };
            
            double Xm = this.Horizon_Canvas.Width / 2;
            double Ym = Xm;
            int rotationalCorrection = -90;
            double radiusStart = this.Horizon_Canvas.Width * 0.75 / 2;
            double radiusEndBig = this.Horizon_Canvas.Width * 0.85 / 2;
            double radiusEndSmall = this.Horizon_Canvas.Width * 0.8 / 2;

            
            for (int i = 0; i < angels.Length; i++)
            {
                double radius = bigIndicator[i] ? radiusEndBig : radiusEndSmall;
                int thickness = bigIndicator[i] ? 4 : 2;
                Line l = indicatorLines[i];
                l.StrokeThickness = thickness;
                l.X1 = radiusStart * Math.Cos(Math.PI / 180 * (angels[i]+rotationalCorrection)) + Xm;
                l.Y1 = radiusStart * Math.Sin(Math.PI / 180 * (angels[i]+ rotationalCorrection)) + Ym;
                l.X2 = radius * Math.Cos(Math.PI / 180 * (angels[i] + rotationalCorrection)) + Xm;
                l.Y2 = radius * Math.Sin(Math.PI / 180 * (angels[i] + rotationalCorrection)) + Ym;                
            }      
        }

        private void drawIndicator(double rotation)
        {
            double radiusStart = this.Horizon_Canvas.Width * 0.75 / 2;
            int rotationalCorrection = -90;

            double Xm = this.Horizon_Canvas.Width / 2;
            double Ym = Xm;

            double height = this.Horizon_Canvas.Height / 30;         

            double x0 = radiusStart         * Math.Cos(Math.PI / 180 * (rotation + rotationalCorrection)) + Xm;
            double y0 = radiusStart         * Math.Sin(Math.PI / 180 * (rotation + rotationalCorrection)) + Ym;   
            double x1 = (radiusStart-height)  * Math.Cos(Math.PI / 180 * (rotation + rotationalCorrection)) + Xm;
            double y1 = (radiusStart-height)  * Math.Sin(Math.PI / 180 * (rotation + rotationalCorrection)) + Ym;

            Vector v1 = new Vector(y0 - y1, -(x0 - x1));
            Vector v1i = new Vector(-(y0 - y1), x0 - x1);

            v1 = Vector.Divide(v1, 2);
            v1i = Vector.Divide(v1i,2);
            Point p1 = Vector.Add(v1, new Point(x1, y1));
            Point p2 = Vector.Add(v1i, new Point(x1, y1));

            indicator.Points.Clear();
            indicator.Points.Add(new Point(x0, y0));
            indicator.Points.Add(p1);
            indicator.Points.Add(p2);
        }

        private void Horizon_Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            
            Horizon_Canvas.Children.Add(horizon);
            Horizon_Canvas.Children.Add(ground);
            Horizon_Canvas.Children.Add(indicator);

            indicator.Fill = Brushes.Red;
            horizon.Stroke = Brushes.White;
            horizon.StrokeThickness = 5;
            ground.Fill = new SolidColorBrush(Color.FromRgb(0x7d, 0x52, 0x33));
             
            drawingTurnIndicator();
            drawIndicator(0);
            drawingHorizon();           
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.rotation = e.NewValue;
            drawIndicator(rotation);

            if (rotation > 180)
            {
                 this.rotation = -180 + (rotation - 180);
            }
            if (rotation < -180)
            {
                this.rotation = 180 + (rotation + 180);
            }
            drawingHorizon();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateCanvasSize();
            drawingTurnIndicator();
            drawIndicator(this.rotation);
            drawingHorizon();
        }
    }

    
    
}
