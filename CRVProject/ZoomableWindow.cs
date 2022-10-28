using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CRVProject
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
            Cv2.NamedWindow(title);
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
            Mat r = new Mat(rect.Width, rect.Height, image.Type());
            Cv2.Transform(image, r, getScaleMatrix(zoom));

            Cv2.ImShow(title, r);
        }

        void mouseCallback(MouseEventTypes @event, int x, int y, MouseEventFlags flags, IntPtr userData)
        {
            if(@event == MouseEventTypes.MouseWheel)
            {
                zoom *= MathF.Pow(1.1f, y);
            }
        }

        static Mat getScaleMatrix(float scale)
        {
            Mat m = new Mat(3, 3, MatType.CV_32SC1);
            m.SetIdentity(new Scalar(scale));
            m.Set<float>(2, 2, 1);
            return m;
        }
    }
}
