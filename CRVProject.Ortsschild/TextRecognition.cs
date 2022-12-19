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
        public Mat Image = new Mat();
        public Mat GrayscaleImage = new Mat();
        public Mat OtsuImage = new Mat();
        public Mat OtsuImageDraw = new Mat();

        public void Dispose()
        {
            Image.Dispose();
            GrayscaleImage.Dispose();
            OtsuImage.Dispose();
            OtsuImageDraw.Dispose();
        }

        public string Run(Mat image)
        {
            this.Image.Dispose();
            this.Image = image.Clone();

            // Graustufen, Otsu
            using Mat hsv = new Mat();
            //Cv2.CvtColor(image, GrayscaleImage, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
            GrayscaleImage = Cv2.Split(hsv)[2];
            Cv2.Threshold(GrayscaleImage, OtsuImage, 0, 255, ThresholdTypes.Otsu);
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

            Console.WriteLine($"Parent finden: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            #region Bouning Boxen sortieren
            // Sortieren von contorus von links nach rechts
            string fpath = "Letters_DB/";
            string letters = "abcdefghijklmnopqrstuvwxyz!";
            //string letters = "Heilbron";
            string ftype = ".png";
            double temp_min = 0;
            double diff;
            string correct_word = ""; // Zeile 1 Buchstaben auf dem Schild
            char correct_letter = '\0';

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
                    // Console.WriteLine("(" + boundingRect.X + "," + boundingRect.Y+")");
                    // index im contour array für die Kontur eines Buchstaben, also nur die die man braucht
                    BoundingRectIdx[counter_k] = i;
                    //Console.WriteLine(num_parents);
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
                BoundingRectX[i] += i * 0.000001; // Addiere eine kleine Menge zu den Zahlen um diese zu unterscheiden
            }

            BoundingRectX.CopyTo(BoundingRectX_temp, 0);

            Array.Sort(BoundingRectX_temp, BoundingRectIdx);
            Array.Sort(BoundingRectHeight);

            double thresh_diff = BoundingRectHeight[(int)(BoundingRectHeight.Length / 2)]; // Median Höhe der Boundignbox

            // Maske, die kennzeichnet zu welcher Zeile welcher Buchstabe gehört z. B 000001111 hallowelt
            int mask_var = 0;

            Array.Sort(BoundingRectY, BoundingRectX);
            // Y- Werte Differenzieren, sobald eine Differez größer als der Median ist, ist das ein Zeilenumbruch auf dem Ortsschild

            for (int i = 1; i < BoundingRectY.Length; i++)
            {
                if (Math.Abs(BoundingRectY[i] - BoundingRectY[i - 1]) > thresh_diff)
                {
                    mask_var++;
                }
                lineMask[i] = mask_var;
            }

            Array.Sort(BoundingRectX, lineMask);

            #endregion

            Console.WriteLine($"Boxen sortieren: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            #region Hauptfunktion Vergleich mit Template
            // TEST A
            /*static Dictionary<string, Mat> LoadDatabase(string directory)
            { 
                var di = new DirectoryInfo(directory);
                var dictionary = new Dictionary<char, Mat>();
                foreach (var file in di.GetFiles("*.png"))
                {
                    dictionary.Add(file.Name[0], Cv2.ImRead(file.FullName));
                }
                return dictionary;
            }*/
            // TEST B

            string temp_char;
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
                        // Lower Case Upper Case
                        if (caseLU == 0)
                        {
                            temp_char = letters[j] + "_klein";
                        }
                        else
                        {
                            temp_char = letters[j].ToString();
                        }

                        // Buchstabe aus der Datenbank
                        using Mat temp_template = new Mat();
                        Mat temp_template_read = Cv2.ImRead(fpath + temp_char + ftype, ImreadModes.Grayscale);
                        Debug.Assert(!temp_template_read.Empty());
                        temp_template_read.ConvertTo(temp_template, MatType.CV_8UC1);

                        //zugeschnittenes Otsu image
                        Mat temp_otsu_image = OtsuImage[boundingRect];
                        Mat temp_otsu_image_original = new Mat();

                        //zugeschnittenes Otsu image auf Größe des Templates bringen

                        temp_otsu_image.CopyTo(temp_otsu_image_original);
                        Cv2.Resize(temp_otsu_image, temp_otsu_image, temp_template.Size());
                        //temp_otsu_image = temp_otsu_image.Resize(temp_template.Size());

                        // nochmal otsu da nach interpolation wieder graustufen auftreten
                        Cv2.Threshold(temp_otsu_image, temp_otsu_image, 0, 255, ThresholdTypes.Otsu);

                        //Ergebnis matrix
                        Mat res = new Mat(temp_template.Size(), MatType.CV_8UC1);

                        //Vergleiche Buchstaben mit zugeschnittenem Bild
                        Cv2.Compare(temp_otsu_image, temp_template, res, CmpType.EQ);

                        //Übereinstimmung in Prozent, division durch 255 um Zähler zu normieren
                        diff = Cv2.Sum(res).Val0 / (res.Width * res.Height) / 255;

                        switch (letters[j])
                        {
                            // für i, j, !, i-punkt muss die Übereinstimmung über das Seitenverältnis berechnet werden
                            case '!':
                                if (Math.Abs(temp_otsu_image_original.Height / thresh_diff) < 0.5)
                                {
                                    correct_letter = '!';
                                }
                                break;
                            case 'i':
                                if (caseLU == 0 && Math.Abs((double)temp_otsu_image_original.Width / (double)temp_otsu_image_original.Height - (double)temp_template.Width / (double)temp_template.Height) < 0.1)
                                {
                                    correct_letter = 'i';
                                }
                                break;
                            case 'j':
                                if (caseLU == 0 && Math.Abs((double)temp_otsu_image_original.Width / (double)temp_otsu_image_original.Height - (double)temp_template.Width / (double)temp_template.Height) < 0.01)
                                {
                                    correct_letter = 'j';
                                }
                                break;
                            default:
                                // über die Anzahl der gleich Pixel
                                if (diff > temp_min)
                                {
                                    temp_min = diff;
                                    if (caseLU == 0)
                                    {
                                        correct_letter = letters[j];
                                    }
                                    else
                                    {
                                        correct_letter = letters[j].ToString().ToUpper()[0]; // Großbuchstaben erkennen
                                    }
                                }
                                break;
                        }
                    }
                }
                // Gesamtes Wort zusammensetzen
                correct_word += correct_letter;
                //Console.WriteLine("correct letter: " +correct_letter);
                Cv2.PutText(OtsuImageDraw, correct_letter.ToString(), new OpenCvSharp.Point(boundingRect.X, boundingRect.Y), HersheyFonts.Italic, 1, new Scalar(150, 150, 150));
                //Cv2.PutText(otsu_image, boundingRect.X.ToString(), new OpenCvSharp.Point(boundingRect.X, boundingRect.Y), HersheyFonts.Italic, 1, new Scalar(150, 150, 150));
                temp_min = 0;    // resette das temporären Minimum für die nächste Iteration
            }
            #endregion

            Console.WriteLine($"Templates: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            #region ende

            string[] seperateWords = new[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };

            // Maske auf das Wort anwenden und wörter trennen sowie Zeilen zuordnen
            for (int line = 0; line <= mask_var; line++)
            {
                for (int i = 0; i < correct_word.Length; i++)
                {
                    if (lineMask[i] == line)
                    {
                        seperateWords[line] += correct_word[i];
                    }
                }
            }

            for (int line = 0; line <= mask_var; line++)
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
            foreach(var replacement in replacements)
            {
                string[] words = replacement.Split(',');
                for (int i = 0; i < words.Length - 1; i++)
                    finalText = finalText.Replace(words[i], words.Last());
            }

            Cv2.ImShow("Final Image", OtsuImageDraw);
            //Cv2.WaitKey();
            //Cv2.DestroyAllWindows();

            /* To do:
                * Confusionmatrix
                * Parameter auslagern, alle außerhalb des programmes als globale variablen, keine Zahlen im code
                * Zweite Zeile soll auch erkannt werden können
                * Als kleines Bild den Ortsnamen schreiben, in anderem Schriftformat
                * Als video Auf der Leinwand sichtbarkeit überprüfen
                * 
                * 
                * Alle Bounding Boxen und alle Buchstaben auf die gleiche Größe bringen 
            */
            #endregion

            Console.WriteLine($"Ende: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            return finalText;
        }
    }
}
