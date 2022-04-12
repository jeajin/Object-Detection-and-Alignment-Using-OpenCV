using System;
using System.Windows.Forms;
using OpenCvSharp;

namespace OpenCV
{
    internal static class test
    {
        static void Contour(Point[][] contour, Mat src)
        {
            for (int i = 0; i < contour.Length; i++)
            {
                Cv2.DrawContours(src, contour, i, Scalar.Yellow, 3, LineTypes.AntiAlias);
                Console.WriteLine("사각형 X 축 : " + Cv2.BoundingRect(contour[i]).X + " / 사각형 Y 축 : " + Cv2.BoundingRect(contour[i]).Y
                    + " / 사각형 높이 : " + Cv2.BoundingRect(contour[i]).Height + " / 사각형 너비 " + Cv2.BoundingRect(contour[i]).Width);
            }
            Cv2.ImShow("Paint", src);
        }
        static void DrawCenterPoint(Point[][] contour, Mat src)
        {
            int cx5 = 0, cy5 = 0;
            double crossX = 0.0, crossY = 0.0;
            Point[] crossBoxPoint = new Point[contour.Length];
            for (int i = 1; i < contour.Length; i++)
            {
                Moments mmt = Cv2.Moments(contour[i]);
                double cx = mmt.M10 / mmt.M00,
                       cy = mmt.M01 / mmt.M00;
                Cv2.Circle(src, new Point(cx, cy), 3, Scalar.Red, -1, LineTypes.AntiAlias);

                crossBoxPoint[i] = new Point(cx, cy);


                // 십자가 중심점
                if (i >= 1 && i <= 4)
                {
                    crossX += cx;
                    crossY += cy;
                }
                else
                {
                    cx5 = (int)cx;
                    cy5 = (int)cy;
                }

            }
            Cv2.Circle(src, new Point(crossX / 4, crossY / 4), 3, Scalar.Green, -1, LineTypes.AntiAlias);
            Console.WriteLine("crossX : {0}, crossY : {1}", (int)crossX / 4, (int)crossY / 4);

            ///////////////// 

            double Distance = Math.Sqrt(Math.Pow((double)cx5 - (int)crossX / 4, 2) + Math.Pow(cy5 - (int)crossY / 4, 2));
            Console.WriteLine("Distance : " + Distance);

            double x = (int)crossX / 4 - cx5;
            double y = cy5 - (int)crossY / 4;
            double radian = Math.Atan2(y, x);
            double degree = radian * 180 / Math.PI;
            Console.WriteLine("radian : " + radian);
            Console.WriteLine("degree : " + (degree + 360) % 360);


            ////////////////

            Cv2.ImShow("Paint", src);
        }
        static void DrawLine(Point[][] contour, Mat src)
        {
            double crossX = 0.0, crossY = 0.0;
            Point[] crossBoxPoint = new Point[contour.Length];
            Rect rect = Cv2.BoundingRect(contour[5]);

            for (int i = 1; i < contour.Length; i++)
            {
                Moments mmt = Cv2.Moments(contour[i]);
                double cx = mmt.M10 / mmt.M00,
                       cy = mmt.M01 / mmt.M00;
                crossBoxPoint[i] = new Point(cx, cy);


                // 십자가 중심점
                if (i >= 1 && i <= 4)
                {
                    crossX += cx;
                    crossY += cy;
                }

            }

            Point[] crossLinePoint = new Point[4];
            // 각 지점의 crossLinePoint 좌표 값 (6시를 기준으로 반시계 방향이 배열 순서)
            crossLinePoint[0] = crossBoxPoint[1] + crossBoxPoint[2];
            crossLinePoint[0] = new Point(crossLinePoint[0].X / 2, crossLinePoint[0].Y / 2);
            Console.WriteLine("crossLinePoint[0] : {0}", crossLinePoint[0]);

            crossLinePoint[1] = crossBoxPoint[1] + crossBoxPoint[3];
            crossLinePoint[1] = new Point(crossLinePoint[1].X / 2, crossLinePoint[1].Y / 2);
            Console.WriteLine("crossLinePoint[1] : {0}", crossLinePoint[1]);

            crossLinePoint[2] = crossBoxPoint[3] + crossBoxPoint[4];
            crossLinePoint[2] = new Point(crossLinePoint[2].X / 2, crossLinePoint[2].Y / 2);
            Console.WriteLine("crossLinePoint[2] : {0}", crossLinePoint[2]);

            crossLinePoint[3] = crossBoxPoint[2] + crossBoxPoint[4];
            crossLinePoint[3] = new Point(crossLinePoint[3].X / 2, crossLinePoint[3].Y / 2);
            Console.WriteLine("crossLinePoint[3] : {0}", crossLinePoint[3]);

            Cv2.Line(src, crossLinePoint[1], crossLinePoint[3], Scalar.Orange, 10, LineTypes.AntiAlias);
            Cv2.Line(src, crossLinePoint[0], crossLinePoint[2], Scalar.Orange, 10, LineTypes.AntiAlias);

            Cv2.Line(src, new Point(rect.X + rect.Width / 2, rect.Y), new Point(rect.X + rect.Width / 2, rect.Y + rect.Height), Scalar.Orange, 10, LineTypes.AntiAlias);
            Cv2.Line(src, new Point(rect.X, rect.Y + rect.Height / 2), new Point(rect.X + rect.Width, rect.Y + rect.Height / 2), Scalar.Orange, 10, LineTypes.AntiAlias);

            Cv2.ImShow("Paint", src);
        }
        static void Copy()
        {
            Mat src = new Mat("./Resources/all_mask_key.png");
            Mat bin = new Mat();

            Cv2.CvtColor(src, bin, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(bin, bin, 200, 255, ThresholdTypes.Binary);

            Cv2.FindContours(bin, out Point[][] contour, out HierarchyIndex[] hierarchy,
            RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            Contour(contour, src);
            DrawLine(contour, src);
            DrawCenterPoint(contour, src);

            Mat matrix = Cv2.GetRotationMatrix2D(new Point2f(src.Width / 2, src.Height / 2), 45.0, 1.0);
            Cv2.WarpAffine(src, src, matrix, new Size(src.Width, src.Height));



            Cv2.ImShow("Paint", src);

            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }
        static void MakeCross(Mat draw, int x, int y)
        {
            Cv2.Rectangle(draw, new Point(x + 40, y), new Point(x + 60, y + 100), Scalar.Red, -1);
            Cv2.Rectangle(draw, new Point(x, y + 40), new Point(x + 100, y + 60), Scalar.Red, -1);
            //return draw;
        }

        static void MakeFourBox(Mat draw, int x, int y)
        {
            Cv2.Rectangle(draw, new Point(x, y), new Point(x + 30, y + 30), Scalar.Yellow, -1);
            Cv2.Rectangle(draw, new Point(x, y + 70), new Point(x + 30, y + 100), Scalar.Yellow, -1);
            Cv2.Rectangle(draw, new Point(x + 70, y), new Point(x + 100, y + 30), Scalar.Yellow, -1);
            Cv2.Rectangle(draw, new Point(x + 70, y + 70), new Point(x + 100, y + 100), Scalar.Yellow, -1);

        }

        static void Main()
        {
            Mat cross = new Mat(new Size(1600, 900), MatType.CV_8UC3);
            Mat crossTemp = new Mat(cross.Size(), MatType.CV_8UC3);
            Mat fourBox = new Mat(cross.Size(), MatType.CV_8UC3);
            Mat fourBoxTemp = new Mat(cross.Size(), MatType.CV_8UC3);

            // Cv2.ImShow("Drawing", draw);
            // Cv2.MoveWindow("Drawing", 100, 100);

            MakeCross(cross, 60, 100);
            MakeFourBox(fourBox, 300, 300);

            Mat matrix = Cv2.GetRotationMatrix2D(new Point2f(60, 100), 60, 1.0);
            Cv2.WarpAffine(cross, crossTemp, matrix, new Size(cross.Width, cross.Height));



            Mat matrix2 = Cv2.GetRotationMatrix2D(new Point2f(300, 300), 45.0, 1.0);
            Cv2.WarpAffine(fourBox, fourBoxTemp, matrix2, new Size(fourBox.Width, fourBox.Height));



            Mat bin = new Mat();

    


            Cv2.ImShow("Drawinsdfg", crossTemp);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();

        }

    }

}