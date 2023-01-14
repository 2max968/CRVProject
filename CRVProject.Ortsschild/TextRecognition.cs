using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRVProject.Ortsschild
{
    public class TextRecognition : IDisposable
    {
        static Dictionary<char, (Mat klein, Mat gross)> templates
            = new Dictionary<char, (Mat klein, Mat gross)>();

        public Mat Image = new Mat();
        public Mat GrayscaleImage = new Mat();
        public Mat OtsuImage = new Mat();
        public Mat OtsuImageDraw = new Mat();
        public Mat CannyImage = new Mat();
        public Mat hsv = new Mat();
        public static void Init()
        {
            string letters = "abcdefghijklmnopqrstuvwxyz!";
            foreach (var letter in letters)
            {
                string fname1 = "letters_DB/" + ("" + letter).ToLower() + "_klein.png";
                string fname2 = "letters_DB/" + ("" + letter).ToUpper() + ".png";

                if (letter != 'i' && letter != '!')
                {
                    Mat mat1 = Cv2.ImRead(fname1).Resize(new OpenCvSharp.Size(64, 64));
                    Mat mat2 = Cv2.ImRead(fname2).Resize(new OpenCvSharp.Size(64, 64));
                    //mat1.ConvertTo(mat1, MatType.CV_8UC1);
                    //mat2.ConvertTo(mat2, MatType.CV_8UC1);
                    Cv2.CvtColor(mat1, mat1, ColorConversionCodes.RGB2GRAY);
                    Cv2.CvtColor(mat2, mat2, ColorConversionCodes.RGB2GRAY);
                    templates.Add(letter, (mat1, mat2));
                }
                else
                {

                    Mat mat1 = Cv2.ImRead(fname1);
                    Mat mat2 = Cv2.ImRead(fname2);
                    Cv2.CvtColor(mat1, mat1, ColorConversionCodes.RGB2GRAY);
                    Cv2.CvtColor(mat2, mat2, ColorConversionCodes.RGB2GRAY);
                    // mat1.ConvertTo(mat1, MatType.CV_8UC1);
                    // mat2.ConvertTo(mat2, MatType.CV_8UC1);

                    templates.Add(letter, (mat1, mat2));
                }
            }
        }

        public void Dispose()
        {
            Image.Dispose();
            GrayscaleImage.Dispose();
            OtsuImage.Dispose();
            OtsuImageDraw.Dispose();
        }


        public bool Preprocess(Mat image)
        {
            this.Image.Dispose();
            this.Image = image.Clone();
            Cv2.CvtColor(image, GrayscaleImage, ColorConversionCodes.BGR2GRAY);
            //Cv2.ImShow("grayscale", GrayscaleImage);
            //Cv2.WaitKey();
            Cv2.Threshold(GrayscaleImage, OtsuImage, 0, 255, ThresholdTypes.Otsu);
            //Cv2.ImShow("Otsu Image", OtsuImage);
            //Cv2.WaitKey();
            Cv2.Canny(OtsuImage, CannyImage, 0, 255);
            //Cv2.ImShow("Canny Image", CannyImage);
            //Cv2.WaitKey();
            double rho = 1;
            double theta = Math.PI / 180;
            int threshold = 10;
            double minLength = 200; // minimale linienlänge
            double maxLengthGap = 10; // maximale lücke

            LineSegmentPoint[] houghLines = Cv2.HoughLinesP(CannyImage, rho, theta, threshold, minLength, maxLengthGap);
            for (int i = 0; i < houghLines.Length; i++)
            {
                Cv2.Line(CannyImage, houghLines[i].P1, houghLines[i].P2, new Scalar(150, 150, 150));
                if (houghLines[i].P1.Y > image.Height / 3 && houghLines[i].P1.Y < 2 * image.Height / 3 && houghLines[i].P2.Y > image.Height / 3 && houghLines[i].P2.Y < 2 * image.Height / 3)
                {
                    Console.WriteLine("ist Ausfahrtsschild");
                    //Cv2.ImShow("Hough Lines", CannyImage);
                    //Cv2.WaitKey();
                    return true;  // es liegt eine horizontale linie im mittleren vertikalen drittel des bildes vor was nur bei einem geteilten schild vorkommen kann
                }
            }


            Console.WriteLine("ist KEIN Ausfahrtsschild");
            //Cv2.ImShow("Hough Lines", CannyImage);
            //Cv2.WaitKey();
            return false;
        }


        public string Run(Mat image, bool istAusfahrt, bool debug)
        {
            this.Image.Dispose();
            this.Image = image.Clone();

            // Graustufen, Otsu
            if (istAusfahrt == true)
            {
                Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
                GrayscaleImage = Cv2.Split(hsv)[2];
                Cv2.Threshold(GrayscaleImage, OtsuImage, 0, 255, ThresholdTypes.Otsu);
                Cv2.Dilate(OtsuImage, OtsuImage, new Mat(), null, 2);
            }
            else
            {
                Cv2.CvtColor(image, GrayscaleImage, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(GrayscaleImage, OtsuImage, 0, 255, ThresholdTypes.Otsu);
            }


            OtsuImage.CopyTo(OtsuImageDraw);

            Stopwatch stp = Stopwatch.StartNew();

            #region Parent finden
            // Häufigster Parent ist der um Alle Buchstaben
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(OtsuImage, out contours, out hierarchy, mode: RetrievalModes.CComp, method: ContourApproximationModes.ApproxSimple);

            int[] hierarchy_parents = new int[hierarchy.Length];
            int num_parents = 0;
            int found_parent = -1;

            // parents von structs in array
            for (int i = 0; i < hierarchy.Length; i++)
            {
                hierarchy_parents[i] = hierarchy[i].Parent;
            }

            // finde den am häufigsten auftretenden Parent
            for (int i = 0; i < hierarchy.Length; i++)
            {
                // Bsp. 1 mit 103287501982 vergleichen und zählen, 0 mit 103287501982 vergleichen und zählen, 3 mit 103287501982 vergleichen ...
                if (hierarchy_parents.Count(n => n == hierarchy_parents[i]) > num_parents && hierarchy_parents[i] != -1)   // da -1 sehr oft vorkommt
                {
                    num_parents = hierarchy_parents.Count(n => n == hierarchy_parents[i]);
                    found_parent = hierarchy_parents[i];
                }
            }

            #endregion

            if(debug) Console.WriteLine($"Parent finden: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            #region Bouning Boxen sortieren
            // Sortieren von contorus von links nach rechts
            string letters = "abcdefghijklmnopqrstuvwxyz!";
            double temp_min = 0;
            double diff = 100;
            string correctWord = ""; // Zeile 1 Buchstaben auf dem Schild
            char correctLetter = '\0';

            double[] BoundingRectX = new double[num_parents];
            double[] BoundingRectX_temp = new double[num_parents];
            double[] BoundingRectY = new double[num_parents];
            double[] BoundingRectHeight = new double[num_parents];
            int[] BoundingRectIdx = new int[num_parents];
            double[] lineMask = new double[num_parents];

            int counter_k = 0;
            // Bounding Boxen Zeichnen und Soriterung vorbereiten
            for (int i = 0; i < contours.Length; i++)
            {
                // Wenn eine Bounding Box die richtinge Anzahl an Parents hat (found_parents) ist es ein Buchstabe
                if (hierarchy[i].Parent == found_parent)
                {
                    var contour = contours[i];
                    var boundingRect = Cv2.BoundingRect(contour);
                    // Zeiche Bounding Boxen in das otsu_image
                    Cv2.Rectangle(OtsuImageDraw, new OpenCvSharp.Point(boundingRect.X, boundingRect.Y), new OpenCvSharp.Point(boundingRect.X + boundingRect.Width - 1, boundingRect.Y + boundingRect.Height - 1), new Scalar(150, 150, 150), 1);
                    BoundingRectX[counter_k] = boundingRect.X;
                    BoundingRectY[counter_k] = boundingRect.Y;
                    BoundingRectHeight[counter_k] = boundingRect.Height;

                    // index im contour array für die Kontur eines Buchstaben, also nur die die man braucht
                    BoundingRectIdx[counter_k] = i;
                    counter_k++;
                }
            }

            // Buchstabe durch vier Dinge kategorisiert : index, x, y, zeile
            // Sortiere die Y-Werte und Indizes wie die X-Koordinaten
            // temporäre variable benötigt, da zweimal sortiert wird und die originale nach dem ersten sortieren unbrauchbar wird

            // sortierungsschritte und Reihenfolge
            // 1. BoundingRectX so wie BoundingRectY
            // 2. lineMask      so wie BoundingRectY (Bei der Erstellung in der Schleife)
            // 3. lineMask      so wie BoundinRectX
            //
            //
            //(4. BoundingRectIdx so wie BoundingRectX
            // Am Ende soll sich die Reihenfolge auf BoundingRectX beziehen also alles von links nach rechts
            // Notwending, da sich die Array.Sort funktion bei gleichen keys nicht merkt welcher wert zu welchem Key gehört

            for (int i = 0; i < num_parents; i++)
            {
                BoundingRectX[i] += i * 0.000001; // Um gleiche Zahlen beim sortieren zu unterscheiden und eindeutig zuzuordnen
            }

            BoundingRectX.CopyTo(BoundingRectX_temp, 0);

            Array.Sort(BoundingRectX_temp, BoundingRectIdx);
            Array.Sort(BoundingRectHeight);
            double medianHeight = 0;

            if (istAusfahrt == true)
            {
                medianHeight = BoundingRectHeight[(int)(BoundingRectHeight.Length / 2)] * 1.3; // In diesem Fall gibt es nur eine Zeile also macht es keinen Sinn zu differenzieren
            }
            else
            {
                medianHeight = BoundingRectHeight[(int)(BoundingRectHeight.Length / 2)]; // Median Höhe der Boundignboxen
            }
            // Maske, die kennzeichnet zu welcher Zeile welcher Buchstabe gehört z. B 000001111 hallowelt
            int maskVar = 0;

            Array.Sort(BoundingRectY, BoundingRectX);
            // Y- Werte Differenzieren, sobald eine Differez größer als der Median ist, ist das ein Zeilenumbruch auf dem Ortsschild

            for (int i = 1; i < BoundingRectY.Length; i++)
            {
                if (Math.Abs(BoundingRectY[i] - BoundingRectY[i - 1]) > medianHeight)
                {
                    maskVar++;
                }
                lineMask[i] = maskVar;
            }

            Array.Sort(BoundingRectX, lineMask);

            #endregion
            if(debug) Console.WriteLine($"Boxen sortieren: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            #region Hauptfunktion Vergleich mit Template
            //string temp_char;
            for (int i = 0; i < num_parents; i++)
            {
                // "wrong" order 52492
                int k = BoundingRectIdx[i];
                // Nochmal Konturen berechnen, jetzt in richtiger Reihenfolge
                var contour = contours[k];
                var boundingRect = Cv2.BoundingRect(contour);
                // Bounding rect wird zu groß selektiert also kleiner machen
                boundingRect = new Rect(new OpenCvSharp.Point(boundingRect.X + 1, boundingRect.Y + 1), new OpenCvSharp.Size(boundingRect.Width - 2, boundingRect.Height - 2));

                for (int caseLU = 0; caseLU <= 1; caseLU++) // Lower Case Upper Cas
                {
                    for (int j = 0; j < letters.Length; j++)
                    {
                        // Buchstabe aus der Datenbank
                        Mat temp_template = new Mat();

                        // Lower Case Upper Case
                        if (caseLU == 0)
                        {
                            //temp_char = letters[j] + "_klein";
                            temp_template = templates[letters[j]].klein;

                        }
                        else
                        {
                            //temp_char = letters[j].ToString();
                            temp_template = templates[letters[j]].gross;
                        }

                        // Otsu Bild zuschneiden
                        Mat temp_otsu_image = OtsuImage[boundingRect];
                        Mat tempOtsuImageOriginal = new Mat();
                        temp_otsu_image.CopyTo(tempOtsuImageOriginal);

                        // ratio wird nur in den switch cases benötigt, wird aber schon vorhere definiert, damit es durch das resizen der Bilder nicht überschrieben werden sowie die resize aktionen nicht zweimal durchgeführt werden müssen
                        double ratio = Math.Abs((double)tempOtsuImageOriginal.Width / (double)tempOtsuImageOriginal.Height);
                        // Verleichen mit template
                        // OtsuImage auf Größe des Templates bringen
                        temp_otsu_image = temp_otsu_image.Resize(temp_template.Size());

                        // nochmal otsu da nach interpolation wieder graustufen auftreten
                        Cv2.Threshold(temp_otsu_image, temp_otsu_image, 0, 255, ThresholdTypes.Otsu);

                        //Ergebnis matrix
                        Mat res = new Mat(temp_template.Size(), MatType.CV_8UC1);

                        //Vergleiche Buchstaben mit zugeschnittenem Bild
                        Cv2.Compare(temp_otsu_image, temp_template, res, CmpType.EQ);

                        //Übereinstimmung in Prozent, division durch 255 um Zähler zu normieren
                        diff = Cv2.Sum(res).Val0 / (res.Width * res.Height) / 255;

                        /*Cv2.ImShow("cutout", temp_otsu_image);
                        Cv2.ImShow("template", temp_template);
                        Cv2.WaitKey();
                        if(debug) Console.WriteLine(diff);*/
                        switch (letters[j])
                        {
                            //Sonderfälle: für s, S, i, i-punkt ('!') muss die Übereinstimmung über das Seitenverältnis berechnet werden
                            case '!':
                                if (tempOtsuImageOriginal.Height / medianHeight < 0.5)
                                {
                                    correctLetter = '!';
                                }
                                break;
                            case 'i':
                                if (caseLU == 0 && 0.18 < ratio && ratio < 0.25 && diff > 0.8)
                                {
                                    correctLetter = 'i';
                                    // beende beide schleifen bzw. füge den Buchstaben sofort hinzu
                                    j = letters.Length;
                                    caseLU = 1;
                                }
                                else if (caseLU == 1 && 0.11 < ratio && ratio < 0.17 && diff > 0.8)
                                {
                                    correctLetter = 'I';
                                    // beende beide schleifen bzw. füge den Buchstaben sofort hinzu
                                    j = letters.Length;
                                    caseLU = 1;
                                }

                                break;
                            default:
                                if (diff > temp_min)
                                {
                                    temp_min = diff;
                                    if (caseLU == 0)
                                    {
                                        correctLetter = letters[j];
                                    }
                                    else if (caseLU == 1 && tempOtsuImageOriginal.Height > medianHeight * 0.95)
                                    {
                                        correctLetter = letters[j].ToString().ToUpper()[0]; // Großbuchstaben erkennen
                                    }
                                }
                                break;
                        }
                    }
                }
                // Gesamtes Wort zusammensetzen
                correctWord += correctLetter;
                //if(debug) Console.WriteLine("correct letter: " + correctLetter);
                Cv2.PutText(OtsuImageDraw, correctLetter.ToString(), new OpenCvSharp.Point(boundingRect.X, boundingRect.Y), HersheyFonts.Italic, 1, new Scalar(150, 150, 150));
                //Cv2.PutText(otsu_image, boundingRect.X.ToString(), new OpenCvSharp.Point(boundingRect.X, boundingRect.Y), HersheyFonts.Italic, 1, new Scalar(150, 150, 150));
                temp_min = 0;    // resette das temporären Minimum für die nächste Iteration
            }
            #endregion

            if(debug) Console.WriteLine($"Templates: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            #region ende

            string[] seperateWords = new[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };

            // Maske auf das Wort anwenden und wörter trennen sowie Zeilen zuordnen
            for (int line = 0; line <= maskVar; line++)
            {
                for (int i = 0; i < correctWord.Length; i++)
                {
                    if (lineMask[i] == line)
                    {
                        seperateWords[line] += correctWord[i];
                    }
                }
            }

            for (int line = 0; line <= maskVar; line++)
            {
                for (int i = 1; i < seperateWords[line].Length; i++)
                {
                    if (Char.IsUpper(seperateWords[line][i]) && Char.IsLower(seperateWords[line][i - 1]))
                    {
                        seperateWords[line] = seperateWords[line].Insert(i, " "); // z. B. bei HalloWelt wird erkannt, dass bei "oW" getrennt werden soll: Hallo Welt
                    }
                }
            }

            string finalText = string.Join('\n',
                seperateWords.Where(word => !string.IsNullOrWhiteSpace(word)));

            // Zeichen ersetzen öäüij Punkte
            string[] replacements = File.ReadAllLines("replacements.txt");
            foreach (var replacement in replacements)
            {
                string[] words = replacement.Split(',');
                for (int i = 0; i < words.Length - 1; i++)
                    finalText = finalText.Replace(words[i], words.Last());
            }

            /* To do:
                * Confusionmatrix
                * Parameter auslagern, alle außerhalb des programmes als globale variablen, keine Zahlen im code
                * Zweite Zeile soll auch erkannt werden können
                * Als kleines Bild den Ortsnamen schreiben, in anderem Schriftformat
                * Als video Auf der Leinwand sichtbarkeit überprüfen
                * 
                * 
                * Alle Bounding Boxen und alle Buchstaben auf die gleiche Größe bringen
                * 
                * präsentation
                * 12 min + Video präsentation 4 bis 5 min + Fragen Bis Ende Januar
            */
            #endregion

            if(debug) Console.WriteLine($"Ende: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            return finalText;
        }
    }
}