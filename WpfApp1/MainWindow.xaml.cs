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

using System.Drawing;
using ConsoleMorphVOXPro;
using AForge.Math;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //一个帧的长度为2048，重叠部分大小为1536
        Canvas canvasOs = new Canvas();
        String fileName = @"C:\Users\13643\Desktop\毕业设计\test\0_0.86_0_100.wav";
        String weiFileName = "wei.wav";
        private int samplingRate = 4410;
        private int unitOfOneSecond = 4410/5;// 在canvas上一秒单位长度
        private int heightZipCount = 50;
        int frame = 2048;
        int frameZip = 1536;
        int startNumber = 2048 * 29;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Point[] points= Init3();
            DrawXY();
            DrawMap(points);
            this.Content = canvasOs;
        }
        private Point[] Init3()
        {
            WAVReader wAVReader = new WAVReader();
            wAVReader.ReadWAVFile(fileName);
            canvasOs.Width = this.Width;
            canvasOs.Height = this.Height;

            EndpointDetection endpointDetection = new EndpointDetection();
            wAVReader = ZipWav(wAVReader);
            wAVReader = endpointDetection.EndpointDetectionDeal(wAVReader);
            
            Point[] points = new Point[wAVReader.stereos.Count];
            for (int i = 0; i < points.Length; i++)
            {
                double x = (i * 1.0 / 44100) * unitOfOneSecond;

                double y = TranY(wAVReader.stereos[i ].Left / heightZipCount);
                points[i] = new Point(x, y);
            }
            return points;
        }
        private Point[] Init2()
        {
            WAVReader wAVReader = new WAVReader();
            wAVReader.ReadWAVFile(fileName);
            wAVReader = ZipWav(wAVReader);
            //Complex[] complices = new Complex[frame];
            canvasOs.Width = this.Width;
            canvasOs.Height = this.Height;
            int i_0 = 0;//前面为0的部分
            while (wAVReader.stereos[i_0].Left == 0 && wAVReader.stereos[i_0].Right == 0)
            {
                i_0++;
            }

            Point[] points = new Point[wAVReader.stereos.Count-i_0];
            for (int i = 0; i < points.Length; i++)
            {
                double x = (i * 1.0 / samplingRate) * unitOfOneSecond;
    
                double y = TranY(wAVReader.stereos[i+i_0].Left / heightZipCount);
                points[i] = new Point(x, y);
            }
            return points;
        }
        private Point[] Init()
        {
            WAVReader wAVReader = new WAVReader();
            wAVReader.ReadWAVFile(fileName);
            Complex[] complices = new Complex[frame];
            

            canvasOs.Width = this.Width;
            canvasOs.Height = this.Height;

            
            int i_0 = 0;//前面为0的部分
            while(wAVReader.stereos[i_0].Left==0&& wAVReader.stereos[i_0].Right == 0)
            {
                i_0++;
            }
            for (int i = 0; i < complices.Length; i++)
            {
                complices[i] = new Complex(wAVReader.stereos[i + i_0+startNumber].Right * Hamming(i, frame), 0);
            }

            FourierTransform.DFT(complices, FourierTransform.Direction.Backward);
            double max = 0;
            Point[] points = new Point[frame];
            for (int i=0;i<points.Length;i++)
            {
                double x = (i * 1.0 / 44100) * unitOfOneSecond;
                if (complices[i].Magnitude > max)
                    max = complices[i].Magnitude;
                double y = TranY( complices[i].Magnitude / heightZipCount);
                points[i] = new Point(x,y );
            }
            return points;
        }
        /// <summary>
        /// 水平方向是X轴，竖直方向是Y轴
        /// </summary>
        public void DrawXY()
        {
            double canvasWidth = canvasOs.Width;
            double canvasHeight = canvasOs.Height;

            Line xline = new Line();
            xline.Stroke = System.Windows.Media.Brushes.LightSteelBlue;

            xline.X1 = 0;
            xline.Y1 = canvasHeight / 2;

            xline.X2 = canvasWidth;
            xline.Y2 = canvasHeight / 2;

            canvasOs.Children.Add(xline);
        }
        private void DrawMap(Point []points)
        {
            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 0.5;
            StreamGeometry streamGeometry = BuildRegularPolygon(points, false, false);
            streamGeometry.FillRule = FillRule.EvenOdd;
            streamGeometry.Freeze();
            myPath.Data = streamGeometry;
            canvasOs.Children.Add(myPath);
        }
        private StreamGeometry BuildRegularPolygon(Point[] values, bool isClosed, bool isfilled)
        {            // c is the center, r is the radius,            // numSides the number of sides, offsetDegree the offset in Degrees.            // Do not add the last point.           
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(values[0], isfilled /* is filled */, isClosed /* is closed */);
                for (int i = 1; i < values.Length; i++)
                {
                    ctx.LineTo(values[i], true /* is stroked */, false /* is smooth join */);
                }
            }
            return geometry;
        }
        private double Hamming(int index,int N)
        {
            if (index > N || index < 0)
                return 0;
            else
                return 0.54 - 0.46 * Math.Cos(2 * Math.PI * index / (N - 1));
        }
        private double TranX(double x)
        {
            return x;
        }
        private double TranY(double y)
        {
            return this.canvasOs.Height/2-y;
        }
        private Point TranPoint(double x, double y)
        {
            return new Point(x, TranY(y));
        }
        private WAVReader ZipWav(WAVReader wAVReader)
        {
            int beishu = 10;
            WAVReader newWav = new WAVReader();
            newWav.stereos = new List<Stereo>();
            
            for (int i=wAVReader.stereos.Count-1;i>=0;i-=beishu)
            {

                    newWav.stereos.Add(wAVReader.stereos[i]);
                    //wAVReader.stereos.RemoveAt(i);
            }
            return newWav;
        }

    }
}
