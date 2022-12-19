namespace CRVProject.Ortsschild.WinForms
{
    partial class OutputForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbIn = new System.Windows.Forms.PictureBox();
            this.pbBin = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbIn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBin)).BeginInit();
            this.SuspendLayout();
            // 
            // pbIn
            // 
            this.pbIn.Location = new System.Drawing.Point(12, 12);
            this.pbIn.Name = "pbIn";
            this.pbIn.Size = new System.Drawing.Size(450, 300);
            this.pbIn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIn.TabIndex = 0;
            this.pbIn.TabStop = false;
            // 
            // pbBin
            // 
            this.pbBin.Location = new System.Drawing.Point(468, 12);
            this.pbBin.Name = "pbBin";
            this.pbBin.Size = new System.Drawing.Size(450, 300);
            this.pbBin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbBin.TabIndex = 1;
            this.pbBin.TabStop = false;
            // 
            // OutputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(935, 450);
            this.Controls.Add(this.pbBin);
            this.Controls.Add(this.pbIn);
            this.Name = "OutputForm";
            this.Text = "OutputForm";
            ((System.ComponentModel.ISupportInitialize)(this.pbIn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBox pbIn;
        private PictureBox pbBin;
    }
}