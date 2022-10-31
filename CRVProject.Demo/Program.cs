using OpenCvSharp; //OpenCv Library
using CRVProject.Helper;

while (true)
{
    int ind = Util.SelectGUI(false, "Otsu", "HUE Shift", "Show Webcam", "Write Text", "Show Grid Window", "Draw Histogram", "Zoomable Window", "Exit");
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
        // Bild aus JPEG Datei laden
        Mat image = Cv2.ImRead("Image.jpg");

        // Bild von RGB in HSV konvertieren
        Mat hsv = new Mat();
        Cv2.CvtColor(image, hsv, ColorConversionCodes.RGB2HSV);

        // Original Bild anzeigen, damit ein Fenster erstellt wird
        Cv2.ImShow("Output", image);

        // Eine Trackbar zum Einstellen der HUE Verschiebung in dem Fenster "Output" erstellen
        // Wenn der Wert der Trackbar geändert wird, wird die Lambda Funktion "(pos, userData) => {...}" aufgerufen
        Cv2.CreateTrackbar("HUE", "Output", 255, (sliderValue, userData) =>
        {
            // HSV Bild in die einzelnen Kanäle H/S/V aufsplitten
            Mat[] hsv_channels = Cv2.Split(hsv);

            unsafe
            {
                // Auf jedes Pixel im H-channel den Wert der Trackbar aufaddieren
                hsv_channels[0].ForEachAsByte((value, position) =>
                {
                    *value = (byte)(*value + sliderValue);
                });
            }

            Cv2.ImShow("H", hsv_channels[0]);

            // Die einzelnen Kanäle wieder zu einem einzigen Bild zusammenfügen
            Mat hsv_clone = new Mat();
            Cv2.Merge(hsv_channels, hsv_clone);

            // Das zusammengesetzte neue HSV Bild zurück in RGB konvertieren
            Mat rgb = new Mat();
            Cv2.CvtColor(hsv_clone, rgb, ColorConversionCodes.HSV2RGB);

            // Fertiges Bild anzeigen
            Cv2.ImShow("Output", rgb);

            // Jede erstellte Matrix vom Arbeitsspeicher entfernen
            foreach (var hsv_channel in hsv_channels)
                hsv_channel.Dispose();
            hsv_clone.Dispose();
        });

        // Den bereich der Trackbar auf [-128, 128] setzten, die Funktion CreateTrackbar() kann nur
        // den oberen Grenzwert setzen, der untere ist immer 0
        //Cv2.SetTrackbarMax("HUE", "Output", 128);
        //Cv2.SetTrackbarMin("HUE", "Output", -128);

        Cv2.WaitKey();
        Cv2.DestroyAllWindows();
        hsv.Dispose();
    }
    // Show Webcam
    else if (ind == 2)
    {
        // Ein Objekt des Typs VideoCapture erstellen. Mit dieser Klasse kann man Bilder aus Videos 
        // oder von einer Kamera bekommen
        VideoCapture webcam = new VideoCapture();
        // Die erste Kamera des PCs öffnen
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
    // Show Grid Window
    else if(ind == 4)
    {
        using Mat image = Cv2.ImRead("Image.jpg");
        using Mat gray = new Mat();
        Cv2.CvtColor(image, gray, ColorConversionCodes.RGB2GRAY);
        using Mat bin = new Mat();
        Cv2.Threshold(gray, bin, 0, 255, ThresholdTypes.Otsu);
        using Mat canny = new Mat();
        Cv2.Canny(bin, canny, 100, 200);
        using Mat graph1 = Util.DrawHistogram(image);
        using Mat graph2 = Util.DrawHistogram(gray);

        ImageGridWindow wnd = new ImageGridWindow(2, 3);
        wnd.SetImage(0, 0, image);
        wnd.SetImage(1, 0, gray);
        wnd.SetImage(0, 1, graph1);
        wnd.SetImage(1, 1, graph2);
        wnd.SetImage(0, 2, bin);
        wnd.SetImage(1, 2, canny);
        wnd.AddTrackbar("Brightness", -255, 255);
        wnd.OnTrackbarValueChanged += (wnd, name, value) =>
        {
            using Mat image2 = image + new Scalar(value, value, value);
            using Mat graph12 = Util.DrawHistogram(image2);
            wnd.SetImage(0, 0, image2);
            wnd.SetImage(0, 1, graph12);
            if (value == -255)
                wnd.SetImage(0, 2, null);
        };
        wnd.Run();
    }
    // Draw Histogramm
    else if(ind == 5)
    {
        using Mat image = new Mat("Image.jpg");
        using Mat graph = Util.DrawHistogram(image);
        Cv2.ImShow("histogram", graph);
        Cv2.WaitKey();
        Cv2.DestroyAllWindows();
    }
    // Zoomable Window
    else if(ind == 6)
    {
        using Mat image = new Mat("Image.jpg");
        ZoomableWindow wnd = new ZoomableWindow(image);
        wnd.Show();
    }
    // Exit
    else
    {
        return;
    }
}