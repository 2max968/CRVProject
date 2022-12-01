using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CRVProject.Helper;
using OpenCvSharp;

namespace CRVProject.Ortsschild;

public class Locator : IDisposable
{
    private Mat image;
    public float AreaThreshhold = 0.0002f;
    public float DilationErotionSize = 0.01f;
    public List<Mat> Ortsschilder { get; private set; } = new List<Mat>();
    public Mat? BinarizedImage = null;
    public int OutputWidth = 900;
    public int OutputHeight = 600;
    public Mat? CutoutImage;
    public Mat? Corners;
    public Mat? CannyImage;

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
        Stopwatch stp = new Stopwatch();
        stp.Start();

        if (BinarizedImage == null)
            Binarize();
        
        CannyImage = new Mat();
        Cv2.Canny(BinarizedImage, CannyImage, 127, 128);
        
        // Dilate and Erode image to close small gaps in yellow areas
        int kernelSize = (int)(image.Height * DilationErotionSize);
        using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(kernelSize, kernelSize));
        Cv2.Dilate(BinarizedImage, BinarizedImage, kernel);
        Cv2.Erode(BinarizedImage, BinarizedImage, kernel);
        
        // Detect Contours in binarized image
        //using var contourImage = new Mat();
        //Cv2.Canny(BinarizedImage, contourImage, 127, 256);

        var contours = Cv2.FindContoursAsArray(/*contourImage*/BinarizedImage, RetrievalModes.List, ContourApproximationModes.ApproxNone);
        foreach (var contour in contours)
        {
            var conLength = Cv2.ArcLength(contour, true);
            var conArea = Cv2.ContourArea(contour, false);
            var relArea = conArea / (image.Width * image.Height);
            var approx = Cv2.ApproxPolyDP(contour, 0.02 * conLength, true);

            if (approx?.Length == 4 && relArea > AreaThreshhold)
            {
                // Extract sign from image
                var rectIn = approx.Select(p => new Point2f(p.X, p.Y));
                rectIn = MakeContourClockwise(rectIn.ToArray());
                var rectOut = new Point2f[]
                {
                    new Point2f(0, 0),
                    new Point2f(OutputWidth, 0),
                    new Point2f(OutputWidth, OutputHeight),
                    new Point2f(0, OutputHeight)
                };
                Mat output = new Mat();
                Mat transform = Cv2.GetPerspectiveTransform(rectIn, rectOut);
                Cv2.WarpPerspective(image, output, transform, new Size(OutputWidth, OutputHeight));
                Ortsschilder.Add(output);

                //cutoutContour(contour);
                var cornerRoi = Cv2.BoundingRect(contour);
                int expand = cornerRoi.Height / 5;
                cornerRoi.X -= expand / 2;
                cornerRoi.Y -= expand / 2;
                cornerRoi.Width += expand;
                cornerRoi.Height += expand;
                Corners = image[cornerRoi].Clone();
                rectIn = FindCorners(CannyImage[cornerRoi], contour);
                var lines = Cv2.HoughLinesP(CannyImage[cornerRoi], 1, Math.PI / 180, (int)conLength / 8);
                foreach (var line in lines)
                    Cv2.Line(Corners, line.P1, line.P2, new Scalar(0, 255, 0));
                Cv2.Ellipse(Corners, new Point(10, 10), new Size(4, 4), 0, 0, 360, new Scalar(0, 0, 255));
                foreach (var point in rectIn)
                    Cv2.Ellipse(Corners, point.ToPoint(), new Size(4, 4), 0, 0, 360, new Scalar(0, 0, 255));
            }
        }
    }

    void cutoutContour(Point[]? contour)
    {
        if (contour == null)
            return;
        CutoutImage = new Mat(image.Height, image.Width, MatType.CV_8UC1);
        CutoutImage.SetTo(new Scalar(0, 0, 0));
        Cv2.DrawContours(CutoutImage, new[] { contour }, -1, new Scalar(255), 1);
        var lines = Cv2.HoughLinesP(CutoutImage, 10, 10, contour.Length / 64);
        CutoutImage = new Mat(image.Height, image.Width, MatType.CV_8UC3);
        foreach (var line in lines)
            Cv2.Line(CutoutImage, line.P1, line.P2, new Scalar(0, 0, 255), 11, LineTypes.AntiAlias);
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

        while (true)
        {
            var p1 = points[^1];
            var p2 = points[0];
            var p3 = points[1];
            if (p1.Y > p2.Y && p2.X < p3.X)
                break;
            rotateContour(points);
        }

        return points;
    }

    public void Dispose()
    {
        image.Dispose();
        BinarizedImage?.Dispose();
        CutoutImage?.Dispose();
        foreach (var img in Ortsschilder)
        {
            img?.Dispose();
        }
    }

    [return: NotNullIfNotNull("mat")]
    [return: NotNullIfNotNull("contour")]
    public Point2f[]? FindCorners(Mat? mat, Point[]? contour)
    {
        if (mat == null || contour == null)
            return null;

        double conLength = Cv2.ArcLength(contour, true);
        using Mat img = new Mat(mat.Height, mat.Width, MatType.CV_8UC1);
        Cv2.DrawContours(img, new[] { contour }, 0, new Scalar(255));
        Cv2.GaussianBlur(img, img, new Size(19, 19), 1);
        Point2f[] points = new Point2f[4];
        var corners = Cv2.GoodFeaturesToTrack(img, 4, 0.5f, conLength / 8, null, mat.Height / 100, false, 0);
        return corners;

    }
}