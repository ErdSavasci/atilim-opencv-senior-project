﻿using System;
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
using RegExp = System.Text.RegularExpressions;

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
        private FastTemplateMatcher fastTemplateMatcher;
        private MouseMovement mouseMovement;
        private FingertipRecognizer fingertipRecognizer;
        private HandRecognizer handRecognizer;
        private FingertipDetector fingertipDetector;
        private PictureBox loadingGIFPicureBox;
        private Thread start;
        private double maxAreaHSV = 0.0D;
        private bool readyToClick = false;
        private Cursor currentCursor;
        private bool isFirstBigRect = true;
        private int boundaryWidth;
        private int boundaryHeight;
        private int distanceX;
        private int distanceY;
        private RectangleF bigRectangle;
        private RectangleF detectedRectangle;
        private string[] macroComboBoxSelections;
        private bool isOnce = true;
        #endregion

        public Camera(ImageBox captureImageBox, DsDevice systemCamera, int cameraIndex, PictureBox handOverlayPictureBox, PictureBox loadingGIFPicureBox, string[] macroComboBoxSelections)
        {
            this.loadingGIFPicureBox = loadingGIFPicureBox;
            this.handOverlayPictureBox = handOverlayPictureBox;
            this.captureImageBox = captureImageBox;
            this.macroComboBoxSelections = macroComboBoxSelections;
            CvInvoke.UseOpenCL = true;

            deviceIndex = cameraIndex;

            currentCursor = new Cursor(Cursor.Current.Handle);


            if (cameraIndex != -1)
            {
                try
                {
                    List<Resolution> availableResolutions = new List<Resolution>();
                    availableResolutions = GetAllAvailableResolutions(systemCamera);
                    Thread prepareThread = new Thread(() =>
                    {
                        //Initializing new OpenCv camera instance and assigning an event after it to trigger a function when frame is being captured
                        capture = new VideoCapture(cameraIndex);
                        capture.ImageGrabbed += ProcessFrame;

                        //Gets all available resolutions that the default camera has and set the max resolution
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
                        fastTemplateMatcher = new FastTemplateMatcher();
                        fastTemplateMatcher.InitializeFastTemplateMatching();

                        //Initialize Mouse Movement
                        mouseMovement = new MouseMovement();

                        //Initialize Fingertip Recognizer
                        fingertipRecognizer = new FingertipRecognizer();

                        //Initialize Hand Recognizer
                        handRecognizer = new HandRecognizer();

                        //Initialize Hand Recognizer
                        fingertipDetector = new FingertipDetector();

                        canStartCamera = true;
                    });
                    prepareThread.IsBackground = true;
                    prepareThread.Start();
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
                fastTemplateMatcher.ChangeFastTemplateMatchingHandPos(false);
            }
            else if (leftHandPos != false)
            {
                leftHandPos = false;
                fastTemplateMatcher.ChangeFastTemplateMatchingHandPos(true);
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
                            CvInvoke.Rectangle(frame, new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, (int)(frame.Height / 1.50)), new MCvScalar(255, 255, 0), 2);
                            croppedFrame = new Mat(frame, new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, (int)(frame.Height / 1.50)));
                        }
                        else
                        {
                            if (firstFrameCaptured)
                            {
                                croppedFrame = new Mat(frame, new Rectangle(frame.Width / 26, frame.Width / 26, frame.Width / 2, (int)(frame.Height / 1.50)));
                                //firstFrameCaptured = false;
                            }
                            else
                            {
                                croppedFrame = new Mat(frame, new Rectangle(0, 0, frame.Width, frame.Height));
                                CvInvoke.PyrDown(croppedFrame, croppedFrame);
                            }
                        }
                    }
                    else
                    {
                        if (!isActivated)
                        {
                            CvInvoke.Rectangle(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, (int)(frame.Height / 1.50)), new MCvScalar(255, 255, 0), 2);
                            croppedFrame = new Mat(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, (int)(frame.Height / 1.50)));
                        }
                        else
                        {
                            if (firstFrameCaptured)
                            {
                                croppedFrame = new Mat(frame, new Rectangle(frame.Width - (frame.Width / 26) - (frame.Width / 2), frame.Width / 26, frame.Width / 2, (int)(frame.Height / 1.50)));
                                //firstFrameCaptured = false;
                            }
                            else
                            {
                                croppedFrame = new Mat(frame, new Rectangle(0, 0, frame.Width, frame.Height));
                                CvInvoke.PyrDown(croppedFrame, croppedFrame);
                            }
                        }
                    }

                    //CAMSHIFT METHOD FOR DETECTING AND TRACKING HAND BY HSV VALUES OF HAND
                    /*if (isActivated && firstFrameCaptured)
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
                    }*/

                    CvInvoke.CvtColor(croppedFrame, hsvCroppedFrame, ColorConversion.Bgr2Hsv);
                    Mat hsvMask = new Mat();
                    CvInvoke.InRange(hsvCroppedFrame, new VectorOfInt(new int[] { 100, 50, 0 }), new VectorOfInt(new int[] { 125, 255, 255 }), hsvMask);
                    Mat closedMorph = new Mat();
                    Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(1, 1), new Point(-1, -1));
                    CvInvoke.MorphologyEx(hsvMask, closedMorph, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                    CvInvoke.MorphologyEx(closedMorph, closedMorph, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                    VectorOfVectorOfPoint contoursHSV = new VectorOfVectorOfPoint();
                    Mat hierarchyHSV = new Mat();
                    CvInvoke.FindContours(closedMorph.Clone(), contoursHSV, hierarchyHSV, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
                    if (contoursHSV.Size > 0)
                    {
                        maxAreaHSV = CvInvoke.ContourArea(contoursHSV[0]);
                        int maxAreaIndexHSV = 0;
                        for (int i = 0; i < contoursHSV.Size; i++)
                        {
                            if (CvInvoke.ContourArea(contoursHSV[i]) > maxAreaHSV)
                            {
                                maxAreaHSV = CvInvoke.ContourArea(contoursHSV[i]);
                                maxAreaIndexHSV = i;
                            }
                        }
                        maxAreaHSV = CvInvoke.ContourArea(contoursHSV[maxAreaIndexHSV]);
                    }
                    else
                    {
                        maxAreaHSV = 0.0D;
                    }
                    //Console.WriteLine("Maximum HSV Area: " + maxAreaHSV);

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

                    //FAST TEMPLATE MATCHING METHOD
                    List<Match> bestRepresentatives = fastTemplateMatcher.FindHandByFastTemplateMatching(ref filteredCroppedFrame, isActivated);
                    if (bestRepresentatives != null && bestRepresentatives.Count > 1)
                    {
                        bool skipFirstOne = true;
                        int downSampleCount = 1;
                        handRectangleFastTemplate = new Rectangle(0, 0, croppedFrame.Width, croppedFrame.Height);
                        foreach (var item in bestRepresentatives)
                        {
                            if (!skipFirstOne)
                            {
                                int extraValue = (croppedFrame.Width / croppedFrame.Height);
                                if (isActivated)
                                    extraValue = (croppedFrame.Width / croppedFrame.Height);
                                int rectX = item.BoundingRect.X * downSampleCount;
                                if (rectX >= 50)
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
                        if (bestRepresentatives.Count > 1)
                        {
                            HANDFOUND = true;
                        }

                        //croppedFrame = new Mat(croppedFrame, new Rectangle(handRectangleFastTemplate.X, handRectangleFastTemplate.Y, Math.Min(handRectangleFastTemplate.Width, croppedFrame.Width - handRectangleFastTemplate.X), Math.Min(handRectangleFastTemplate.Height + 500, croppedFrame.Height - handRectangleFastTemplate.Y)));
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
                        LPPoint initialPoint;
                        Native.GetCursorPos(out initialPoint);
                        Rectangle detectedRect = detectedHaarCascadeRectangles.First();

                        if (!leftHandPos)
                            detectedRect.X = Math.Abs(croppedFrame.Width - detectedRect.X - detectedRect.Width);

                        CvInvoke.Rectangle(croppedFrame, detectedRect, new MCvScalar(255, 0, 0), 2);
                        Console.WriteLine(handPalmClosedCount + " Closed Hand Palm Found!!");

                        if (isActivated)
                        {
                            if (x1 == -1 && y1 == -1 && Math.Abs(GetBigRectX() - initialPoint.X) <= 200 && Math.Abs(GetBigRectY() - initialPoint.Y) <= 200)
                                Native.SetCursorPos(GetBigRectX(), GetBigRectY());
                            else
                                Native.SetCursorPos(GetBigRectX(), GetBigRectY());

                            //////////////////////////////////////////////////////////////////////
                            detectedRectangle = new RectangleF(
                                        detectedRect.X,
                                        detectedRect.Y,
                                        250,
                                        250);

                            SetBigRectSize((float)(detectedRectangle.Width * 1.5), (float)(detectedRectangle.Height * 1.5));
                            SetDetectRextXY((int)detectedRectangle.X * 3, (int)detectedRectangle.Y * 3);

                            MoveCursor(detectedRectangle, bigRectangle);
                            //////////////////////////////////////////////////////////////////////
                        }
                    }
                    else
                    {
                        x1 = -1;
                        y1 = -1;
                        x2 = -1;
                        y2 = -1;
                    }

                    //THRESHOLD METHOD AND CONTOUR DETECTION
                    CvInvoke.GaussianBlur(filteredCroppedFrame, filteredCroppedFrame, new Size(41, 41), 0);
                    CvInvoke.Threshold(filteredCroppedFrame, filteredCroppedFrame, 41, 255, ThresholdType.BinaryInv | ThresholdType.Otsu);
                    //CvInvoke.AdaptiveThreshold(filteredCroppedFrame, filteredCroppedFrame, 127, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 7, 2);

                    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                    Mat hierarchy = new Mat();
                    CvInvoke.FindContours(filteredCroppedFrame.Clone(), contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                    //IF NO CONTOUR IS FOUND, THEN DON'T DO ANYTHING
                    if ((contours.Size > 0 && maxAreaHSV > 0.0D) || HANDFOUND)
                    {
                        //RECOGNIZE HAND AND DRAW BORDER
                        VectorOfPoint maxResult = handRecognizer.RecognizeHand(contours, leftHandPos);
                        boundingRect = handRecognizer.GetBoundingRectangle(maxResult);

                        handWidth = boundingRect.Width;
                        handHeight = boundingRect.Height;

                        //TO FIND THE CENTER OF THE CONTOUR-------------------------------------------------------------
                        int maxAreaIndex = handRecognizer.GetMaxAreaIndexOfContours();
                        centerPoint = handRecognizer.FindCenterPoint(contours, maxAreaIndex);
                        handLocX = centerPoint.X;
                        handLocY = centerPoint.Y;
                        //----------------------------------------------------------------------------------------------                

                        Mat defects = handRecognizer.GetDefects();
                        Matrix<int> matrix = handRecognizer.GetMatrixOfDefects();

                        fingertipCoordinates = fingertipDetector.DetectFingers(defects, matrix, maxResult, ref count_defects, handPalmClosedCount, leftHandPos, ref croppedFrame);
                    }

                    //STOP OVERLAY HAND PICTURE FROM BLINKING
                    if (HANDFOUND || isActivated || (contours.Size > 0 && maxAreaHSV > 0.0D))
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

                    //TO FIND THE THUMB INDEXES AND NAMES-----------------------------------------------------------
                    if (count_defects > 0 && contours.Size > 0 && maxAreaHSV > 0.0D)
                    {
                        double m11 = CvInvoke.cvGetCentralMoment(ref mcv, 1, 1);
                        double m20 = CvInvoke.cvGetCentralMoment(ref mcv, 2, 0);
                        double m02 = CvInvoke.cvGetCentralMoment(ref mcv, 0, 2);
                        int contourAxisAngle = CalcTilt(m11, m20, m02);
                        fingertipCoordinates = fingertipCoordinates.OrderBy(p => p.X).ToList();
                        centerPoint.Y += (((boundingRect.Y + boundingRect.Height) - centerPoint.Y) / 2);

                        fingertipNames = fingertipRecognizer.NameFingers(ref croppedFrame, centerPoint, contourAxisAngle, fingertipCoordinates, leftHandPos, count_defects);
                        CvInvoke.Circle(croppedFrame, centerPoint, 1, new MCvScalar(0, 0, 255), 10);
                        //CvInvoke.Circle(croppedFrame, centerPoint, boundingRect.Width / 4, new MCvScalar(0, 255, 0), 3);
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
                    if (!isActivated && HANDFOUND)
                    {
                        activate = true;
                        timeDelay = DateTime.Now.Second - pastTime;
                    }
                    else if (isActivated && HANDFOUND)
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

                    if (timeDelay >= 5) //If hand is hold for 5 seconds or more
                    {
                        if (!isActivated && activate)
                        {
                            isActivated = true;
                            firstFrameCaptured = true;
                            ActivateMouseControl();
                        }
                        else if (isActivated && !activate)
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

                    //KEYBOARD CONTROL VIA MACRO KEYS
                    if (isActivated)
                    {
                        if (handPalmClosedCount > 0)
                            readyToClick = true;
                        else if (readyToClick)
                        {
                            if (count_defects == 4)
                            {
                                bool leftMouseButtonClicked = true;
                                bool rightMouseButtonClicked = true;

                                foreach (Finger finger in fingertipNames)
                                {
                                    if (finger == Finger.THUMB)
                                    {
                                        leftMouseButtonClicked = false;
                                    }
                                    else if (finger == Finger.LITTLE)
                                    {
                                        if (leftMouseButtonClicked)
                                            rightMouseButtonClicked = false;
                                    }
                                }

                                if (leftMouseButtonClicked && !rightMouseButtonClicked)
                                {
                                    LPPoint currentCursorPos;
                                    Native.GetCursorPos(out currentCursorPos);

                                    Native.mouse_event(Globals.MOUSEEVENT_LEFTDOWN, currentCursorPos.X, currentCursorPos.Y, 0, 0);
                                    Thread.Sleep(100);
                                    Native.mouse_event(Globals.MOUSEEVENT_LEFTUP, currentCursorPos.X, currentCursorPos.Y, 0, 0);

                                    Console.WriteLine("LEFT CLICK EVENT FIRED");
                                }
                                else if (rightMouseButtonClicked && !leftMouseButtonClicked)
                                {
                                    LPPoint currentCursorPos;
                                    Native.GetCursorPos(out currentCursorPos);

                                    Native.mouse_event(Globals.MOUSEEVENT_RIGHTDOWN, currentCursorPos.X, currentCursorPos.Y, 0, 0);
                                    Thread.Sleep(100);
                                    Native.mouse_event(Globals.MOUSEEVENT_RIGHTUP, currentCursorPos.X, currentCursorPos.Y, 0, 0);

                                    Console.WriteLine("RIGHT CLICK EVENT FIRED");
                                }

                                readyToClick = false;
                            }
                            else if (count_defects == 1 && macroComboBoxSelections[0] != null)
                            {
                                if (fingertipNames[0] == Finger.INDEX)
                                {
                                    /*IntPtr activeWindow = Native.GetForegroundWindow();
                                    StringBuilder activeWindowText = new StringBuilder(512);
                                    Native.GetWindowText(activeWindow, activeWindowText, 512);
                                    string activeWindowTextString = activeWindowText.ToString();

                                    IntPtr expectedWindow = IntPtr.Zero;
                                    foreach (Process process in Process.GetProcesses())
                                    {
                                        if (process.MainWindowTitle.Contains(activeWindowTextString))
                                            expectedWindow = process.MainWindowHandle;
                                    }
                                    if (expectedWindow != IntPtr.Zero)
                                        Native.SetForegroundWindow(expectedWindow);

                                    Console.WriteLine(activeWindowTextString);*/

                                    string macroKey = macroComboBoxSelections[0].Replace("Key", "");
                                    macroKey = RegExp.Regex.Replace(macroKey, @"\s+", "");

                                    Console.WriteLine(macroKey + " IS FIRED");

                                    SendKeys.SendWait("{" + macroKey + "}");

                                    readyToClick = false;
                                }
                            }
                            else if (count_defects == 2 && macroComboBoxSelections[1] != null)
                            {
                                if ((fingertipNames[0] == Finger.INDEX && fingertipNames[1] == Finger.MIDDLE) || (fingertipNames[0] == Finger.MIDDLE && fingertipNames[1] == Finger.INDEX))
                                {
                                    string macroKey = macroComboBoxSelections[1].Replace("Key", "");
                                    macroKey = RegExp.Regex.Replace(macroKey, @"\s+", "");

                                    Console.WriteLine(macroKey + " IS FIRED");

                                    SendKeys.SendWait("{" + macroKey + "}");

                                    readyToClick = false;
                                }
                            }
                            else if (count_defects == 3 && macroComboBoxSelections[2] != null)
                            {
                                if ((fingertipNames[0] == Finger.INDEX && fingertipNames[1] == Finger.MIDDLE && fingertipNames[2] == Finger.RING) || (fingertipNames[0] == Finger.RING && fingertipNames[1] == Finger.MIDDLE && fingertipNames[2] == Finger.INDEX))
                                {
                                    string macroKey = macroComboBoxSelections[2].Replace("Key", "");
                                    macroKey = RegExp.Regex.Replace(macroKey, @"\s+", "");

                                    Console.WriteLine(macroKey + " IS FIRED");

                                    SendKeys.SendWait("{" + macroKey + "}");

                                    readyToClick = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        x1 = -1;
                        y1 = -1;
                    }

                    MainForm mainForm = Application.OpenForms.OfType<MainForm>().First();
                    mainForm.ClearLoadingAnimationPictureBox();

                    captureImageBox.Image = frame;

                    /*Console.WriteLine("Detected FingerTip Number: " + count_defects + " Finger Index [X1]: " + x1 + " [X2]: " + x2
                        + " [Y1]: " + y1 + " [Y2]: " + y2);*/

                    //CALCULATIONS FOR DELAY (DELAY IS NEEDED TO BE PUT TO DRAW FRAMES TO THE SCREEN CORRECTLY)
                    milliSeconds2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    milliSecondsDiff = milliSeconds2 - milliSeconds;

                    if (milliSecondsDiff > 1000)
                    {
                        milliSecondsDiff = 1000;
                    }

                    frameRate = Math.Abs(1000 / milliSecondsDiff);

                    if (firstFrameCaptured)
                    {
                        Action action = new Action(() => { captureImageBox.Visible = true; });
                        captureImageBox.Invoke(action);
                    }

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


        public int GetActiveDeviceIndex()
        {
            return deviceIndex;
        }

        public void SetFirstFrameCaptured(bool firstFrameCaptured)
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

        public void FindBoundary(RectangleF detectedRectangle, RectangleF bigRectangle)
        {
            boundaryWidth = Math.Abs(((int)bigRectangle.Width - (int)detectedRectangle.Width) / 2);
            boundaryHeight = Math.Abs(((int)bigRectangle.Height - (int)detectedRectangle.Height) / 2);

            distanceX = Math.Abs(GetBigRectX() - GetDetectedRectX());
            distanceY = Math.Abs(GetBigRectY() - GetDetectedRectY());
        }

        public void SetBigRectXY(int x, int y)
        {
            bigRectangle.X = x;
            bigRectangle.Y = y;
        }
        public int GetBigRectX()
        {
            return (int)bigRectangle.X;
        }
        public int GetBigRectY()
        {
            return (int)bigRectangle.Y;
        }

        public int GetDetectedRectX()
        {
            return (int)detectedRectangle.X;
        }

        public int GetDetectedRectY()
        {
            return (int)detectedRectangle.Y;
        }

        public void SetDetectRextXY(int x, int y)
        {
            detectedRectangle.X = x;
            detectedRectangle.Y = y;
        }

        public void SetBigRectSize(float width, float height)
        {
            bigRectangle.Width = width;
            bigRectangle.Height = height;
        }
        public void MoveCursor(RectangleF detectedRectangle, RectangleF bigRectangle)
        {
            LPPoint p;
            Native.GetCursorPos(out p);

            int initialX = p.X;
            int initialY = p.Y;

            Console.WriteLine("Big Rectangle:      " + GetBigRectX() + " , " + GetBigRectY());
            Console.WriteLine("Detected Rectangle: " + detectedRectangle);
            FindBoundary(detectedRectangle, bigRectangle);

            ScreenProperties screenProperties = new ScreenProperties();

            Console.WriteLine(distanceX + " , " + distanceY + " , " + boundaryWidth + " , " + boundaryHeight + "\n");

            if ((int)detectedRectangle.X != 0 || (int)detectedRectangle.Y != 0)
            {
                if (distanceX < boundaryWidth && distanceY < boundaryHeight)
                {
                    p.X = GetBigRectX();
                    p.Y = GetBigRectY();

                    if (x1 == -1 && y1 == -1 && Math.Abs(p.X - initialX) <= 200 && Math.Abs(p.Y - initialY) <= 200)
                        Native.SetCursorPos(p.X, p.Y);
                    else
                        Native.SetCursorPos(p.X, p.Y);

                    Console.WriteLine("In the Rectangle " + bigRectangle);
                }
                else
                {
                    p.X = GetDetectedRectX();
                    p.Y = GetDetectedRectY();

                    if (x1 == -1 && y1 == -1 && Math.Abs(p.X - initialX) <= 200 && Math.Abs(p.Y - initialY) <= 200)
                        Native.SetCursorPos(p.X, p.Y);
                    else
                        Native.SetCursorPos(p.X, p.Y);

                    SetBigRectXY(p.X, p.Y);                   
                }

                x1 = p.X;
                y1 = p.Y;
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
            if (start == null)
            {
                start = new Thread(() =>
                {
                    while (capture == null) ;
                    while (!canStartCamera) ;
                    captureInProgress = true;
                    capture.Start();
                });
                start.IsBackground = true;
                start.Start();
            }
            else if (!start.IsAlive)
            {
                start = new Thread(() =>
                {
                    while (capture == null) ;
                    while (!canStartCamera) ;
                    captureInProgress = true;
                    capture.Start();
                });
                start.IsBackground = true;
                start.Start();
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

        public bool IsActive()
        {
            return captureInProgress;
        }

        public Size GetFrameSize()
        {
            return new Size((int)width, (int)height);
        }

        public void SetMacroComboBoxSelections(string[] macroComboBoxSelections)
        {
            this.macroComboBoxSelections = macroComboBoxSelections;
        }
    }
}
