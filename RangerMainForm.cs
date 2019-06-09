﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Management;
using System.Collections.Generic;

namespace Ranger
{
    public partial class RangerMainForm : Form
    {
        private ImageList m_smallImageList = new ImageList();
        private ImageList m_largeImageList = new ImageList();
        private Etier.IconHelper.IconListManager m_iconListManager;

        private FilePane m_activePane = null;
        ManagementEventWatcher m_driveWatcher = new ManagementEventWatcher();

        private ViewFilter.ViewMask m_viewMask = 0;

        public static string DefaultEditorPath { get; private set; }

        private Config m_config = new Config();

        private enum BookmarkType
        {
            Directory,
            File
        }

        public RangerMainForm()
        {
            InitializeComponent();
        }

        private void RangerMainForm_Load(object sender, EventArgs e)
        {
            string configFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"YellowDroid\Ranger\settings.txt");
            m_config.Load(configFilename);

            LayoutFromConfig();

            LeftFilePane.LoadFromConfig(m_config, "left_");
            RightFilePane.LoadFromConfig(m_config, "right_");

            m_smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            m_largeImageList.ColorDepth = ColorDepth.Depth32Bit;

            m_smallImageList.ImageSize = new System.Drawing.Size(16, 16);
            m_largeImageList.ImageSize = new System.Drawing.Size(32, 32);

            m_iconListManager = new Etier.IconHelper.IconListManager(m_smallImageList, m_largeImageList);

            DrivesTreeView.ImageList = m_smallImageList;
            LeftFilePane.ListView.SmallImageList = m_smallImageList;
            LeftFilePane.ListView.LargeImageList = m_largeImageList;
            RightFilePane.ListView.SmallImageList = m_smallImageList;
            RightFilePane.ListView.LargeImageList = m_largeImageList;

            LeftFilePane.IconListManager = m_iconListManager;
            RightFilePane.IconListManager = m_iconListManager;

            // Bookmarks
            SetBookmarksFromConfig();

            // View Mask
            if (m_config.GetValue("showhiddenfiles", false.ToString()) != false.ToString())
            {
                m_viewMask |= ViewFilter.ViewMask.ShowHidden;
                showHiddenFilesToolStripMenuItem.Checked = true;
            }

            if (m_config.GetValue("showsystemfiles", false.ToString()) != false.ToString())
            {
                m_viewMask |= ViewFilter.ViewMask.ShowSystem;
                showSystemFilesToolStripMenuItem.Checked = true;
            }

            DefaultEditorPath = m_config.GetValue("defaulteditorpath");

            LeftFilePane.SetViewMask(m_viewMask);
            RightFilePane.SetViewMask(m_viewMask);

            // Pane paths

            string firstReadyDrivePath = BuildDrivesTreeView();

            string leftDefaultPath = m_config.GetValue("leftpath", firstReadyDrivePath);
            string rightDefaultPath = m_config.GetValue("rightpath", firstReadyDrivePath);

            LeftFilePane.OnFocusEvent += LeftFilePane_OnFocusEvent;
            RightFilePane.OnFocusEvent += RightFilePane_OnFocusEvent;

            LeftFilePane.OnPathChangedEvent += FilePane_OnPathChangedEvent;
            RightFilePane.OnPathChangedEvent += FilePane_OnPathChangedEvent;

            LeftFilePane.OnSyncronisationRequested += FilePane_OnSyncronisationRequested;
            RightFilePane.OnSyncronisationRequested += FilePane_OnSyncronisationRequested;

            m_activePane = LeftFilePane;

            RightFilePane.SetDirectory(rightDefaultPath);
            LeftFilePane.SetDirectory(leftDefaultPath);

            // Drive monitor
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");
            m_driveWatcher.EventArrived += DriveWatcher_EventArrived;
            m_driveWatcher.Query = query;
            m_driveWatcher.Start();

