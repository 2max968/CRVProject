// CRV Project Buchstabenerkennung
// Version 06.12.2022 17:27

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Net;

Console.WriteLine("Beginne Buchstaben einzulesen...");
//Mat image = Cv2.ImRead("Heilbronn.jpg");
Mat image = Cv2.ImRead("Sulzdorf1.png");

// Matrizen erstellen
Mat grayscale_image = new Mat();
Mat otsu_image = new Mat();
Mat grayscale_template = new Mat();
Mat otsu_template = new Mat();

// Graustufen
Cv2.CvtColor(image, grayscale_image, ColorConversionCodes.RGB2GRAY);

// Otsu
Cv2.Threshold(grayscale_image, otsu_image, 0, 255, ThresholdTypes.Otsu);
Cv2.Threshold(grayscale_template, otsu_template, 0, 255, ThresholdTypes.Otsu);

// Objekterkennung mit Hierarchie
OpenCvSharp.Point[][] contours;
HierarchyIndex[] hierarchy;
Cv2.FindContours(otsu_image, out contours, out hierarchy, mode: RetrievalModes.CComp, method: ContourApproximationModes.ApproxSimple);

// Sortieren von contorus von links nach rechts
Console.WriteLine("Number of found contours: " + contours.Length);

string fpath = "Letters_DB/";
//string letters = "abcdefghijklmnopqrstuvwxyzäöüABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÜ";
string letters = "Heilbron";
string ftype = ".png";
double temp_min = 0;
double diff;
double BoundingRectYmean = 0;
double BoundingRectCounter = 0;
string correct_word = "";
char correct_letter = '\0';

int[] BoundingRectPixel = new int[contours.Length]; 
int[] BoundingRectIdx = new int[contours.Length];

// Bounding Boxen Zeichnen und Soriterung vorbereiten
for (int i = 0; i < contours.Length; i++)
{
    BoundingRectIdx[i] = i;
    // Die Boundingboxen von Buchstaben sind dadurch definiert, dass Diese alle den gleiche Parent (3) besitzen
    // Alle boundingboxen die in Frage kommen:
    //if (hierarchy[i].Parent == 3)
    //{
        var contour = contours[i];
        var boundingRect = Cv2.BoundingRect(contour);
        // Zeiche Bounding Boxen in das otsu_image
        Cv2.Rectangle(otsu_image, new OpenCvSharp.Point(boundingRect.X, boundingRect.Y), new OpenCvSharp.Point(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height), new Scalar(150, 150, 150), 1);
        BoundingRectPixel[i] = boundingRect.X;
        BoundingRectYmean += boundingRect.Y;
        BoundingRectCounter += 1;
    //}
}
// Schwellenwert für die äöüij Punkte in Y-Richtung
BoundingRectYmean = BoundingRectYmean / BoundingRectCounter;
// Sortiere die X Koordinaten der Eckpunkte der Bounding Boxen und deren zugehöriger Index von links nach rechts
Array.Sort(BoundingRectPixel, BoundingRectIdx);


for (int i = 0; i < contours.Length; i++)
{
    int k = BoundingRectIdx[i];
    // Aufgrund der nicht dynamischen Definition des Arrays sind die ersten indices 
    if (BoundingRectPixel[i] > 0)
    {
        // Nochmal Konturen berechnen, jetzt in richtiger Reihenfolge

        var contour = contours[k];
        var boundingRect = Cv2.BoundingRect(contour);

        for (int j = 0; j < letters.Length; j++)
        {
            char temp_char = letters[j];

            // Buchstabe aus der Datenbank
            Mat temp_template = new Mat();
            Mat temp_template_read = Cv2.ImRead(fpath + temp_char + ftype, ImreadModes.Grayscale);

            temp_template_read.ConvertTo(temp_template, MatType.CV_8UC1);
            //Cv2.ImShow("Template", temp_template);

            //zugeschnittenes Otsu image
            Mat temp_otsu_image = otsu_image[boundingRect];
            //Cv2.ImShow("image", otsu_image);
            //Cv2.ImShow("otsu image", temp_otsu_image);

            //zugeschnittenes Otsu image auf Größe des Templates bringen
            temp_otsu_image = temp_otsu_image.Resize(temp_template.Size());

            //Ergebnis matrix
            Mat res = new Mat(temp_template.Size(), MatType.CV_8UC1);
            //Cv2.ImShow("Result", res);

            // Vergleiche Buchstaben mit zugeschnittenem Bild
            Cv2.Compare(temp_otsu_image, temp_template, res, CmpType.EQ);
            // Übereinstimmung in Prozent, division durch 255 um Zähler zu normieren
            diff = Cv2.Sum(res).Val0 / (res.Width * res.Height) / 255;
            // if new min < old min, new letter is more likely the right one 
            if (diff > temp_min)
            {
                temp_min = diff;
                correct_letter = temp_char;
            }
        }
        // Wenn letzte Iteration dann
        // Füge Buchstaben dem Wort hinzu
        correct_word += correct_letter;
        Cv2.PutText(otsu_image, correct_letter.ToString(), new OpenCvSharp.Point(boundingRect.X, boundingRect.Y), HersheyFonts.Italic, 1, new Scalar(150, 150, 150));
        temp_min = 0;    // resette das temporären Minimum für die nächste Iteration

    }

}

correct_word = correct_word.Replace("oii", "ö");
correct_word = correct_word.Replace("ioi", "ö");
correct_word = correct_word.Replace("iio", "ö");

correct_word = correct_word.Replace("aii", "ä");
correct_word = correct_word.Replace("iai", "ä");
correct_word = correct_word.Replace("iia", "ä");

correct_word = correct_word.Replace("uii", "ü");
correct_word = correct_word.Replace("iui", "ü");
correct_word = correct_word.Replace("iiu", "ü");

correct_word = correct_word.Replace("ij", "j");
correct_word = correct_word.Replace("ji", "j");

correct_word = correct_word.Replace("ii", "i");




Cv2.ImShow("Final Image", otsu_image);
Console.WriteLine("found word: " + correct_word);
Cv2.WaitKey();
Cv2.DestroyAllWindows();

/* To do:
    * Selktiere die außerste Bounding Box für jeden Buchstaben auf Grundlage der Hierarchie
    * resize das template auf die höhe und breite der gefundenen Bounding box
    * Loop der durch jeden buchstaben iteriert und eine Prozentzahl aufschreibt
    * Jeweilige Buchstaben anhand der Koordinaten der Bounding Box sortieren und Wort ausgeben
    * Boundingbox schräg 
    * Opencv funktion ver XOR dern

    * Confusionmatrix
    * Frage: Braucht man Zero padding überhaupt?
    * Parameter auslagern, alle außerhalb des programmes als globale variablen, keine Zahlen im code
*/
