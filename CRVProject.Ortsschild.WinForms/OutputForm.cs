using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRVProject.Ortsschild.WinForms
{
    public partial class OutputForm : Form
    {
        Mat image;

        public OutputForm(Mat image)
        {
            InitializeComponent();

            this.image = image.Clone();

            pbIn.Image = ImageConverter.Mat2Bitmap(image);
            using Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            using Mat bin = new Mat();
            Cv2.Threshold(gray, bin, 0, 255, ThresholdTypes.Otsu);

            int kernelSize1 = 20;
            using var kernel1 = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(kernelSize1, kernelSize1));
            Cv2.Erode(bin, bin, kernel1);
            Cv2.Dilate(bin, bin, kernel1);

            pbBin.Image = ImageConverter.Mat2Bitmap(bin);
        }


    }
}
