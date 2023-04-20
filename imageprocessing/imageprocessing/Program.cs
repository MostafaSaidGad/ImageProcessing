using OpenCvSharp;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace ImageProcessing
{
    class program
    {
        static void Main(string[] args)
        {
            string path = "C:\\Users\\Andrew Seha\\Desktop\\Mostafa's project\\Laboratory Project\\rqim-barkhas-master\\rqim-barkhas-master\\images\\t10";
            Mat pic = Cv2.ImRead(path, ImreadModes.Grayscale);
            Mat pic2 = Cv2.ImRead(path, ImreadModes.Color);


            Mat pic_thresh = new Mat();
            Cv2.Threshold(pic, pic_thresh, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

            Point[][] contours;
            HierarchyIndex[] Hierarchy;
            Cv2.FindContours(pic_thresh, out contours, out Hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Cv2.DrawContours(pic2, contours, -1, Scalar.Red, 1);
            Cv2.ImShow("PicContour", pic2);

            for (int i = 0; i < contours.Length; i++)
            {
                //Get the index of the parent contour for the current contour
                int parentIdx = Hierarchy[i].Parent;

                // Check if the current contour has a parent contour
                if (parentIdx >= 0)
                {
                    // Draw a line connecting the current contour to its parent contour
                 Cv2.Line(pic, contours[i][0], contours[parentIdx][0], Scalar.Red, 2);
                }
            }

           Cv2.ImShow("Contour Hierarchy", pic);
            Cv2.WaitKey(0);
        }
    }
}