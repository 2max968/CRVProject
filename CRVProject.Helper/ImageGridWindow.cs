using System.Runtime.InteropServices;
using OpenCvSharp;

namespace CRVProject.Helper
{
    public class ImageGridWindow
    {
        public delegate void TrackbarValueChangedEvent(ImageGridWindow wnd, string trackbarName, int trackbarValue);
        public delegate void TickEvent(ImageGridWindow wnd);

        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public Scalar BackgroundColor { get; set; } = Scalar.White;
        public Scalar ForegroundColor { get; set; } = Scalar.Gray;
        public bool Visible { get; private set; } = false;
        public int Margin { get; set; } = 16;
        Mat?[] images;
        string title;
        Size tmpWndSize = new Size(0, 0);
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
        List<Trackbar> trackbars = new List<Trackbar>();
        public event TrackbarValueChangedEvent? OnTrackbarValueChanged;
        public event TickEvent? OnTick;
        private Rect[]? imageRectangles;

        public ImageGridWindow(int columns, int rows)
        {
            this.Rows = rows;
            this.Columns = columns;
            this.images = new Mat[rows * columns];
            title = Guid.NewGuid().ToString();
        }

        ~ImageGridWindow()
        {
            Hide();
        }

        public void Run()
        {
            Visible = true;
            Cv2.NamedWindow(title);
            Cv2.ResizeWindow(title, Width, Height);
            List<IntPtr> memToDealloc = new List<IntPtr>();
            foreach(var tb in trackbars)
            {
                var currentUserData = Marshal.StringToHGlobalUni(tb.Name);
                memToDealloc.Add(currentUserData);
                Cv2.CreateTrackbar(tb.Name, title, 1, (int pos, IntPtr userData) =>
                {
                    string name = Marshal.PtrToStringUni(userData) ?? "";
                    OnTrackbarValueChanged?.Invoke(this, name, pos);
                }, currentUserData);
                Cv2.SetTrackbarMin(tb.Name, title, tb.Min);
                Cv2.SetTrackbarMax(tb.Name, title, tb.Max);
            }
            render();
            while(Cv2.GetWindowProperty(title, WindowPropertyFlags.Visible) != 0)
            {
                render();
                Cv2.SetMouseCallback(title, mouseCallback);
                OnTick?.Invoke(this);
                Cv2.WaitKey(100);
            }
            Visible = false;
            foreach (var ptr in memToDealloc)
                Marshal.FreeHGlobal(ptr);
        }

        [Obsolete]
        public void Show()
        {
            Visible = true;
            Cv2.NamedWindow(title, WindowFlags.AutoSize);
            render();
            Cv2.SetMouseCallback(title, mouseCallback);
        }

        [Obsolete]
        public void Hide()
        {
            Visible = false;
            if(Cv2.GetWindowProperty(title, WindowPropertyFlags.Visible) != 0)
                Cv2.DestroyWindow(title);
        }

        public void SetImage(int x, int y, Mat? image)
        {
            images[y * Columns + x]?.Dispose();
            Mat? clone = image?.Clone();
            images[y * Columns + x] = clone;
            render();
        }

        void render()
        {
            if (!Visible)
                return;

            var rect = Cv2.GetWindowImageRect(title);

            Mat outMat = new Mat(rect.Height, rect.Width, MatType.CV_8UC3);
            Size tileSize = new Size(rect.Width / Columns, rect.Height / Rows);

            imageRectangles = new Rect[images.Length];
            outMat.SetTo(BackgroundColor);
            for(int y = 0; y < Rows; y++)
            {
                for(int x = 0; x < Columns; x++)
                {
                    Rect tileRect = new Rect(x * tileSize.Width + Margin / 2,
                        y * tileSize.Height + Margin / 2,
                        tileSize.Width - Margin,
                        tileSize.Height - Margin);

                    Cv2.Rectangle(outMat, tileRect, ForegroundColor, -1);
                    
                    imageRectangles[y * Columns + x] = tileRect;

                    Mat? img = images[y * Columns + x];
                    if (img != null && !img.Empty())
                    {
                        float scale1 = (float)tileRect.Width / img.Width;
                        float scale2 = (float)tileRect.Height / img.Height;
                        float scale = MathF.Min(scale1, scale2);
                        Rect imageRect = new Rect(tileRect.X + (int)(tileRect.Width - img.Width * scale) / 2,
                            tileRect.Y + (int)(tileRect.Height - img.Height * scale) / 2,
                            (int)(img.Width * scale),
                            (int)(img.Height * scale));

                        Mat smallImage = new Mat();
                        Cv2.Resize(img, smallImage, imageRect.Size, 0, 0, InterpolationFlags.Cubic);
                        convertImage(smallImage, smallImage);
                        smallImage.CopyTo(outMat[imageRect]);
                        smallImage.Dispose();
                    }

                    Cv2.Rectangle(outMat, tileRect, ForegroundColor, 1);
                }
            }

            Cv2.ImShow(title, outMat);
        }

        static void convertImage(Mat src, Mat dst)
        {
            var type = src.Type();

            if(type == MatType.CV_8UC3)
            {
                if (src != dst)
                    src.CopyTo(dst);
            }
            else if(type == MatType.CV_8UC1)
            {
                Cv2.CvtColor(src, dst, ColorConversionCodes.GRAY2RGB);
            }
            else if(type == MatType.CV_16UC4)
            {
                Cv2.CvtColor(src, dst, ColorConversionCodes.RGBA2RGB);
            }
            else
            {
                throw new Exception($"Matrixtype {type} is not supported in {typeof(ImageGridWindow).Name}");
            }
        }

        void mouseCallback(MouseEventTypes @event, int x, int y, MouseEventFlags flags, IntPtr userData)
        {
            if (@event == MouseEventTypes.LButtonUp && imageRectangles != null)
            {
                for (int i = 0; i < imageRectangles.Length; i++)
                {
                    if (imageRectangles[i].Contains(x, y))
                    {
                        Cv2.NamedWindow($"{title} {i}", WindowFlags.Normal);
                        Cv2.ResizeWindow($"{title} {i}", new Size(800, 600));
                        Cv2.ImShow($"{title} {i}", images[i]);
                    }
                }
            }
        }

        public void AddTrackbar(string name , int min, int max)
        {
            trackbars.Add(new Trackbar(name, min, max));
        }

        public void RemoveTrackbar(string name)
        {
            var tb = trackbars.Where(tb => tb.Name == name).FirstOrDefault();
            if(tb != null)
                trackbars.Remove(tb);
        }

        public int GetTrackbarValue(string name)
        {
            return Cv2.GetTrackbarPos(name, title);
        }

        public class Trackbar
        {
            public string Name;
            public int Min;
            public int Max;

            public Trackbar(string name, int min, int max)
            {
                Name = name;
                Min = min;
                Max = max;
            }
        }
    }
}
