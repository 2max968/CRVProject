using CRVProject.Helper;
using CRVProject.Ortsschild;
using OpenCvSharp;

Configuration.LoadConfiguration();

var frameSize = new Size(800, 600);

VideoCapture vid = new VideoCapture();
vid.Open("vid.mp4");
VideoWriter outp = new VideoWriter();
outp.Open("outp.mp4", FourCC.Default, 
    vid.Fps, 
    frameSize);

while (true)
{
    using var frame = new Mat();
    if (!vid.Read(frame))
    {
        vid.PosFrames = 0;
        break;
    }
    Cv2.Resize(frame, frame, frameSize, interpolation: InterpolationFlags.Linear);
    using var locator = new Locator(frame);
    locator.RunLocator();
    if (locator.Contours.Count > 0)
    {
        var contour = locator.Contours[0];
        Cv2.DrawContours(frame, new[] { contour.Select(p => p.ToPoint()) }, -1, new Scalar(0, 255, 0));
        var ang = locator.GetAnglesMinMax(contour);
        var aspect = locator.GetAspectRatio(contour);
        Cv2.PutText(frame, $"Angles: [{ang.min}°; {ang.max}°]", new Point(4, 16), HersheyFonts.HersheyPlain, 1, new Scalar(0, 0, 255));
        Cv2.PutText(frame, $"Aspect Ratio: [{aspect}]", new Point(4, 32), HersheyFonts.HersheyPlain, 1, new Scalar(0, 0, 255));
    }
    outp.Write(frame);
    Cv2.ImShow("---", frame);
    Cv2.WaitKey(1);
}

vid.Dispose();
outp.Dispose();