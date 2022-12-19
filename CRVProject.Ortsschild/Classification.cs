using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRVProject.Ortsschild
{
    public class Classification
    {
        public static Schildtyp Classify(Mat image)
        {
            if (CalculateSharpness(image) < 1)
                return Schildtyp.Unscharf;

            using Mat hsv = new Mat();
            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
            using Mat bin1 = new Mat();
            using Mat bin2 = new Mat();
            Cv2.InRange(hsv, new Scalar(0, 150, 100), new Scalar(5, 256, 256), bin1);
            Cv2.InRange(hsv, new Scalar(250, 150, 100), new Scalar(255, 256, 256), bin2);
            using Mat bin = bin1 | bin2;

            int kernelSize = 30;
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(kernelSize, kernelSize));
            Cv2.Erode(bin, bin, kernel);
            Cv2.Dilate(bin, bin, kernel);

            List<Point> contours = new List<Point>();
            foreach (var contour in Cv2.FindContoursAsArray(bin, RetrievalModes.List, ContourApproximationModes.ApproxNone))
                contours.AddRange(contour);
            

            if (contours.Count > 0)
            {
                var boundingBox = Cv2.BoundingRect(contours);

                var relHeight = boundingBox.Height / (float)image.Height;
                if (relHeight > 0.8)
                    return Schildtyp.OrtsausfahrtWeiss;
                else if (relHeight < 0.55 && relHeight > 0.35)
                    return Schildtyp.Ortsausfahrt;
                else
                    return Schildtyp.Unbekannt;
            }

            return Schildtyp.Ortseinfart;
        }

        public static double CalculateSharpness(Mat image)
        {
            using Mat diff = new Mat();
            using Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.Sobel(gray, diff, MatType.CV_32FC1, 1, 1);
            using Mat diffSquared = diff.Mul(diff);
            return Cv2.Mean(diffSquared).Val0;
        }
    }

    public enum Schildtyp
    {
        Ortseinfart,
        Ortsausfahrt,
        OrtsausfahrtWeiss,
        Unscharf,
        Unbekannt
    }
}
