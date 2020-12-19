namespace Ranger
{
    partial class ImageForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageForm));
            this.ViewOptionsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ViewPixelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewBilinearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewBicubicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.fitToScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetZoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.galleryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImageBoxControl = new Cyotek.Windows.Forms.ImageBox();
            this.ImageSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.ViewOptionsContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ViewOptionsContextMenuStrip
            // 
            this.ViewOptionsContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewPixelsToolStripMenuItem,
            this.ViewBilinearToolStripMenuItem,
            this.ViewBicubicToolStripMenuItem,
            this.toolStripMenuItem1,
            this.fitToScreenToolStripMenuItem,
            this.ResetZoomToolStripMenuItem,
            this.toolStripMenuItem3,
            this.galleryToolStripMenuItem,
            this.toolStripMenuItem4,
            this.saveAsToolStripMenuItem});
            this.ViewOptionsContextMenuStrip.Name = "ViewOptionsContextMenuStrip";
            this.ViewOptionsContextMenuStrip.ShowCheckMargin = true;
            this.ViewOptionsContextMenuStrip.Size = new System.Drawing.Size(162, 176);
            this.ViewOptionsContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ViewOptionsContextMenuStrip_Opening);
            // 
            // ViewPixelsToolStripMenuItem
            // 
            this.ViewPixelsToolStripMenuItem.Name = "ViewPixelsToolStripMenuItem";
            this.ViewPixelsToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.ViewPixelsToolStripMenuItem.Text = "Pixels";
            this.ViewPixelsToolStripMenuItem.Click += new System.EventHandler(this.ViewPixelsToolStripMenuItem_Click);
            // 
            // ViewBilinearToolStripMenuItem
            // 
            this.ViewBilinearToolStripMenuItem.Name = "ViewBilinearToolStripMenuItem";
            this.ViewBilinearToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.ViewBilinearToolStripMenuItem.Text = "Bilinear";
            this.ViewBilinearToolStripMenuItem.Click += new System.EventHandler(this.ViewBilinearToolStripMenuItem_Click);
            // 
            // ViewBicubicToolStripMenuItem
            // 
            this.ViewBicubicToolStripMenuItem.Name = "ViewBicubicToolStripMenuItem";
            this.ViewBicubicToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.ViewBicubicToolStripMenuItem.Text = "Bicubic";
            this.ViewBicubicToolStripMenuItem.Click += new System.EventHandler(this.ViewBicubicToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(158, 6);
            // 
            // fitToScreenToolStripMenuItem
            // 
            this.fitToScreenToolStripMenuItem.Name = "fitToScreenToolStripMenuItem";
            this.fitToScreenToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.fitToScreenToolStripMenuItem.Text = "Fit to Screen";
            this.fitToScreenToolStripMenuItem.Click += new System.EventHandler(this.fitToScreenToolStripMenuItem_Click);
            // 
            // ResetZoomToolStripMenuItem
            // 
            this.ResetZoomToolStripMenuItem.Name = "ResetZoomToolStripMenuItem";
            this.ResetZoomToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.ResetZoomToolStripMenuItem.Text = "Zoom 1:1";
            this.ResetZoomToolStripMenuItem.Click += new System.EventHandler(this.ResetZoomToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(158, 6);
            // 
            // galleryToolStripMenuItem
            // 
            this.galleryToolStripMenuItem.Name = "galleryToolStripMenuItem";
            this.galleryToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.galleryToolStripMenuItem.Text = "Gallery";
            this.galleryToolStripMenuItem.Click += new System.EventHandler(this.galleryToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(158, 6);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // ImageBoxControl
            // 
            this.ImageBoxControl.BackColor = System.Drawing.Color.DimGray;
            this.ImageBoxControl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ImageBoxControl.ContextMenuStrip = this.ViewOptionsContextMenuStrip;
            this.ImageBoxControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImageBoxControl.GridDisplayMode = Cyotek.Windows.Forms.ImageBoxGridDisplayMode.None;
            this.ImageBoxControl.Location = new System.Drawing.Point(0, 0);
            this.ImageBoxControl.Name = "ImageBoxControl";
            this.ImageBoxControl.Size = new System.Drawing.Size(1314, 940);
            this.ImageBoxControl.TabIndex = 0;
            this.ImageBoxControl.ZoomChanged += new System.EventHandler(this.ImageBoxControl_ZoomChanged);
            // 
            // ImageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1314, 940);
            this.Controls.Add(this.ImageBoxControl);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ImageForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ImageForm";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ImageForm_KeyUp);
            this.ViewOptionsContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Cyotek.Windows.Forms.ImageBox ImageBoxControl;
        private System.Windows.Forms.ContextMenuStrip ViewOptionsContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ViewPixelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewBilinearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewBicubicToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem ResetZoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem galleryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog ImageSaveFileDialog;
        private System.Windows.Forms.ToolStripMenuItem fitToScreenToolStripMenuItem;
    }
}