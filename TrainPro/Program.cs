using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace ImageProcessing
{
    class program
    {
        static void Main(string[] args)
        {
            string path = "C:\\Users\\DELL\\Desktop\\Mostafa's Project\\My Project\\t12.png";
            //string path = "C:\\Users\\DELL\\Desktop\\Mostafa's Project\\My Project\\t11.png";
            //string path = "C:\\Users\\DELL\\Desktop\\Mostafa's Project\\My Project\\t13.png";
            //string path = "C:\\Users\\DELL\\Desktop\\Mostafa's Project\\My Project\\t10.png";

            Mat pic = Cv2.ImRead(path, ImreadModes.Grayscale);
            Mat pic2 = Cv2.ImRead(path, ImreadModes.Color);

            Mat pic_thresh = new Mat();
            Cv2.Threshold(pic, pic_thresh, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

            Cv2.ImShow("Threshold", pic_thresh);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] Hierarchy;
            Cv2.FindContours(pic_thresh, out contours, out Hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.DrawContours(pic2, contours, -1, Scalar.Red, 1);
            Cv2.ImShow("Primary Contours", pic2);

            //int minContourLength = 100; // Set the minimum contour length
            //int minContourLength = 50;
            //int minContourLength = 40;
            int minContourLength = 25;
            int Good_Contour = 0;
            List<int> Good_Contour_Ref = new List<int>();
            List<double> GrayMean = new List<double>();

            for (int i = 0; i < contours.Length; i++)
            {
                // Check if the length of the contour is greater than or equal to the minimum length
                if (contours[i].Length >= minContourLength)
                {
                    // Draw the contour on the original image with a green color and a thickness of 1
                    Cv2.DrawContours(pic2, new OpenCvSharp.Point[][] { contours[i] }, -1, Scalar.Blue, 1);
                    Good_Contour_Ref.Add(i);
                    Good_Contour++;
                    Cv2.ImShow("Contours after filtration", pic2);

                    // Create a mask for the contour
                    Mat Contour_Mask = Mat.Zeros(pic.Rows, pic.Cols, MatType.CV_8UC1);
                    Cv2.DrawContours(Contour_Mask, contours, i, 255, -1);

                    // Calculate the mean gray value inside the contour
                    Scalar meanGray = Cv2.Mean(pic, Contour_Mask);
                    double grayMeanValue = meanGray.Val0;
                    GrayMean.Add(grayMeanValue);

                    Console.WriteLine("Contour #{0},\tGrayMean={1}", i, grayMeanValue);
                }
            }
            Console.WriteLine("Number of all detected contours after thresholding = {0}", contours.Length);
            Console.WriteLine("Number of all good detected contours (after filtration) = {0}", Good_Contour);  // The number of contours with length >= minContourLength
            Console.Write("----------------------------------------------------------------\n");

            // Find the mean and standard deviation of the gray mean values
            double mean = GrayMean.Average();
            double variance = GrayMean.Select(val => (val - mean) * (val - mean)).Sum() / (GrayMean.Count - 1);
            double standardDeviation = Math.Sqrt(variance);

            // Calculate the appropriate cutoff value
            double cutoff = mean + 0.75 * standardDeviation;
            //double cutoff = mean + 3 * standardDeviation;

            // Scan for good quads
            int goodQuadCount = 0;
            List<int> goodQuadReferences = new List<int>();
            for (int i = 0; i < contours.Length; i++)
            {
                // Check if the length of the contour is greater than or equal to the minimum length
                if (contours[i].Length >= minContourLength)
                {
                    // Create a mask for the contour
                    Mat contourMask = Mat.Zeros(pic.Rows, pic.Cols, MatType.CV_8UC1);
                    Cv2.DrawContours(contourMask, contours, i, 255, -1);

                    // Compute the mean gray value inside the contours
                    Scalar meanGray = Cv2.Mean(pic, contourMask);
                    double grayMeanValue = meanGray.Val0;

                    // Check if the gray mean value is less than the cutoff value
                    if (grayMeanValue < cutoff)
                    {
                        Cv2.DrawContours(pic2, contours, i, Scalar.Green, Cv2.FILLED);
                        goodQuadReferences.Add(i);
                        goodQuadCount++;
                    }
                }
            }
            Cv2.ImShow("Finalized Contours", pic2);

            //print good quads ref number
            Console.WriteLine("\nGood Quad Reference: ");
            for (int i = 0; i < goodQuadReferences.Count; i++)
            {
                Console.WriteLine(goodQuadReferences[i]);
            }
            Console.WriteLine("totall detected good quads ={0}", goodQuadReferences.Count);
            Console.Write("----------------------------------------------------------------\n");


            // detecting corner points using APROXIMATEPOLYDP algorithm
            //double Epsilon = 0.5;
            double Epsilon = 0.03;
            //double Epsilon = 0.1;

            OpenCvSharp.Point[][] NotFilteredPoints;
            NotFilteredPoints = new OpenCvSharp.Point[goodQuadReferences.Count][];
            for (int k = 0; k < goodQuadReferences.Count; k++)
            {
                NotFilteredPoints[k] = Cv2.ApproxPolyDP(contours[goodQuadReferences[k]], Epsilon * Cv2.ArcLength(contours[goodQuadReferences[k]], true), true);
            }
            for (int k = 0; k < NotFilteredPoints.Length; k++)
            {
                for (int i = 0; i < NotFilteredPoints[k].Length; i++)
                {
                    Cv2.Circle(pic2, NotFilteredPoints[k][i].X, NotFilteredPoints[k][i].Y, 1, Scalar.Red, 2);
                }
            }
            Cv2.ImShow("PicWithNonFilteredPoints", pic2);

            /// filtering out close corner points
            OpenCvSharp.Point[][] FilteredPoints = NotFilteredPoints;
            /////////////////close points filtration constant////////////
            //double filtrationconstant = 0.05;
            //double filtrationconstant = 0.0.03;
            //double filtrationconstant = 0.015;
            double filtration_constant = 0.025;

            for (int k = 0; k < FilteredPoints.Length; k++)
            {
                for (int i = 0; i < FilteredPoints[k].Length; i++)
                {

                    for (int j = 0; j < FilteredPoints[k].Length; j++)
                    {
                        double distance_between_two_points = Math.Sqrt(Math.Pow((FilteredPoints[k][i].X - FilteredPoints[k][j].X), 2) + Math.Pow((FilteredPoints[k][i].Y - FilteredPoints[k][j].Y), 2));

                        if (distance_between_two_points < filtration_constant * Cv2.ArcLength(contours[goodQuadReferences[k]], true)) 
                        {
                            int midx = (FilteredPoints[k][i].X + FilteredPoints[k][j].X) / 2;
                            int midy = (FilteredPoints[k][i].Y + FilteredPoints[k][j].Y) / 2;
                            FilteredPoints[k][i].X = midx;
                            FilteredPoints[k][j].X = midx;
                            FilteredPoints[k][i].Y = midy;
                            FilteredPoints[k][j].Y = midy;                            
                        }
                    }
                }
            }


            //draw filterd points
            for (int k = 0; k < FilteredPoints.Length; k++)
            {
                for (int i = 0; i < FilteredPoints[k].Length; i++)
                {
                    Cv2.Circle(pic2, FilteredPoints[k][i].X, FilteredPoints[k][i].Y, 1, Scalar.Black, 2); //drawing fitted points between the close points
                    //Debug.WriteLine(FilteredPoints[k][i].X); Debug.WriteLine(FilteredPoints[k][i].Y);
                }
                 if (FilteredPoints[k].Length > 2)
                 {
                     for (int i = 0; i < FilteredPoints[k].Length - 1; i++)
                     {
                         Cv2.Line(pic2, FilteredPoints[k][i], FilteredPoints[k][i + 1], Scalar.Black, 1); //Drawing fitted lines between the points of the same contour
                     }
                 }
            }

                Cv2.ImShow("Filtered Corner Points + Ftted Lines", pic2);


                // Create a HashSet<Point> to store unique points
                 HashSet<OpenCvSharp.Point>[] finalSet = new HashSet<OpenCvSharp.Point>[FilteredPoints.Length];

                 // Iterate over the rows of the filteredPoints array
                 for (int k = 0; k < FilteredPoints.Length; k++)
                 {
                     finalSet[k] = new HashSet<OpenCvSharp.Point>();

                     // Iterate over the points in the current row
                     foreach (OpenCvSharp.Point point in FilteredPoints[k])
                     {
                         finalSet[k].Add(point);
                     }
                 }

                 foreach (HashSet<OpenCvSharp.Point> set in finalSet)
                 {
                     foreach (OpenCvSharp.Point point in set)
                     {
                         Console.Write("{0}\t", point);
                     }
                     Console.WriteLine();
                 }

                Cv2.WaitKey(0);
          
        }
    }
}
