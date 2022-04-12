using System;
using System.Windows.Forms;
using OpenCvSharp;

namespace OpenCV
{
    internal static class test
    {
        static Point[][] Contour(Mat Board)
        {
            Mat bin = new Mat();


            //cross  contour, drawline
            Cv2.CvtColor(Board, bin, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(bin, bin, 70, 255, ThresholdTypes.Binary);
            Cv2.FindContours(bin, out Point[][] contour, out HierarchyIndex[] hierarchy,
            RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            for (int i = 0; i < contour.Length; i++)
            {
                Cv2.DrawContours(Board, contour, i, Scalar.Blue, 2);
            }
            return contour;
        }
        static Point[] DrawLine(Point[][] contour, Mat src,int type)
        {
            if (contour.Length == 1 || type == 0)
            {
                RotatedRect rect = Cv2.MinAreaRect(contour[0]);
                Point[] spot = new Point[4];
                //Cv2.ImShow("CPaint", cross);
                for (int j = 0; j < 4; j++)
                {
                    spot[j] = new Point(rect.Points()[j].X, rect.Points()[j].Y);
                    /*
                                        spot[j] = new Point(rect.Points()[j].X, rect.Points()[j].Y) + new Point(rect.Points()[(j + 1) % 4].X, rect.Points()[(j + 1) % 4].Y);
                                        spot[j] = new Point(spot[j].X / 2, spot[j].Y / 2);*/
                }
                Cv2.Line(src, spot[0], spot[2], Scalar.Orange, 5);
                Cv2.Line(src, spot[1], spot[3], Scalar.Orange, 5);
                return spot;
            }
            else // fourBox
            {
                double crossX = 0.0, crossY = 0.0;
                Point[] crossBoxPoint = new Point[contour.Length];

                for (int i = 0; i < contour.Length; i++)
                {
                    Moments mmt = Cv2.Moments(contour[i]);
                    double cx = mmt.M10 / mmt.M00,
                           cy = mmt.M01 / mmt.M00;
                    crossBoxPoint[i] = new Point(cx, cy);
                    Console.WriteLine("crossLinePoint[{0}]", crossBoxPoint[i]);

                    crossX += cx;
                    crossY += cy;
                }

                Point[] crossLinePoint = new Point[4];

                crossLinePoint[0] = crossBoxPoint[0] + crossBoxPoint[1];
                crossLinePoint[0] = new Point(crossLinePoint[0].X / 2, crossLinePoint[0].Y / 2);
                //Console.WriteLine("crossLinePoint[0] : {0}", crossLinePoint[0]);

                crossLinePoint[1] = crossBoxPoint[1] + crossBoxPoint[3];
                crossLinePoint[1] = new Point(crossLinePoint[1].X / 2, crossLinePoint[1].Y / 2);
                //Console.WriteLine("crossLinePoint[1] : {0}", crossLinePoint[1]);

                crossLinePoint[2] = crossBoxPoint[2] + crossBoxPoint[3];
                crossLinePoint[2] = new Point(crossLinePoint[2].X / 2, crossLinePoint[2].Y / 2);
                //Console.WriteLine("crossLinePoint[2] : {0}", crossLinePoint[2]);

                crossLinePoint[3] = crossBoxPoint[2] + crossBoxPoint[0];
                crossLinePoint[3] = new Point(crossLinePoint[3].X / 2, crossLinePoint[3].Y / 2);
                //Console.WriteLine("crossLinePoint[3] : {0}", crossLinePoint[3]);

                Cv2.Line(src, crossLinePoint[1], crossLinePoint[3], Scalar.Red, 5);
                Cv2.Line(src, crossLinePoint[0], crossLinePoint[2], Scalar.Red, 5);

                return crossLinePoint;
            }
        }
        static void MakeCross(Mat draw, int x, int y)
        {
            Cv2.Rectangle(draw, new Point(x + 40, y), new Point(x + 60, y + 100), Scalar.Red, -1);
            Cv2.Rectangle(draw, new Point(x, y + 40), new Point(x + 100, y + 60), Scalar.Red, -1);
        }
        static void MakeFourBox(Mat draw, int x, int y)
        {
            Cv2.Rectangle(draw, new Point(x, y), new Point(x + 30, y + 30), Scalar.Yellow, -1);
            Cv2.Rectangle(draw, new Point(x, y + 70), new Point(x + 30, y + 100), Scalar.Yellow, -1);
            Cv2.Rectangle(draw, new Point(x + 70, y), new Point(x + 100, y + 30), Scalar.Yellow, -1);
            Cv2.Rectangle(draw, new Point(x + 70, y + 70), new Point(x + 100, y + 100), Scalar.Yellow, -1);
        }
        static Point RandomValueXY(int x, int y)
        {
            Random random = new Random();
            return new Point(random.Next(x, y), random.Next(x, y));
        }
        static int RandomValueAngle(int x)
        {
            Random random = new Random();
            return random.Next(0, x);
        }
       
        static Point[] AllCall(Mat board, int type,int randomValueAngle, Point randomValueXY)
        {
            // 도형 좌표값 랜덤 생성
            
            
            Console.WriteLine("makeDegree : " + randomValueAngle);
            if (type == 0)
                MakeCross(board, randomValueXY.X, randomValueXY.Y);
            else if (type == 1)
                MakeFourBox(board, randomValueXY.X, randomValueXY.Y);
            Spin(board, board, randomValueXY, randomValueAngle);
            Point[][] contour = Contour(board);
            Point[] t = DrawLine(contour, board, type);
            return t;
        }
        static double Theta(Point a, Point b)
        {
            double x = (int)a.X - (int)b.X;
            double y = (int)b.Y - (int)a.Y;
            double radian = Math.Atan2(y, x);


            return radian * 57.295779513082320876798154814105;
        }
        static void Spin(Mat board,Mat dst ,Point randomValueXY, int randomValueAngle)
        {
            // 표식 회전
            Mat matrix = Cv2.GetRotationMatrix2D(new Point2f(randomValueXY.X + 50, randomValueXY.Y + 50), randomValueAngle, 1.0);
            Cv2.WarpAffine(board, dst, matrix, new Size(board.Width, board.Height));
        }
        static Mat move(Mat board,int x,int y, Point offset)
        {   Mat temp = new Mat(board.Size(), MatType.CV_8UC3);
            Cv2.Rectangle(temp, new Point(x + 40, y)+ offset, new Point(x + 60, y + 100) + offset, Scalar.Red, -1) ;
            Cv2.Rectangle(temp, new Point(x, y + 40) + offset, new Point(x + 100, y + 60) + offset, Scalar.Red, -1);
            return temp;
        }
        static void Main()
        {  
            // 도형을 그릴 보드 생성
            Mat crossBoard = new Mat(new Size(1000, 800), MatType.CV_8UC3);
            Mat fourBoxBoard = new Mat(crossBoard.Size(), MatType.CV_8UC3);

            // 도형 각도 랜덤 생성
            Point randomValueXY_C = RandomValueXY(100, 700);
            
            int randomValueAngle_C = RandomValueAngle(360);
            Point[] crossSpot = AllCall(crossBoard, 0, randomValueAngle_C, randomValueXY_C);
            Mat crossBoardTemp = new Mat(crossBoard.Size(), MatType.CV_8UC3);
            crossBoard.CopyTo(crossBoardTemp);
          
            Point randomValueXY_F = RandomValueXY(100, 700);
            int randomValueAngle_F = RandomValueAngle(359);
            Point[] FourBoxSpot = AllCall(fourBoxBoard, 1, randomValueAngle_F, randomValueXY_F);

            //두 표식의 거리 계산
            Point test = crossSpot[0] + crossSpot[2];
            test = new Point(test.X / 2, test.Y / 2);

            Point ftest = FourBoxSpot[0] + FourBoxSpot[2];
            ftest = new Point(ftest.X / 2, ftest.Y / 2);

            double Distance = Math.Sqrt(Math.Pow((double)test.X - (double)ftest.X, 2)
                                      + Math.Pow((double)test.Y - (double)ftest.Y, 2));

            Console.WriteLine("cross {0}", Distance);

            //두 표식 각도 계산

            double pointDegree = Theta(test, ftest);
            double axisDegree = (double)(Theta(crossSpot[0], crossSpot[2]) - Theta(FourBoxSpot[0], FourBoxSpot[2]));

            Console.WriteLine("mov x: {0}, y: {1}", Math.Cos(pointDegree/ 57.295779513082320876798154814105) * Distance, Math.Sin(pointDegree/ 57.295779513082320876798154814105) * Distance);

            

            Console.WriteLine("cross x: {0}, y: {1}     fourbox x: {2}, y: {3}", test.X, test.Y, ftest.X, ftest.Y);

            int direct = (90 - (axisDegree + 360) % 90) > (axisDegree + 360) % 90 ? -1 : 1;
            axisDegree = (90 - (axisDegree + 360) % 90) > (axisDegree + 360) % 90 ? (axisDegree + 360) % 90 : 90 - (axisDegree + 360) % 90;
    
            Console.WriteLine("pointDegree : " + (pointDegree + 360) % 360);
            Console.WriteLine("axisDegree : {0}" , axisDegree);


            //  Cv2.ImShow("FPaint", fourBox);
            Mat add = new Mat();
            Cv2.Add(fourBoxBoard, crossBoard, add);
            Cv2.ImShow("FPaint", add);
            if (Cv2.WaitKey(20) == 'q') 
            { int a = 0; }

            for (int i = 0; i <= (int)(axisDegree+0.5); i++)
            {
                Spin(crossBoard, crossBoardTemp, randomValueXY_C, direct * i);
                Cv2.Add(fourBoxBoard, crossBoardTemp, add);
                Cv2.ImShow("FPaint", add);
                if (Cv2.WaitKey(5) == 'q') break;
            }
            Mat temp = new Mat();
            for (int i = 0; i <= (int)(Distance + 0.5); i++)
            {
                
                temp= move(crossBoard,randomValueXY_C.X, randomValueXY_C.Y, new Point(-(int)(Math.Cos(pointDegree / 57.295779513082320876798154814105) * i), (int)(Math.Sin(pointDegree / 57.295779513082320876798154814105) * i)));
              
                //move(board, dst, ofur, crossBoardTemp, 0);
                // AllCall(crossBoardTemp, 0, 0, randomValueXY_C+new Point(Math.Cos(pointDegree / 57.295779513082320876798154814105)*i, Math.Sin(pointDegree / 57.295779513082320876798154814105) * i));
                Cv2.Add(temp,fourBoxBoard, add);
                Cv2.ImShow("FPaint", add);
                if (Cv2.WaitKey(1) == 'q') break;
            }
            Spin(temp, temp, randomValueXY_F, randomValueAngle_C+(int)axisDegree* direct);



            Mat bin = new Mat();


            //cross  contour, drawline
            Cv2.CvtColor(temp, bin, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(bin, bin, 70, 255, ThresholdTypes.Binary);
            Cv2.FindContours(bin, out Point[][] contour, out HierarchyIndex[] hierarchy,
            RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            for (int i = 0; i < contour.Length; i++)
            {
                Cv2.DrawContours(temp, contour, i, Scalar.Blue, 2);
            }

            Cv2.Add(temp, fourBoxBoard, add);
            Cv2.ImShow("FPaint", add);

            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }

    }

}