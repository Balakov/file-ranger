namespace Ranger
{
    partial class RangerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RangerForm));
            this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.PathTextBox = new System.Windows.Forms.TextBox();
            this.NavigationToolStrip = new System.Windows.Forms.ToolStrip();
            this.BookmarksContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.organiseBookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BackToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ForwardToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ParentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.RecycleBinButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MyMainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setDefaultEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showHiddenFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSystemFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDotFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCurrentDirectoryToToolstripToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCurrentDirectoryToTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FilePaneSplitContainer = new System.Windows.Forms.SplitContainer();
            this.DrivesTreeView = new System.Windows.Forms.TreeView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.LeftFilePane = new Ranger.FilePane();
            this.RightFilePane = new Ranger.FilePane();
            this.SetEditorFileDialog = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
            this.MainSplitContainer.Panel1.SuspendLayout();
            this.MainSplitContainer.Panel2.SuspendLayout();
            this.MainSplitContainer.SuspendLayout();
            this.NavigationToolStrip.SuspendLayout();
            this.BookmarksContextMenuStrip.SuspendLayout();
            this.MyMainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FilePaneSplitContainer)).BeginInit();
            this.FilePaneSplitContainer.Panel1.SuspendLayout();
            this.FilePaneSplitContainer.Panel2.SuspendLayout();
            this.FilePaneSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainSplitContainer
            // 
            this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.MainSplitContainer.IsSplitterFixed = true;
            this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.MainSplitContainer.Name = "MainSplitContainer";
            this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // MainSplitContainer.Panel1
            // 
            this.MainSplitContainer.Panel1.Controls.Add(this.PathTextBox);
            this.MainSplitContainer.Panel1.Controls.Add(this.NavigationToolStrip);
            this.MainSplitContainer.Panel1.Controls.Add(this.MyMainMenuStrip);
            // 
            // MainSplitContainer.Panel2
            // 
            this.MainSplitContainer.Panel2.Controls.Add(this.FilePaneSplitContainer);
            this.MainSplitContainer.Size = new System.Drawing.Size(1249, 842);
            this.MainSplitContainer.SplitterDistance = 90;
            this.MainSplitContainer.TabIndex = 0;
            this.MainSplitContainer.TabStop = false;
            // 
            // PathTextBox
            // 
            this.PathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathTextBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PathTextBox.Location = new System.Drawing.Point(6, 24);
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.Size = new System.Drawing.Size(1238, 23);
            this.PathTextBox.TabIndex = 0;
            this.PathTextBox.Enter += new System.EventHandler(this.PathTextBox_Enter);
            this.PathTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PathTextBox_KeyDown);
            // 
            // NavigationToolStrip
            // 
            this.NavigationToolStrip.AllowDrop = true;
            this.NavigationToolStrip.ContextMenuStrip = this.BookmarksContextMenuStrip;
            this.NavigationToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.NavigationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BackToolStripButton,
            this.ForwardToolStripButton,
            this.toolStripSeparator1,
            this.ParentToolStripButton,
            this.toolStripSeparator2,
            this.RecycleBinButton,
            this.toolStripSeparator3});
            this.NavigationToolStrip.Location = new System.Drawing.Point(0, 52);
            this.NavigationToolStrip.Name = "NavigationToolStrip";
            this.NavigationToolStrip.Size = new System.Drawing.Size(1249, 38);
            this.NavigationToolStrip.Stretch = true;
            this.NavigationToolStrip.TabIndex = 4;
            this.NavigationToolStrip.Text = "toolStrip1";
            this.NavigationToolStrip.DragDrop += new System.Windows.Forms.DragEventHandler(this.BookmarkContainer_DragDrop);
            this.NavigationToolStrip.DragOver += new System.Windows.Forms.DragEventHandler(this.BookmarkContainer_DragOver);
            // 
            // BookmarksContextMenuStrip
            // 
            this.BookmarksContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.toolStripMenuItem2,
            this.organiseBookmarksToolStripMenuItem});
            this.BookmarksContextMenuStrip.Name = "BookmarksContextMenuStrip";
            this.BookmarksContextMenuStrip.Size = new System.Drawing.Size(193, 76);
            this.BookmarksContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.BookmarksContextMenuStrip_Opening);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.deleteToolStripMenuItem.Text = "Delete Bookmark...";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteBookmark_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.renameToolStripMenuItem.Text = "Edit Bookmark...";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.RenameToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(189, 6);
            // 
            // organiseBookmarksToolStripMenuItem
            // 
            this.organiseBookmarksToolStripMenuItem.Name = "organiseBookmarksToolStripMenuItem";
            this.organiseBookmarksToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.organiseBookmarksToolStripMenuItem.Text = "Organise Bookmarks...";
            this.organiseBookmarksToolStripMenuItem.Click += new System.EventHandler(this.OrganiseBookmarksToolStripMenuItem_Click);
            // 
            // BackToolStripButton
            // 
            this.BackToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("BackToolStripButton.Image")));
            this.BackToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BackToolStripButton.Name = "BackToolStripButton";
            this.BackToolStripButton.Size = new System.Drawing.Size(36, 35);
            this.BackToolStripButton.Text = "Back";
            this.BackToolStripButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.BackToolStripButton.Click += new System.EventHandler(this.BackToolStripButton_Click);
            // 
            // ForwardToolStripButton
            // 
            this.ForwardToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("ForwardToolStripButton.Image")));
            this.ForwardToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ForwardToolStripButton.Name = "ForwardToolStripButton";
            this.ForwardToolStripButton.Size = new System.Drawing.Size(54, 35);
            this.ForwardToolStripButton.Text = "Forward";
            this.ForwardToolStripButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ForwardToolStripButton.Click += new System.EventHandler(this.ForwardToolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // ParentToolStripButton
            // 
            this.ParentToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("ParentToolStripButton.Image")));
            this.ParentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ParentToolStripButton.Name = "ParentToolStripButton";
            this.ParentToolStripButton.Size = new System.Drawing.Size(45, 35);
            this.ParentToolStripButton.Text = "Parent";
            this.ParentToolStripButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ParentToolStripButton.Click += new System.EventHandler(this.ParentToolStripButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 38);
            // 
            // RecycleBinButton
            // 
            this.RecycleBinButton.Image = ((System.Drawing.Image)(resources.GetObject("RecycleBinButton.Image")));
            this.RecycleBinButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RecycleBinButton.Name = "RecycleBinButton";
            this.RecycleBinButton.Size = new System.Drawing.Size(71, 35);
            this.RecycleBinButton.Text = "Recycle Bin";
            this.RecycleBinButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.RecycleBinButton.Click += new System.EventHandler(this.RecycleBinButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 38);
            // 
            // MyMainMenuStrip
            // 
            this.MyMainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.bookmarksToolStripMenuItem});
            this.MyMainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MyMainMenuStrip.Name = "MyMainMenuStrip";
            this.MyMainMenuStrip.Size = new System.Drawing.Size(1249, 24);
            this.MyMainMenuStrip.TabIndex = 3;
            this.MyMainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setDefaultEditorToolStripMenuItem,
            this.toolStripMenuItem1,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // setDefaultEditorToolStripMenuItem
            // 
            this.setDefaultEditorToolStripMenuItem.Name = "setDefaultEditorToolStripMenuItem";
            this.setDefaultEditorToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.setDefaultEditorToolStripMenuItem.Text = "Set Default Editor...";
            this.setDefaultEditorToolStripMenuItem.Click += new System.EventHandler(this.SetDefaultEditorToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(171, 6);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.QuitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showHiddenFilesToolStripMenuItem,
            this.showSystemFilesToolStripMenuItem,
            this.showDotFilesToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // showHiddenFilesToolStripMenuItem
            // 
            this.showHiddenFilesToolStripMenuItem.CheckOnClick = true;
            this.showHiddenFilesToolStripMenuItem.Name = "showHiddenFilesToolStripMenuItem";
            this.showHiddenFilesToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.showHiddenFilesToolStripMenuItem.Text = "Show Hidden Files";
            this.showHiddenFilesToolStripMenuItem.Click += new System.EventHandler(this.ChangeViewFilter_Click);
            // 
            // showSystemFilesToolStripMenuItem
            // 
            this.showSystemFilesToolStripMenuItem.CheckOnClick = true;
            this.showSystemFilesToolStripMenuItem.Name = "showSystemFilesToolStripMenuItem";
            this.showSystemFilesToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.showSystemFilesToolStripMenuItem.Text = "Show System Files";
            this.showSystemFilesToolStripMenuItem.Click += new System.EventHandler(this.ChangeViewFilter_Click);
            // 
            // showDotFilesToolStripMenuItem
            // 
            this.showDotFilesToolStripMenuItem.CheckOnClick = true;
            this.showDotFilesToolStripMenuItem.Name = "showDotFilesToolStripMenuItem";
            this.showDotFilesToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.showDotFilesToolStripMenuItem.Text = "Show Dot Files";
            this.showDotFilesToolStripMenuItem.Click += new System.EventHandler(this.ChangeViewFilter_Click);
            // 
            // bookmarksToolStripMenuItem
            // 
            this.bookmarksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCurrentDirectoryToToolstripToolStripMenuItem,
            this.addCurrentDirectoryToTreeToolStripMenuItem});
            this.bookmarksToolStripMenuItem.Name = "bookmarksToolStripMenuItem";
            this.bookmarksToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.bookmarksToolStripMenuItem.Text = "Bookmarks";
            // 
            // addCurrentDirectoryToToolstripToolStripMenuItem
            // 
            this.addCurrentDirectoryToToolstripToolStripMenuItem.Name = "addCurrentDirectoryToToolstripToolStripMenuItem";
            this.addCurrentDirectoryToToolstripToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.addCurrentDirectoryToToolstripToolStripMenuItem.Text = "Add Current Directory to Toolstrip";
            this.addCurrentDirectoryToToolstripToolStripMenuItem.Click += new System.EventHandler(this.AddCurrentDirectoryToToolstripToolStripMenuItem_Click);
            // 
            // addCurrentDirectoryToTreeToolStripMenuItem
            // 
            this.addCurrentDirectoryToTreeToolStripMenuItem.Name = "addCurrentDirectoryToTreeToolStripMenuItem";
            this.addCurrentDirectoryToTreeToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.addCurrentDirectoryToTreeToolStripMenuItem.Text = "Add Current Directory to Tree";
            this.addCurrentDirectoryToTreeToolStripMenuItem.Click += new System.EventHandler(this.AddCurrentDirectoryToTreeToolStripMenuItem_Click);
            // 
            // FilePaneSplitContainer
            // 
            this.FilePaneSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FilePaneSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.FilePaneSplitContainer.Name = "FilePaneSplitContainer";
            // 
            // FilePaneSplitContainer.Panel1
            // 
            this.FilePaneSplitContainer.Panel1.Controls.Add(this.DrivesTreeView);
            // 
            // FilePaneSplitContainer.Panel2
            // 
            this.FilePaneSplitContainer.Panel2.Controls.Add(this.splitContainer3);
            this.FilePaneSplitContainer.Size = new System.Drawing.Size(1249, 748);
            this.FilePaneSplitContainer.SplitterDistance = 205;
            this.FilePaneSplitContainer.TabIndex = 0;
            this.FilePaneSplitContainer.TabStop = false;
            // 
            // DrivesTreeView
            // 
            this.DrivesTreeView.AllowDrop = true;
            this.DrivesTreeView.ContextMenuStrip = this.BookmarksContextMenuStrip;
            this.DrivesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrivesTreeView.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DrivesTreeView.HideSelection = false;
            this.DrivesTreeView.Location = new System.Drawing.Point(0, 0);
            this.DrivesTreeView.Name = "DrivesTreeView";
            this.DrivesTreeView.ShowNodeToolTips = true;
            this.DrivesTreeView.Size = new System.Drawing.Size(205, 748);
            this.DrivesTreeView.TabIndex = 0;
            this.DrivesTreeView.TabStop = false;
            this.DrivesTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.DrivesTreeView_BeforeExpand);
            this.DrivesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.DrivesTreeView_AfterSelect);
            this.DrivesTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.BookmarkContainer_DragDrop);
            this.DrivesTreeView.DragOver += new System.Windows.Forms.DragEventHandler(this.BookmarkContainer_DragOver);
            this.DrivesTreeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DrivesTreeView_MouseClick);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.LeftFilePane);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.RightFilePane);
            this.splitContainer3.Size = new System.Drawing.Size(1040, 748);
            this.splitContainer3.SplitterDistance = 520;
            this.splitContainer3.TabIndex = 0;
            this.splitContainer3.TabStop = false;
            // 
            // LeftFilePane
            // 
            this.LeftFilePane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftFilePane.Location = new System.Drawing.Point(0, 0);
            this.LeftFilePane.Name = "LeftFilePane";
            this.LeftFilePane.Size = new System.Drawing.Size(520, 748);
            this.LeftFilePane.TabIndex = 0;
            // 
            // RightFilePane
            // 
            this.RightFilePane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightFilePane.Location = new System.Drawing.Point(0, 0);
            this.RightFilePane.Name = "RightFilePane";
            this.RightFilePane.Size = new System.Drawing.Size(516, 748);
            this.RightFilePane.TabIndex = 0;
            // 
            // SetEditorFileDialog
            // 
            this.SetEditorFileDialog.FileName = "openFileDialog1";
            // 
            // RangerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1249, 842);
            this.Controls.Add(this.MainSplitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MyMainMenuStrip;
            this.Name = "RangerForm";
            this.Text = "File Ranger";
            this.Load += new System.EventHandler(this.RangerMainForm_Load);
            this.MainSplitContainer.Panel1.ResumeLayout(false);
            this.MainSplitContainer.Panel1.PerformLayout();
            this.MainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
            this.MainSplitContainer.ResumeLayout(false);
            this.NavigationToolStrip.ResumeLayout(false);
            this.NavigationToolStrip.PerformLayout();
            this.BookmarksContextMenuStrip.ResumeLayout(false);
            this.MyMainMenuStrip.ResumeLayout(false);
            this.MyMainMenuStrip.PerformLayout();
            this.FilePaneSplitContainer.Panel1.ResumeLayout(false);
            this.FilePaneSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FilePaneSplitContainer)).EndInit();
            this.FilePaneSplitContainer.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer MainSplitContainer;
        private System.Windows.Forms.SplitContainer FilePaneSplitContainer;
        private System.Windows.Forms.TreeView DrivesTreeView;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private FilePane LeftFilePane;
        private FilePane RightFilePane;
        private System.Windows.Forms.MenuStrip MyMainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showHiddenFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSystemFilesToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip BookmarksContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog SetEditorFileDialog;
        private System.Windows.Forms.ToolStripMenuItem setDefaultEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStrip NavigationToolStrip;
        private System.Windows.Forms.ToolStripButton BackToolStripButton;
        private System.Windows.Forms.ToolStripButton ForwardToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton ParentToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.TextBox PathTextBox;
        private System.Windows.Forms.ToolStripMenuItem bookmarksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCurrentDirectoryToToolstripToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCurrentDirectoryToTreeToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton RecycleBinButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem showDotFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem organiseBookmarksToolStripMenuItem;
    }
}

