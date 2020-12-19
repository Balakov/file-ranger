using Cyotek.Windows.Forms;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Ranger
{
    public partial class ImageForm : Form
    {
        private class ImageState
        {
            public int Rotation;
        }

        private string m_imageDirectory;
        private string m_imageFileName;
        private int m_currentFileIndex = 0;
        private int m_maxFileIndex = 0;
        private bool m_isFullScreen = false;

        private Config m_config;
        private readonly string[] m_supportedExtensions;
        private IWindowManager m_windowManager;

        private Dictionary<string, ImageState> m_imageStates = new Dictionary<string, ImageState>();

        public ImageForm()
        {
            InitializeComponent();
        }

        public ImageForm(string imagePath, Config config, string[] supportedExtensions, IWindowManager windowManager)
        {
            m_config = config;
            m_supportedExtensions = supportedExtensions;
            m_windowManager = windowManager;

            InitializeComponent();

            ImageBoxControl.BackColor = Color.FromArgb(255, 63,63,63);

            if (Enum.TryParse(m_config.GetValue("interpolation_mode", ImageBoxControl.InterpolationMode.ToString()), out System.Drawing.Drawing2D.InterpolationMode mode))
            {
                ImageBoxControl.InterpolationMode = mode;
            }

            LayoutFromConfig();

            string imageDirectory = Path.GetDirectoryName(imagePath);

            if (!string.IsNullOrEmpty(imageDirectory))
            {
                var files = ScanImageFiles(imageDirectory);
                int currentFileIndex = FindImageIndexInFiles(imagePath, files);

                SetImage(imageDirectory, imagePath, currentFileIndex, files.Count);
            }
            else if(imagePath == "CLIPBOARD")
            {
                SetImage(imagePath, imagePath, 0, 0);
            }

            this.Closing += new System.ComponentModel.CancelEventHandler(this.MyOnClosing);
        }

        private void LayoutFromConfig()
        {
            int xPos = Convert.ToInt32(m_config.GetValue("imagewindow_xpos", this.Location.X.ToString()));
            int yPos = Convert.ToInt32(m_config.GetValue("imagewindow_ypos", this.Location.Y.ToString()));
            int width = Convert.ToInt32(m_config.GetValue("imagewindow_width", this.Size.Width.ToString()));
            int height = Convert.ToInt32(m_config.GetValue("imagewindow_height", this.Size.Height.ToString()));
            this.Size = new System.Drawing.Size(width, height);
            this.Location = new Point(xPos, yPos);

            bool isMaximised = Convert.ToBoolean(m_config.GetValue("imagewindow_maximised", "false"));
            if (isMaximised)
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void MyOnClosing(object o, System.ComponentModel.CancelEventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                m_config.SetValue("imagewindow_maximised", (this.WindowState == FormWindowState.Minimized).ToString());
                m_config.SetValue("imagewindow_xpos", this.Location.X.ToString());
                m_config.SetValue("imagewindow_ypos", this.Location.Y.ToString());
                m_config.SetValue("imagewindow_width", this.Size.Width.ToString());
                m_config.SetValue("imagewindow_height", this.Size.Height.ToString());
            }

            m_windowManager.ClosingImageWindow(this);
        }

        private void ImageBoxControl_ZoomChanged(object sender, EventArgs e)
        {
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            if (m_imageFileName != null)
            {
                Text = $"{Path.GetFileName(m_imageFileName)} - {m_currentFileIndex}/{m_maxFileIndex} - {ImageBoxControl.Image.Width}x{ImageBoxControl.Image.Height} - {ImageBoxControl.Zoom}%";
            }
            else
            {
                Text = $"No file - {m_currentFileIndex}/{m_maxFileIndex} - {ImageBoxControl.Zoom}%";
            }
        }

        private void SetImage(string imageDirectory, string imageFileName, int currentIndex, int maxIndex)
        {
            m_imageDirectory = imageDirectory;
            m_imageFileName = imageFileName;
            m_currentFileIndex = currentIndex + 1;
            m_maxFileIndex = maxIndex;

            var oldImage = ImageBoxControl.Image;

            if (imageFileName != null)
            {
                if (imageFileName == "CLIPBOARD")
                {
                    ImageBoxControl.Image = Clipboard.GetImage();
                    ImageBoxControl.Text = null;
                }
                else
                {
                    try
                    {
                        var image = ImageLoader.Load(imageFileName);
                        LoadUserRotationState(imageFileName, image);
                        ImageBoxControl.Image = image;
                        ImageBoxControl.Text = null;
                    }
                    catch (Exception e)
                    {
                        ImageBoxControl.Image = null;
                        ImageBoxControl.Text = e.Message;
                        m_currentFileIndex = 0;
                    }
                }
            }
            else
            {
                ImageBoxControl.Image = null;
                ImageBoxControl.Text = "No File";
                m_currentFileIndex = 0;
            }

            if (oldImage != null)
            {
                oldImage.Dispose();
            }

            ImageBoxControl.ZoomToFit();

            UpdateTitle();
        }

        private void ViewPixelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageBoxControl.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            m_config.SetValue("interpolation_mode", ImageBoxControl.InterpolationMode.ToString());
        }

        private void ViewBilinearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageBoxControl.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            m_config.SetValue("interpolation_mode", ImageBoxControl.InterpolationMode.ToString());
        }

        private void ViewBicubicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageBoxControl.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            m_config.SetValue("interpolation_mode", ImageBoxControl.InterpolationMode.ToString());
        }

        private void ViewOptionsContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            ViewPixelsToolStripMenuItem.Checked = false;
            ViewBicubicToolStripMenuItem.Checked = false;
            ViewBilinearToolStripMenuItem.Checked = false;

            switch (ImageBoxControl.InterpolationMode)
            {
                case System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor:
                    ViewPixelsToolStripMenuItem.Checked = true;
                    break;
                case System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear:
                    ViewBilinearToolStripMenuItem.Checked = true;
                    break;
                case System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic:
                    ViewBicubicToolStripMenuItem.Checked = true;
                    break;
            }
        }

        private void ResetZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageBoxControl.Zoom = 100;
        }

        private void galleryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_windowManager.OpenRangerWindow(m_imageDirectory);
        }

        private void fitToScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageBoxControl.ZoomToFit();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageSaveFileDialog.Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*";
            var now = DateTime.Now;
            ImageSaveFileDialog.FileName = $"Clipboard_{now.ToString("yyyy-MM-dd_HH-mm-ss")}.png";
            if (ImageSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ImageBoxControl.Image.Save(ImageSaveFileDialog.FileName);
            }
        }

        private void ImageForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.PageUp:
                case Keys.Up:
                    PreviousFile();
                    break;
                case Keys.Right:
                case Keys.PageDown:
                case Keys.Down:
                    NextFile();
                    break;
                case Keys.Space:
                case Keys.Enter:
                    if (e.Shift)
                    {
                        PreviousFile();
                    }
                    else
                    {
                        NextFile();
                    }
                    break;
                case Keys.Home:
                    FirstFile();
                    break;
                case Keys.End:
                    LastFile();
                    break;
                case Keys.R:
                    if (e.Shift)
                    {
                        RotateImage(-90);
                    }
                    else
                    {
                        RotateImage(90);
                    }
                    ImageBoxControl.ZoomToFit();
                    break;
                case Keys.G:
                    m_windowManager.OpenRangerWindow(m_imageDirectory);
                    break;
                case Keys.F11:
                    FullScreen(!m_isFullScreen);
                    break;
                case Keys.Escape:
                    if (m_isFullScreen)
                    {
                        FullScreen(false);
                    }
                    else
                    {
                        Close();
                    }
                    break;
            }
        }

        private void NextFile()
        {
            var files = ScanImageFiles(m_imageDirectory);
            int currentFileIndex = FindImageIndexInFiles(m_imageFileName, files);

            int nextIndex = (currentFileIndex == -1) ? 0 : currentFileIndex + 1;

            if (nextIndex < files.Count && files.Count > 0)
            {
                SetImage(m_imageDirectory, files[nextIndex], nextIndex, files.Count);
            }
        }

        private void PreviousFile()
        {
            var files = ScanImageFiles(m_imageDirectory);
            int currentFileIndex = FindImageIndexInFiles(m_imageFileName, files);

            int previousIndex = (currentFileIndex == -1) ? 0 : currentFileIndex - 1;

            if (previousIndex >= 0 && files.Count > 0)
            {
                SetImage(m_imageDirectory, files[previousIndex], previousIndex, files.Count);
            }
        }

        private void FirstFile()
        {
            var files = ScanImageFiles(m_imageDirectory);

            if (files.Count > 0)
            {
                SetImage(m_imageDirectory, files[0], 0, files.Count);
            }
        }
        private void LastFile()
        {
            var files = ScanImageFiles(m_imageDirectory);

            if (files.Count > 0)
            {
                SetImage(m_imageDirectory, files[files.Count - 1], files.Count - 1, files.Count);
            }
        }

        private List<string> ScanImageFiles(string directory)
        {
            List<string> files = new List<string>();

            foreach (string file in Directory.EnumerateFiles(directory))
            {
                var extension = Path.GetExtension(file).ToLower();

                if(m_supportedExtensions.Contains(extension))
                {
                    files.Add(file);
                }
            }

            return files;
        }

        private int FindImageIndexInFiles(string currentFile, List<string> files)
        {
            return files.FindIndex(x => x == currentFile);
        }

        private void RotateImage(int degrees)
        {
            if (!m_imageStates.TryGetValue(m_imageFileName, out var state))
            {
                state = new ImageState();
                m_imageStates.Add(m_imageFileName, state);
            }

            state.Rotation += degrees;

            while (state.Rotation >= 360)
            {
                state.Rotation -= 360;
            }

            if (degrees > 0)
            {
                ImageBoxControl.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            else
            {
                ImageBoxControl.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }

            ImageBoxControl.ZoomToFit();
        }

        private void LoadUserRotationState(string fileName, Image image)
        {
            if (m_imageStates.TryGetValue(fileName, out var state))
            {
                switch (state.Rotation)
                {
                    case 90:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 180:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 270:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }

                ImageBoxControl.ZoomToFit();
            }
        }

        private void FullScreen(bool fullScreen)
        {
            if (fullScreen)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                ImageBoxControl.ZoomToFit();
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }

            m_isFullScreen = fullScreen;
        }

    }
}
