
using System;
using System.Windows.Forms;
using OpenCvSharp;

namespace OpenCV
{
    internal static class test
    {
        const double RADIAN  = 57.295779513082320876798154814105;
//5
        // 랜덤 좌표 생성 함수
        static Point RandomValueXY(int x, int y)
        {
            Random random = new Random();
            return new Point(random.Next(x, y), random.Next(x, y));
        }

        // 랜덤한 축의 각도 생성 함수
        static int RandomValueAngle(int x)
        {
            Random random = new Random();
            return random.Next(0, x);
        }

        //6
        static Point[] AllCall(Mat board, int type, int randomValueAngle, Point randomValueXY)
        {

            // 주어진 조건에 따라 도형을 그림
            if (type == 0)
                MakeCross(board, randomValueXY.X, randomValueXY.Y);
            else if (type == 1)
                MakeFourBox(board, randomValueXY.X, randomValueXY.Y);

            // 주어진 값에 따라 도형을 회전시킴
            Spin(board, board, randomValueXY, randomValueAngle);
            // 윤곽선 검사 후, 윤곽선 생성
            Point[][] contour = Contour(board);
            // 축을 그리고 축의 끝점을 반환함
            Point[] t = DrawLine(contour, board, type);
            return t; // 끝점 위치 = t;
        }

        //7
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

        //8
        static void Spin(Mat board, Mat dst, Point randomValueXY, int randomValueAngle)
        {
            // 도형 회전, 도형의 중심을 기준으로 회전
            // 아파인 연산 수행
            Mat matrix = Cv2.GetRotationMatrix2D(new Point2f(randomValueXY.X + 50, randomValueXY.Y + 50), randomValueAngle, 1.0);
            Cv2.WarpAffine(board, dst, matrix, new Size(board.Width, board.Height));
        }

