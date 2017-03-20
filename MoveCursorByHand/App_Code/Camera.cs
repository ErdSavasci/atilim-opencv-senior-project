using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Drawing;
using MaterialSkin.Controls;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;
using System.Threading;
using System.Diagnostics;
using Emgu.CV.VideoSurveillance;

namespace MoveCursorByHand.App_Code
{
    public class Camera
    {
        #region Camera Variables
        private VideoCapture capture = null;
        private bool captureInProgress = false;
        private double width = 0.0;
        private double height = 0.0;
        private Mat frame = null;
        private Mat downFrame = null;
        private Mat grayFrame = null;
        private Mat grayCroppedFrame = null;
        private Mat smallGrayFrame = null;
        private Mat smoothedGrayFrame = null;
        private Mat cannyFrame = null;
        private ImageBox captureImageBox = null;
        private CascadeClassifier haarCascade = null;
        private int timeDelay = 0;
        private int pastTime = 0;
        private bool activate = false, isActivated = false;
        private bool leftHandPos = false;
        private int x1 = -1, x2 = -1, y1 = -1, y2 = -1;
        private Mat roi_hist;
        private Mat hsv_roi;
        private Mat mask;
        private Mat backProject;
        private Mat hsv;
        private Rectangle trackWindow;
        private int toss = 0;
        private int handPalmClosedCount = 0;
        private int count_defects = 0;
        private List<Point> fingertipCoordinates;
        private List<Finger> fingertipNames;
        private Thread drawThread = null;
        private int analyzeRectYPos = 0;
        private bool isAscending = true;
        private int backgroundFrame = 500;
        private BackgroundSubtractorMOG2 bg;
        private bool thumbFound = false, indexFound = false;
        #endregion

