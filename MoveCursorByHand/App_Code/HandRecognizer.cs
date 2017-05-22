using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;

namespace MoveCursorByHand.App_Code
{
    class HandRecognizer
    {
        private Point centerPoint;
        private int maxAreaIndex = 0;
        private Matrix<int> matrix;
        private Mat defects;

        public HandRecognizer()
        {

        }

        public VectorOfPoint RecognizeHand(VectorOfVectorOfPoint contours, bool leftHandPos)
        {
            double maxArea = CvInvoke.ContourArea(contours[0]);
            maxAreaIndex = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i]) > maxArea)
                {
                    maxArea = CvInvoke.ContourArea(contours[i]);
                    maxAreaIndex = i;
                }
            }

            VectorOfPoint maxResult = new VectorOfPoint(contours[maxAreaIndex].ToArray());                      
            
            VectorOfInt hullResult = new VectorOfInt();
            if (leftHandPos)
                CvInvoke.ConvexHull(maxResult, hullResult);
            else
                CvInvoke.ConvexHull(maxResult, hullResult, true);

            if (leftHandPos)
                CvInvoke.ConvexHull(maxResult, hullResult, false, false);
            else
                CvInvoke.ConvexHull(maxResult, hullResult, true, false);

            defects = new Mat();
            CvInvoke.ConvexityDefects(maxResult, hullResult, defects);

            matrix = new Matrix<int>(defects.Rows, defects.Cols, defects.NumberOfChannels);
            defects.CopyTo(matrix);
            Matrix<int>[] channels = matrix.Split();

            return maxResult;
        }

        public Rectangle GetBoundingRectangle(VectorOfPoint maxResult)
        {
           return CvInvoke.BoundingRectangle(maxResult);
        }

        public int GetMaxAreaIndexOfContours()
        {
            return maxAreaIndex;
        }

        public Matrix<int> GetMatrixOfDefects()
        {
            return matrix;
        }

        public Mat GetDefects()
        {
            return defects;
        }

        public void DrawBoundingRectangle(ref Mat croppedFrame, Rectangle boundingRect)
        {
            CvInvoke.Rectangle(croppedFrame, boundingRect, new MCvScalar(255, 255, 255), 0);
        }

        public Point FindCenterPoint(VectorOfVectorOfPoint contours, int maxAreaIndex)
        {
            MCvMoments mcv = CvInvoke.Moments(contours[maxAreaIndex], true);
            double m00 = CvInvoke.cvGetSpatialMoment(ref mcv, 0, 0);
            double m01 = CvInvoke.cvGetSpatialMoment(ref mcv, 0, 1);
            double m10 = CvInvoke.cvGetSpatialMoment(ref mcv, 1, 0);
            int scale = 1;
            if (m00 != 0)
            {
                int xCenter = (int)Math.Round(m10 / m00) * scale;
                int yCenter = (int)Math.Round(m01 / m00) * scale;
                centerPoint = new Point(xCenter, yCenter);                
            }

            return centerPoint;
        }
    }
}
