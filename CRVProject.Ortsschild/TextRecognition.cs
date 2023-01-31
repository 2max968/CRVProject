using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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
            string Letters = CRVProject.Helper.Configuration.Instance.Recognition.Letters;
            foreach (var letter in Letters)
            {
                string fname1 = "letters_DB/" + ("" + letter).ToLower() + "_klein.png";
                string fname2 = "letters_DB/" + ("" + letter).ToUpper() + ".png";
                bool IsLetter = false;

                if ("!1234567890^".Contains(("" + letter)) == false)
                {
                    IsLetter = true;
                }
                Mat mat1 = new Mat();
                if (letter != 'i' && letter != '!')
                {
                    if (IsLetter) //Wenn es ein Buchstabe ist, einfach ganz normal einlesen, wenn nicht, leere Matrix Mat mat1 = new Mat();
                    {
                        mat1 = Cv2.ImRead(fname1).Resize(new OpenCvSharp.Size(64, 64));
                        Cv2.CvtColor(mat1, mat1, ColorConversionCodes.RGB2GRAY);
                    }

                    Mat mat2 = Cv2.ImRead(fname2).Resize(new OpenCvSharp.Size(64, 64));
                    Cv2.CvtColor(mat2, mat2, ColorConversionCodes.RGB2GRAY);
                    if (IsLetter)
                    {
                        templates.Add(letter, (mat1, mat2));   // bsp. a A, b B, c C ...
                    }
                    else
                    {
                        templates.Add(letter, (mat2, mat1));   // bsp. 3 [0x0], 4 [0x0], 5 [0x0]
                    }
                }
                else
                {
                    // nicht resizen
                    if (IsLetter)
                    {
                        mat1 = Cv2.ImRead(fname1);
                        Cv2.CvtColor(mat1, mat1, ColorConversionCodes.RGB2GRAY);
                    }

                    Mat mat2 = Cv2.ImRead(fname2);
                    Cv2.CvtColor(mat2, mat2, ColorConversionCodes.RGB2GRAY);
                    if (IsLetter)
                    {
                        templates.Add(letter, (mat1, mat2));   // bsp. a A, b B, c C ...
                    }
                    else
                    {
                        templates.Add(letter, (mat2, mat1));   // bsp. 3 [0x0], 4 [0x0], 5 [0x0]
                    }
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


        public bool Preprocess(Mat image, bool debug)
        {
            this.Image.Dispose();
            this.Image = image.Clone();
            Cv2.CvtColor(image, GrayscaleImage, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(GrayscaleImage, OtsuImage, 0, 255, ThresholdTypes.Otsu);
            Cv2.Canny(OtsuImage, CannyImage, 0, 255);

            double rho = CRVProject.Helper.Configuration.Instance.Recognition.rho;
            double theta = CRVProject.Helper.Configuration.Instance.Recognition.theta;
            int threshold = CRVProject.Helper.Configuration.Instance.Recognition.threshold;
            double minLength = CRVProject.Helper.Configuration.Instance.Recognition.minLength; // minimale Linienlänge
            double maxLengthGap = CRVProject.Helper.Configuration.Instance.Recognition.maxLengthGap; // maximale Lücke

            LineSegmentPoint[] houghLines = Cv2.HoughLinesP(CannyImage, rho, theta, threshold, minLength, maxLengthGap);
            for (int i = 0; i < houghLines.Length; i++)
            {
                Cv2.Line(CannyImage, houghLines[i].P1, houghLines[i].P2, new Scalar(150, 150, 150));
                if (houghLines[i].P1.Y > image.Height / 3 && houghLines[i].P1.Y < 2 * image.Height / 3 && houghLines[i].P2.Y > image.Height / 3 && houghLines[i].P2.Y < 2 * image.Height / 3)
                {
                    if(debug) Console.WriteLine("ist Ausfahrtsschild");

                    return true;  // es liegt eine horizontale linie im mittleren vertikalen drittel des bildes vor was nur bei einer Ortsausfahrtstafel vorkommen kann
                }
            }

            if (debug) Console.WriteLine("ist KEIN Ausfahrtsschild");
            return false;
        }


        public string Run(Mat image, bool istAusfahrt, out double textConfidence, bool debug)
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
                Cv2.Erode(OtsuImage, OtsuImage, new Mat(), null, 2);
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
            OpenCvSharp.Point[][] Contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(OtsuImage, out Contours, out hierarchy, mode: RetrievalModes.CComp, method: ContourApproximationModes.ApproxSimple);

            int[] HierarchyParents = new int[hierarchy.Length];
            int NumBoxes = CRVProject.Helper.Configuration.Instance.Recognition.NumBoxes;
            int FoundParent = CRVProject.Helper.Configuration.Instance.Recognition.FoundParent;

            // Parents von Structs in Array
            for (int i = 0; i < hierarchy.Length; i++)
            {
                HierarchyParents[i] = hierarchy[i].Parent;
            }

            // finde den am häufigsten auftretenden Parent
            for (int i = 0; i < hierarchy.Length; i++)
            {
                // Bsp. 1 mit 103287501982 vergleichen und zählen, 0 mit 103287501982 vergleichen und zählen, 3 mit 103287501982 vergleichen ...
                if (HierarchyParents.Count(n => n == HierarchyParents[i]) > NumBoxes && HierarchyParents[i] != -1)   // da -1 sehr oft vorkommt
                {
                    NumBoxes = HierarchyParents.Count(n => n == HierarchyParents[i]);  //Anzahl der gefundenen Buchstaben und Bounding Boxen = Anzahl des häufigsten gemeinsamen Parents
                    FoundParent = HierarchyParents[i];
                }
            }

            #endregion

            if(debug) Console.WriteLine($"Parent finden: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            #region Bouning Boxen sortieren
            // Sortieren von contorus von links nach rechts
            string Letters = CRVProject.Helper.Configuration.Instance.Recognition.Letters;
            double TempMin = CRVProject.Helper.Configuration.Instance.Recognition.TempMin;
            double Diff = CRVProject.Helper.Configuration.Instance.Recognition.Diff;

            string CorrectWord = CRVProject.Helper.Configuration.Instance.Recognition.CorrectWord;
            char CorrectLetter = CRVProject.Helper.Configuration.Instance.Recognition.CorrectLetter;
            double Confidence = CRVProject.Helper.Configuration.Instance.Recognition.Confidence;

            double[] BoundingRectX = new double[NumBoxes];
            double[] BoundingRectX_temp = new double[NumBoxes];
            double[] BoundingRectY = new double[NumBoxes];
            double[] BoundingRectHeight = new double[NumBoxes];
            int[] BoundingRectIdx = new int[NumBoxes];
            double[] LineMask = new double[NumBoxes];

            int CounterK = CRVProject.Helper.Configuration.Instance.Recognition.CounterK;
            // Bounding Boxen Zeichnen und Soriterung vorbereiten
            for (int i = 0; i < Contours.Length; i++)
            {
                // Wenn eine Bounding Box die richtinge Anzahl an Parents hat (found_parents) ist es ein Buchstabe
                if (hierarchy[i].Parent == FoundParent)
                {
                    var Contour = Contours[i];
                    var BoundingRect = Cv2.BoundingRect(Contour);
                    // Zeiche Bounding Boxen in das otsu_image
                    Cv2.Rectangle(OtsuImageDraw, new OpenCvSharp.Point(BoundingRect.X, BoundingRect.Y), new OpenCvSharp.Point(BoundingRect.X + BoundingRect.Width - 1, BoundingRect.Y + BoundingRect.Height - 1), new Scalar(150, 150, 150), 1);
                    BoundingRectX[CounterK] = BoundingRect.X;
                    BoundingRectY[CounterK] = BoundingRect.Y;
                    BoundingRectHeight[CounterK] = BoundingRect.Height;

                    // index im contour array für die Kontur eines Buchstaben, also nur die die man braucht
                    BoundingRectIdx[CounterK] = i;
                    CounterK++;
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
            double epsilon = CRVProject.Helper.Configuration.Instance.Recognition.epsilon; // Eine Sehr kleine Zahl
            for (int i = 0; i < NumBoxes; i++)
            {
                BoundingRectX[i] += i * epsilon; // Um gleiche Zahlen beim sortieren zu unterscheiden und eindeutig zuzuordnen
            }

            BoundingRectX.CopyTo(BoundingRectX_temp, 0);
            Array.Sort(BoundingRectX_temp, BoundingRectIdx);
            Array.Sort(BoundingRectHeight);
            double medianHeight = CRVProject.Helper.Configuration.Instance.Recognition.medianHeight;
            double MedianMul = CRVProject.Helper.Configuration.Instance.Recognition.MedianMul; // Vergrößerung des thresholds für Ausfahrtsschilder, da diese Zeilenweise bearbeitet werden

            if (istAusfahrt == true)
            {
                medianHeight = BoundingRectHeight[(int)(BoundingRectHeight.Length / 2)] * MedianMul; // Ist Ausfahrtsschild, gibt also nur eine Zeile Text (mit evtl KM anzeige)
            }
            else
            {
                medianHeight = BoundingRectHeight[(int)(BoundingRectHeight.Length / 2)]; // Median Höhe der Boundignboxen
            }
            // Maske, die kennzeichnet zu welcher Zeile welcher Buchstabe gehört z. B 000001111 hallowelt
            int maskVar = CRVProject.Helper.Configuration.Instance.Recognition.maskVar;

            Array.Sort(BoundingRectY, BoundingRectX);
            // Y- Werte Differenzieren, sobald eine Differez größer als der Median ist, ist das ein Zeilenumbruch auf dem Ortsschild

            for (int i = 1; i < BoundingRectY.Length; i++)
            {
                if (Math.Abs(BoundingRectY[i] - BoundingRectY[i - 1]) > medianHeight)
                {
                    maskVar++;
                }
                LineMask[i] = maskVar;
            }

            Array.Sort(BoundingRectX, LineMask);

            #endregion
            if(debug) Console.WriteLine($"Boxen sortieren: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            #region Hauptfunktion Vergleich mit Template
            //string temp_char;
            for (int i = 0; i < NumBoxes; i++)
            {
                // "wrong" order 52492
                int k = BoundingRectIdx[i];
                // Nochmal Konturen berechnen, jetzt in richtiger Reihenfolge
                var contour = Contours[k];
                var BoundingRect = Cv2.BoundingRect(contour);
                // Bounding rect wird zu groß selektiert also kleiner machen
                BoundingRect = new Rect(new OpenCvSharp.Point(BoundingRect.X + 1, BoundingRect.Y + 1), new OpenCvSharp.Size(BoundingRect.Width - 2, BoundingRect.Height - 2));
                for (int CaseLU = 0; CaseLU <= 1; CaseLU++) // Lower Case Upper Cas
                {
                    for (int j = 0; j < Letters.Length; j++)
                    {
                        // Buchstabe aus der Datenbank
                        Mat temp_template = new Mat();

                        // Lower Case Upper Case
                        if (CaseLU == 0)
                        {
                            // hier kommt das programm nur hin, wenn es sich in der zweiten Spalte des Dictionaries befindet
                            // sprich die Spalte mit den Kleinbuchstaben, aber ohne Zahlen und Sonderzeichen (Leere Matrix)

                            temp_template = templates[Letters[j]].klein;

                        }
                        else
                        {
                            if ("!1234567890↑".Contains(("" + Letters[j])) == false)
                            {
                                temp_template = templates[Letters[j]].gross;
                            }
                            else
                            {
                                // damit Zahlen und Sonderzeichen nicht doppelt überprüft werden
                                break;
                            }
                        }
                        /*if(BreakFlag == true)
                        {
                            break;
                        }*/
                        // Otsu Bild zuschneiden
                        Mat TempOtsuImage = OtsuImage[BoundingRect];
                        Mat TempOtsuImageOriginal = new Mat();
                        TempOtsuImage.CopyTo(TempOtsuImageOriginal);

                        // ratio wird nur in den switch cases benötigt, wird aber schon vorher definiert, damit es durch das resizen der Bilder nicht überschrieben wird sowie die resize aktionen nicht zweimal durchgeführt werden müssen
                        double ratio = Math.Abs((double)TempOtsuImageOriginal.Width / (double)TempOtsuImageOriginal.Height);
                        double iRatioLBoundLCase = CRVProject.Helper.Configuration.Instance.Recognition.iRatioLBoundLCase;
                        double iRatioUBoundLCase = CRVProject.Helper.Configuration.Instance.Recognition.iRatioUBoundLCase;
                        double iRatioLBoundUCase = CRVProject.Helper.Configuration.Instance.Recognition.iRatioLBoundUCase;
                        double iRatioUBoundUCase = CRVProject.Helper.Configuration.Instance.Recognition.iRatioUBoundUCase;
                        double ExclamThresh1 = CRVProject.Helper.Configuration.Instance.Recognition.ExclamThres1;
                        double ExclamThresh2 = CRVProject.Helper.Configuration.Instance.Recognition.ExclamThres2;
                        double DiffThresh = CRVProject.Helper.Configuration.Instance.Recognition.DiffThresh;
                        double MedianHeightMul = CRVProject.Helper.Configuration.Instance.Recognition.MedianHeightMul;
                        // Verleichen mit template
                        // OtsuImage auf Größe des Templates bringen
                        TempOtsuImage = TempOtsuImage.Resize(temp_template.Size());

                        // nochmal otsu weil nach interpolation wieder graustufen auftreten
                        Cv2.Threshold(TempOtsuImage, TempOtsuImage, 0, 255, ThresholdTypes.Otsu);

                        //Ergebnis matrix
                        Mat res = new Mat(temp_template.Size(), MatType.CV_8UC1);

                        //Vergleiche Buchstaben mit zugeschnittenem Bild
                        Cv2.Compare(TempOtsuImage, temp_template, res, CmpType.EQ);

                        //Übereinstimmung in Prozent, division durch 255 um Zähler zu normieren
                        Diff = Cv2.Sum(res).Val0 / (res.Width * res.Height) / 255;

                        /*Cv2.ImShow("cutout", temp_otsu_image);
                        Cv2.ImShow("template", temp_template);
                        Cv2.WaitKey();
                        if(debug) Console.WriteLine(diff);
                        if(debug) Console.WriteLine(correctLetter);*/
                        switch (Letters[j])
                        {
                            //Sonderfälle: für s, S, i, i-punkt ('!') muss die Übereinstimmung über das Seitenverältnis berechnet werden
                            case '!':
                                if (TempOtsuImageOriginal.Height / medianHeight < ExclamThresh1 && istAusfahrt == false)
                                {

                                    CorrectLetter = '!';
                                    TempMin = Diff;
                                }
                                else if (TempOtsuImageOriginal.Height / medianHeight < ExclamThresh2 && istAusfahrt == true) //leicht härtere Bedignung für
                                                                                                                             //Ausfahrtsschilder, da diese ja an
                                                                                                                             //der mittelinie geteilt werden und somit nur
                                                                                                                             //eine Zeile text besitzen, was bedeutet, dass
                                                                                                                             //mehrere buchstaben kleiner als 50% vom median sein
                                                                                                                             //können, bedingt durch die übergroßen Pfeile und die
                                                                                                                             //zweite sonderzeile die nur hier auf tritt und aus sehr
                                                                                                                             //kleinen Buchstaben besteht
                                {
                                    CorrectLetter = '!';
                                    TempMin = Diff;
                                }

                                break;
                            case 'i':
                                if (CaseLU == 0 && iRatioLBoundLCase < ratio && ratio < iRatioUBoundLCase && Diff > DiffThresh)
                                {
                                    CorrectLetter = 'i';
                                    TempMin = Diff;
                                    // beende beide schleifen bzw. füge den Buchstaben sofort hinzu
                                    j = Letters.Length;
                                    CaseLU = 1;
                                }
                                else if (CaseLU == 1 && iRatioLBoundUCase < ratio && ratio < iRatioUBoundUCase && Diff > DiffThresh)
                                {
                                    CorrectLetter = 'I';
                                    TempMin = Diff;
                                    // beende beide schleifen bzw. füge den Buchstaben sofort hinzu
                                    j = Letters.Length;
                                    CaseLU = 1;
                                }
                                break;
                            default:
                                if (Diff > TempMin)
                                {
                                    TempMin = Diff;
                                    if (CaseLU == 0)
                                    {
                                        CorrectLetter = Letters[j];
                                    }
                                    else if (CaseLU == 1 && TempOtsuImageOriginal.Height > medianHeight * MedianHeightMul)
                                    {
                                        CorrectLetter = Letters[j].ToString().ToUpper()[0]; // Großbuchstaben erkennen
                                    }
                                }
                                break;
                        }
                    }
                }
                // Gesamtes Wort zusammensetzen
                //if(debug) Console.WriteLine("Wahrscheinlichkeit fuer '" + correctLetter + "' beträgt " + TempMin);
                //if(debug) Console.WriteLine(TempMin);
                CorrectWord += CorrectLetter;
                Confidence += TempMin; //Wahrscheinlichkeit für korrekten Buchstaben
                //if(debug) Console.WriteLine("correct letter: " + correctLetter);
                Cv2.PutText(OtsuImageDraw, CorrectLetter.ToString(), new OpenCvSharp.Point(BoundingRect.X, BoundingRect.Y), HersheyFonts.Italic, 1, new Scalar(150, 150, 150));
                //Cv2.PutText(otsu_image, boundingRect.X.ToString(), new OpenCvSharp.Point(boundingRect.X, boundingRect.Y), HersheyFonts.Italic, 1, new Scalar(150, 150, 150));
                TempMin = 0;    // resette das temporären Minimum für die nächste Iteration
            }
            Confidence = Confidence / NumBoxes;
            if(debug) Console.WriteLine("Wahrscheinlickeit: " + Confidence);

            #endregion

            if(debug) Console.WriteLine($"Templates: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            #region ende

            string[] seperateWords = new[] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };

            // Maske auf das Wort anwenden und wörter trennen sowie Zeilen zuordnen
            for (int line = 0; line <= maskVar; line++)
            {
                for (int i = 0; i < CorrectWord.Length; i++)
                {
                    if (LineMask[i] == line)
                    {
                        seperateWords[line] += CorrectWord[i];
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
            #endregion

            if(debug) Console.WriteLine($"Ende: {stp.ElapsedMilliseconds} ms");
            stp.Restart();

            textConfidence = Confidence;
            return finalText;
        }
    }
}