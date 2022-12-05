using OpenCvSharp;

namespace CRVProject.Ortsschild.WinForms
{
    public partial class Form1 : Form
    {
        ViewMode viewMode = ViewMode.Original;
        VideoCapture? video = null;
        Mat? image = null;
        int videoPosition = 0;

        public Form1()
        {
            InitializeComponent();
        }

        void openImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = $"Files|" + String.Join(';', CRVProject.Helper.Util.SupportedImageTypes.Select(str => $"*.{str}")) + ";*.mp4";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                video?.Dispose();
                image?.Dispose();
                video = null;
                image = null;
                if(ofd.FileName.ToLower().EndsWith(".mp4"))
                {
                    video = new VideoCapture();
                    video.Open(ofd.FileName);
                    pbPlayback.Visible = true;
                    pbPlayback.Maximum = video.FrameCount;
                    pbPlayback.Value = 0;
                }
                else
                {
                    image = Cv2.ImRead(ofd.FileName);
                    pbPlayback.Visible = false;
                }

                videoPosition = 0;
                showImage();
            }
        }

        void showImage()
        {
            Mat frame;
            if (video is not null)
            {
                videoPosition %= video.FrameCount;
                video.PosFrames = videoPosition;
                frame = new Mat();
                video.Read(frame);
                lblImageInfo.Text = $"{frame.Width}x{frame.Height}; Frame {videoPosition}/{video.FrameCount}; FPS: {video.Fps}";
                pbPlayback.Value = videoPosition;
            }
            else if(image is not null)
            {
                frame = image.Clone();
                lblImageInfo.Text = $"{frame.Width}x{frame.Height}";
            }
            else
            {
                lblImageInfo.Text = "No images loaded";
                pb.Image?.Dispose();
                pb.Image = null;
                return;
            }

            using Locator locator = new Locator(frame);
            locator.Binarize();

            pb.Image?.Dispose();
            pb.Image = null;
            if (viewMode == ViewMode.Original)
                pb.Image = ImageConverter.Mat2Bitmap(frame);
            else if (viewMode == ViewMode.Binarized)
                pb.Image = ImageConverter.Mat2Bitmap(locator.BinarizedImage ?? new Mat());
            else if(viewMode == ViewMode.Regions)
            {
                using Mat gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                using Mat red = gray * (255 - locator.BinarizedImage);
                using Mat green = gray * (locator.BinarizedImage);
                using Mat blue = new Mat(gray.Width, gray.Height, MatType.CV_8UC1);
                using Mat rgb = new Mat();
                Cv2.Merge(new[] { red, green, blue }, rgb);
                pb.Image = ImageConverter.Mat2Bitmap(rgb);
            }
        }

        private void btnOriginal_Click(object sender, EventArgs e)
        {
            viewMode = ViewMode.Original;
            showImage();
        }

        private void btnBin_Click(object sender, EventArgs e)
        {
            viewMode = ViewMode.Binarized;
            showImage();
        }

        private void btnAreas_Click(object sender, EventArgs e)
        {
            viewMode = ViewMode.Regions;
            showImage();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            openImage();
        }

        private void btnForwardFrame_Click(object sender, EventArgs e)
        {
            if (video is null) return;
            videoPosition++;
            showImage();
        }

        private void btnBackFrame_Click(object sender, EventArgs e)
        {
            if (video is null) return;
            videoPosition--;
            showImage();
        }

        private void btnForwardSecond_Click(object sender, EventArgs e)
        {
            if (video is null) return;
            videoPosition += (int)video.Fps;
            showImage();
        }

        private void btnBackSecond_Click(object sender, EventArgs e)
        {
            if (video is null) return;
            videoPosition -= (int)video.Fps;
            showImage();
        }
    }

    public enum ViewMode
    {
        Original,
        Binarized,
        Regions
    }
}