        public Camera(ImageBox captureImageBox, DsDevice systemCamera)
        {
            this.captureImageBox = captureImageBox;
            CvInvoke.UseOpenCL = true;
            try
            {
                //Initializing new OpenCv camera instance and assigning an event after it to trigger a function when frame is being captured
                capture = new VideoCapture();
                capture.ImageGrabbed += ProcessFrame;

                //Gets all available resolutions that the default camera has and set the max resolution
                List<Resolution> availableResolutions = new List<Resolution>();
                availableResolutions = GetAllAvailableResolutions(systemCamera);
                if (availableResolutions != null)
                {
                    capture.SetCaptureProperty(CapProp.FrameWidth, availableResolutions.ElementAt(availableResolutions.Count - 1).getWidth());
                    capture.SetCaptureProperty(CapProp.FrameHeight, availableResolutions.ElementAt(availableResolutions.Count - 1).getHeight());
                }

                //Gets current width and height
                width = capture.GetCaptureProperty(CapProp.FrameWidth);
                height = capture.GetCaptureProperty(CapProp.FrameHeight);

                //capture.SetCaptureProperty(CapProp.Brightness, 150.0);

                //Different Frame objects for different purposes
                frame = new Mat();
                downFrame = new Mat();
                grayCroppedFrame = new Mat();
                smallGrayFrame = new Mat();
                smoothedGrayFrame = new Mat();
                cannyFrame = new Mat();
                grayFrame = new Mat();

                roi_hist = new Mat();
                hsv_roi = new Mat();
                mask = new Mat();
                backProject = new Mat();
                hsv = new Mat();
                trackWindow = new Rectangle();
                bg = new BackgroundSubtractorMOG2(500, 3, false);

                fingertipCoordinates = new List<Point>();
                fingertipNames = new List<Finger>();

                //Flips the video capture device in horizontal axis
                capture.FlipHorizontal = true;

                string system = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.LastIndexOf("\\"));
                system = system.Substring(0, system.LastIndexOf("\\"));
                system = system.Substring(0, system.LastIndexOf("\\"));
                haarCascade = new CascadeClassifier(system + "\\Resources\\aGest.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void changeImageBox(ImageBox captureImageBox)
        {
            this.captureImageBox = captureImageBox;
        }

        public List<Resolution> GetAllAvailableResolutions(DsDevice device)
        {
            try
            {
                int hr = 0;
                int max = 0;
                int bitCount = 0;

                IBaseFilter baseFilter = null;
                IFilterGraph2 filterGraph2 = new FilterGraph() as IFilterGraph2;
                hr = filterGraph2.AddSourceFilterForMoniker(device.Mon, null, device.Name, out baseFilter);
                IPin pRaw = DsFindPin.ByCategory(baseFilter, PinCategory.Capture, 0);

                List<Resolution> availableResolutions = new List<Resolution>();

                VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
                IEnumMediaTypes mediaTypesEnum;
                hr = pRaw.EnumMediaTypes(out mediaTypesEnum);

                AMMediaType[] mediaTypes = new AMMediaType[1];
                IntPtr fetched = IntPtr.Zero;
                hr = mediaTypesEnum.Next(1, mediaTypes, fetched);

                while (fetched != null && mediaTypes[0] != null)
                {
                    Marshal.PtrToStructure(mediaTypes[0].formatPtr, videoInfoHeader);
                    if (videoInfoHeader.BmiHeader.Size != 0 && videoInfoHeader.BmiHeader.BitCount != 0)
                    {
                        if (videoInfoHeader.BmiHeader.BitCount > bitCount)
                        {
                            availableResolutions.Clear();
                            max = 0;
                            bitCount = videoInfoHeader.BmiHeader.BitCount;
                        }

                        availableResolutions.Add(new Resolution(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height));
                        if (videoInfoHeader.BmiHeader.Width > max || videoInfoHeader.BmiHeader.Height > max)
                        {
                            max = Math.Max(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
                        }
                    }
                    hr = mediaTypesEnum.Next(1, mediaTypes, fetched);
                }

                return availableResolutions;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void ChangeHandPosition(string pos)
        {
            if (pos.Equals("Left"))
            {
                leftHandPos = true;
            }
            else
            {
                leftHandPos = false;
            }
        }

        private void MouseMovement(int x1, int x2, int y1, int y2, Mat frame)
        {
            Point initialCursorPosition;
            Native.GetCursorPos(out initialCursorPosition);
            ScreenProperties screenProperties = new ScreenProperties();

            double mouseSensitivity = 0.5; //0 < mouseSensitivity < 1
            double resolutionDiff = Math.Min((double)screenProperties.getWidth() / frame.Width, (double)screenProperties.getHeight() / frame.Height);

            initialCursorPosition.X = Math.Min(Math.Max(0, initialCursorPosition.X + (int)(((x2 - x1) * resolutionDiff) * (mouseSensitivity * 2.3))), screenProperties.getWidth());
            initialCursorPosition.Y = Math.Min(Math.Max(0, initialCursorPosition.Y + (int)(((y2 - y1) * resolutionDiff) * (mouseSensitivity * 2.3))), screenProperties.getHeight());

            Native.SetCursorPos(initialCursorPosition.X, initialCursorPosition.Y);
        }

        private int CalcTilt(double m11, double m20, double m02)
        {
            double diff = m20 - m02;
            if (diff == 0)
            {
                if (m11 == 0)
                {
                    return 0;
                }
                else if (m11 > 0)
                {
                    return 45;
                }
                else
                {
                    return -45;
                }
            }

            double theta = 0.5 * Math.Atan2(2 * m11, diff);
            int tilt = (int)Math.Round(theta * (180.0 / Math.PI));

            if (m11 == 0)
            {
                if (diff > 0 || diff < 0)
                {
                    return 0;
                }
            }
            else if (m11 > 0)
            {
                if (diff > 0 || diff < 0)
                {
                    return tilt;
                }
            }
            else if (m11 < 0)
            {
                if (diff > 0 || diff < 0)
                {
                    return 180 + tilt;
                }
            }

            return 0;
        }

        private void NameFingers(Mat frame, Point centerPoint, int contourAxisAngle, ref List<Point> fingertipCoordinates)
        {
            fingertipNames.Clear();
            for (int i = 0; i < 5; i++)
            {
                fingertipNames.Add(Finger.UNKNOWN);
            }

            LabelThumbIndex(fingertipCoordinates, ref fingertipNames, centerPoint, contourAxisAngle);
            labelUnknownFingertips(ref fingertipNames);
            if (leftHandPos)
                fingertipNames.Reverse();
            DrawFingertipNames(frame, fingertipCoordinates, fingertipNames);
        }

        private void LabelThumbIndex(List<Point> fingertipCoordinates, ref List<Finger> fingertipNames, Point centerPoint, int contourAxisAngle)
        {
            indexFound = false;
            thumbFound = false;
            int i = Math.Min(fingertipCoordinates.Count - 1, 4);
            while (i >= 0)
            {
                int xOffset = fingertipCoordinates[i].X - centerPoint.X;
                int yOffset = centerPoint.Y - fingertipCoordinates[i].Y;
                double theta = Math.Atan2(yOffset, xOffset);
                int angleTips = (int)Math.Round(theta * (180.0 / Math.PI));
                int angle = angleTips + (90 - contourAxisAngle);
                angle = Math.Abs(angle);

                //Console.WriteLine("Angle: " + angle);

                if (!leftHandPos)
                {
                    if (count_defects == 5)
                    {
                        if (angle <= 130 && angle > 60 && !indexFound)
                        {
                            indexFound = true;
                            fingertipNames[i] = Finger.INDEX;

                            //Console.WriteLine("Index Angle: " + angle);
                        }

                        if (angle <= 200 && angle > 120 && !thumbFound)
                        {
                            thumbFound = true;
                            fingertipNames[i] = Finger.THUMB;

                            //Console.WriteLine("Thumb Angle: " + angle);
                        }
                    }
                    else
                    {
                        if (angle <= 150 && angle > 120 && !indexFound)
                        {
                            indexFound = true;
                            fingertipNames[i] = Finger.INDEX;

                            //Console.WriteLine("Index Angle: " + angle);
                        }

                        else if (angle <= 200 && angle > 150 && !thumbFound)
                        {
                            thumbFound = true;
                            fingertipNames[i] = Finger.THUMB;

                            //Console.WriteLine("Thumb Angle: " + angle);
                        }
                    }
                }
                else
                {
                    if (count_defects == 5)
                    {
                        if (angle <= 130 && angle > 60 && !indexFound)
                        {
                            indexFound = true;
                            fingertipNames[i] = Finger.INDEX;

                            //Console.WriteLine("Index Angle: " + angle);
                        }

                        if (angle <= 200 && angle > 120 && !thumbFound)
                        {
                            thumbFound = true;
                            fingertipNames[i] = Finger.THUMB;

                            //Console.WriteLine("Thumb Angle: " + angle);
                        }
                    }
                    else
                    {
                        if ((angle <= 150 && angle > 50) && !indexFound && (i == 3 || i == 1 || i == 0))
                        {
                            indexFound = true;
                            fingertipNames[i] = Finger.INDEX;

                            //Console.WriteLine("Index Angle: " + angle);
                        }

                        else if (angle <= 200 && angle > 150 && !thumbFound && (i == 4 || i == 0))
                        {
                            thumbFound = true;
                            fingertipNames[i] = Finger.THUMB;

                            //Console.WriteLine("Thumb Angle: " + angle);
                        }
                    }
                }


                i--;
            }
        }

        private void labelUnknownFingertips(ref List<Finger> fingertipNames)
        {
            int i = 0;
            while (i < fingertipNames.Count && fingertipNames[i] == Finger.UNKNOWN)
            {
                i++;
            }
            if (i == fingertipNames.Count)
            {
                return;
            }

            Finger finger = fingertipNames[i];
            LabelUnknownFingertipsByOrder(ref fingertipNames, i, finger, "forward");
            if (count_defects == 5) //WHEN NO UNKNOWNS FOUND
                LabelUnknownFingertipsByOrder(ref fingertipNames, i, finger, "backward");
        }

        private void LabelUnknownFingertipsByOrder(ref List<Finger> fingertipNames, int i, Finger finger, string order)
        {
            if (order.Equals("backward") && i > 0)
            {
                i--;
                while (i >= 0 && finger != Finger.UNKNOWN)
                {
                    if (fingertipNames[i] == Finger.UNKNOWN)
                    {
                        finger = finger.getPrevious();

                        if (!fingertipNames.Contains(finger))
                        {
                            fingertipNames[i] = finger;
                        }
                        else
                        {
                            int containedIndex = fingertipNames.IndexOf(finger);
                            fingertipNames[containedIndex] = Finger.UNKNOWN;
                            fingertipNames[i] = finger;
                        }
                    }
                    else
                    {
                        finger = fingertipNames[i];
                    }

                    i--;
                }
            }
            else if (i < fingertipNames.Count - 1)
            {
                i++;
                while (i < fingertipNames.Count && finger != Finger.UNKNOWN)
                {
                    if (fingertipNames[i] == Finger.UNKNOWN)
                    {
                        finger = finger.getNext();

                        if (!fingertipNames.Contains(finger))
                        {
                            fingertipNames[i] = finger;
                        }
                        else
                        {
                            int containedIndex = fingertipNames.IndexOf(finger);
                            fingertipNames[containedIndex] = Finger.UNKNOWN;
                            fingertipNames[i] = finger;
                        }
                    }
                    else
                    {
                        finger = fingertipNames[i];
                    }

                    i++;
                }
            }
        }

        private void DrawFingertipNames(Mat frame, List<Point> fingertipCoordinates, List<Finger> fingertipNames)
        {
            int rev = Math.Min(fingertipCoordinates.Count, 5);
            if (rev == 5 || !leftHandPos) //NO UNKNOWN FINGERS
            {
                for (int i = 0; i < Math.Min(fingertipCoordinates.Count, 5); i++)
                {
                    CvInvoke.PutText(frame, fingertipNames[i].ToString(), fingertipCoordinates[i], FontFace.HersheySimplex, 1.3, new MCvScalar(255, 255, 255));
                }
            }
            else
            {
                int diff = fingertipNames.Count - Math.Min(fingertipCoordinates.Count, 5);
                int skippedIndex = 0;

                for (int i = 4; (i - diff) >= 0; i--)
                {
                    if (fingertipNames[i] != Finger.UNKNOWN)
                        CvInvoke.PutText(frame, fingertipNames[i].ToString(), fingertipCoordinates[i - diff + skippedIndex], FontFace.HersheySimplex, 1.3, new MCvScalar(255, 255, 255));
                    else
                        skippedIndex++;
                }
                for (int i = skippedIndex - 1; i >= 0; i--)
                {
                    if (fingertipNames[i] != Finger.UNKNOWN)
                        CvInvoke.PutText(frame, fingertipNames[i + (diff - 1)].ToString(), fingertipCoordinates[i], FontFace.HersheySimplex, 1.3, new MCvScalar(255, 255, 255));
                }
            }
        }

        private void DrawAnalyzeRectangle(Mat frame)
        {
            CvInvoke.Rectangle(frame, new Rectangle(0, analyzeRectYPos, frame.Width, 7), new MCvScalar(0, 0, 255), 2);
            if (analyzeRectYPos < frame.Height && isAscending)
                analyzeRectYPos = analyzeRectYPos + 10;
            else if (!isAscending)
            {
                analyzeRectYPos = analyzeRectYPos - 10;
            }

            if (analyzeRectYPos >= frame.Height)
            {
                isAscending = false;
            }
            else if (analyzeRectYPos <= 0)
            {
                isAscending = true;
            }
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            //frame -> croppedFrame -> grayCroppedFrame

            toss = (toss + 1) % 5;
            fingertipCoordinates.Clear();
            if (capture != null && capture.Ptr != IntPtr.Zero)
            {
                capture.Retrieve(frame, 0);
                CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);
                CvInvoke.PyrDown(frame, downFrame);
                Mat croppedFrame = new Mat();

                if (leftHandPos)
                {
                    if (!isActivated)
                    {
                        CvInvoke.Rectangle(frame, new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, frame.Width / 2), new MCvScalar(255, 255, 0), 2);
                        croppedFrame = new Mat(frame, new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, frame.Width / 2));

                        trackWindow = new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, frame.Width / 2);
                        CvInvoke.CvtColor(croppedFrame, hsv_roi, ColorConversion.Bgr2Hsv);
                        CvInvoke.InRange(hsv_roi, new VectorOfInt(new int[] { 0, 60, 32 }), new VectorOfInt(new int[] { 180, 255, 255 }), mask);
                        CvInvoke.CalcHist(new VectorOfMat(new Mat[] { hsv_roi }), new int[] { 0 }, mask, roi_hist, new int[] { 180 }, new float[] { 0, 180 }, true);
                        CvInvoke.Normalize(roi_hist, roi_hist, 0, 255, NormType.MinMax);
                    }
                    else
                    {
                        croppedFrame = new Mat(frame, new Rectangle(0, 0, frame.Width, frame.Height));

                        CvInvoke.CvtColor(croppedFrame, hsv, ColorConversion.Bgr2Hsv);
                        CvInvoke.CalcBackProject(new VectorOfMat(new Mat[] { hsv }), new int[] { 0 }, roi_hist, backProject, new float[] { 0, 180 }, 1);
                        CvInvoke.CamShift(backProject, ref trackWindow, new MCvTermCriteria(10, 1));
                        CvInvoke.Rectangle(croppedFrame, trackWindow, new MCvScalar(255, 0, 0), 2);
                    }
                }
                else
                {
                    if (!isActivated)
                    {
                        CvInvoke.Rectangle(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, frame.Width / 2), new MCvScalar(255, 255, 0), 2);
                        croppedFrame = new Mat(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, frame.Width / 2));

                        trackWindow = new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, frame.Width / 2);
                        CvInvoke.CvtColor(croppedFrame, hsv_roi, ColorConversion.Bgr2Hsv);
                        CvInvoke.InRange(hsv_roi, new VectorOfInt(new int[] { 0, 60, 32 }), new VectorOfInt(new int[] { 180, 255, 255 }), mask);
                        CvInvoke.CalcHist(new VectorOfMat(new Mat[] { hsv_roi }), new int[] { 0 }, mask, roi_hist, new int[] { 180 }, new float[] { 0, 180 }, true);
                        CvInvoke.Normalize(roi_hist, roi_hist, 0, 255, NormType.MinMax);
                    }
                    else
                    {
                        CvInvoke.Rectangle(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, frame.Width / 2), new MCvScalar(255, 255, 0), 2);
                        croppedFrame = new Mat(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, frame.Width / 2));

