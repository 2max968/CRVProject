using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CRVProject
{
    public class Util
    {
        const int K_ARROWUP = 2490368;
        const int K_ARROWDOWN = 2621440;
        const int K_ENTER = 13;
        const int K_CANCEL = 27;

        public static int Select(string comment, params string[] options)
        {
            char[] chars = new char[options.Length];
            Console.WriteLine(comment);
            for(int i = 0; i < options.Length; i++)
            {
                char chr = (char)('1' + i);
                if (i >= 9)
                    chr = (char)('a' + i - 9);
                chars[i] = chr;
                Console.WriteLine($" {chr}) {options[i]}");
            }
            Console.Write("> ");

            while(true)
            {
                var inp = Console.ReadKey(true);
                if (chars.Contains(inp.KeyChar))
                {
                    int selectedIndex = chars.Select((chr, index) => (chr, index)).Where(i => i.chr == inp.KeyChar).First().index;
                    Console.WriteLine($"Selected '{options[selectedIndex]}'");
                    return selectedIndex;
                }
            }
        }

        public static float[] CalcHist(Mat image, int channel = 0)
        {
            using Mat hist = new Mat();
            Cv2.CalcHist(new Mat[] { image },
                new int[] { channel },
                null,
                hist,
                1,
                new int[] { 255 },
                new Rangef[] { new Rangef(0, 255) });

            float[] data = new float[255];
            for (int i = 0; i < 255; i++)
                data[i] = hist.At<float>(i);
            return data;
        }

        public static Mat DrawHistogram(float[] data, float max = float.NaN)
        {
            Mat graph = new Mat(255, 255, MatType.CV_8UC1);
            graph.SetTo(new Scalar(0));
            if(!float.IsNormal(max))
                max = data.Max();
            for (int i = 0; i < 255; i++)
            {
                int barHeight = (int)(data[i] * graph.Height / max);
                int y = graph.Height - barHeight;
                Cv2.Line(graph, i, y, i, 255, new Scalar(255));
            }
            return graph;
        }

        public static Mat DrawHistogram(Mat image, int channel = -1)
        {
            var type = image.Type();

            if(type == MatType.CV_8UC3 && channel < 0)
            {
                var dr = CalcHist(image, 0);
                var dg = CalcHist(image, 1);
                var db = CalcHist(image, 2);
                var mr = dr.Max();
                var mg = dg.Max();
                var mb = db.Max();
                var max = MathF.Max(mr, MathF.Max(mg, mb));
                using Mat r = DrawHistogram(dr, max);
                using Mat g = DrawHistogram(dg, max);
                using Mat b = DrawHistogram(db, max);
                Mat rgb = new Mat();
                Cv2.Merge(new[] {r,g,b}, rgb);
                return rgb;
            }

            if (channel < 0)
                channel = 0;

            float[] data = CalcHist(image, channel);
            return DrawHistogram(data);
        }

        public static int SelectGUI(bool canCancel, params string[] options)
        {
            Size btnSize = new Size(200, 32);
            int selected = 0;
            bool clicked = false;
            string title = Guid.NewGuid().ToString();

            Cv2.NamedWindow(title, WindowFlags.AutoSize);

            while(true)
            {
                using Mat img = new Mat(btnSize.Height * options.Length, btnSize.Width, MatType.CV_8UC3);
                img.SetTo(new Scalar(255, 255, 255));
                for(int i = 0; i < options.Length;i++)
                {
                    if(selected == i)
                    {
                        Cv2.Rectangle(img, new Rect(0, btnSize.Height * i, btnSize.Width, btnSize.Height), Scalar.DeepSkyBlue, -1);
                    }
                    Cv2.Rectangle(img, new Rect(0, btnSize.Height * i, btnSize.Width, btnSize.Height), Scalar.Black);
                    Cv2.PutText(img, options[i], new Point(4, btnSize.Height * (i + 0.5)),
                        HersheyFonts.HersheyPlain, 1, Scalar.Black);
                }

                Cv2.ImShow(title, img);
                Cv2.SetMouseCallback(title, (@event, x, y, flags, userData) =>
                {
                    if (@event == MouseEventTypes.MouseMove || @event == MouseEventTypes.LButtonDown)
                    {
                        int hoverPos = y / btnSize.Height;
                        if (hoverPos >= 0 && hoverPos < options.Length)
                            selected = hoverPos;
                    }
                    if (@event == MouseEventTypes.LButtonDown)
                    {
                        clicked = true;
                    }
                });

                int key = Cv2.WaitKeyEx(100);
                if (key == K_ENTER || clicked)
                    break;
                if(key == K_ARROWUP)
                {
                    selected--;
                    if (selected < 0)
                        selected = options.Length - 1;
                }
                if(key == K_ARROWDOWN)
                {
                    selected++;
                    if (selected >= options.Length)
                        selected = 0;
                }
                if (canCancel)
                {
                    if (key == K_CANCEL)
                    {
                        selected = -1;
                        break;
                    }
                    if (Cv2.GetWindowProperty(title, WindowPropertyFlags.Visible) == 0)
                    {
                        selected = -1;
                        break;
                    }
                }
            }

            if (Cv2.GetWindowProperty(title, WindowPropertyFlags.Visible) != 0)
                Cv2.DestroyWindow(title);
            return selected;
        }

        public static Scalar GetColor(string htmlColor)
        {
            if(htmlColor.Length == 6)
            {
                if (byte.TryParse(htmlColor.Substring(0,2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r)
                    && byte.TryParse(htmlColor.Substring(2,2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g)
                    && byte.TryParse(htmlColor.Substring(4,2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                {
                    return new Scalar(b, g, r);
                }
            }
            else if(htmlColor.Length == 3)
            {
                if (byte.TryParse(htmlColor.Substring(0, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r)
                    && byte.TryParse(htmlColor.Substring(1, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g)
                    && byte.TryParse(htmlColor.Substring(3, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                {
                    return new Scalar(b * 16, g * 16, r * 16);
                }
            }
            else if((htmlColor.Length == 7 || htmlColor.Length == 4) && htmlColor[0] == '#')
            {
                return GetColor(htmlColor.Substring(1));
            }

            throw new ArgumentException($"Can't parse {htmlColor}", "htmlColor");
        }
    }
}
