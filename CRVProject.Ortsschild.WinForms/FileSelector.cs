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
    public partial class FileSelector : Form
    {
        public string SelectedPath { get; set; } = "";

        public FileSelector()
        {
            InitializeComponent();

            DirectoryInfo dir = new DirectoryInfo("Images");
            foreach(var file in dir.GetFiles())
            {
                if(CRVProject.Helper.Util.SupportedImageTypes.Select(type => file.Name.EndsWith(type)).Count() > 0 || file.Name.EndsWith(".mp4"))
                {
                    ListViewItem itm = new ListViewItem(file.Name);
                    itm.Text = file.Name;
                    itm.Tag = file.FullName;
                    listView1.Items.Add(itm);
                }
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            string? filename = listView1.FocusedItem?.Tag as string;
            if (filename is not null)
            {
                SelectedPath = filename;
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            string? filename = listView1.FocusedItem?.Tag as string;
            if (filename is not null)
            {
                if(filename.EndsWith(".mp4"))
                {
                    //pbPreview.Image?.Dispose();
                    pbPreview.Image = null;
                }
                else
                {
                    //pbPreview.Image?.Dispose();
                    pbPreview.ImageLocation = filename;
                }
            }
        }
    }
}
