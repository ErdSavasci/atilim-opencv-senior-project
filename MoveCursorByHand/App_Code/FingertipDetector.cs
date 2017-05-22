using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MoveCursorByHand.App_Code
{
    class FingertipDetector
    {
        private List<Point> fingertipCoordinates;

        public FingertipDetector()
        {
            fingertipCoordinates = new List<Point>();
        }

        public List<Point> DetectFingers(Mat defects, Matrix<int> matrix, VectorOfPoint maxResult, ref int count_defects, int handPalmClosedCount,
            bool leftHandPos, ref Mat croppedFrame)
        {
            fingertipCoordinates.Clear();

            for (int i = 0; i < defects.Rows; i++)
            {
                int s = 0, e = 0, f = 0, d = 0;

                Point start, end, far;
                if (i < matrix.Data.Length)
                {
                    s = matrix.Data[i, 0]; //Start Point Index
                    e = matrix.Data[i, 1]; //End Point Index
                    f = matrix.Data[i, 2]; //Farthest Point Index
                    d = matrix.Data[i, 3]; //Approximate Distance to Farthest Point Index
                }
                if (s < maxResult.Size)
                    start = maxResult[s]; //Start Point
                else
                    start = maxResult[maxResult.Size - 1];
                if (e < maxResult.Size)
                    end = maxResult[e]; //End Point
                else
                    end = maxResult[maxResult.Size - 1];
                if (f < maxResult.Size)
                    far = maxResult[f]; //Farthest Point
                else
                    far = maxResult[maxResult.Size - 1];
                double a = Math.Sqrt(Math.Pow((end.X - start.X), 2) + Math.Pow((end.Y - start.Y), 2));
                double b = Math.Sqrt(Math.Pow((far.X - start.X), 2) + Math.Pow((far.Y - start.Y), 2));
                double c = Math.Sqrt(Math.Pow((end.X - far.X), 2) + Math.Pow((end.Y - far.Y), 2));
                double angle = Math.Acos((Math.Pow(b, 2) + Math.Pow(c, 2) - Math.Pow(a, 2)) / (2 * b * c)) * 57;
                if (angle <= 90)
                {
                    count_defects = Math.Min(5, ++count_defects); //Detected Finger Numbers

                    if (handPalmClosedCount > 0)
                    {
                        count_defects = 0;
                    }

                    if (count_defects > 0 && fingertipCoordinates.Count < 5)
                    {
                        if (!leftHandPos)
                        {
                            CvInvoke.Circle(croppedFrame, start, 1, new MCvScalar(0, 0, 255), 10);
                            fingertipCoordinates.Add(start);
                        }
                        else
                        {
                            CvInvoke.Circle(croppedFrame, end, 1, new MCvScalar(0, 0, 255), 10);
                            fingertipCoordinates.Add(end);
                        }
                    }
                }
                /*CvInvoke.Line(croppedFrame, start, end, new MCvScalar(0, 255, 0), 2);*/

                /*if (fingertipCoordinates.Count > 0 && fingertipCoordinates.Select(g => g.X).Max() - fingertipCoordinates.Select(g => g.X).Min() < frameWidth &&
                           fingertipCoordinates.Select(g => g.Y).Max() - fingertipCoordinates.Select(g => g.Y).Min() < frameWidth) //THRESHOLD VALUE: 350 (MAX DISTANCE BETWEEN FINGERS)
                {
                    dontDraw = false;
                }*/             
            }

            return fingertipCoordinates;
        }       
    }
}
