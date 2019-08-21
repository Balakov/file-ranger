using System;
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
        private readonly ImageList m_smallImageList = new ImageList();
        private IconListManager m_iconListManager;

        private FilePane m_activePane = null;
        private readonly ManagementEventWatcher m_driveWatcher = new ManagementEventWatcher();

        private ViewFilter.ViewMask m_viewMask = 0;

        public static string DefaultEditorPath { get; private set; }

        private readonly Config m_config = new Config();
        private static RangerMainForm s_instance = null;

        private bool m_preventDirectoryChangeEventOnTreeNodeSelect = false;

        private enum BookmarkType
        {
            Directory,
            File
        }

        private enum BookmarkContainer
        {
            Toolbar,
            TreeView
        }

        public RangerMainForm()
        {
            s_instance = this;

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

            m_smallImageList.ImageSize = new System.Drawing.Size(16, 16);

            m_iconListManager = new IconListManager(m_smallImageList);

            DrivesTreeView.ImageList = m_smallImageList;
            LeftFilePane.ListView.SmallImageList = m_smallImageList;
            RightFilePane.ListView.SmallImageList = m_smallImageList;

            LeftFilePane.IconListManager = m_iconListManager;
            RightFilePane.IconListManager = m_iconListManager;

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

            if (m_config.GetValue("showdotfiles", false.ToString()) != false.ToString())
            {
                m_viewMask |= ViewFilter.ViewMask.ShowDot;
                showDotFilesToolStripMenuItem.Checked = true;
            }

            DefaultEditorPath = m_config.GetValue("defaulteditorpath");

            LeftFilePane.SetViewMask(m_viewMask);
            RightFilePane.SetViewMask(m_viewMask);

            // Pane paths

            string firstReadyDrivePath = BuildDrivesTreeView();

            // Bookmarks
            SetBookmarksFromConfig();

            string leftDefaultPath = m_config.GetValue("leftpath", firstReadyDrivePath);
            string rightDefaultPath = m_config.GetValue("rightpath", firstReadyDrivePath);

            LeftFilePane.OnFocusEvent += LeftFilePane_OnFocusEvent;
            RightFilePane.OnFocusEvent += RightFilePane_OnFocusEvent;

            LeftFilePane.OnPathChangedEvent += FilePane_OnPathChangedEvent;
            RightFilePane.OnPathChangedEvent += FilePane_OnPathChangedEvent;

            LeftFilePane.OnSyncronisationRequested += FilePane_OnSyncronisationRequested;
            RightFilePane.OnSyncronisationRequested += FilePane_OnSyncronisationRequested;

            LeftFilePane.OnSearchRequested += FilePane_OnSearchRequested;
            RightFilePane.OnSearchRequested += FilePane_OnSearchRequested;

            m_activePane = LeftFilePane;

            RightFilePane.SetDirectory(rightDefaultPath);
            LeftFilePane.SetDirectory(leftDefaultPath);

            // Drive monitor
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");
            m_driveWatcher.EventArrived += DriveWatcher_EventArrived;
            m_driveWatcher.Query = query;
            m_driveWatcher.Start();

            /*
            // Add the system recycle bin icon to the button
            int dummyIcon;
            var icon = Etier.IconHelper.IconReader.GetStockIcon(Etier.IconHelper.Shell32.SHSTOCKICONID.SIID_RECYCLER, out dummyIcon);
            RecycleBinButton.Image = icon.ToBitmap();
            */

            this.Closing += new System.ComponentModel.CancelEventHandler(this.MyOnClosing);
        }

        private void MyOnClosing(object o, System.ComponentModel.CancelEventArgs e)
        {
            m_driveWatcher.Stop();
            LeftFilePane.ShutdownFileWatcher();
            RightFilePane.ShutdownFileWatcher();

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
            m_config.SetValue("showdotfiles", m_viewMask.HasFlag(ViewFilter.ViewMask.ShowDot).ToString());

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
                    m_config.SetValue($"bookmark{i}", $"Toolbar|Directory|{tag.DisplayName}|{tag.Path}");
                    i++;
                }
                else if (item.Tag is BookmarkFileTag)
                {
                    var tag = item.Tag as BookmarkFileTag;
                    m_config.SetValue($"bookmark{i}", $"Toolbar|File|{tag.DisplayName}|{tag.Path}");
                    i++;
                }
            }

            foreach (TreeNode item in DrivesTreeView.Nodes[0].Nodes)
            {
                if (item.Tag is BookmarkDirectoryTag)
                {
                    var tag = item.Tag as BookmarkDirectoryTag;
                    m_config.SetValue($"bookmark{i}", $"Tree|Directory|{tag.DisplayName}|{tag.Path}");
                    i++;
                }
                else if (item.Tag is BookmarkFileTag)
                {
                    var tag = item.Tag as BookmarkFileTag;
                    m_config.SetValue($"bookmark{i}", $"Tree|File|{tag.DisplayName}|{tag.Path}");
                    i++;
                }
            }

            LeftFilePane.SaveToConfig(m_config, "left_");
            RightFilePane.SaveToConfig(m_config, "right_");

            m_config.Save();
        }

        private string BuildDrivesTreeView()
        {
            DrivesTreeView.BeginUpdate();

            // Store all the bookmark nodes
            var bookmarks = new List<TreeNode>();
            if (DrivesTreeView.Nodes.Count > 0)
            {
                foreach (TreeNode node in DrivesTreeView.Nodes[0].Nodes)
                {
                    if (node.Tag is BookmarkTag)
                    {
                        bookmarks.Add(node);
                    }
                }

                DrivesTreeView.Nodes.Clear();
            }

            TreeNode root = DrivesTreeView.Nodes.Add("Computer");
            root.Tag = new RootTag();
            root.ImageIndex = m_iconListManager.AddStockIcon(StockIconID.DesktopPC);

            string firstReadyDrivePath = null;

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    firstReadyDrivePath = firstReadyDrivePath ?? drive.RootDirectory.FullName;

                    AddDriveToTree(root, drive);
                }
            }

            // Add the recycle bin
            /*
            int recycleBinIconIndex = m_iconListManager.AddStockIcon(StockIconID.Recycler);

            root.Nodes.Add(new TreeNode($"Recycle Bin", recycleBinIconIndex, recycleBinIconIndex)
            {
                Tag = new RecycleBinTag()
            });
            */

            // Re-add the bookmarks
            foreach (var bookmark in bookmarks)
            {
                root.Nodes.Add(bookmark);
            }

            root.Expand();

            DrivesTreeView.EndUpdate();

            return firstReadyDrivePath;
        }

        private void AddDriveToTree(TreeNode root, DriveInfo drive)
        {
            int folderIconIndex = m_iconListManager.AddDriveIcon(drive.RootDirectory.FullName, false);

            TreeNode node = new TreeNode($"{drive.Name} ({drive.VolumeLabel})", folderIconIndex, folderIconIndex)
            {
                Tag = new LazyDirectoryTag(drive.RootDirectory.FullName)
            };

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

            foreach (var drive in DriveInfo.GetDrives())
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

        private void SetBookmarksFromConfig()
        {
            int i = 0;
            string bookmark;

            do
            {
                bookmark = m_config.GetValue($"bookmark{i}");
                if (bookmark != null)
                {
                    string[] bookmarkData = bookmark.Split('|');

                    if (bookmarkData.Length  == 4)
                    {
                        BookmarkContainer container = (bookmarkData[0] == "Tree") ? BookmarkContainer.TreeView : BookmarkContainer.Toolbar;
                        BookmarkType type = (bookmarkData[1] == "File") ? BookmarkType.File : BookmarkType.Directory;
                        string displayName = bookmarkData[2];
                        string path = bookmarkData[3];

                        AddBookmark(path, displayName, type, container);
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

        private void FilePane_OnSearchRequested(object sender, EventArgs e)
        {
            EverythingSearchForm form = new EverythingSearchForm(OnSearchResultActivated);
            form.Show();
        }

        private void OnSearchResultActivated(string path)
        {
            m_activePane.SetDirectory(Path.GetDirectoryName(path), path, FilePane.GrabFocusType.GrabFocus);
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
            PathTextBox.Text = m_activePane.CurrentPath;
            UpdateTitleBar();
        }

        private void RightFilePane_OnFocusEvent(object sender, EventArgs e)
        {
            LeftFilePane.DeactivatePane();
            RightFilePane.ActivatePane();
            m_activePane = RightFilePane;
            PathTextBox.Text = m_activePane.CurrentPath;
            UpdateTitleBar();
        }

        private void DrivesTreeView_MouseClick(object sender, MouseEventArgs e)
        {
            TreeViewHitTestInfo info = DrivesTreeView.HitTest(e.X, e.Y);
            if (info.Node != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // If the selection is different we'll call the event handler anyway. Only need 
                    // to force the event handler call if the node is the same as the currently selected one.
                    if (DrivesTreeView.SelectedNode == info.Node)
                    {
                        DrivesTreeView_AfterSelect(sender, new TreeViewEventArgs(info.Node));
                    }
                }
                else
                {
                    m_preventDirectoryChangeEventOnTreeNodeSelect = true;
                    DrivesTreeView.SelectedNode = info.Node;
                    m_preventDirectoryChangeEventOnTreeNodeSelect = false;
                }
            }
        }

        private void DrivesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!m_preventDirectoryChangeEventOnTreeNodeSelect)
            {
                string newDir = null;
                string fileToSelect = null;

                if (e.Node.Tag is DirectoryTag)
                {
                    newDir = (e.Node.Tag as DirectoryTag).Path;
                }
                else if (e.Node.Tag is LazyDirectoryTag)
                {
                    newDir = (e.Node.Tag as LazyDirectoryTag).Path;
                }
                else if (e.Node.Tag is BookmarkDirectoryTag)
                {
                    newDir = (e.Node.Tag as BookmarkDirectoryTag).Path;
                }
                else if (e.Node.Tag is BookmarkFileTag)
                {
                    string path = (e.Node.Tag as BookmarkFileTag).Path;
                    newDir = Path.GetDirectoryName(path);
                    fileToSelect = Path.GetFileName(path);
                }
                else if (e.Node.Tag is RecycleBinTag)
                {
                    ExploreRecycleBin();
                }

                if (newDir != null)
                {
                    m_activePane.SetDirectory(newDir, fileToSelect);
                }
            }
        }

        private void ExploreRecycleBin()
        {
            System.Diagnostics.Process.Start("explorer.exe", "shell:RecycleBinFolder");
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

                            string leafName = Path.GetFileName(dir);
                            Color itemColour;
                            if (ViewFilter.FilterViewByAttributes(di.Attributes, m_viewMask, leafName.StartsWith("."), out itemColour))
                            {
                                int folderIconIndex = m_iconListManager.AddFolderIcon(dir, false);

                                TreeNode node = new TreeNode(leafName, folderIconIndex, folderIconIndex)
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

        private void ChangeViewFilter_Click(object sender, EventArgs e)
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

            if (showDotFilesToolStripMenuItem.Checked)
            {
                m_viewMask |= ViewFilter.ViewMask.ShowDot;
            }

            LeftFilePane.SetViewMask(m_viewMask);
            RightFilePane.SetViewMask(m_viewMask);

            BuildDrivesTreeView();
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        public static void TriggerDefaultEditorSelect()
        {
            s_instance?.SetDefaultEditorToolStripMenuItem_Click(null, null);
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

        private void BookmarkContainer_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)) ||
                e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
        }

        private void BookmarkContainer_DragDrop(object sender, DragEventArgs e)
        {
            List<string> pathsToAdd = new List<string>();
            BookmarkContainer containerType = (sender == NavigationToolStrip) ? BookmarkContainer.Toolbar : BookmarkContainer.TreeView;

            if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)))
            {
                foreach (ListViewItem current in (ListView.SelectedListViewItemCollection)e.Data.GetData(typeof(ListView.SelectedListViewItemCollection)))
                {
                    if (current.Tag is PathTag)
                    {
                        pathsToAdd.Add((current.Tag as PathTag).Path);
                        break;  // Only add first
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (var file in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    pathsToAdd.Add(file);
                    break;  // Only add first
                }
            }

            foreach (string path in pathsToAdd)
            {
                if (Directory.Exists(path))
                {
                    AddBookmark(path, Path.GetFileName(path), BookmarkType.Directory, containerType);
                    break;  // Only add first
                }
                else if (File.Exists(path))
                {
                    AddBookmark(path, Path.GetFileName(path), BookmarkType.File, containerType);
                    break;  // Only add first
                }
            }
        }

        private void AddBookmark(string path, string displayName, BookmarkType type, BookmarkContainer container)
        {
            int imageIndex = 0;
            BookmarkTag tag = null;

            if (type == BookmarkType.Directory)
            {
                imageIndex = m_iconListManager.AddFolderIcon(path, true);
                tag = new BookmarkDirectoryTag(path);
            }
            else
            {
                imageIndex = m_iconListManager.AddFileIcon(path, true);
                tag = new BookmarkFileTag(path);
            }

            if (tag != null)
            {
                tag.DisplayName = displayName;

                if (container == BookmarkContainer.Toolbar)
                {
                    var item = new ToolStripButtonWithContextMenu()
                    {
                        Text = displayName,
                        Image = m_smallImageList.Images[imageIndex],
                        TextImageRelation = TextImageRelation.ImageAboveText,
                        Tag = tag,
                        ContextMenuStrip = BookmarksContextMenuStrip,
                        Font = BackToolStripButton.Font,
                        ToolTipText = (tag as BookmarkTag).Path
                    };

                    item.Click += Bookmark_Click;

                    NavigationToolStrip.Items.Add(item);
                }
                else if (container == BookmarkContainer.TreeView) 
                {
                    TreeNode item = new TreeNode(displayName);
                    item.ImageIndex = item.SelectedImageIndex = imageIndex;
                    item.Tag = tag;
                    item.ToolTipText = (tag as BookmarkTag).Path;

                    DrivesTreeView.Nodes[0].Nodes.Add(item);
                }
            }
        }

        private void Bookmark_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripItem)
            {
                ToolStripItem item = sender as ToolStripItem;

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

        private BookmarkTag BookmarkButtonFromContextClick(object sender, out object UIItem)
        {
            UIItem = null;

            // The menu item will be the child of a ContextMenu that has a tag of type ToolStripButtonWithContextMenu.
            if (sender is ToolStripItem)
            {
                ToolStripItem item = sender as ToolStripItem;

                if (item.Owner is ContextMenuStrip)
                {
                    ContextMenuStrip contextMenu = item.Owner as ContextMenuStrip;

                    if (contextMenu.Tag is ToolStripButtonWithContextMenu)
                    {
                        var button = contextMenu.Tag as ToolStripButtonWithContextMenu;

                        UIItem = button;
                        return button.Tag as BookmarkTag;
                    }
                    else if (contextMenu.SourceControl is TreeView)
                    {
                        TreeView tree = contextMenu.SourceControl as TreeView;
                        if (tree.SelectedNode.Tag is BookmarkTag)
                        {
                            UIItem = tree.SelectedNode;
                            return tree.SelectedNode.Tag as BookmarkTag;
                        }
                    }
                }
            }

            return null;
        }

        private void DeleteBookmark_Click(object sender, EventArgs e)
        {
            object UIItem;
            var bookmarkTag = BookmarkButtonFromContextClick(sender, out UIItem);
            if (bookmarkTag != null)
            {
                if (MessageBox.Show($"Are you sure you want to delete this bookmark?",
                                    "Delete Bookmark",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (UIItem is ToolStripButtonWithContextMenu)
                    {
                        NavigationToolStrip.Items.Remove(UIItem as ToolStripButtonWithContextMenu);
                    }
                    else if(UIItem is TreeNode)
                    {
                        // Remove from tree
                        DrivesTreeView.Nodes[0].Nodes.Remove(UIItem as TreeNode);
                    }
                }
            }
        }

        private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            object UIItem;
            var bookmarkTag = BookmarkButtonFromContextClick(sender, out UIItem);
            if (bookmarkTag != null)
            {
                var form = new RenameBookmarkForm(bookmarkTag.DisplayName);

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    bookmarkTag.DisplayName = form.DisplayName;

                    if (UIItem is ToolStripButtonWithContextMenu)
                    {
                        (UIItem as ToolStripButtonWithContextMenu).Text = form.DisplayName;
                    }
                    else if(UIItem is TreeNode)
                    {
                        (UIItem as TreeNode).Text = form.DisplayName;
                    }
                }
            }
        }

        private void AddCurrentDirectoryToToolstripToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string displayName = Path.GetFileName(m_activePane.CurrentPath.TrimEnd(new char[] { '\\' }));
            AddBookmark(m_activePane.CurrentPath, displayName, BookmarkType.Directory, BookmarkContainer.Toolbar);
        }

        private void AddCurrentDirectoryToTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string displayName = Path.GetFileName(m_activePane.CurrentPath.TrimEnd(new char[] { '\\' }));
            AddBookmark(m_activePane.CurrentPath, displayName, BookmarkType.Directory, BookmarkContainer.TreeView);
        }

        private void BookmarksContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(sender is ContextMenuStrip)
            {
                ContextMenuStrip contextMenu = sender as ContextMenuStrip;

                if (contextMenu.SourceControl is TreeView)
                {
                    var tree = (contextMenu.SourceControl as TreeView);
                    if (tree.SelectedNode?.Tag as BookmarkTag != null)
                    {
                        return;
                    }

                    e.Cancel = true;
                }
            }
        }

        private void PathTextBox_Enter(object sender, EventArgs e)
        {
            PathTextBox.SelectAll();
        }

        private void RecycleBinButton_Click(object sender, EventArgs e)
        {
            ExploreRecycleBin();
        }

    }
}
