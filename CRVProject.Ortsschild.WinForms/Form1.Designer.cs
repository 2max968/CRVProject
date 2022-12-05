namespace CRVProject.Ortsschild.WinForms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnOriginal = new System.Windows.Forms.ToolStripButton();
            this.btnBin = new System.Windows.Forms.ToolStripButton();
            this.btnAreas = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.bntPlayPause = new System.Windows.Forms.ToolStripButton();
            this.btnBackSecond = new System.Windows.Forms.ToolStripButton();
            this.btnBackFrame = new System.Windows.Forms.ToolStripButton();
            this.btnForwardFrame = new System.Windows.Forms.ToolStripButton();
            this.btnForwardSecond = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pb = new System.Windows.Forms.PictureBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblImageInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.pbPlayback = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpen,
            this.toolStripSeparator2,
            this.btnOriginal,
            this.btnBin,
            this.btnAreas,
            this.toolStripSeparator1,
            this.bntPlayPause,
            this.btnBackSecond,
            this.btnBackFrame,
            this.btnForwardFrame,
            this.btnForwardSecond});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(834, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = global::CRVProject.Ortsschild.WinForms.Properties.Resources.folder_picture;
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(23, 22);
            this.btnOpen.Text = "Open File";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnOriginal
            // 
            this.btnOriginal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOriginal.Image = global::CRVProject.Ortsschild.WinForms.Properties.Resources.picture;
            this.btnOriginal.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOriginal.Name = "btnOriginal";
            this.btnOriginal.Size = new System.Drawing.Size(23, 22);
            this.btnOriginal.Text = "Original Image";
            this.btnOriginal.Click += new System.EventHandler(this.btnOriginal_Click);
            // 
            // btnBin
            // 
            this.btnBin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnBin.Image = global::CRVProject.Ortsschild.WinForms.Properties.Resources.contrast;
            this.btnBin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBin.Name = "btnBin";
            this.btnBin.Size = new System.Drawing.Size(23, 22);
            this.btnBin.Text = "Binarized Image";
            this.btnBin.Click += new System.EventHandler(this.btnBin_Click);
            // 
            // btnAreas
            // 
            this.btnAreas.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAreas.Image = global::CRVProject.Ortsschild.WinForms.Properties.Resources.chart_pie;
            this.btnAreas.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAreas.Name = "btnAreas";
            this.btnAreas.Size = new System.Drawing.Size(23, 22);
            this.btnAreas.Text = "Show Areas";
            this.btnAreas.Click += new System.EventHandler(this.btnAreas_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // bntPlayPause
            // 
            this.bntPlayPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bntPlayPause.Image = global::CRVProject.Ortsschild.WinForms.Properties.Resources.control_play_blue;
            this.bntPlayPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bntPlayPause.Name = "bntPlayPause";
            this.bntPlayPause.Size = new System.Drawing.Size(23, 22);
            this.bntPlayPause.Text = "Play/Pause";
            this.bntPlayPause.Click += new System.EventHandler(this.bntPlayPause_Click);
            // 
            // btnBackSecond
            // 
            this.btnBackSecond.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnBackSecond.Image = global::CRVProject.Ortsschild.WinForms.Properties.Resources.control_rewind_blue;
            this.btnBackSecond.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBackSecond.Name = "btnBackSecond";
            this.btnBackSecond.Size = new System.Drawing.Size(23, 22);
            this.btnBackSecond.Text = "1s Back";
            this.btnBackSecond.Click += new System.EventHandler(this.btnBackSecond_Click);
            // 
            // btnBackFrame
            // 
            this.btnBackFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnBackFrame.Image = global::CRVProject.Ortsschild.WinForms.Properties.Resources.control_start_blue;
            this.btnBackFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBackFrame.Name = "btnBackFrame";
            this.btnBackFrame.Size = new System.Drawing.Size(23, 22);
            this.btnBackFrame.Text = "1 Frame Back";
            this.btnBackFrame.Click += new System.EventHandler(this.btnBackFrame_Click);
            // 
            // btnForwardFrame
            // 
            this.btnForwardFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnForwardFrame.Image = global::CRVProject.Ortsschild.WinForms.Properties.Resources.control_end_blue;
            this.btnForwardFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnForwardFrame.Name = "btnForwardFrame";
            this.btnForwardFrame.Size = new System.Drawing.Size(23, 22);
            this.btnForwardFrame.Text = "1 Frame Forward";
            this.btnForwardFrame.Click += new System.EventHandler(this.btnForwardFrame_Click);
            // 
            // btnForwardSecond
            // 
            this.btnForwardSecond.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnForwardSecond.Image = global::CRVProject.Ortsschild.WinForms.Properties.Resources.control_fastforward_blue;
            this.btnForwardSecond.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnForwardSecond.Name = "btnForwardSecond";
            this.btnForwardSecond.Size = new System.Drawing.Size(23, 22);
            this.btnForwardSecond.Text = "1 Second Forward";
            this.btnForwardSecond.Click += new System.EventHandler(this.btnForwardSecond_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pb);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyGrid1);
            this.splitContainer1.Size = new System.Drawing.Size(834, 482);
            this.splitContainer1.SplitterDistance = 566;
            this.splitContainer1.TabIndex = 1;
            // 
            // pb
            // 
            this.pb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pb.Location = new System.Drawing.Point(0, 0);
            this.pb.Name = "pb";
            this.pb.Size = new System.Drawing.Size(566, 482);
            this.pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb.TabIndex = 0;
            this.pb.TabStop = false;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(264, 482);
            this.propertyGrid1.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblImageInfo,
            this.pbPlayback});
            this.statusStrip1.Location = new System.Drawing.Point(0, 485);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(834, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblImageInfo
            // 
            this.lblImageInfo.Name = "lblImageInfo";
            this.lblImageInfo.Size = new System.Drawing.Size(118, 17);
            this.lblImageInfo.Text = "toolStripStatusLabel1";
            // 
            // pbPlayback
            // 
            this.pbPlayback.Name = "pbPlayback";
            this.pbPlayback.Size = new System.Drawing.Size(300, 16);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 507);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "Form1";
            this.Text = "9";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pb)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ToolStrip toolStrip1;
        private SplitContainer splitContainer1;
        private PictureBox pb;
        private PropertyGrid propertyGrid1;
        private ToolStripButton btnOriginal;
        private ToolStripButton btnBin;
        private ToolStripButton btnAreas;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton bntPlayPause;
        private ToolStripButton btnBackSecond;
        private ToolStripButton btnBackFrame;
        private ToolStripButton btnForwardFrame;
        private ToolStripButton btnForwardSecond;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblImageInfo;
        private ToolStripButton btnOpen;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripProgressBar pbPlayback;
    }
}