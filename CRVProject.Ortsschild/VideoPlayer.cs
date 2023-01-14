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

        public VideoPlayer(VideoCapture cap)
        {
            this.cap = cap;
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
            Cv2.DrawContours(binOutput, locator.Contours, -1, new Scalar(0, 255, 0), 4);

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
                    string text = tr.Run(locator.Ortsschilder[0], true, false);
                    Cv2.Rectangle(output, new Rect(ImageWidth - 100, 0, 100, 64), new Scalar(0, 0, 0), -1);
                    Cv2.PutText(output, text, new Point(ImageWidth - 96, 16), HersheyFonts.HersheyPlain, 1, new Scalar(255, 255, 255));
                    Console.WriteLine(String.Join(", ", text.Split('\n', '\r').Select(s => $"\"{s}\"")));
                }
                catch(Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }

            Cv2.ImShow(title, output);
        }
    }
}
