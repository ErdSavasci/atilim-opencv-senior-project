﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MoveCursorByHand.App_Code
{
    class FingertipRecognizer
    {
        private List<Finger> fingertipNames;
        private bool indexFound;
        private bool thumbFound;

        public FingertipRecognizer()
        {
            fingertipNames = new List<Finger>();
        }

        public void NameFingers(ref Mat frame, Point centerPoint, int contourAxisAngle, ref List<Point> fingertipCoordinates, bool leftHandPos, int count_defects)
        {
            fingertipNames.Clear();
            for (int i = 0; i < 5; i++)
            {
                fingertipNames.Add(Finger.UNKNOWN);
            }

            LabelThumbIndex(fingertipCoordinates, ref fingertipNames, centerPoint, contourAxisAngle, leftHandPos, count_defects);
            labelUnknownFingertips(ref fingertipNames, count_defects);
            if (leftHandPos)
                fingertipNames.Reverse();
            DrawFingertipNames(ref frame, fingertipCoordinates, fingertipNames, leftHandPos);
        }

        private void LabelThumbIndex(List<Point> fingertipCoordinates, ref List<Finger> fingertipNames, Point centerPoint, int contourAxisAngle, bool leftHandPos, int count_defects)
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

        public void labelUnknownFingertips(ref List<Finger> fingertipNames, int count_defects)
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

        public void LabelUnknownFingertipsByOrder(ref List<Finger> fingertipNames, int i, Finger finger, string order)
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

        public void DrawFingertipNames(ref Mat frame, List<Point> fingertipCoordinates, List<Finger> fingertipNames, bool leftHandPos)
        {
            int rev = Math.Min(fingertipCoordinates.Count, 5);
            if (rev == 5 || !leftHandPos) //NO UNKNOWN FINGERS
            {
                for (int i = 0; i < Math.Min(fingertipCoordinates.Count, 5); i++)
                {
                    if (fingertipNames[i] != Finger.UNKNOWN)
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
    }
}