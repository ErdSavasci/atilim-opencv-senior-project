using Accord.Extensions.Imaging.Algorithms.LINE2D;
using Accord.Extensions.Math.Geometry;
using DotImaging;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplatePyramid = Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplatePyramid<Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplate>;

namespace MoveCursorByHand.App_Code
{
    class FastTemplateMatching
    {
        private List<TemplatePyramid> templatePyr;

        public FastTemplateMatching()
        {

        }

        public async void InitializeFastTemplateMatching()
        {
            templatePyr = await Task.Run(() => FromRightHandFilesAsync());
        }

        public async void ChangeFastTemplateMatchingHandPos(bool rightHandPos)
        {
            templatePyr.Clear();
            if (rightHandPos)
            {
                templatePyr = await Task.Run(() => FromRightHandFilesAsync());
            }
            else
            {
                templatePyr = await Task.Run(() => FromLeftHandFilesAsync());
            }
        }

        public async Task<List<TemplatePyramid>> FromRightHandFilesAsync()
        {
            List<TemplatePyramid> list = new List<TemplatePyramid>();
            string curDir = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            curDir = curDir.Substring(0, curDir.LastIndexOf("\\"));
            string resDir = Path.Combine(curDir, "Resources", "RightHand_BW");
            string[] files = Directory.GetFiles(resDir, "*.jpg");

            object synObj = new object();
            await Task.Run(() =>
            {
                Parallel.ForEach(files, delegate (string file)
                {
                    Gray<byte>[,] preparedBWImage = ImageIO.LoadGray(file).Clone();

                    try
                    {
                        TemplatePyramid tp = TemplatePyramid.CreatePyramidFromPreparedBWImage(preparedBWImage, new FileInfo(file).Name);
                        lock (synObj)
                        {
                            list.Add(tp);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
            });

            return list;
        }

        public async Task<List<TemplatePyramid>> FromLeftHandFilesAsync()
        {
            List<TemplatePyramid> list = new List<TemplatePyramid>();
            string curDir = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            curDir = curDir.Substring(0, curDir.LastIndexOf("\\"));
            string resDir = Path.Combine(curDir, "Resources", "LeftHand_BW");
            string[] files = Directory.GetFiles(resDir, "*.jpg");

            object synObj = new object();
            await Task.Run(() =>
            {
                Parallel.ForEach(files, delegate (string file)
                {
                    Gray<byte>[,] preparedBWImage = ImageIO.LoadGray(file).Clone();

                    try
                    {
                        TemplatePyramid tp = TemplatePyramid.CreatePyramidFromPreparedBWImage(preparedBWImage, new FileInfo(file).Name);
                        lock (synObj)
                        {
                            list.Add(tp);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
            });

            return list;
        }

        public List<Match> FindHandByFastTemplateMatching(ref Mat bgrFrame, bool isActivated)
        {
            if (templatePyr != null)
            {
                int downSampleCount = 1;
                Mat scaledBgrFrame = new Mat();
                scaledBgrFrame = bgrFrame.Clone();
                int threshold = 100;
                if (isActivated)
                    threshold = 200;
                while (scaledBgrFrame.Height > threshold) //80X80 or 160x160 is good
                {
                    CvInvoke.PyrDown(scaledBgrFrame, scaledBgrFrame);
                    downSampleCount *= 2;
                }

                List<Match> bestMatchList = new List<Match>();
                List<Match> matchList = new List<Match>();
                LinearizedMapPyramid linMapPyr = LinearizedMapPyramid.CreatePyramid(scaledBgrFrame.Bitmap.ToArray().ToGray());
                matchList = linMapPyr.MatchTemplates(templatePyr, 88);
                IList<Cluster<Match>> matchGroups = new MatchClustering(0).Group(matchList.ToArray());

                Match downSampleInfo = new Match();
                downSampleInfo.Score = downSampleCount;
                bestMatchList.Insert(0, downSampleInfo);

                foreach (var item in matchGroups)
                {
                    bestMatchList.Add(item.Representative);
                }

                return bestMatchList;
            }
            else
            {
                return null;
            }
        }
    }
}
