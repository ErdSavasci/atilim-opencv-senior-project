using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Drawing;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Threading;
using System.Diagnostics;
using Emgu.CV.VideoSurveillance;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using Accord.Extensions.Imaging.Algorithms.LINE2D;

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
        private Mat filteredCroppedFrame = null;
        private Mat detectHandPalmClosedFrame = null;
        private Mat croppedFrame = null;
        private Mat maskedCroppedFrame = null;
        private Mat analyzeRectFrame = null;
        private Mat hsvCroppedFrame;
        private ImageBox captureImageBox = null;
        private CascadeClassifier haarCascade = null;
        private int timeDelay = 0;
        private int pastTime = 0;
        private bool activate = false, isActivated = false;
        private bool leftHandPos = false;
        private int x1 = -1, x2 = -1, y1 = -1, y2 = -1;
        private Mat camShiftHist;
        private Mat camShiftMask;       
        private Mat camShiftBackProject;
        private bool canStartCamera = false;
        private Rectangle camShiftTrackWindow;
        private int handPalmClosedCount = 0;
        private int count_defects = 0;
        private List<Point> fingertipCoordinates;
        private List<Finger> fingertipNames;
        private int analyzeRectYPos = 0;
        private bool isAscending = true;
        private double frameRate = 10.0;
        private long milliSeconds = 0;
        private long milliSeconds2 = 0;
        private long milliSecondsDiff = 0;
        private bool firstFrameCaptured = true;
        private BackgroundSubtractorMOG2 bgSubtractor;
        private Point handPoint;
        private Size handSize;
        private int deviceIndex = 0;
        private int handWidth, handHeight, handLocX, handLocY;
        private bool HANDFOUND = false;
        private Rectangle handRectangleFastTemplate;
        private PictureBox handOverlayPictureBox;
        private FastTemplateMatching fastTemplateMatching;
        private MouseMovement mouseMovement;
        private FingertipRecognizer fingertipRecognizer;
        #endregion

        public Camera(ImageBox captureImageBox, DsDevice systemCamera, int cameraIndex, PictureBox handOverlayPictureBox)
        {
            this.handOverlayPictureBox = handOverlayPictureBox;
            this.captureImageBox = captureImageBox;
            CvInvoke.UseOpenCL = true;

            deviceIndex = cameraIndex;

            if (cameraIndex != -1)
            {
                canStartCamera = true;
                try
                {
                    //Initializing new OpenCv camera instance and assigning an event after it to trigger a function when frame is being captured
                    capture = new VideoCapture(cameraIndex);

                    capture.ImageGrabbed += ProcessFrame;

                    //Gets all available resolutions that the default camera has and set the max resolution
                    List<Resolution> availableResolutions = new List<Resolution>();
                    availableResolutions = GetAllAvailableResolutions(systemCamera);
                    if (availableResolutions != null)
                    {
                        capture.SetCaptureProperty(CapProp.FrameWidth, availableResolutions.ElementAt(availableResolutions.Count - 1).getWidth());
                        capture.SetCaptureProperty(CapProp.FrameHeight, availableResolutions.ElementAt(availableResolutions.Count - 1).getHeight());

                        //Gets current width and height
                        width = capture.GetCaptureProperty(CapProp.FrameWidth);
                        height = capture.GetCaptureProperty(CapProp.FrameHeight);

                        int index = availableResolutions.Count - 1;
                        while ((width == 0.0 || height == 0.0) && index >= 0)
                        {
                            capture.SetCaptureProperty(CapProp.FrameWidth, availableResolutions.ElementAt(index).getWidth());
                            capture.SetCaptureProperty(CapProp.FrameHeight, availableResolutions.ElementAt(index).getHeight());

                            //Gets current width and height
                            width = capture.GetCaptureProperty(CapProp.FrameWidth);
                            height = capture.GetCaptureProperty(CapProp.FrameHeight);

                            index--;
                        }
                    }

                    //Different Frame objects for different purposes
                    frame = new Mat();
                    filteredCroppedFrame = new Mat();
                    croppedFrame = new Mat();
                    detectHandPalmClosedFrame = new Mat();
                    maskedCroppedFrame = new Mat();
                    hsvCroppedFrame = new Mat();
                    analyzeRectFrame = new Mat();
                    camShiftBackProject = new Mat();
                    camShiftHist = new Mat();
                    camShiftMask = new Mat();                    
                    bgSubtractor = new BackgroundSubtractorMOG2(0, 50, false);

                    handWidth = -1;
                    handHeight = -1;
                    handLocX = -1;
                    handLocY = -1;

                    fingertipCoordinates = new List<Point>();
                    fingertipNames = new List<Finger>();

                    //Flips the video capture device in horizontal axis
                    capture.FlipHorizontal = true;

                    //Get Resources Path of Project
                    string system = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.LastIndexOf("\\"));
                    system = system.Substring(0, system.LastIndexOf("\\"));
                    system = system.Substring(0, system.LastIndexOf("\\"));

                    //Get HaarCascade XML File
                    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MoveCursorByHand.aGest.xml");
                    StreamReader streamReader = new StreamReader(stream);
                    string aGestContent = streamReader.ReadToEnd();
                    stream.Close();
                    streamReader.Close();
                    string xmlCascadeFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Temp" + @"\aGest.xml";
                    File.Create(xmlCascadeFilePath).Dispose();
                    StreamWriter streamWriter = new StreamWriter(xmlCascadeFilePath);
                    streamWriter.Write(aGestContent);
                    streamWriter.Close();
                    haarCascade = new CascadeClassifier(xmlCascadeFilePath);

                    //Initialize Fast Template Matching
                    fastTemplateMatching = new FastTemplateMatching();
                    fastTemplateMatching.InitializeFastTemplateMatching();

                    //Initialize Mouse Movement
                    mouseMovement = new MouseMovement();

                    //Initialize Fingertip Recognizer
                    fingertipRecognizer = new FingertipRecognizer();

                    MainForm mainForm = Application.OpenForms.OfType<MainForm>().First();
                    mainForm.clearLoadingAnimationPictureBox();
                }
                catch (Exception ex)
                {
                    canStartCamera = false;
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Bitmap noSignalBitmap = Properties.Resources.no_signal;
                Rectangle captureRect = new Rectangle(0, 0, noSignalBitmap.Width, noSignalBitmap.Height);
                BitmapData bitmapData = noSignalBitmap.LockBits(captureRect, ImageLockMode.ReadWrite, noSignalBitmap.PixelFormat);
                IntPtr firstPixelData = bitmapData.Scan0;
                int singleLineBytes = bitmapData.Stride;

                Mat noSignalMat = new Mat(noSignalBitmap.Height, noSignalBitmap.Width, DepthType.Cv8U, 3, firstPixelData, singleLineBytes);

                noSignalBitmap.UnlockBits(bitmapData);

                captureImageBox.Image = noSignalMat;
            }
        }

        public void ChangeImageBox(ImageBox captureImageBox)
        {
            this.captureImageBox = captureImageBox;
        }

        public void SetBackgroundImage(Bitmap sourceBitmap)
        {
            Rectangle captureRect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
            BitmapData bitmapData = sourceBitmap.LockBits(captureRect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
            IntPtr firstPixelData = bitmapData.Scan0;
            int singleLineBytes = bitmapData.Stride;

            Mat sourceBitmapMat = new Mat(sourceBitmap.Height, sourceBitmap.Width, DepthType.Cv8U, 3, firstPixelData, singleLineBytes);

            sourceBitmap.UnlockBits(bitmapData);

            captureImageBox.Image = sourceBitmapMat;
        }

        public List<Resolution> GetAllAvailableResolutions(DsDevice device)
        {
            int hr = 0;
            int max = 0;
            int bitCount = 0;

            List<Resolution> availableResolutions = new List<Resolution>();
            VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
            IEnumMediaTypes mediaTypesEnum;
            AMMediaType[] mediaTypes = new AMMediaType[1];
            IntPtr fetched = IntPtr.Zero;
            IBaseFilter baseFilter = null;
            IFilterGraph2 filterGraph2 = new FilterGraph() as IFilterGraph2;
            hr = filterGraph2.AddSourceFilterForMoniker(device.Mon, null, device.Name, out baseFilter);
            IPin pRaw = DsFindPin.ByCategory(baseFilter, PinCategory.Capture, 0);

            hr = pRaw.EnumMediaTypes(out mediaTypesEnum);
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

        public void ChangeHandPosition(string pos)
        {
            if (pos.Equals("Left") && leftHandPos != true)
            {
                leftHandPos = true;
                fastTemplateMatching.ChangeFastTemplateMatchingHandPos(false);       
            }
            else if (leftHandPos != false)
            {
                leftHandPos = false;
                fastTemplateMatching.ChangeFastTemplateMatchingHandPos(true);
            }
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

        private void DrawAnalyzeRectangle(ref Mat frame)
        {
            CvInvoke.Rectangle(frame, new Rectangle(0, analyzeRectYPos, frame.Width, 7), new MCvScalar(0, 0, 255), -2);
            if (analyzeRectYPos < frame.Height && isAscending)
                analyzeRectYPos = analyzeRectYPos + 20;
            else if (!isAscending)
            {
                analyzeRectYPos = analyzeRectYPos - 20;
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
            //frame -> croppedFrame -> filteredCroppedFrame
            //All GUI operations are applied to croppedFrame
            //croppedFrame (BGR) / filteredCroppedFrame (BW)

            HANDFOUND = false;
            count_defects = 0;
            Point centerPoint = new Point();
            MCvMoments mcv = new MCvMoments();
            Rectangle boundingRect = new Rectangle();
            try
            {
                milliSeconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                fingertipCoordinates.Clear();
                if (capture != null && capture.Ptr != IntPtr.Zero)
                {
                    capture.Retrieve(frame, 0);

                    if (leftHandPos)
                    {
                        if (!isActivated)
                        {
                            CvInvoke.Rectangle(frame, new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, frame.Width / 2), new MCvScalar(255, 255, 0), 2);
                            croppedFrame = new Mat(frame, new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, frame.Width / 2));
                        }
                        else
                        {
                            if (firstFrameCaptured)
                            {
                                croppedFrame = new Mat(frame, new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, frame.Width / 2));
                            }
                            else
                            {
                                croppedFrame = new Mat(frame, new Rectangle(0, 0, frame.Width, frame.Height));
                            }
                        }
                    }
                    else
                    {
                        if (!isActivated)
                        {
                            CvInvoke.Rectangle(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, frame.Width / 2), new MCvScalar(255, 255, 0), 2);
                            croppedFrame = new Mat(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, frame.Width / 2));
                        }
                        else
                        {
                            if (firstFrameCaptured)
                            {
                                croppedFrame = new Mat(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, frame.Width / 2));
                            }
                            else
                            {
                                croppedFrame = new Mat(frame, new Rectangle(0, 0, frame.Width, frame.Height));
                            }
                        }
                    }

                    //CAMSHIFT METHOD FOR DETECTING AND TRACKING HAND BY HSV VALUES OF HAND
                    if (isActivated && firstFrameCaptured)
                    {
                        if (handWidth != -1 && handHeight != -1 && handLocX != -1 && handLocY != -1)
                        {
                            CvInvoke.CvtColor(croppedFrame, hsvCroppedFrame, ColorConversion.Bgr2Hsv);
                            CvInvoke.InRange(hsvCroppedFrame, new VectorOfInt(new int[] { 0, 10, 60 }), new VectorOfInt(new int[] { 20, 150, 255 }), camShiftMask);
                            CvInvoke.CalcHist(new VectorOfMat(new Mat[] { hsvCroppedFrame }), new int[] { 0 }, camShiftMask, camShiftHist, new int[] { 180 }, new float[] { 0, 180 }, false);
                            CvInvoke.Normalize(camShiftHist, camShiftHist, 0, 255, NormType.MinMax);                           

                            handPoint = new Point(handLocX, handLocY);
                            handSize = new Size(handWidth, handHeight);
                            camShiftTrackWindow = new Rectangle(handPoint, handSize);                           
                        }
                        firstFrameCaptured = false;
                    }
                    else if (isActivated)
                    {
                        if (handWidth != -1 && handHeight != -1 && handLocX != -1 && handLocY != -1
                            && handSize.Width != -1 && handSize.Height != -1 && handPoint.X != -1 && handPoint.Y != -1)
                        {
                            if (!leftHandPos)
                            {
                                handLocX = handLocX + (frame.Width) - (frame.Width / 26) - (frame.Width / 2);
                                handLocY = handLocY + (frame.Width / 26);
                            }
                            else
                            {
                                handLocX = handLocX + (frame.Width / 26);
                                handLocY = handLocY + (frame.Width / 26);
                            }

                            handPoint = new Point(handLocX, handLocY);
                            handSize = new Size(handWidth, handHeight);
                            camShiftTrackWindow = new Rectangle(handPoint, handSize);

                            CvInvoke.CvtColor(croppedFrame, hsvCroppedFrame, ColorConversion.Bgr2Hsv);
                            CvInvoke.CalcBackProject(new VectorOfMat(new Mat[] { hsvCroppedFrame }), new int[] { 0 }, camShiftHist, camShiftBackProject, new float[] { 0, 180 }, 1);
                            CvInvoke.CamShift(camShiftBackProject, ref camShiftTrackWindow, new MCvTermCriteria(10, 1));
                            //CvInvoke.Rectangle(frame, camShiftTrackWindow, new MCvScalar(255, 0, 0), 5);

                            //croppedFrame = new Mat(croppedFrame, new Rectangle(camShiftTrackWindow.X - camShiftTrackWindow.X / 2, camShiftTrackWindow.Y - camShiftTrackWindow.Y / 2, (int)Math.Min(camShiftTrackWindow.Width * 1.5, croppedFrame.Width - camShiftTrackWindow.X), (int)Math.Min(camShiftTrackWindow.Height * 1.5, croppedFrame.Height - camShiftTrackWindow.Y)));
                        }
                    }                   
                   
                    //FAST TEMPLATE MATCHING METHOD
                    List<Match> bestRepresentatives = fastTemplateMatching.FindHandByFastTemplateMatching(ref croppedFrame, isActivated);
                    if (bestRepresentatives != null && bestRepresentatives.Count > 1)
                    {
                        bool skipFirstOne = true;
                        int downSampleCount = 1;
                        handRectangleFastTemplate = new Rectangle(0, 0, croppedFrame.Width, croppedFrame.Height);
                        foreach (var item in bestRepresentatives)
                        {
                            if (!skipFirstOne) {
                                int extraValue = (croppedFrame.Width / croppedFrame.Height);
                                if(isActivated)
                                    extraValue = (croppedFrame.Width / croppedFrame.Height);
                                int rectX = item.BoundingRect.X * downSampleCount;
                                if(rectX >= 50)
                                    rectX = item.BoundingRect.X * downSampleCount - 50;
                                handRectangleFastTemplate = new Rectangle(rectX, item.BoundingRect.Y * downSampleCount, Math.Min(item.BoundingRect.Width * downSampleCount + 50, croppedFrame.Width - item.BoundingRect.X), item.BoundingRect.Height * downSampleCount);
                                CvInvoke.Rectangle(croppedFrame, handRectangleFastTemplate, new MCvScalar(0, 255, 255), 2);                          
                            }
                            else
                            {
                                downSampleCount = (int)item.Score > 0 ? (int)item.Score : 1;
                                skipFirstOne = false;
                            }                            
                        }
                        if(bestRepresentatives.Count > 1)
                        {
                            HANDFOUND = true;
                        }
                       
                        croppedFrame = new Mat(croppedFrame, new Rectangle(handRectangleFastTemplate.X, handRectangleFastTemplate.Y, Math.Min(handRectangleFastTemplate.Width, croppedFrame.Width - handRectangleFastTemplate.X), Math.Min(handRectangleFastTemplate.Height, croppedFrame.Height - handRectangleFastTemplate.Y)));
                    }

                    //BACKGROUND SUBTRACTOR METHOD
                    CvInvoke.BilateralFilter(croppedFrame, filteredCroppedFrame, 5, 50, 100);
                    bgSubtractor.Apply(filteredCroppedFrame, maskedCroppedFrame);
                    Matrix<int> element = new Matrix<int>(3, 3);
                    element.SetValue(1);
                    CvInvoke.Erode(maskedCroppedFrame, maskedCroppedFrame, element, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                    CvInvoke.BitwiseAnd(filteredCroppedFrame, filteredCroppedFrame, filteredCroppedFrame, maskedCroppedFrame);

                    CvInvoke.CvtColor(filteredCroppedFrame, filteredCroppedFrame, ColorConversion.Bgr2Gray);

                    if (!isActivated)
                    {
                        if (leftHandPos)
                            analyzeRectFrame = new Mat(frame, new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, frame.Width / 2));
                        else
                            analyzeRectFrame = new Mat(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, frame.Width / 2));

                        DrawAnalyzeRectangle(ref analyzeRectFrame);                       
                    }

                    //STOP OVERLAY HAND PICTURE FROM BLINKING
                    if (HANDFOUND || isActivated)
                    {
                        Action action = new Action(() => { handOverlayPictureBox.Visible = false; });
                        handOverlayPictureBox.Invoke(action);
                    }
                    else
                    {
                        if (!handOverlayPictureBox.Visible)
                        {
                            Action action = new Action(() => { handOverlayPictureBox.Visible = true; });
                            handOverlayPictureBox.Invoke(action);
                        }
                    }

                    //HAAR CASCADE METHOD FOR DETECTING CLOSED HAND
                    detectHandPalmClosedFrame = croppedFrame.Clone();
                    CvInvoke.CvtColor(detectHandPalmClosedFrame, detectHandPalmClosedFrame, ColorConversion.Bgr2Gray);

                    if (!leftHandPos)
                    {
                        CvInvoke.Flip(detectHandPalmClosedFrame, detectHandPalmClosedFrame, FlipType.Horizontal);
                    }

                    Rectangle[] detectedHaarCascadeRectangles = haarCascade.DetectMultiScale(detectHandPalmClosedFrame, 1.4, 4);
                    handPalmClosedCount = detectedHaarCascadeRectangles.Count();                    

                    if (handPalmClosedCount > 0)
                    {
                        Rectangle detectedRect = detectedHaarCascadeRectangles.First();
                        if (!leftHandPos)
                            detectedRect.X = Math.Abs(croppedFrame.Width - detectedRect.X - detectedRect.Width);
                        CvInvoke.Rectangle(croppedFrame, detectedRect, new MCvScalar(255, 0, 0), 2);
                        Console.WriteLine(handPalmClosedCount + " Closed Hand Palm Found!!");
                    }

                    //THRESHOLD METHOD AND CONTOUR DETECTION
                    CvInvoke.GaussianBlur(filteredCroppedFrame, filteredCroppedFrame, new Size(41, 41), 0);
                    CvInvoke.Threshold(filteredCroppedFrame, filteredCroppedFrame, 60, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);
                    //CvInvoke.AdaptiveThreshold(filteredCroppedFrame, filteredCroppedFrame, 127, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 7, 2);

                    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                    Mat hierarchy = new Mat();
                    CvInvoke.FindContours(filteredCroppedFrame.Clone(), contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxNone);

                    //IF NO CONTOUR IS FOUND, THEN DON'T DO ANYTHING
                    if (contours.Size > 0 && HANDFOUND)
                    {
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
                        boundingRect = CvInvoke.BoundingRectangle(maxResult);
                        handWidth = boundingRect.Width;
                        handHeight = boundingRect.Height;
                        CvInvoke.Rectangle(croppedFrame, boundingRect, new MCvScalar(255, 255, 255), 0);
                        VectorOfInt hullResult = new VectorOfInt();
                        if (leftHandPos)
                            CvInvoke.ConvexHull(maxResult, hullResult);
                        else
                            CvInvoke.ConvexHull(maxResult, hullResult, true);

                        if (leftHandPos)
                            CvInvoke.ConvexHull(maxResult, hullResult, false, false);
                        else
                            CvInvoke.ConvexHull(maxResult, hullResult, true, false);

                        Mat defects = new Mat();
                        CvInvoke.ConvexityDefects(maxResult, hullResult, defects);

                        Matrix<int> matrix = new Matrix<int>(defects.Rows, defects.Cols, defects.NumberOfChannels);
                        defects.CopyTo(matrix);
                        Matrix<int>[] channels = matrix.Split();

                        //TO FIND THE CENTER OF THE CONTOUR-------------------------------------------------------------
                        centerPoint = new Point();
                        mcv = CvInvoke.Moments(contours[maxAreaIndex], true);
                        double m00 = CvInvoke.cvGetSpatialMoment(ref mcv, 0, 0);
                        double m01 = CvInvoke.cvGetSpatialMoment(ref mcv, 0, 1);
                        double m10 = CvInvoke.cvGetSpatialMoment(ref mcv, 1, 0);
                        int scale = 1;
                        if (m00 != 0)
                        {
                            int xCenter = (int)Math.Round(m10 / m00) * scale;
                            int yCenter = (int)Math.Round(m01 / m00) * scale;
                            centerPoint = new Point(xCenter, yCenter);

                            handLocX = centerPoint.X;
                            handLocY = centerPoint.Y;
                        }
                        //----------------------------------------------------------------------------------------------                

                        CvInvoke.DrawContours(filteredCroppedFrame, contours, -1, new MCvScalar(0, 255, 0), 3);
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
                    }

                    //TO FIND THE THUMB INDEXES AND NAMES-----------------------------------------------------------
                    if (count_defects > 0)
                    {
                        double m11 = CvInvoke.cvGetCentralMoment(ref mcv, 1, 1);
                        double m20 = CvInvoke.cvGetCentralMoment(ref mcv, 2, 0);
                        double m02 = CvInvoke.cvGetCentralMoment(ref mcv, 0, 2);
                        int contourAxisAngle = CalcTilt(m11, m20, m02);
                        fingertipCoordinates = fingertipCoordinates.OrderBy(p => p.X).ToList();
                        fingertipRecognizer.NameFingers(ref croppedFrame, centerPoint, contourAxisAngle, ref fingertipCoordinates, leftHandPos, count_defects);

                        CvInvoke.Circle(croppedFrame, centerPoint, 1, new MCvScalar(0, 0, 255), 10);
                        CvInvoke.Circle(croppedFrame, centerPoint, boundingRect.Width / 4, new MCvScalar(0, 255, 0), 3);
                    }
                    //----------------------------------------------------------------------------------------------

                    if (!isActivated)
                    {
                        if (leftHandPos)
                            CvInvoke.PutText(frame, count_defects.ToString(), new Point((frame.Width / 26) + (frame.Width / 4), frame.Width / 26), FontFace.HersheySimplex, Math.Min(1.0 + (frame.Height / 960), 2.5), new MCvScalar(255, 255, 255), 2);
                        else
                            CvInvoke.PutText(frame, count_defects.ToString(), new Point(frame.Width - (frame.Width / 26) - (frame.Width / 2) + (frame.Width / 4), frame.Width / 26), FontFace.HersheySimplex, Math.Min(1.0 + (frame.Height / 960), 2.5), new MCvScalar(255, 255, 255), 2);
                    }

                    //ACTIVATING MOUSE CONTROL BY MINIMIZING WINDOW AFTER HOLDING HAND IN FRONT OF WEBCAM FOR 5 SECONDS
                    if (!isActivated && count_defects > 4 && HANDFOUND)
                    {
                        activate = true;
                        timeDelay = DateTime.Now.Second - pastTime;
                    }
                    else if (isActivated && count_defects < 1 && !HANDFOUND)
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

                    if (timeDelay >= 10)
                    {
                        if (isActivated && !activate)
                        {
                            isActivated = false;
                            DeactivateMouseControl();
                            x1 = -1;
                            x2 = -1;
                            y1 = -1;
                            y2 = -1;
                        }

                        pastTime = DateTime.Now.Second;
                        timeDelay = 0;
                        activate = false;
                    }
                    else if (timeDelay >= 5) //If hand is hold for 5 seconds or more
                    {
                        if (!isActivated && activate)
                        {
                            isActivated = true;
                            firstFrameCaptured = true;
                            ActivateMouseControl();

                            pastTime = DateTime.Now.Second;
                            timeDelay = 0;
                            activate = false;
                        }                        
                    }

                    //MOUSE CONTROL WITH INDEX FINGER 
                    if (isActivated && fingertipCoordinates.Count > 3 && HANDFOUND)
                    {
                        int fingerCount = 0;
                        if (x1 == -1 && y1 == -1)
                        {
                            if (leftHandPos)
                            {
                                foreach (Finger finger in fingertipNames)
                                {
                                    if(finger == Finger.INDEX)
                                    {
                                        x1 = fingertipCoordinates[fingerCount].X;
                                        y1 = fingertipCoordinates[fingerCount].Y;

                                        if (!leftHandPos)
                                        {
                                            x1 = x1 + handRectangleFastTemplate.X;
                                            y1 = y1 + handRectangleFastTemplate.Y;
                                        }
                                        else
                                        {
                                            x1 = x1 + handRectangleFastTemplate.X;
                                            y1 = y1 + handRectangleFastTemplate.Y;
                                        }
                                    }
                                    fingerCount++;
                                }                               
                            }
                            else
                            {
                                foreach (Finger finger in fingertipNames)
                                {
                                    if (finger == Finger.INDEX)
                                    {
                                        x1 = fingertipCoordinates[fingerCount].X;
                                        y1 = fingertipCoordinates[fingerCount].Y;

                                        if (!leftHandPos)
                                        {
                                            x1 = x1 + handRectangleFastTemplate.X;
                                            y1 = y1 + handRectangleFastTemplate.Y;
                                        }
                                        else
                                        {
                                            x1 = x1 + handRectangleFastTemplate.X;
                                            y1 = y1 + handRectangleFastTemplate.Y;
                                        }
                                    }
                                    fingerCount++;
                                }
                            }
                        }
                        else
                        {
                            if (leftHandPos)
                            {
                                foreach (Finger finger in fingertipNames)
                                {
                                    if (finger == Finger.INDEX)
                                    {
                                        x2 = fingertipCoordinates[fingerCount].X;
                                        y2 = fingertipCoordinates[fingerCount].Y;

                                        if (!leftHandPos)
                                        {
                                            x2 = x2 + handRectangleFastTemplate.X;
                                            y2 = y2 + handRectangleFastTemplate.Y;
                                        }
                                        else
                                        {
                                            x2 = x2 + handRectangleFastTemplate.X;
                                            y2 = y2 + handRectangleFastTemplate.Y;
                                        }
                                    }
                                    fingerCount++;
                                }
                            }
                            else
                            {
                                foreach (Finger finger in fingertipNames)
                                {
                                    if (finger == Finger.INDEX)
                                    {
                                        x2 = fingertipCoordinates[fingerCount].X;
                                        y2 = fingertipCoordinates[fingerCount].Y;

                                        if (!leftHandPos)
                                        {
                                            x2 = x2 + handRectangleFastTemplate.X;
                                            y2 = y2 + handRectangleFastTemplate.Y;
                                        }
                                        else
                                        {
                                            x2 = x2 + handRectangleFastTemplate.X;
                                            y2 = y2 + handRectangleFastTemplate.Y;
                                        }
                                    }
                                    fingerCount++;
                                }
                            }

                            mouseMovement.MoveMouse(x1, x2, y1, y2, ref frame);

                            x1 = -1;
                            y1 = -1;
                        }
                    }
                    else
                    {
                        x1 = -1;
                        y1 = -1;
                    }

                    captureImageBox.Image = frame;

                    Console.WriteLine("Detected FingerTip Number: " + count_defects + " Finger Index [X1]: " + x1 + " [X2]: " + x2
                        + " [Y1]: " + y1 + " [Y2]: " + y2);

                    //CALCULATIONS FOR DELAY (DELAY IS NEEDED TO BE PUT TO DRAW FRAMES TO THE SCREEN CORRECTLY)
                    milliSeconds2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    milliSecondsDiff = milliSeconds2 - milliSeconds;

                    if (milliSecondsDiff > 1000)
                    {
                        milliSecondsDiff = 1000;
                    }

                    frameRate = Math.Abs(1000 / milliSecondsDiff);

                    if (!isActivated && firstFrameCaptured)
                    {
                        firstFrameCaptured = false;
                        Thread.Sleep(100);
                    }
                    else
                    {
                        Thread.Sleep((int)(Math.Min(1000.0 / frameRate, 125)));
                    }
                }

                GC.Collect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public int getActiveDeviceIndex()
        {
            return deviceIndex;
        }

        public void setFirstFrameCaptured(bool firstFrameCaptured)
        {
            this.firstFrameCaptured = firstFrameCaptured;
        }

        public void SetIsActivated(bool isActivated)
        {
            this.isActivated = isActivated;
        }

        private void ActivateMouseControl()
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

        private void DeactivateMouseControl()
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
            if (capture != null)
            {
                capture.Dispose();
                capture = null;
            }
        }

        public void Start()
        {
            if (capture != null && canStartCamera)
            {
                captureInProgress = true;
                capture.Start();
            }
        }

        public void Pause()
        {
            if (capture != null && canStartCamera)
            {
                captureInProgress = false;
                capture.Pause();
            }
        }

        public void Stop()
        {
            if (capture != null && canStartCamera)
            {
                captureInProgress = false;
                capture.Stop();
            }
        }

        public bool isActive()
        {
            return captureInProgress;
        }

        public Size getFrameSize()
        {
            return new Size((int)width, (int)height);
        }
    }
}