        //9
        static Point[][] Contour(Mat Board)
        {
            Mat bin = new Mat();


            //cross  contour, drawline
            // 이미지의 색깔 채널을 그레이스케일로 변환
            Cv2.CvtColor(Board, bin, ColorConversionCodes.BGR2GRAY);
            // 그레이스케일된 이미지를 흑백으로 구분하여 처리 -> 임계값에 이하면 흑색처리!=백색
            Cv2.Threshold(bin, bin, 70, 255, ThresholdTypes.Binary);
            // 이분화된 값으로 윤곽선 검사
            Cv2.FindContours(bin, out Point[][] contour, out HierarchyIndex[] hierarchy,
            RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            // 윤곽선의 개수만큼 윤곽선 그리기
            for (int i = 0; i < contour.Length; i++)
            {
                Cv2.DrawContours(Board, contour, i, Scalar.Blue, 2);
            }
            return contour;
        }

        //10
        static Point[] DrawLine(Point[][] contour, Mat src, int type)
        {
            // 윤곽선 값을 이용해서 축들과 관련된 계산을 수행하는 함수

            // 타입이 0이거나 윤곽선 개수가 하나일시 cross 이미지로 판명 후, 축 생성
            if (contour.Length == 1 || type == 0)
            {
                RotatedRect rect = Cv2.MinAreaRect(contour[0]);
                Point[] spot = new Point[4];

                for (int j = 0; j < 4; j++)
                {
                    spot[j] = new Point(rect.Points()[j].X, rect.Points()[j].Y);

                }
                Cv2.Line(src, spot[0], spot[2], Scalar.Orange, 5);
                Cv2.Line(src, spot[1], spot[3], Scalar.Orange, 5);
                return spot; // 축 값의 좌표들
            }
            else // 아니라면, fourBox로 판명후, 축 생성
            {
                double crossX = 0.0, crossY = 0.0;
                Point[] fourBoxPoint = new Point[contour.Length];

                for (int i = 0; i < contour.Length; i++)
                {
                    Moments mmt = Cv2.Moments(contour[i]);
                    double cx = mmt.M10 / mmt.M00,
                           cy = mmt.M01 / mmt.M00;
                    fourBoxPoint[i] = new Point(cx, cy);
                    Console.WriteLine("crossLinePoint[{0}]", fourBoxPoint[i]);

                    crossX += cx;
                    crossY += cy;
                }

                int[,] caculationOrder = { { 0, 1 }, { 1, 3 }, { 2, 3 }, { 2, 0 } };
                Point[] fourBoxLinePoint = GetFourBoxLinPoint(fourBoxPoint, caculationOrder);

                Cv2.Line(src, fourBoxLinePoint[1], fourBoxLinePoint[3], Scalar.Red, 10);
                Cv2.Line(src, fourBoxLinePoint[0], fourBoxLinePoint[2], Scalar.Red, 5);

                return fourBoxLinePoint; // 축 값의 좌표들
            }
        }

        static Point[] GetFourBoxLinPoint(Point[] fourBoxPoint, int[,] caculationOrder)
        {
            int caculationOrderRowSize = (int)(caculationOrder.Length / caculationOrder.Rank);
            Point[] fourBoxLinePoint = new Point[fourBoxPoint.Length];
            for (int i = 0; i < caculationOrderRowSize; i++)
            {
                fourBoxLinePoint[i] = GetCenterPoint(fourBoxPoint[caculationOrder[i,0]], fourBoxPoint[caculationOrder[i,1]]);
            }
            return fourBoxLinePoint;
        }

        static Point GetCenterPoint(Point P1, Point P2)
        {
            Point distanceBetweenTwoPoint = P1 + P2;
            return new Point(distanceBetweenTwoPoint.X / 2, distanceBetweenTwoPoint.Y / 2);
        }

        //12
        static double Theta(Point a, Point b)
        {
            // 아크탄젠트를 이용하여 각도 계산
            // 아크탄젠트는 -180~180을 반환
            double x = (int)a.X - (int)b.X;
            double y = (int)b.Y - (int)a.Y;
            // 아크탄젠트2는 0~360을 반환
            double radian = Math.Atan2(y, x);

            // 라디안을 이용한 각도 값 반환'
            // 실수는 180/Pi 이다
            return radian * RADIAN ;
        }

        //15
        static Mat move(Mat board, int x, int y)
        {   // 주어진 좌표에서 offset 만큼 이동 하여 cross 이미지 생성
            Mat temp = new Mat(board.Size(), MatType.CV_8UC3);
            Cv2.Rectangle(temp, new Point(x + 40, y) , new Point(x + 60, y + 100) , Scalar.Red, -1);
            Cv2.Rectangle(temp, new Point(x, y + 40)  , new Point(x + 100, y + 60) , Scalar.Red, -1);
            return temp;
        }
        static void Main()
        {
            // 1
            // 도형을 그릴 보드 생성
            Mat crossBoard = new Mat(new Size(1000, 800), MatType.CV_8UC3);
            Mat fourBoxBoard = new Mat(crossBoard.Size(), MatType.CV_8UC3);

            //2
            // 도형 각도 랜덤 생성
            Point randomValueXY_C = RandomValueXY(100, 700);
            int randomValueAngle_C = RandomValueAngle(360);
            // allcall로가서 값전달 (이동위치, 인자(0이들어오면 크로스를 그림), 랜덤생성된 각도, 좌표)
            Point[] crossSpot = AllCall(crossBoard, 0, randomValueAngle_C, randomValueXY_C);
            // 생성된 도형을 복사하고, 이는 도형의 축 정렬에 사용한 이미지임
            // 원본 이미지 하나만 사용(회전)하게되면 픽셀이 깨지기 때문에, 원본은 유지하되 사용된 이미지는 Temp라는 변수에 넣어 사용한다. 
            Mat crossBoardTemp = new Mat(crossBoard.Size(), MatType.CV_8UC3);
            crossBoard.CopyTo(crossBoardTemp);

            // 포박스 -> 도형각도 랜덤 생성
            Point randomValueXY_F = RandomValueXY(100, 700);
            int randomValueAngle_F = RandomValueAngle(359);
            // allcall로가서 값전달 (이동위치, 인자(1이들어오면 상자를 그림), 랜덤생성된 각도, 좌표)
            Point[] FourBoxSpot = AllCall(fourBoxBoard, 1, randomValueAngle_F, randomValueXY_F);

            //3
            //두 표식의 거리 계산
            // 축의 끝점을 이용해서 cross의 중심좌표를 구함
            Point test = crossSpot[0] + crossSpot[2];
            test = new Point(test.X / 2, test.Y / 2);

            // 축의 끝점을 이용해서 fourBox의 중심좌표를 구함
            Point ftest = FourBoxSpot[0] + FourBoxSpot[2];
            ftest = new Point(ftest.X / 2, ftest.Y / 2);

            //4
            // 구해진 중심좌표들로 cross와 fourBox의 거리를 구함
            double Distance = Math.Sqrt(Math.Pow((double)test.X - (double)ftest.X, 2)
                                      + Math.Pow((double)test.Y - (double)ftest.Y, 2));

            // Console.WriteLine("cross {0}", Distance); // 거리 확인용

            //11
            //두 표식 각도 계산
            double pointDegree = Theta(test, ftest);
            double axisDegree = (double)(Theta(crossSpot[0], crossSpot[2]) - Theta(FourBoxSpot[0], FourBoxSpot[2]));

            // Console.WriteLine("mov x: {0}, y: {1}", Math.Cos(pointDegree / 57.295779513082320876798154814105) * Distance, Math.Sin(pointDegree / 57.295779513082320876798154814105) * Distance); // 두 점의 x,y 좌표 값의 차이를 삼각함수를 이용하여 구함
            // Console.WriteLine("cross x: {0}, y: {1}     fourbox x: {2}, y: {3}", test.X, test.Y, ftest.X, ftest.Y);

            // 축을 기준으로 오른/왼쪽 각도를 계산하여 값이 작은 방향으로 회전 방향을 정함
            int direct = (90 - (axisDegree + 360) % 90) > (axisDegree + 360) % 90 ? -1 : 1;
            // 정렬 할 축을 기준으로 좌/우 중 값이 작은 각도 저장
            axisDegree = (90 - (axisDegree + 360) % 90) > (axisDegree + 360) % 90 ? (axisDegree + 360) % 90 : 90 - (axisDegree + 360) % 90;

            //13
            // cross, fourBox의 이미지를 더함
            Mat add = new Mat();
            Cv2.Add(fourBoxBoard, crossBoard, add);
            Cv2.ImShow("FPaint", add);

            // 프로그램 실행 시 도형 확인을 위한 wait 함수
            /*    if (Cv2.WaitKey(2000) == 'q')
                {
                    int a = 0;
                }*/
            Cv2.WaitKey(0);

            // axisDegree값 만큼 반복하며 1도씩 이미지 회전
            for (int i = 0; i <= (int)(axisDegree + 0.5); i++)
            {
                // 회전 방향은 direct로 설정
                Spin(crossBoard, crossBoardTemp, randomValueXY_C, direct * i);
                // 이미지 깨짐 방지를 위해 원본 이미지 유지
                Cv2.Add(fourBoxBoard, crossBoardTemp, add);
                Cv2.ImShow("FPaint", add);
                if (Cv2.WaitKey(5) == 'q') break;
            }

            //14
            Mat temp = new Mat();

            crossBoardTemp = new Mat(crossBoard.Size(), MatType.CV_8UC3);
            for (int i = 0; i <= (int)(Distance + 0.5); i++)
            {
                int newX = randomValueXY_C.X + -(int)(Math.Cos(pointDegree / RADIAN ) * i),
                    newY = randomValueXY_C.Y + (int)(Math.Sin(pointDegree / RADIAN ) * i);
                temp = move(crossBoard, newX, newY);
                Spin(temp, crossBoardTemp, new Point(newX, newY), randomValueAngle_C + (int)axisDegree * direct);
      
                DrawLine(Contour(crossBoardTemp), crossBoardTemp, 0);

                Cv2.Add(crossBoardTemp, fourBoxBoard, add);
                Cv2.ImShow("FPaint", add);
                if (Cv2.WaitKey(1) == 'q') break;
            }         

            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }
    }
}

