using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRVProject.Ortsschild
{
    public class VideoPlayer
    {
        VideoCapture cap;
        string filename = "";
        int millisPerFrame;
        double fps;
        string title = "Video Player <space> - Play/Pause; <n> - Next Frame; <p> - Previous Frame; <f> - fast";
        bool playing = true;
        int WindowWidth = 800;
        int WindowHeight = 800;
        int ImageWidth;
        int ImageHeight;
        int OutputWidth = 300;
        int OutputHeight = 200;
        TextMemory memory = new TextMemory();

        public VideoPlayer(VideoCapture cap, string filename)
        {
            this.cap = cap;
            this.filename = filename;
            fps = cap.Fps;
            millisPerFrame = (int)(1000.0 / fps);

            float w = cap.FrameWidth;
            float h = cap.FrameHeight;
            float s1 = WindowWidth / w;
            float s2 = WindowHeight / 2 / h;
            float s = Math.Min(s1, s2);
            ImageWidth = (int)(w * s);
            ImageHeight = (int)(h * s);
        }

        public void Run()
        {
            Stopwatch stp = new Stopwatch();
            bool redraw = false;
            bool fast = false;
            while(true)
            {
                stp.Restart();
                using Mat mat = new Mat();
                if (playing || redraw)
                {
                    redraw = false;
                    cap.Read(mat);
                    DoFrame(mat);
                    if (!playing)
                        cap.PosFrames--;
                    if (cap.PosFrames >= cap.FrameCount)
                        cap.PosFrames = 0;
                }
                int leftFrametime = millisPerFrame - (int)stp.ElapsedMilliseconds;
                if (leftFrametime < 1 || fast)
                    leftFrametime = 1;
                int key = Cv2.WaitKey(leftFrametime);
                if (key == ' ')
                    playing = !playing;
                else if (key == 'p')
                {
                    cap.PosFrames--;
                    redraw = true;
                }
                else if (key == 'n')
                {
                    cap.PosFrames++;
                    redraw = true;
                }
                else if(key == 'f')
                {
                    fast = !fast;
                }
                else if(key == 's')
                {
                    string fname = $"{DateTime.Now.ToString("yy.MM.dd_hh.mm.ss")} {filename}cap.png";
                    using Mat capture = new Mat();
                    cap.Read(capture);
                    cap.PosFrames--;
                    Cv2.ImWrite(fname, capture);
                }
                if (Cv2.GetWindowProperty(title, WindowPropertyFlags.Visible) == 0)
                    break;
            }

        }

        public void DoFrame(Mat frame)
        {
            using Locator locator = new Locator(frame);
            locator.RunLocator();
            using Mat binOutput = new Mat();
            Cv2.CvtColor(locator.BinarizedImage, binOutput, ColorConversionCodes.GRAY2BGR);
            Cv2.DrawContours(binOutput, locator.Contours
                .Select(ps => ps.Select(p => p.ToPoint())),
                -1, new Scalar(0, 255, 0), 4);

            Cv2.Resize(frame, frame, new Size(ImageWidth, ImageHeight), 0, 0, InterpolationFlags.Cubic);
            Cv2.Resize(binOutput, binOutput, new Size(ImageWidth, ImageHeight), 0, 0, InterpolationFlags.Cubic);

            using Mat output = new Mat(ImageHeight * 2, ImageWidth, MatType.CV_8UC3);
            frame.CopyTo(output[new Rect(0, 0, ImageWidth, ImageHeight)]);
            binOutput.CopyTo(output[new Rect(0, ImageHeight, ImageWidth, ImageHeight)]);

            if (locator.Ortsschilder.Count > 0)
            {
                using Mat schild = new Mat();
                Cv2.Resize(locator.Ortsschilder[0], schild, new Size(OutputWidth, OutputHeight), 0, 0, InterpolationFlags.Cubic);
                schild.CopyTo(output[new Rect(0, 0, OutputWidth, OutputHeight)]);
                try
                {
                    TextRecognition tr = new TextRecognition();
                    bool isAusfahrt = tr.Preprocess(locator.Ortsschilder[0], false);
                    string text = tr.Run(locator.Ortsschilder[0], isAusfahrt, out double confidence, false);
                    Cv2.Rectangle(output, new Rect(ImageWidth - 100, 0, 100, 64), new Scalar(0, 0, 0), -1);
                    Cv2.PutText(output, text, new Point(ImageWidth - 96, 16), HersheyFonts.HersheyPlain, 1, new Scalar(255, 255, 255));

                    double size = Cv2.ContourArea(locator.Contours[0]) / (frame.Width * frame.Height);
                    Console.WriteLine($"[{(isAusfahrt ? 'A' : 'E')}][{Math.Round(confidence * 100)}%][{Math.Round(size * 100)}%]" + String.Join(", ", text.Split('\n', '\r').Select(s => $"\"{s}\"")));
                    memory.PushEntry(cap.PosFrames / cap.Fps, confidence, text, cap.PosFrames, size, isAusfahrt);
                }
                catch(Exception ex)
                {
                    /*Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();*/
                }
            }

            var result = memory.GetResult(cap.PosFrames / cap.Fps);
            if (result is not null)
            {
                Console.WriteLine("==> " + (result.Ausfahrt ? "Ausfahrt" : "Einfahrt") + ":\n" + result.Text);
                int tmpPos = cap.PosFrames;
                cap.PosFrames = result.FramePos;
                using Mat resMat = new Mat();
                cap.Read(resMat);
                cap.PosFrames = tmpPos;
                //using var l = new Locator(resMat);
                //l.RunLocator();
                Cv2.Resize(resMat, resMat, new Size(800, 450), 0, 0, InterpolationFlags.Cubic);
                Cv2.ImShow("Result", resMat);
                while (Cv2.GetWindowProperty("Result", WindowPropertyFlags.Visible) != 0)
                    Cv2.WaitKey(100);
            }

            Cv2.ImShow(title, output);
        }
    }
}
