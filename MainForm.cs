using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Ranger
{
    public interface IWindowManager
    {
        void OpenRangerWindow(string path);
        void OpenImageWindow(string path);
        void ClosingRangerWindow(Form form);
        void ClosingImageWindow(Form form);
    }

    public class MainForm : Form, IWindowManager
    {
        private readonly Config m_config = new Config();
        private Form m_openGalleryWindow = null;
        private List<Form> m_openImageWindows = new List<Form>();
        private ThumbnailCache m_thumbnailCache = new ThumbnailCache();
        private Timer m_startupTimer = new Timer();

        private readonly string[] m_supportedExtensions =
        {
            ".afdesign",
            ".bmp",
            ".dds",
            ".emf",
            //".eps",
            ".gif",
            ".ico",
            ".jpeg",
            ".jpg",
            ".jfif",
            ".png",
            ".pcx",
            //".pdf",
            ".psd",
            ".svg",
            ".tiff",
            ".tif",
            ".tga",
            //".wmf"
        };

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.Load += MainForm_Load;

            string configFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"YellowDroid\Gaze\settings.txt");
            m_config.Load(configFilename);

            m_startupTimer.Interval = 250;
            m_startupTimer.Tick += StartupimerTick;
            m_startupTimer.Start();

            this.Closing += new System.ComponentModel.CancelEventHandler(this.MyOnClosing);
        }

        private void StartupimerTick(object sender, EventArgs e)
        {
            m_startupTimer.Stop();

            string[] args = Environment.GetCommandLineArgs();
            
            if (args.Length > 1)
            {
                if (m_supportedExtensions.Contains(Path.GetExtension(args[1]).ToLower()))
                {
                    OpenImageWindow(args[1]);
                }
                else
                {
                    OpenRangerWindow(args[1]);
                }
            }
            else
            {
                OpenRangerWindow(null);
            }
        }

        private void MyOnClosing(object o, System.ComponentModel.CancelEventArgs e)
        {
            m_config.Save();
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            this.Size = new Size(0, 0);
            this.Opacity = 0;
        }

        private void CheckForClose()
        {
            if (m_openGalleryWindow == null && m_openImageWindows.Count == 0)
            {
                Close();
            }
        }

        public void OpenRangerWindow(string path)
        {
            if (m_openGalleryWindow != null)
            {
                m_openGalleryWindow.Activate();
            }
            else
            {
                var rangerForm = new RangerForm(path, m_config, m_supportedExtensions, m_thumbnailCache, this);
                m_openGalleryWindow = rangerForm;
                rangerForm.Show();
            }
        }

        public void OpenImageWindow(string path)
        {
            var imageForm = new ImageForm(path, m_config, m_supportedExtensions, this);
            m_openImageWindows.Add(imageForm);
            imageForm.Show();
        }

        public void ClosingRangerWindow(Form form)
        {
            m_openGalleryWindow = null;
            CheckForClose();

            // Copy the collection as it will be modified when the forms close
            var openWindowsListCopy = m_openImageWindows.ToList();

            foreach (Form imageForm in openWindowsListCopy)
            {
                imageForm.Close();
            }
        }

        public void ClosingImageWindow(Form form)
        {
            m_openImageWindows.Remove(form);
            CheckForClose();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "MainForm";
            this.ResumeLayout(false);

        }
    }
}
