using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace Kamerawagen
{
    internal partial class SnapshotForm : Form
    {
        Bitmap image;
        public SnapshotForm()
        {
            InitializeComponent();
        }

        public void SetImage(Bitmap bitmap)
        {
            timeBox.Text = DateTime.Now.ToLongTimeString();

            lock (this)
            {
                Bitmap old = (Bitmap) pictureBox.Image;
                image = new Bitmap(bitmap);
                pictureBox.Image = image;
                if ( old != null )
                    {
                        old.Dispose( );
                    }
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(saveFileDialog.FileName).ToLower();
                ImageFormat format = ImageFormat.Jpeg;

                if (ext == ".bmp")
                {
                    format = ImageFormat.Bmp;
                }
                else if (ext == ".png")
                {
                    format = ImageFormat.Png;
                }

                try
                {
                    lock (this)
                    {
                        image.Save(saveFileDialog.FileName, format);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed saving the snapshot.\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
