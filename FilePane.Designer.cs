namespace Ranger
{
    partial class FilePane
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.PathLinkLabel = new System.Windows.Forms.LinkLabel();
            this.StatusBar = new System.Windows.Forms.StatusStrip();
            this.DriveSpaceStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SelectionStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.FilePaneContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.nNwFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newShortcutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusBarRefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.FileListView = new Ranger.FlickerFreeListView();
            this.FileColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AttribColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ModifiedColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.StatusBar.SuspendLayout();
            this.FilePaneContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // PathLinkLabel
            // 
            this.PathLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PathLinkLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PathLinkLabel.Location = new System.Drawing.Point(3, 2);
            this.PathLinkLabel.Name = "PathLinkLabel";
            this.PathLinkLabel.Size = new System.Drawing.Size(616, 23);
            this.PathLinkLabel.TabIndex = 0;
            this.PathLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.PathLinkLabel_LinkClicked);
            // 
            // StatusBar
            // 
            this.StatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DriveSpaceStatusLabel,
            this.SelectionStatusLabel});
            this.StatusBar.Location = new System.Drawing.Point(0, 821);
            this.StatusBar.Name = "StatusBar";
            this.StatusBar.Size = new System.Drawing.Size(622, 22);
            this.StatusBar.SizingGrip = false;
            this.StatusBar.TabIndex = 2;
            this.StatusBar.Text = "statusStrip1";
            // 
            // DriveSpaceStatusLabel
            // 
            this.DriveSpaceStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.DriveSpaceStatusLabel.Name = "DriveSpaceStatusLabel";
            this.DriveSpaceStatusLabel.Size = new System.Drawing.Size(60, 17);
            this.DriveSpaceStatusLabel.Text = "FreeSpace";
            // 
            // SelectionStatusLabel
            // 
            this.SelectionStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SelectionStatusLabel.Name = "SelectionStatusLabel";
            this.SelectionStatusLabel.Size = new System.Drawing.Size(547, 17);
            this.SelectionStatusLabel.Spring = true;
            this.SelectionStatusLabel.Text = "Selection";
            this.SelectionStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FilePaneContextMenuStrip
            // 
            this.FilePaneContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.openWithToolStripMenuItem,
            this.EditToolStripMenuItem,
            this.toolStripMenuItem4,
            this.nNwFileToolStripMenuItem,
            this.NewFolderToolStripMenuItem,
            this.newShortcutToolStripMenuItem,
            this.toolStripMenuItem1,
            this.copyToolStripMenuItem,
            this.cutToolStripMenuItem,
            this.PasteToolStripMenuItem,
            this.toolStripMenuItem3,
            this.propertiesToolStripMenuItem});
            this.FilePaneContextMenuStrip.Name = "FilePaneContextMenuStrip";
            this.FilePaneContextMenuStrip.Size = new System.Drawing.Size(147, 242);
            this.FilePaneContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.FilePaneContextMenuStrip_Opening);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            this.openWithToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openWithToolStripMenuItem.Text = "Open with...";
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
            // 
            // EditToolStripMenuItem
            // 
            this.EditToolStripMenuItem.Name = "EditToolStripMenuItem";
            this.EditToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.EditToolStripMenuItem.Text = "Edit";
            this.EditToolStripMenuItem.Click += new System.EventHandler(this.EditToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(143, 6);
            // 
            // nNwFileToolStripMenuItem
            // 
            this.nNwFileToolStripMenuItem.Name = "nNwFileToolStripMenuItem";
            this.nNwFileToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.nNwFileToolStripMenuItem.Text = "New File";
            this.nNwFileToolStripMenuItem.Click += new System.EventHandler(this.NewFileToolStripMenuItem_Click);
            // 
            // NewFolderToolStripMenuItem
            // 
            this.NewFolderToolStripMenuItem.Name = "NewFolderToolStripMenuItem";
            this.NewFolderToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.NewFolderToolStripMenuItem.Text = "New Folder";
            this.NewFolderToolStripMenuItem.Click += new System.EventHandler(this.NewFolderToolStripMenuItem_Click);
            // 
            // newShortcutToolStripMenuItem
            // 
            this.newShortcutToolStripMenuItem.Name = "newShortcutToolStripMenuItem";
            this.newShortcutToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.newShortcutToolStripMenuItem.Text = "New Shortcut";
            this.newShortcutToolStripMenuItem.Click += new System.EventHandler(this.NewShortcutToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(143, 6);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItem_Click);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.CutToolStripMenuItem_Click);
            // 
            // PasteToolStripMenuItem
            // 
            this.PasteToolStripMenuItem.Name = "PasteToolStripMenuItem";
            this.PasteToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.PasteToolStripMenuItem.Text = "Paste";
            this.PasteToolStripMenuItem.Click += new System.EventHandler(this.PasteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(143, 6);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.propertiesToolStripMenuItem.Text = "Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.PropertiesToolStripMenuItem_Click);
            // 
            // StatusBarRefreshTimer
            // 
            this.StatusBarRefreshTimer.Interval = 250;
            this.StatusBarRefreshTimer.Tick += new System.EventHandler(this.StatusBarRefreshTimer_Tick);
            // 
            // FileListView
            // 
            this.FileListView.AllowDrop = true;
            this.FileListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FileColumnHeader,
            this.SizeColumnHeader,
            this.AttribColumnHeader,
            this.ModifiedColumnHeader});
            this.FileListView.ContextMenuStrip = this.FilePaneContextMenuStrip;
            this.FileListView.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileListView.FullRowSelect = true;
            this.FileListView.LabelEdit = true;
            this.FileListView.Location = new System.Drawing.Point(3, 23);
            this.FileListView.Name = "FileListView";
            this.FileListView.Size = new System.Drawing.Size(616, 795);
            this.FileListView.TabIndex = 0;
            this.FileListView.UseCompatibleStateImageBehavior = false;
            this.FileListView.View = System.Windows.Forms.View.Details;
            this.FileListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.FileListView_AfterLabelEdit);
            this.FileListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.FileListView_ColumnClick);
            this.FileListView.ItemActivate += new System.EventHandler(this.FileListView_ItemActivate);
            this.FileListView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.FileListView_ItemDrag);
            this.FileListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.FileListView_ItemSelectionChanged);
            this.FileListView.DragDrop += new System.Windows.Forms.DragEventHandler(this.FileListView_DragDrop);
            this.FileListView.DragOver += new System.Windows.Forms.DragEventHandler(this.FileListView_DragOver);
            this.FileListView.Enter += new System.EventHandler(this.FileListView_Enter);
            this.FileListView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FileListView_KeyUp);
            // 
            // FileColumnHeader
            // 
            this.FileColumnHeader.Text = "File";
            this.FileColumnHeader.Width = 300;
            // 
            // SizeColumnHeader
            // 
            this.SizeColumnHeader.Text = "Size";
            this.SizeColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SizeColumnHeader.Width = 99;
            // 
            // AttribColumnHeader
            // 
            this.AttribColumnHeader.Text = "Attrib";
            // 
            // ModifiedColumnHeader
            // 
            this.ModifiedColumnHeader.Text = "Modified";
            this.ModifiedColumnHeader.Width = 126;
            // 
            // FilePane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.StatusBar);
            this.Controls.Add(this.FileListView);
            this.Controls.Add(this.PathLinkLabel);
            this.Name = "FilePane";
            this.Size = new System.Drawing.Size(622, 843);
            this.StatusBar.ResumeLayout(false);
            this.StatusBar.PerformLayout();
            this.FilePaneContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel PathLinkLabel;
        private FlickerFreeListView FileListView;
        private System.Windows.Forms.ColumnHeader FileColumnHeader;
        private System.Windows.Forms.ColumnHeader SizeColumnHeader;
        private System.Windows.Forms.ColumnHeader AttribColumnHeader;
        private System.Windows.Forms.ColumnHeader ModifiedColumnHeader;
        private System.Windows.Forms.StatusStrip StatusBar;
        private System.Windows.Forms.ToolStripStatusLabel DriveSpaceStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel SelectionStatusLabel;
        private System.Windows.Forms.ContextMenuStrip FilePaneContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem nNwFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem NewFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem PasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.Timer StatusBarRefreshTimer;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newShortcutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
    }
}
