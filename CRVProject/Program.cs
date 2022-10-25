using OpenCvSharp;
using CRVProject;

while (true)
{
    int ind = Util.Select("Start a program:", "Otsu", "HUE Shift", "Show Webcam", "Write Text", "Exit");
    Console.WriteLine("Seleced option " + ind);

    // Otsu
    if (ind == 0)
    {
        // Bild aus JPEG-Datei laden
        Mat image = Cv2.ImRead("Image.jpg");

        // Bild in Graustufe konvertieren
        Mat grayscale = new Mat();
        Cv2.CvtColor(image, grayscale, ColorConversionCodes.RGB2GRAY);

        // Ostumethode zum Binarisieren anwenden
        Mat otsu = new Mat();
        Cv2.Threshold(grayscale, otsu, 0, 255, ThresholdTypes.Otsu);

        // Bilder anzeigen
        Cv2.ImShow("Original", image);
        Cv2.ImShow("Binarized", otsu);

        // Auf Tastatureingabe wartem
        Cv2.WaitKey();

        // Alle Fenster schließen
        Cv2.DestroyAllWindows();

        // Matrizen aus RAM löschen
        image.Dispose();
        grayscale.Dispose();
        otsu.Dispose();
    }
    // HUE Shift
    else if (ind == 1)
    {
        Mat image = Cv2.ImRead("Image.jpg");
        Mat hsv = new Mat();
        Cv2.CvtColor(image, hsv, ColorConversionCodes.RGB2HSV);

        Cv2.ImShow("Output", image);
        Cv2.CreateTrackbar("HUE", "Output", 255, (pos, userData) =>
        {
            Mat[] hsv_channels = Cv2.Split(hsv);
            hsv_channels[0] += pos;
            Mat hsv_clone = new Mat();
            Cv2.Merge(hsv_channels, hsv_clone);
            Mat rgb = new Mat();
            Cv2.CvtColor(hsv_clone, rgb, ColorConversionCodes.HSV2RGB);
            Cv2.ImShow("Output", rgb);
        });
        Cv2.SetTrackbarMax("HUE", "Output", 128);
        Cv2.SetTrackbarMin("HUE", "Output", -128);

        Cv2.WaitKey();
        Cv2.DestroyAllWindows();
    }
    // Show Webcam
    else if (ind == 2)
    {
        VideoCapture webcam = new VideoCapture();
        webcam.Open(0);
        Mat frame = new Mat();
        while (true)
        {
            webcam.Read(frame);
            Cv2.ImShow("Webcam", frame);
            if (Cv2.WaitKey(16) >= 0)
                break;
        }
        Cv2.DestroyAllWindows();
        frame.Dispose();
    }
    else if(ind == 3)
    {
        Mat image = new Mat(400, 600, MatType.CV_8UC3);
        Cv2.Rectangle(image, new Rect(0, 0, image.Width, image.Height), new Scalar(255, 255, 255), -1);
        Cv2.PutText(image, "Hallo Welt", new Point(4, 16), HersheyFonts.HersheyPlain, 1, new Scalar(0, 255, 0));
        Cv2.ImShow("Text", image);
        Cv2.WaitKey();
        Cv2.DestroyAllWindows();
        image.Dispose();
    }
    // Exit
    else
    {
        return;
    }
}