                        CvInvoke.CvtColor(croppedFrame, hsv, ColorConversion.Bgr2Hsv);
                        CvInvoke.CalcBackProject(new VectorOfMat(new Mat[] { hsv }), new int[] { 0 }, roi_hist, backProject, new float[] { 0, 180 }, 1);
                        CvInvoke.CamShift(backProject, ref trackWindow, new MCvTermCriteria(10, 1));
                        CvInvoke.Rectangle(croppedFrame, trackWindow, new MCvScalar(255, 0, 0), 2);
                    }
                }

                /*
                CvInvoke.CvtColor(croppedFrame, croppedFrame, ColorConversion.Bgr2Hsv);
                Mat hue = new Mat(croppedFrame.Size, croppedFrame.Depth, 1);
                CvInvoke.MixChannels(croppedFrame, hue, new int[] { 0, 0 });
                CvInvoke.CalcHist(new VectorOfMat(new Mat[] { hue }), new int[] { 1 }, mask, roi_hist, new int[] { 180 }, new float[] { 0, 180 }, true);
                CvInvoke.Normalize(roi_hist, roi_hist, 0, 255, NormType.MinMax);
                CvInvoke.CalcBackProject();*/

                //croppedFrame = new Mat(frame, new Rectangle(30, 30, frame.Width - 300, frame.Height - 300));

                CvInvoke.CvtColor(croppedFrame, grayCroppedFrame, ColorConversion.Bgr2Gray);

                /*
                CvInvoke.Threshold(grayCroppedFrame, grayCroppedFrame, 60, 255, ThresholdType.Binary);
                Matrix<byte> kernel = new Matrix<byte>(3, 3, 1);
                CvInvoke.MorphologyEx(grayCroppedFrame, grayCroppedFrame, MorphOp.Erode, kernel, new Point(-1, -1), 3, BorderType.Default, new MCvScalar());
                kernel = new Matrix<byte>(7, 7, 1);
                CvInvoke.MorphologyEx(grayCroppedFrame, grayCroppedFrame, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                kernel = new Matrix<byte>(9, 9, 1);
                CvInvoke.MorphologyEx(grayCroppedFrame, grayCroppedFrame, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                CvInvoke.MedianBlur(grayCroppedFrame, grayCroppedFrame, 15);
                */

                //CvInvoke.EqualizeHist(grayCroppedFrame, grayCroppedFrame);

                //CvInvoke.Normalize(grayCroppedFrame, grayCroppedFrame);
                //CvInvoke.PyrDown(grayFrame, smallGrayFrame);
                //CvInvoke.PyrUp(smallGrayFrame, smoothedGrayFrame);
                //CvInvoke.Canny(smoothedGrayFrame, cannyFrame, 100, 60);

                if (!isActivated)
                    DrawAnalyzeRectangle(croppedFrame);

                Mat detectHandPalmClosedFrame = new Mat();
                detectHandPalmClosedFrame = grayCroppedFrame.Clone();

                /*if (toss == 4 && handPalmClosedCount == 0)
                {
                    CvInvoke.Flip(detectHandPalmClosedFrame, detectHandPalmClosedFrame, FlipType.Horizontal);
                }
                else if (handPalmClosedCount > 0)
                {
                    CvInvoke.Flip(detectHandPalmClosedFrame, detectHandPalmClosedFrame, FlipType.Horizontal);
                }*/

                if (!leftHandPos)
                {
                    CvInvoke.Flip(detectHandPalmClosedFrame, detectHandPalmClosedFrame, FlipType.Horizontal);
                }

                Rectangle[] detectedHaarCascadeRectangles = haarCascade.DetectMultiScale(detectHandPalmClosedFrame, 1.4, 4);
                handPalmClosedCount = detectedHaarCascadeRectangles.Count();

                if (handPalmClosedCount > 0)
                {
                    if (x1 == -1 && y1 == -1)
                    {
                        x1 = detectedHaarCascadeRectangles.First().X;
                        y1 = detectedHaarCascadeRectangles.First().Y;
                    }
                    else if (x2 != x1 || y2 != y1)
                    {
                        x2 = detectedHaarCascadeRectangles.First().X;
                        y2 = detectedHaarCascadeRectangles.First().Y;

                        if (isActivated)
                            MouseMovement(x1, x2, y1, y2, frame);
                    }

                    Rectangle detectedRect = detectedHaarCascadeRectangles.First();
                    //detectedRect.X = Math.Abs(croppedFrame.Width - (detectedRect.X * 2));

                    //CvInvoke.Rectangle(croppedFrame, detectedRect, new MCvScalar(255, 0, 0), 2);
                    Console.WriteLine(handPalmClosedCount + " Closed Hand Found!!");
                }

                CvInvoke.GaussianBlur(grayCroppedFrame, grayCroppedFrame, new Size(35, 35), 0);
                CvInvoke.Threshold(grayCroppedFrame, grayCroppedFrame, 70, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);

                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat hierarchy = new Mat();
                CvInvoke.FindContours(grayCroppedFrame.Clone(), contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxNone);
                double maxArea = CvInvoke.ContourArea(contours[0]);
                int maxAreaIndex = 0;
                for (int i = 0; i < contours.Size; i++)
                {
                    if (CvInvoke.ContourArea(contours[i]) > maxArea)
                    {
                        maxArea = CvInvoke.ContourArea(contours[i]);
                        maxAreaIndex = i;
                    }
                }
                VectorOfPoint maxResult = new VectorOfPoint(contours[maxAreaIndex].ToArray());
                //CvInvoke.Max(contours, contours[0], maxResult);
                Rectangle boundingRect = CvInvoke.BoundingRectangle(maxResult);
                CvInvoke.Rectangle(croppedFrame, boundingRect, new MCvScalar(255, 255, 255), 0);
                VectorOfInt hullResult = new VectorOfInt();
                if (leftHandPos)
                    CvInvoke.ConvexHull(maxResult, hullResult);
                else
                    CvInvoke.ConvexHull(maxResult, hullResult, true);

                Mat drawingFrame = new Mat(croppedFrame.Rows, croppedFrame.Cols, DepthType.Cv8U, 1);
                VectorOfVectorOfPoint newMaxResult = new VectorOfVectorOfPoint(maxResult);
                //CvInvoke.DrawContours(croppedFrame, newMaxResult, 0, new MCvScalar(0, 255, 0), 0);
                //CvInvoke.DrawContours(drawingFrame, hullResult, 0, new MCvScalar(0, 0, 255), 0);
                //CvInvoke.Polylines(croppedFrame, newMaxResult, true, new MCvScalar(0, 255, 0), 1, LineType.AntiAlias);

                //Image<Gray, byte> drawing = new Image<Gray, byte>(smallGrayFrame.Rows, smallGrayFrame.Cols);//new Mat(smallGrayFrame.Rows, smallGrayFrame.Cols, DepthType.Cv8U, 0);
                //drawing.Draw(hullResult.ToArray(), new Gray(0), 1); //

                //croppedFrame.ToImage<Gray, byte>().Draw(maxResult.ToArray(), new Gray(0), 1); //                

                if (leftHandPos)
                    CvInvoke.ConvexHull(maxResult, hullResult, false, false);
                else
                    CvInvoke.ConvexHull(maxResult, hullResult, true, false);

                Mat defects = new Mat();
                CvInvoke.ConvexityDefects(maxResult, hullResult, defects);
                count_defects = 0;

                Matrix<int> matrix = new Matrix<int>(defects.Rows, defects.Cols, defects.NumberOfChannels);
                defects.CopyTo(matrix);
                Matrix<int>[] channels = matrix.Split();

                //TO FIND THE CENTER OF THE CONTOUR-------------------------------------------------------------
                Point centerPoint = new Point();
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
                    CvInvoke.Circle(croppedFrame, centerPoint, 1, new MCvScalar(0, 0, 255), 10);
                    CvInvoke.Circle(croppedFrame, centerPoint, boundingRect.Width / 4, new MCvScalar(0, 255, 0), 3);
                }
                //----------------------------------------------------------------------------------------------                

                CvInvoke.DrawContours(grayCroppedFrame, contours, -1, new MCvScalar(0, 255, 0), 3);
                int maxY = matrix.Data[0, 1];
                for (int i = 0; i < defects.Rows; i++)
                {
                    int s = 0, e = 0, f = 0, d = 0;

                    if (matrix.Data[i, 1] > maxY)
                    {
                        maxY = matrix.Data[i, 1];
                        //Console.WriteLine(maxY);
                    }

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

                        if (count_defects > 0)
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
                    CvInvoke.Line(croppedFrame, start, end, new MCvScalar(0, 255, 0), 2);
                }

                //TO FIND THE THUMB INDEXES AND NAMES-----------------------------------------------------------
                if (count_defects > 0)
                {
                    double m11 = CvInvoke.cvGetCentralMoment(ref mcv, 1, 1);
                    double m20 = CvInvoke.cvGetCentralMoment(ref mcv, 2, 0);
                    double m02 = CvInvoke.cvGetCentralMoment(ref mcv, 0, 2);
                    int contourAxisAngle = CalcTilt(m11, m20, m02);
                    fingertipCoordinates = fingertipCoordinates.OrderBy(p => p.X).ToList();
                    NameFingers(croppedFrame, centerPoint, contourAxisAngle, ref fingertipCoordinates);
                }
                //----------------------------------------------------------------------------------------------

                if (!isActivated)
                {
                    if (leftHandPos)
                        CvInvoke.PutText(frame, count_defects.ToString(), new Point((frame.Width / 26) + (frame.Width / 4), frame.Width / 26), FontFace.HersheySimplex, 2.0, new MCvScalar(255, 255, 255), 2);
                    else
                        CvInvoke.PutText(frame, count_defects.ToString(), new Point(frame.Width - (frame.Width / 26) - (frame.Width / 2) + (frame.Width / 4), frame.Width / 26), FontFace.HersheySimplex, 2.0, new MCvScalar(255, 255, 255), 2);
                }
                //Mat mergedFrame = new Mat(drawingFrame.Rows + croppedFrame.Rows, croppedFrame.Cols, DepthType.Cv8U, 3);
                //CvInvoke.HConcat(drawingFrame, croppedFrame, mergedFrame);                                              

                if (count_defects > 4)
                {
                    activate = true;
                    timeDelay = DateTime.Now.Second - pastTime;
                }
                else if (count_defects < 1)
                {
                    activate = false;
                    timeDelay = DateTime.Now.Second - pastTime;
                }
                else
                {
                    activate = false;
                    pastTime = DateTime.Now.Second;
                    timeDelay = 0;
                }

                if (timeDelay >= 55) //If hand is hold for 5 seconds or more
                {
                    if (!isActivated && activate)
                    {
                        //ActivateControl();
                        isActivated = true;
                    }
                    else if (isActivated && !activate)
                    {
                        DeactivateControl();
                        isActivated = false;
                        x1 = -1;
                        x2 = -1;
                        y1 = -1;
                        y2 = -1;
                    }

                    pastTime = DateTime.Now.Second;
                    timeDelay = 0;
                    activate = false;
                }

                captureImageBox.Image = frame; //downFrame
                
                                     
            }
        }

        public void SetIsActivated(bool isActivated)
        {
            this.isActivated = isActivated;
        }

        private void ActivateControl()
        {
            MainForm form = Application.OpenForms.OfType<MainForm>().First();
            form.Invoke(new EventHandler(delegate
            {
                form.Hide();
                MinimizedForm form2 = Application.OpenForms.OfType<MinimizedForm>().Count() > 0 ? Application.OpenForms.OfType<MinimizedForm>().First() : new MinimizedForm();
                form2.SetCamera(this);
                form2.TopMost = true;
                form2.Show();
            }));
        }

        private void DeactivateControl()
        {
            if (Application.OpenForms.OfType<MinimizedForm>().Count() == 1)
            {
                MinimizedForm form = Application.OpenForms.OfType<MinimizedForm>().First();
                form.Invoke(new EventHandler(delegate
                {
                    form.Hide();
                    MainForm mainForm = Application.OpenForms.OfType<MainForm>().First();
                    mainForm.SetCamera(this);
                    mainForm.Show();
                    ScreenProperties screenProperties = new ScreenProperties(mainForm);
                    Process currentProcess = Process.GetCurrentProcess();
                    IntPtr hWnd = currentProcess.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        Native.SetForegroundWindow(hWnd);
                        Native.ShowWindow(hWnd, 9);
                        Native.MoveWindow(hWnd, screenProperties.getWidth() / 2 - (mainForm.Width / 2), screenProperties.getHeight() / 2 - (mainForm.Height / 2), mainForm.Width, mainForm.Height, true);
                    }
                }));
            }
        }

        public void ReleaseResources()
        {
            capture.ImageGrabbed += null;
            if (capture != null)
            {
                capture.Dispose();
                capture = null;
            }
        }

        public void Start()
        {
            captureInProgress = true;
            capture.Start();
        }

        public void Pause()
        {
            captureInProgress = false;
            capture.Pause();
        }

        public void Stop()
        {
            captureInProgress = false;
            capture.Stop();
        }

        public bool isActive()
        {
            return captureInProgress;
        }
    }
}