            this.Closing += new System.ComponentModel.CancelEventHandler(this.MyOnClosing);
        }

        private string BuildDrivesTreeView()
        {
            DrivesTreeView.BeginUpdate();

            DrivesTreeView.Nodes.Clear();

            TreeNode root = DrivesTreeView.Nodes.Add("Computer");
            root.Tag = new RootTag();

            string firstReadyDrivePath = null;

            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    firstReadyDrivePath = firstReadyDrivePath ?? drive.RootDirectory.FullName;

                    AddDriveToTree(root, drive);
                }
            }

            root.Expand();

            DrivesTreeView.EndUpdate();

            return firstReadyDrivePath;
        }

        private void AddDriveToTree(TreeNode root, DriveInfo drive)
        {
            int folderIconIndex = m_iconListManager.AddDriveIcon(drive.RootDirectory.FullName);

            TreeNode node = new TreeNode($"{drive.Name} ({drive.VolumeLabel})", folderIconIndex, folderIconIndex);
            node.Tag = new LazyDirectoryTag(drive.RootDirectory.FullName);
            node.Nodes.Add("Dummy");// Enable the "Expand" icon

            root.Nodes.Add(node);
        }

        private void UpdateDrivesTreeView()
        {
            // Refresh if files have been deleted or added.
            Dictionary<string, TreeNode> drivesInView = new Dictionary<string, TreeNode>();
            List<DriveInfo> drivesToAdd = new List<DriveInfo>();

            // Build up a list of items in the view
            // Then run through all of the drives nulling out the items that we find
            // Anything left in the list of view items must have been deleted
            foreach (TreeNode node in DrivesTreeView.Nodes[0].Nodes)
            {
                if (node.Tag is LazyDirectoryTag)
                {
                    drivesInView.Add((node.Tag as LazyDirectoryTag).Path.ToLower(), node);
                }
                else if (node.Tag is DirectoryTag)
                {
                    drivesInView.Add((node.Tag as DirectoryTag).Path.ToLower(), node);
                }
            }

            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    string key = drive.RootDirectory.FullName.ToLower();

                    if (!drivesInView.ContainsKey(key))
                    {
                        // New directory to add
                        drivesToAdd.Add(drive);
                    }
                    else
                    {
                        // Mark as still present on disk by setting the ListViewItem to null
                        drivesInView[key] = null;
                    }
                }
            }

            // Find all of the items that have been deleted
            List<TreeNode> itemsToDelete = drivesInView.Where(x => x.Value != null).Select(x => x.Value).ToList();

            DrivesTreeView.BeginUpdate();
            DrivesTreeView.SuspendLayout();

            foreach (TreeNode item in itemsToDelete)
            {
                item.Remove();
            }

            foreach (var drive in drivesToAdd)
            {
                AddDriveToTree(DrivesTreeView.Nodes[0], drive);
            }

            DrivesTreeView.EndUpdate();
            DrivesTreeView.ResumeLayout();
        }

        private void DriveWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { DriveWatcher_EventArrived(sender, e); });
            }
            else
            {
                UpdateDrivesTreeView();
            }
        }

        private void MyOnClosing(object o, System.ComponentModel.CancelEventArgs e)
        {
            m_config.Clear();

            if (this.WindowState != FormWindowState.Minimized)
            {
                m_config.SetValue("maximised", (this.WindowState == FormWindowState.Minimized).ToString());
                m_config.SetValue("xpos", this.Location.X.ToString());
                m_config.SetValue("ypos", this.Location.Y.ToString());
                m_config.SetValue("width", this.Size.Width.ToString());
                m_config.SetValue("height", this.Size.Height.ToString());
            }

            m_config.SetValue("treeviewsplitterdistance", FilePaneSplitContainer.SplitterDistance.ToString());

            m_config.SetValue("leftpath", LeftFilePane.CurrentPath);
            m_config.SetValue("rightpath", RightFilePane.CurrentPath);

            m_config.SetValue("showhiddenfiles", m_viewMask.HasFlag(ViewFilter.ViewMask.ShowHidden).ToString());
            m_config.SetValue("showsystemfiles", m_viewMask.HasFlag(ViewFilter.ViewMask.ShowSystem).ToString());

            if (DefaultEditorPath != null)
            {
                m_config.SetValue("defaulteditorpath", DefaultEditorPath);
            }

            int i = 0;
            foreach (ToolStripItem item in NavigationToolStrip.Items)
            {
                if (item.Tag is BookmarkDirectoryTag)
                {
                    var tag = item.Tag as BookmarkDirectoryTag;
                    m_config.SetValue($"bookmark{i}", $"D{tag.DisplayName}|{tag.Path}");
                    i++;
                }
                else if(item.Tag is BookmarkFileTag)
                {
                    var tag = item.Tag as BookmarkFileTag;
                    m_config.SetValue($"bookmark{i}", $"F{tag.DisplayName}|{tag.Path}");
                    i++;
                }
            }

            LeftFilePane.SaveToConfig(m_config, "left_");
            RightFilePane.SaveToConfig(m_config, "right_");

            m_config.Save();
        }

        private void SetBookmarksFromConfig()
        {
            int i = 0;
            string bookmark;

            do
            {
                bookmark = m_config.GetValue($"bookmark{i}");
                if (bookmark != null)
                {
                    string pathData = bookmark.Substring(1);
                    int separatorIndex = pathData.IndexOf("|");
                    if (separatorIndex != -1)
                    {
                        string displayName = pathData.Substring(0, separatorIndex);
                        string path = pathData.Substring(separatorIndex + 1);

                        if (bookmark.StartsWith("D"))
                        {
                            AddBookmark(path, displayName, BookmarkType.Directory);
                        }
                        if (bookmark.StartsWith("F"))
                        {
                            AddBookmark(path, displayName, BookmarkType.File);
                        }
                    }
                }

                i++;
            } while (bookmark != null);
        }

        private void LayoutFromConfig()
        {
            int xPos = Convert.ToInt32(m_config.GetValue("xpos", this.Location.X.ToString()));
            int yPos = Convert.ToInt32(m_config.GetValue("ypos", this.Location.Y.ToString()));
            int width = Convert.ToInt32(m_config.GetValue("width", this.Size.Width.ToString()));
            int height = Convert.ToInt32(m_config.GetValue("height", this.Size.Height.ToString()));
            this.Size = new System.Drawing.Size(width, height);
            this.Location = new Point(xPos, yPos);

            bool isMaximised = Convert.ToBoolean(m_config.GetValue("maximised", "false"));
            if (isMaximised)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            FilePaneSplitContainer.SplitterDistance = Convert.ToInt32(m_config.GetValue("treeviewsplitterdistance", FilePaneSplitContainer.SplitterDistance.ToString()));
        }

        private void UpdateTitleBar()
        {
            this.Text = $"File Ranger - {m_activePane.CurrentPath}";
        }

        private void FilePane_OnSyncronisationRequested(object sender, EventArgs e)
        {
            if (m_activePane == LeftFilePane)
            {
                RightFilePane.SetDirectory(m_activePane.CurrentPath, null, FilePane.GrabFocusType.NoFocusChange);
            }
            else
            {
                LeftFilePane.SetDirectory(m_activePane.CurrentPath, null, FilePane.GrabFocusType.NoFocusChange);
            }
        }

        private void FilePane_OnPathChangedEvent(object sender, EventArgs e)
        {
            PathTextBox.Text = m_activePane.CurrentPath;
            UpdateTitleBar();
        }

        private void LeftFilePane_OnFocusEvent(object sender, EventArgs e)
        {
            LeftFilePane.ActivatePane();
            RightFilePane.DeactivatePane();
            m_activePane = LeftFilePane;
            UpdateTitleBar();
        }

        private void RightFilePane_OnFocusEvent(object sender, EventArgs e)
        {
            LeftFilePane.DeactivatePane();
            RightFilePane.ActivatePane();
            m_activePane = RightFilePane;
            UpdateTitleBar();
        }

        private void DrivesTreeView_MouseClick(object sender, MouseEventArgs e)
        {
            TreeViewHitTestInfo info = DrivesTreeView.HitTest(e.X, e.Y);
            if (info.Node != null)
            {
                // If the selection is different we'll call the event handler anyway. Only need 
                // to force the event handler call if the node is the same as the currently selected one.
                if (DrivesTreeView.SelectedNode == info.Node)
                {
                    DrivesTreeView_AfterSelect(sender, new TreeViewEventArgs(info.Node));
                }
            }
        }

        private void DrivesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string newDir = null;
            if(e.Node.Tag is DirectoryTag)
            {
                newDir = (e.Node.Tag as DirectoryTag).Path;
            }
            else if (e.Node.Tag is LazyDirectoryTag)
            {
                newDir = (e.Node.Tag as LazyDirectoryTag).Path;
            }

            if (newDir != null)
            {
                m_activePane.SetDirectory(newDir);
            }
        }

        private void PathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                m_activePane.SetDirectory(PathTextBox.Text);
            }
        }

        private void DrivesTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Tag is LazyDirectoryTag)
            {
                string newDir = (e.Node.Tag as LazyDirectoryTag).Path;

                e.Node.Nodes.Clear();   // Remove the dummy
                e.Node.Tag = new DirectoryTag(newDir);  // Convert to a full directory from a lazy

                // Evaulate all of the directories in the new path
                try
                {
                    foreach (string dir in Directory.GetDirectories(newDir).OrderBy(x => x))
                    {
                        try
                        {
                            DirectoryInfo di = new DirectoryInfo(dir);

                            Color itemColour;
                            if (ViewFilter.FilterViewByAttributes(di.Attributes, m_viewMask, out itemColour))
                            {
                                int folderIconIndex = m_iconListManager.AddFolderIcon(dir);

                                TreeNode node = new TreeNode(Path.GetFileName(dir), folderIconIndex, folderIconIndex)
                                {
                                    Tag = new LazyDirectoryTag(dir),
                                    ForeColor = itemColour
                                };

                                node.Nodes.Add("Dummy");
                                e.Node.Nodes.Add(node);
                            }
                        }
                        catch (Exception ex)
                        {
                            e.Node.Nodes.Add(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    e.Node.Nodes.Add(ex.Message);
                }
            }
        }

        private void ShowHiddenFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_viewMask = 0;

            if (showSystemFilesToolStripMenuItem.Checked)
            {
                m_viewMask |= ViewFilter.ViewMask.ShowSystem;
            }

            if (showHiddenFilesToolStripMenuItem.Checked)
            {
                m_viewMask |= ViewFilter.ViewMask.ShowHidden;
            }

            LeftFilePane.SetViewMask(m_viewMask);
            RightFilePane.SetViewMask(m_viewMask);

            BuildDrivesTreeView();
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SetDefaultEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetEditorFileDialog.Title = "Set Default Editor";
            SetEditorFileDialog.FileName = DefaultEditorPath;
            SetEditorFileDialog.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";
            SetEditorFileDialog.Multiselect = false;

            if (SetEditorFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                DefaultEditorPath = SetEditorFileDialog.FileName;
            }
        }

        private void BackToolStripButton_Click(object sender, EventArgs e)
        {
            m_activePane.Back();
        }

        private void ForwardToolStripButton_Click(object sender, EventArgs e)
        {
            m_activePane.Forward();
        }

        private void ParentToolStripButton_Click(object sender, EventArgs e)
        {
            m_activePane.ParentDirectory();
        }

        private void NavigationToolStrip_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)))
            {
                e.Effect = e.AllowedEffect;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = e.AllowedEffect;
            }
        }

        private void NavigationToolStrip_DragDrop(object sender, DragEventArgs e)
        {
            List<string> pathsToAdd = new List<string>();

            if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)))
            {
                foreach (ListViewItem current in (ListView.SelectedListViewItemCollection)e.Data.GetData(typeof(ListView.SelectedListViewItemCollection)))
                {
                    if (current.Tag is PathTag)
                    {
                        pathsToAdd.Add((current.Tag as PathTag).Path);
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (var file in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    pathsToAdd.Add(file);
                }
            }

            foreach (string path in pathsToAdd)
            {
                if (Directory.Exists(path))
                {
                    AddBookmark(path, Path.GetFileName(path), BookmarkType.Directory);
                }
                else if (File.Exists(path))
                {
                    AddBookmark(path, Path.GetFileName(path), BookmarkType.File);
                }
            }
        }

        private void AddBookmark(string path, string displayName, BookmarkType type)
        {
            int imageIndex = 0;
            BookmarkTag tag = null;

            if (type == BookmarkType.Directory)
            {
                imageIndex = m_iconListManager.AddFolderIcon(path);
                tag = new BookmarkDirectoryTag(path);
            }
            else
            {
                imageIndex = m_iconListManager.AddFileIcon(path);
                tag = new BookmarkFileTag(path);
            }

            if (tag != null)
            {
                tag.DisplayName = displayName;

                var item = new ToolStripButtonWithContextMenu()
                {
                    Text = displayName,
                    Image = m_smallImageList.Images[imageIndex],
                    TextImageRelation = TextImageRelation.ImageAboveText,
                    Tag = tag,
                    ContextMenuStrip = BookmarksContextMenuStrip,
                    Font = BackToolStripButton.Font
                };

                item.Click += Bookmark_Click;

                NavigationToolStrip.Items.Add(item);
            }
        }

        private void Bookmark_Click(object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;
            if (item != null)
            {
                if (item.Tag is BookmarkFileTag)
                {
                    string file = (item.Tag as BookmarkFileTag).Path;
                    string directory = Path.GetDirectoryName(file);

                    m_activePane.SetDirectory(directory, file);
                }
                else if (item.Tag is BookmarkDirectoryTag)
                {
                    m_activePane.SetDirectory((item.Tag as BookmarkDirectoryTag).Path);
                }
            }
        }

        private ToolStripButtonWithContextMenu BookmarkButtonFromContextClick(object sender)
        {
            // The menu item will be the child of a ContextMenu that has a tag of type ToolStripButtonWithContextMenu.
            ToolStripItem item = sender as ToolStripItem;
            if (item != null)
            {
                ContextMenuStrip contextMenu = item.Owner as ContextMenuStrip;
                if (contextMenu != null)
                {
                    if (contextMenu.Tag is ToolStripButtonWithContextMenu)
                    {
                        return contextMenu.Tag as ToolStripButtonWithContextMenu;
                    }
                }
            }

            return null;
        }

        private void DeleteBookmark_Click(object sender, EventArgs e)
        {
            var bookmarkButton = BookmarkButtonFromContextClick(sender);
            if (bookmarkButton != null)
            {
                if (MessageBox.Show($"Are you sure you want to delete this bookmark?",
                                    "Delete Bookmark",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    NavigationToolStrip.Items.Remove(bookmarkButton);
                }
            }
        }

        private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bookmarkButton = BookmarkButtonFromContextClick(sender);
            if (bookmarkButton != null)
            {
                var bookmarkTag = (bookmarkButton.Tag as BookmarkTag);
                var form = new RenameBookmarkForm(bookmarkTag.DisplayName);

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    (bookmarkButton.Tag as BookmarkTag).DisplayName = form.DisplayName;
                    bookmarkButton.Text = form.DisplayName;
                }
            }
        }
    }
}