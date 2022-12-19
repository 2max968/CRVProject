using OpenCvSharp;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CRVProject.Ortsschild.WinForms
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        ViewMode viewMode = ViewMode.Original;
        VideoCapture? video = null;
        Mat? image = null;
        int videoPosition = 0;
        Thread? videoThread = null;
        bool runVideo;
        string cvTitle = Guid.NewGuid().ToString();

        public Form1()
        {
            InitializeComponent();

            this.Shown += Form1_Shown;
            this.Resize += Form1_Resize;
            this.ResizeBegin += Form1_ResizeBegin;
        }

        private void Form1_ResizeBegin(object? sender, EventArgs e)
        {
            
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            Cv2.NamedWindow(cvTitle, WindowFlags.KeepRatio);
            var hWnd = Cv2.GetWindowHandle(cvTitle);
            var hParent = GetParent(hWnd);
            ShowWindow(hParent, 0);
            SetParent(hWnd, pb.Handle);
            propertyGrid1.SelectedObject = CRVProject.Helper.Configuration.Instance.Locator;
        }

        void openImage()
        {
            var ofd = new FileSelector();
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                video?.Dispose();
                image?.Dispose();
                video = null;
                image = null;
                if(ofd.SelectedPath.ToLower().EndsWith(".mp4"))
                {
                    video = new VideoCapture();
                    video.Open(ofd.SelectedPath);
                    pbPlayback.Visible = true;
                    pbPlayback.Maximum = video.FrameCount;
                    pbPlayback.Value = 0;
                }
                else
                {
                    image = Cv2.ImRead(ofd.SelectedPath);
                    pbPlayback.Visible = false;
                }

                videoPosition = 0;
                runVideo = false;
                showImage();
            }
        }

        void dispMat(Mat? mat)
        {
            if (mat is null)
                return;

            var rect = Cv2.GetWindowImageRect(cvTitle);
            using Mat oFrame = new Mat(rect.Height, rect.Width, mat.Type());
            oFrame.SetTo(new Scalar(0, 0, 0));

            using Mat small = new Mat();
            float s1 = rect.Width / (float)mat.Width;
            float s2 = rect.Height / (float)mat.Height;
            float s = Math.Min(s1, s2);
            Cv2.Resize(mat, small, new OpenCvSharp.Size(mat.Width * s, mat.Height * s), 0, 0, InterpolationFlags.Cubic);
            small.CopyTo(oFrame[new Rect(0, 0, small.Width, small.Height)]);
            Cv2.ImShow(cvTitle, oFrame);
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
                //lblImageInfo.Text = $"{frame.Width}x{frame.Height}; Frame {videoPosition}/{video.FrameCount}; FPS: {video.Fps}";
                //pbPlayback.Value = videoPosition;
            }
            else if (image is not null)
            {
                frame = image.Clone();
                //lblImageInfo.Text = $"{frame.Width}x{frame.Height}";
            }
            else
            {
                //lblImageInfo.Text = "No images loaded";
                //pb.Image?.Dispose();
                //pb.Image = null;
                return;
            }

            using Locator locator = new Locator(frame);
            locator.Binarize();
            locator.RunLocator();

            var outputSize = pb.Size;
            Cv2.ResizeWindow(cvTitle, outputSize.Width, outputSize.Height);
            dispMat(frame);
            if (viewMode == ViewMode.Original)
                dispMat(frame);
            else if (viewMode == ViewMode.Binarized)
                dispMat(locator.BinarizedImage);
            else if(viewMode == ViewMode.Regions)
            {
                using Mat gray = new Mat();
                using Mat grayInv = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.BitwiseNot(gray, grayInv);
                using Mat red = grayInv;
                using Mat green = gray * (locator.BinarizedImage);
                using Mat blue = new Mat(gray.Width, gray.Height, MatType.CV_8UC1);
                using Mat rgb = new Mat();
                Cv2.Merge(new[ ] { red, green, blue }, rgb);
                dispMat(rgb);
            }

            if (locator.Ortsschilder.Count > 0 
                && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                pbOutput.Image?.Dispose();
                pbOutput.Image = ImageConverter.Mat2Bitmap(locator.Ortsschilder[0]);
                (pbOutput.Tag as IDisposable)?.Dispose();
                pbOutput.Tag = locator.Ortsschilder[0].Clone();

                tbOutInfo.Text = $"Typ: {Classification.Classify(locator.Ortsschilder[0])}\r\n" 
                    + $"Sharpness: {Classification.CalculateSharpness(locator.Ortsschilder[0])}";
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

        private void bntPlayPause_Click(object sender, EventArgs e)
        {
            if(videoThread != null)
            {
                runVideo = false;
                bntPlayPause.Image = Properties.Resources.control_play_blue;
            }
            else
            {
                runVideo = true;
                bntPlayPause.Image = Properties.Resources.control_pause_blue;
                videoThread = new Thread(videoLoop);
                videoThread.Start();
            }
        }

        void videoLoop()
        {
            if (video is null)
            {
                runVideo = false;
                videoThread = null;
                return;
            }
            int frametimeMs = (int)(1000 / video.Fps);

            Stopwatch stp = new Stopwatch();
            while(runVideo)
            {
                stp.Restart();
                videoPosition++;
                /*_ = Invoke(() =>
                {
                    showImage();
                    return true;
                });*/
                showImage();
                int waitTime = (int)(frametimeMs - stp.ElapsedMilliseconds);
                Thread.Sleep(Math.Max(0, waitTime));
            }

            videoThread = null;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            showImage();
        }

        private void btnSaveOutput_Click(object sender, EventArgs e)
        {
            if(pbOutput.Image == null)
            {
                MessageBox.Show("No image to save", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG|*.png|Jpeg|*.jpg";
            if(saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                pbOutput.Image.Save(saveFileDialog.FileName);
            }
        }

        private void pbOutput_Click(object sender, EventArgs e)
        {
            Mat? mat = pbOutput.Tag as Mat;
            if(mat is not null)
            {
                //OutputForm ofrm = new OutputForm(mat);
                //ofrm.ShowDialog();

                TextRecognition tr = new TextRecognition();
                string text = tr.Run(mat);
                tr.Dispose();
                tbOutInfo.Text = text.Replace("\n", "\r\n");
            }
        }
    }

    public enum ViewMode
    {
        Original,
        Binarized,
        Regions
    }
}