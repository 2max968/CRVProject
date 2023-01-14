using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CRVProject.Helper;
using OpenCvSharp;

namespace CRVProject.Ortsschild;

public class Locator : IDisposable
{
    private Mat image;
    public List<Mat> Ortsschilder { get; private set; } = new List<Mat>();
    public List<Point2f[]> Contours { get; private set; } = new List<Point2f[]>();
    public Mat? BinarizedImage = null;
    public Mat? Corners;

    public Locator(Mat image)
    {
        this.image = image;
    }

    public void Binarize()
    {
        double hue = Configuration.Instance.Locator.HueValue;
        double hueTolerance = Configuration.Instance.Locator.HueTolerance;

        // Binarize image to filter all yellow areas
        double hueMin = hue - hueTolerance;
        double hueMax = hue + hueTolerance;
        using Mat hsv = new Mat();
        Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
        var meanValues = Cv2.Mean(hsv);
        double minValue = meanValues.Val3 * Configuration.Instance.Locator.Brightness;
        BinarizedImage = new Mat();
        Cv2.InRange(hsv,
            new Scalar(hueMin, 150, minValue),
            new Scalar(hueMax, 256, 256),
            BinarizedImage);
    }

    public void RunLocator()
    {
        var cfg = Configuration.Instance.Locator;
        Stopwatch stp = new Stopwatch();
        stp.Start();

        if (BinarizedImage == null)
            Binarize();
        
        // Dilate and Erode image to close small gaps in yellow areas
        int kernelSize = (int)(image.Height * cfg.DilationErotionSize);
        using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(kernelSize, kernelSize));
        Cv2.Dilate(BinarizedImage, BinarizedImage, kernel);
        Cv2.Erode(BinarizedImage, BinarizedImage, kernel);

        var contours = Cv2.FindContoursAsArray(/*contourImage*/BinarizedImage, RetrievalModes.List, ContourApproximationModes.ApproxNone);

        Array.Sort(contours, (a, b) =>
        {
            double diff = Cv2.ContourArea(b) - Cv2.ContourArea(a);
            if (diff < 0)
                return -1;
            if (diff == 0)
                return 0;
            return 1;
        });
        
        foreach (var contour in contours)
        {
            var conLength = Cv2.ArcLength(contour, true);
            var conArea = Cv2.ContourArea(contour, false);
            var relArea = conArea / (image.Width * image.Height);
            var approx = Cv2.ApproxPolyDP(contour, 0.02 * conLength, true);

            if (approx?.Length == 4 && relArea > cfg.AreaThreshhold)
            {
                

                // Extract sign from image
                var rectIn = approx.Select(p => new Point2f(p.X, p.Y)).ToArray();
                rectIn = MakeContourClockwise(rectIn);
                var rectOut = new Point2f[]
                {
                    new Point2f(0, 0),
                    new Point2f(cfg.OutputWidth, 0),
                    new Point2f(cfg.OutputWidth, cfg.OutputHeight),
                    new Point2f(0, cfg.OutputHeight)
                };
                Mat output = new Mat();
                Mat transform = Cv2.GetPerspectiveTransform(rectIn, rectOut);
                Cv2.WarpPerspective(image, output, transform, new Size(cfg.OutputWidth, cfg.OutputHeight));
                Contours.Add(rectIn);
                Ortsschilder.Add(output);
            }
        }
    }

    void rotateContour(Point2f[] points)
    {
        var tmp = points[0];
        for (var i = 1; i < points.Length; i++)
            points[i - 1] = points[i];
        points[^1] = tmp;
    }

    public Point2f[] MakeContourClockwise(Point2f[] points)
    {
        Debug.Assert(points.Length == 4);

        if (Cv2.ContourArea(points, true) < 0)
        {
            (points[0], points[1]) = (points[1], points[0]);
            (points[2], points[3]) = (points[3], points[2]);
        }

        int rotations = 0;
        while (true)
        {
            var p1 = points[^1];
            var p2 = points[0];
            var p3 = points[1];
            var p4 = points[2];
            if (p1.Y > p2.Y && p2.X < p3.X && p2.Y < p4.Y && p2.X < p4.X)
                break;
            rotateContour(points);
            rotations++;
            if (rotations > 4)
                break;
        }

        return points;
    }

    public void Dispose()
    {
        image.Dispose();
        BinarizedImage?.Dispose();
        foreach (var img in Ortsschilder)
        {
            img?.Dispose();
        }
    }

    public double[] GetAngles(Point2f[] points)
    {
        var angles = new double[points.Length];
        for(int i = 0; i < points.Length; i++)
        {
            var p1 = points[(i - 1 + points.Length) % points.Length];
            var p2 = points[(i) % points.Length];
            var p3 = points[(i + 1) % points.Length];
            
            var p12 = p2 - p1;
            var p23 = p3 - p2;
            var p12_length = p12.DistanceTo(new Point2f(0,0));
            var p23_length = p23.DistanceTo(new Point2f(0, 0));
            double prod = Point2f.DotProduct(p12, p23);
            angles[i] = Math.Acos(prod / (p12_length * p23_length));
            angles[i] *= 180.0 / Math.PI;
        }
        return angles;
    }

    public (double min, double max, double span) GetAnglesMinMax(Point2f[] points)
    {
        var angles = GetAngles(points);
        var min = angles.Min();
        var max = angles.Max();
        var span = max - min;
        return (min, max, span);
    }

    public double GetAspectRatio(Point2f[] points)
    {
        double width1 = points[1].X - points[0].X;
        double width2 = points[2].X - points[3].X;
        double height1 = points[3].Y - points[0].Y;
        double height2 = points[2].Y - points[1].Y;
        double width = (width1 + width2) / 2.0;
        double height = (height1 + height2) / 2.0;
        return width / height;
    }
}