using OpenCvSharp;

namespace CRVProject.Helper
{
    public class ZoomableWindow
    {
        Mat? image;
        string title;
        float zoom = 1;
        float x = 0;
        float y = 0;

        public ZoomableWindow(Mat image)
        {
            this.image = image;
            title = Guid.NewGuid().ToString();
        }

        public void Show()
        {
            Cv2.NamedWindow(title, WindowFlags.OpenGL);
            Cv2.ResizeWindow(title, 800, 600);
            Cv2.SetMouseCallback(title, mouseCallback);

            while(Cv2.GetWindowProperty(title, WindowPropertyFlags.Visible) != 0)
            {
                render();
                Cv2.WaitKey(100);
            }
        }

        void render()
        {
            if (image == null)
                return;

            var rect = Cv2.GetWindowImageRect(title);
            using Mat tmp = new Mat();
            float scaleX = (float)rect.Width / image.Width;
            float scaleY = (float)rect.Height / image.Height;
            float scale = MathF.Min(scaleX, scaleY);
            Cv2.Resize(image, tmp, new Size(image.Width * scale, image.Height * scale));
            Rect targetRect = new Rect((rect.Width - tmp.Width) / 2, (rect.Height - tmp.Height) / 2, tmp.Width, tmp.Height);
            using Mat window = new Mat(rect.Width, rect.Height, tmp.Type());
            tmp.CopyTo(window[targetRect]);
            Cv2.ImShow(title, window);
        }

        void mouseCallback(MouseEventTypes @event, int x, int y, MouseEventFlags flags, IntPtr userData)
        {
            if(@event == MouseEventTypes.MouseWheel)
            {
                zoom *= MathF.Pow(1.1f, y);
            }
        }
    }
}
