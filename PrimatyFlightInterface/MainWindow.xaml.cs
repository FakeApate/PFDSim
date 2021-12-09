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
    /// also logic for the virtuall horizon
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Plane status
        public double rollRotation = 0;
        public double pitchRotation = 0;
        #endregion

        #region UIElements
        private readonly Line horizon = new();
        private readonly Polygon ground = new();
        private readonly Polygon indicator = new();
        private readonly Line[] rollLines = new Line[11];
        private readonly Canvas PitchIndicatorCanvas = new();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates the biggest possible canvas square
        /// </summary>
        private void UpdateCanvasSize()
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

        /// <summary>
        /// Updates the polygon points that symbolizes the ground.
        /// </summary>
        /// <param name="l1">Point of horizon</param>
        /// <param name="l2">Point of horizon</param>
        private void updateGround(Point l1, Point l2)
        {
            //All 4 points of the canvas
            Point p1 = new(0, 0);
            Point p2 = new(this.Horizon_Canvas.ActualWidth, 0);
            Point p3 = new(0, this.Horizon_Canvas.ActualHeight);
            Point p4 = new(this.Horizon_Canvas.ActualWidth, this.Horizon_Canvas.ActualHeight);

            //If l1 is equals l2 the horizon is not in view anymore
            //The plane is either climbing or descending very fast.
            if (l1 == l2)
            {
                if(l1 == p1 || l1==p2 )
                {
                    ground.Points.Clear();
                    ground.Points.Add(p1);
                    ground.Points.Add(p3);
                    ground.Points.Add(p4);
                    ground.Points.Add(p2);
                    return;
                }
                else if( l1 == p4 || l1 == p3)
                {
                    ground.Points.Clear();
                    return;
                }
            }
            
            ///Todo
            ///not sure this is the right way to determin if the ground is upside down
            if(Math.Abs(this.rollRotation) > 90 && Math.Abs(this.rollRotation) <= 270)
            {
                Point tmp = l1;
                l1 = l2;
                l2 = tmp;
            }

            //Going trough the perimeter of the canvas to find all points that are between l1 --> l2
            List<Point> points = new List<Point>();
            points.Add(p1);
            if (checkPointIntecept(l1, p1, p3)) points.Add(l1);
            if (checkPointIntecept(l2, p1, p3)) points.Add(l2);
            points.Add(p3);
            if (checkPointIntecept(l1, p3, p4)) points.Add(l1);
            if (checkPointIntecept(l2, p3, p4)) points.Add(l2);
            points.Add(p4);
            if (checkPointIntecept(l1, p4, p2)) points.Add(l1);
            if (checkPointIntecept(l2, p4, p2)) points.Add(l2);
            points.Add(p2);
            if (checkPointIntecept(l1, p2, p1)) points.Add(l1);
            if (checkPointIntecept(l2, p2, p1)) points.Add(l2);

            //shifting the array so that l1 is the first member
            var result = points.FindIndex(x => x == l1);
            for (int i = 0; i < result; i++)
            {
                Point tmp = points[0];
                points.RemoveAt(0);
                points.Add(tmp);
            }
            //Deleting all members after l2, those are not importand
            result = points.FindIndex(x => x == l2)+1;
            points.RemoveRange(result, points.Count-result);

            //Updating the points
            ground.Points.Clear();
            ground.Points = new PointCollection(points);
        }

        /// <summary>
        /// Checks if a point <paramref name="pC"/> is inside the line from <paramref name="p1"/> to <paramref name="p2"./>
        /// </summary>
        /// <param name="pC"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns><code>true</code> when crossin.g</returns>
        private bool checkPointIntecept(Point pC, Point p1, Point p2)
        {
            Vector cross = new(pC.X - p1.X, pC.Y - p1.Y);
            Vector line = new(p2.X - p1.X, p2.Y - p1.Y);
            return cross.X * line.Y - cross.Y * line.X == 0;
        }


        #region Draw Methods
        private void DrawHorizon()
        {
            Canvas canvas = this.Horizon_Canvas;
            horizon.X1 = 0;
            horizon.X2 = canvas.ActualWidth;
            double sizeMultiplier = this.Horizon_Canvas.Width;

            Point pM = new(canvas.ActualWidth / 2, canvas.ActualHeight / 2);
            pM.Y = Math.Tan(Math.PI / 180 * (this.pitchRotation)) * sizeMultiplier + pM.Y;
            double radiant = (Math.PI / 180) * rollRotation;
            horizon.Y1 = Math.Tan(radiant * -1) * (pM.X) + (pM.Y);
            horizon.Y2 = Math.Tan(radiant) * (pM.X) + (pM.Y);

            if (horizon.Y1 < 0)
            {
                double overflow = horizon.Y1;
                horizon.Y1 = 0;
                var x = overflow / Math.Tan(radiant * -1);
                horizon.X1 = x > canvas.ActualWidth ? 0 : x;
            }
            else if (horizon.Y1 > canvas.ActualHeight)
            {
                double overflow = horizon.Y1 - canvas.ActualHeight;
                horizon.Y1 = canvas.ActualHeight;
                var x = overflow / Math.Tan(radiant * -1);
                horizon.X1 = x < 0 ? 0 : x;
            }

            if (horizon.Y2 < 0)
            {
                double overflow = horizon.Y2;
                horizon.Y2 = 0;
                var x = canvas.ActualWidth - overflow / Math.Tan(radiant);
                horizon.X2 = x > canvas.ActualWidth ? 0 : x;
            }
            else if (horizon.Y2 > canvas.ActualHeight)
            {
                double overflow = horizon.Y2 - canvas.ActualHeight;
                horizon.Y2 = canvas.ActualHeight;
                var x = canvas.ActualWidth - overflow / Math.Tan(radiant);
                horizon.X2 = x < 0 ? 0 : x;
            }

            updateGround(new(horizon.X1, horizon.Y1), new(horizon.X2, horizon.Y2));
        }

        private void DrawRollIndicatorLines()
        {
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
                Line l;
                if (rollLines[i] != null) l = rollLines[i];
                else
                {
                    l = new Line
                    {
                        Stroke = Brushes.White
                    };
                    Panel.SetZIndex(l, 1);
                    rollLines[i] = l;
                    Horizon_Canvas.Children.Add(l);
                }
                l.StrokeThickness = thickness;
                l.X1 = radiusStart * Math.Cos(Math.PI / 180 * (angels[i] + rotationalCorrection)) + Xm;
                l.Y1 = radiusStart * Math.Sin(Math.PI / 180 * (angels[i] + rotationalCorrection)) + Ym;
                l.X2 = radius * Math.Cos(Math.PI / 180 * (angels[i] + rotationalCorrection)) + Xm;
                l.Y2 = radius * Math.Sin(Math.PI / 180 * (angels[i] + rotationalCorrection)) + Ym;
            }
        }

        private void DrawRollIndicator()
        {
            double rotation = this.rollRotation;
            double radiusStart = this.Horizon_Canvas.Width * 0.75 / 2;
            int rotationalCorrection = -90;

            double Xm = this.Horizon_Canvas.Width / 2;
            double Ym = Xm;

            double height = this.Horizon_Canvas.Height / 30;

            double x0 = radiusStart * Math.Cos(Math.PI / 180 * (rotation + rotationalCorrection)) + Xm;
            double y0 = radiusStart * Math.Sin(Math.PI / 180 * (rotation + rotationalCorrection)) + Ym;
            double x1 = (radiusStart - height) * Math.Cos(Math.PI / 180 * (rotation + rotationalCorrection)) + Xm;
            double y1 = (radiusStart - height) * Math.Sin(Math.PI / 180 * (rotation + rotationalCorrection)) + Ym;

            Vector v1 = new Vector(y0 - y1, -(x0 - x1));
            Vector v1i = new Vector(-(y0 - y1), x0 - x1);

            v1 = Vector.Divide(v1, 2);
            v1i = Vector.Divide(v1i, 2);
            Point p1 = Vector.Add(v1, new Point(x1, y1));
            Point p2 = Vector.Add(v1i, new Point(x1, y1));

            indicator.Points.Clear();
            indicator.Points.Add(new Point(x0, y0));
            indicator.Points.Add(p1);
            indicator.Points.Add(p2);
        }

        private void DrawPitchIndicatorLines()
        {

            double step = 2.5;
            double minAngle = -20 - 10;
            double maxAngle = 20 + 10;
            double sizeMultiplier = this.Horizon_Canvas.Width;
            double smallLength = this.Horizon_Canvas.Width * 0.07;
            double mediumLength = this.Horizon_Canvas.Width * 0.15;
            double largeLength = this.Horizon_Canvas.Width * 0.3;
            double length;
            int indicatorThickness = 2;

            List<Double> angels = new();

            for (double i = minAngle; i <= maxAngle; i += step)
            {
                angels.Add(i);
            }

            Point pM = new(this.Horizon_Canvas.Width / 2, this.Horizon_Canvas.Height / 2);

            double yBot = Math.Tan(Math.PI / 180 * (-20 * -1)) * sizeMultiplier + pM.Y - indicatorThickness;
            double yTop = Math.Tan(Math.PI / 180 * (20 * -1)) * sizeMultiplier + pM.Y + indicatorThickness;
            double x1 = pM.X - largeLength / 2;
            double x2 = pM.X + largeLength / 2;

            PitchIndicatorCanvas.Width = x2 - x1;
            PitchIndicatorCanvas.Height = yBot - yTop;
            PitchIndicatorCanvas.Margin = new Thickness(x1, yTop, 0, 0);
            PitchIndicatorCanvas.Background = Brushes.Transparent;
            PitchIndicatorCanvas.RenderTransformOrigin = pM;

            pM = new(PitchIndicatorCanvas.Width / 2, PitchIndicatorCanvas.Height / 2);
            PitchIndicatorCanvas.Children.Clear();
            double pitchCorrection = 0;

            if (this.pitchRotation > 10 || this.pitchRotation < -10)
            {
                pitchCorrection = (int)(this.pitchRotation / 10) * -10;
            }

            RotateTransform rotate = new RotateTransform
            {
                Angle = rollRotation,
                CenterX = pM.X,
                CenterY = pM.Y
            };

            foreach (var angle in angels)
            {

                if (angle % 1 != 0)
                {
                    length = smallLength;
                }
                else if (angle % 10 != 0)
                {
                    length = mediumLength;
                }
                else
                {
                    length = largeLength;
                }

                double y1Offset = Math.Tan(Math.PI / 180 * ((angle * -1) + this.pitchRotation + pitchCorrection)) * sizeMultiplier + pM.Y;
                double y2Offset = y1Offset;
                double x1Offset = pM.X - length / 2;
                double x2Offset = pM.X + length / 2;
                if (y1Offset >= 0 && y1Offset <= PitchIndicatorCanvas.Height)
                {
                    Line l = new Line
                    {
                        X1 = x1Offset,
                        X2 = x2Offset,
                        Y1 = y1Offset,
                        Y2 = y2Offset,
                        Stroke = Brushes.White,
                        StrokeThickness = indicatorThickness,
                        RenderTransform = rotate
                    };
                    PitchIndicatorCanvas.Children.Add(l);
                }
            }
        }
        #endregion

        #region Events
        private void Horizon_Canvas_Loaded(object sender, RoutedEventArgs e)
        {     
            Horizon_Canvas.Children.Add(PitchIndicatorCanvas);
            Horizon_Canvas.Children.Add(horizon);
            Horizon_Canvas.Children.Add(ground);
            Horizon_Canvas.Children.Add(indicator);

            indicator.Fill = Brushes.Red;
            horizon.Stroke = Brushes.White;
            horizon.StrokeThickness = 5;
            ground.Fill = new SolidColorBrush(Color.FromRgb(0x7d, 0x52, 0x33));

            Panel.SetZIndex(ground, 0);
            Panel.SetZIndex(PitchIndicatorCanvas, 1);
            
            DrawPitchIndicatorLines();
            DrawRollIndicatorLines();
            DrawRollIndicator();
            DrawHorizon(); 
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.rollRotation = e.NewValue;
            DrawRollIndicator();
            DrawPitchIndicatorLines();
            DrawHorizon();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCanvasSize();
            DrawPitchIndicatorLines();
            DrawRollIndicatorLines();
            DrawRollIndicator();
            DrawHorizon();
        }

        private void Slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.pitchRotation = e.NewValue;       
            DrawPitchIndicatorLines();
            DrawHorizon();
        }
        #endregion
    }



}
