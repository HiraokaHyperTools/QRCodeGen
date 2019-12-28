using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QRCodeGen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void qrText_TextChanged(object sender, EventArgs e)
        {
            if (qrText.Text.Length == 0)
            {
                qrImage.Image = null;
                return;
            }

            var writer = new ZXing.QrCode.QRCodeWriter();
            var hints = new Dictionary<ZXing.EncodeHintType, object>();
            hints[ZXing.EncodeHintType.CHARACTER_SET] = "UTF-8";
            var matrix = writer.encode(
                qrText.Text
                    .Replace("\r\n", "\n"),
                ZXing.BarcodeFormat.QR_CODE, 1, 1, hints);

            var scale = 5;
            var bitmap = new Bitmap(scale * matrix.Width, scale * matrix.Height);
            using (var canvas = Graphics.FromImage(bitmap))
            {
                canvas.Clear(Color.White);
                for (int y = 0; y < matrix.Height; y++)
                {
                    for (int x = 0; x < matrix.Width; x++)
                    {
                        if (matrix[x, y])
                        {
                            canvas.FillRectangle(
                                Brushes.Black,
                                new Rectangle(
                                    scale * x,
                                    scale * y,
                                    scale,
                                    scale
                                )
                            );
                        }
                    }
                }
            }

            qrImage.Image = bitmap;
        }

        private void qrImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && qrImage.Image != null)
            {
                var png = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "QRCode.png"
                );
                qrImage.Image.Save(png, ImageFormat.Png);
                var data = new DataObject();
                var files = new StringCollection();
                files.Add(png);
                data.SetFileDropList(files);
                qrImage.DoDragDrop(data, DragDropEffects.All);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            qrText_TextChanged(sender, e);
        }

        private void copyBtn_Click(object sender, EventArgs e)
        {
            if (qrImage.Image != null)
            {
                Clipboard.Clear();
                Clipboard.SetImage(qrImage.Image);
                MessageBox.Show("コピーしました。");
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (qrImage.Image != null && sfdPng.ShowDialog(this) == DialogResult.OK)
            {
                qrImage.Image.Save(sfdPng.FileName, ImageFormat.Png);
                MessageBox.Show("保存しました。");
            }
        }

        private void pasteBtn_Click(object sender, EventArgs e)
        {
            qrText.Text = (Clipboard.GetText() ?? "")
                .Replace("\r\n", "\n").Replace("\n", "\r\n")
                ;
        }

        private void trimBtn_Click(object sender, EventArgs e)
        {
            qrText.Text = qrText.Text
                .Replace("\r\n", "\n").Replace("\n", "\r\n")
                .Replace("\"\"", "\"").Trim().Trim('"');
        }
    }
}
