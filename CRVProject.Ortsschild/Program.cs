using CRVProject.Ortsschild;
using CRVProject.Helper;
using OpenCvSharp;
using System.Diagnostics;

Configuration.LoadConfiguration();

DirectoryInfo imageDirectory = new DirectoryInfo("Images");
if (!imageDirectory.Exists)
{
    Console.WriteLine("Cant find input directory 'Images'");
    return 1;
}

var images = imageDirectory.GetFiles()
    .Where(fi => Util.SupportedImageTypes
        .Select(type => fi.Name.ToLower().EndsWith($".{type}") || fi.Name.ToLower().EndsWith(".mp4"))
        .Contains(true))
    .ToArray();

while (true)
{
    int selectedImage = Util.SelectGUI(true, images.Select(fi=>fi.Name).ToArray());
    if (selectedImage < 0)
        return 0;

    if (images[selectedImage].Extension.ToLower() == ".mp4")
    {
        VideoCapture video = new VideoCapture();
        video.Open(images[selectedImage].FullName);
        double fps = video.Fps;
        int frametimeMillis = (int)(1000 / fps);
        string title = Guid.NewGuid().ToString();

        int w = 600;
        int h = 800;

        while (true)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            Mat frame = new Mat();
            if (!video.Read(frame))
            {
                video.PosFrames = 0;
                continue;
            }
            Locator loc = new Locator(frame);
            loc.Binarize();
            Cv2.Resize(frame, frame, new Size(w, h));
            Cv2.Resize(loc.BinarizedImage, loc.BinarizedImage, new Size(w, h));
            Cv2.CvtColor(loc.BinarizedImage, loc.BinarizedImage, ColorConversionCodes.GRAY2BGR);
            Mat showImage = new Mat(h, w * 2, MatType.CV_8UC3);
            frame.CopyTo(showImage[new Rect(0, 0, w, h)]);
            loc.BinarizedImage.CopyTo(showImage[new Rect(w, 0, w, h)]);
            Cv2.ImShow(title, showImage);
            loc.Dispose();
            int waitMillis = frametimeMillis - (int)stopwatch.ElapsedMilliseconds;
            if (waitMillis <= 0) waitMillis = 1;
            int key = Cv2.WaitKey(waitMillis);
            if (key == 'd')
                video.PosFrames += 60;
            if (key == 'a')
                video.PosFrames -= 60;
            if (Cv2.GetWindowProperty(title, WindowPropertyFlags.Visible) == 0)
                break;
        }
    }
    else
    {
        using var image = Cv2.ImRead(images[selectedImage].FullName);
        using var locator = new Locator(image);
        Util.PixelInfoWindow(image);

        locator.RunLocator();

        if (locator.CutoutImage != null)
            Cv2.ImWrite("cutout.png", locator.CutoutImage);

        ImageGridWindow wnd = new ImageGridWindow(3, 1 + (locator.Ortsschilder.Count + 2 / 3));
        wnd.SetImage(0, 0, image);
        wnd.SetImage(1, 0, locator.BinarizedImage);
        wnd.SetImage(2, 0, locator.Corners);
        for (int i = 0; i < locator.Ortsschilder.Count; i++)
            wnd.SetImage(i % 3, i / 3 + 1, locator.Ortsschilder[i]);
        wnd.Run();
    }
}
