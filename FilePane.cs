using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Ranger
{
    public partial class FilePane : UserControl, ThumbnailCreator.IAddThumbnail
    {
        public enum SortStyle
        {
            Name,
            DateAscending,
            DateDescending,
            SizeAscending,
            SizeDescending
        }

        public enum HistoryUpdateType
        {
            UpdateHistory,
            NoHistoryUpdate
        }

        public enum GrabFocusType
        {
            GrabFocus,
            NoFocusChange
        }

        public enum DirectoryListType
        {
            Normal,
            NetworkShares
        }

        public enum ParentDotDisplay
        {
            Enabled,
            Disabled
        }

        private SortStyle m_currentSortStyle = SortStyle.Name;

        private ViewFilter.ViewMask m_viewMask = 0;

        private IWindowManager m_windowManager { get;  set; }
        private string[] m_supportedImageExtensions { get; set; } = new string[0];

        public string CurrentPath { get; private set; }
        private ulong m_currentDriveFreeSpace = 0;

        public event EventHandler OnFocusEvent;
        public event EventHandler OnPathChangedEvent;
        public event EventHandler OnSyncronisationRequested;
        public event EventHandler OnSearchRequested;

        private readonly FileSystemWatcher m_fileWatcher = new FileSystemWatcher();

        private IconListManager m_iconListManager;

        private readonly char[] m_invalidFilenmameChars = Path.GetInvalidFileNameChars();

        private string m_pendingPathToEdit = null;
        private string m_pendingPathToSelect = null;

        private readonly PathHistory m_history = new PathHistory();

        private ThumbnailCreator m_thumbnailCreator;
        private ThumbnailCache m_thumbnailCache;

        private const int c_thumbnailWidth = 180;
        private const int c_thumbnailHeight = 100;

        public FilePane()
        {
            InitializeComponent();

            m_fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size;
            m_fileWatcher.Filter = "*";
            m_fileWatcher.Changed += OnFilesChanged;
            m_fileWatcher.Created += OnFilesChanged;
            m_fileWatcher.Renamed += OnFilesChanged;
            m_fileWatcher.Deleted += OnFilesChanged;
        }

        public void Initialise(ImageList smallImageList, 
                               IconListManager iconListManager, 
                               string[] supportedImageExtensions, 
                               IWindowManager windowManager,
                               ThumbnailCache thumbnailCache)
        {
            FileListView.SmallImageList = smallImageList;
            m_iconListManager = iconListManager;
            m_supportedImageExtensions = supportedImageExtensions;
            m_windowManager = windowManager;
            m_thumbnailCache = thumbnailCache;
        }

        public void ShutdownFileWatcher()
        {
            m_fileWatcher.EnableRaisingEvents = false;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_fileWatcher.Changed -= OnFilesChanged;
                m_fileWatcher.Created -= OnFilesChanged;
                m_fileWatcher.Renamed -= OnFilesChanged;
                m_fileWatcher.Deleted -= OnFilesChanged;
                m_fileWatcher.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public void OnFilesChanged(object source, FileSystemEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { OnFilesChanged(source, e); });
            }
            else
            {
                UpdateCurrentDirectoryView();
            }
        }

        public void UpdateCurrentDirectoryView()
        {
            // Refresh if files have been deleted or added.
            Dictionary<string, ListViewItem> filesInView = new Dictionary<string, ListViewItem>();
            Dictionary<string, ListViewItem> directoriesInView = new Dictionary<string, ListViewItem>();

            List<string> filesToAdd = new List<string>();
            List<string> directoriesToAdd = new List<string>();

            // Build up a list of items in the view
            // Then run through all of the files on disk nulling out the items that we find
            // Anything left in the list of view item must have been deleted
            foreach (ListViewItem lvi in FileListView.Items)
            {
                if (lvi.Tag is FileTag)
                {
                    filesInView.Add(Path.GetFileName((lvi.Tag as FileTag).Path).ToLower(), lvi);
                }
                else if (lvi.Tag is DirectoryTag && !(lvi.Tag is ParentDirectoryTag)) // Don't delete the ".."
                {
                    directoriesInView.Add(Path.GetFileName((lvi.Tag as DirectoryTag).Path).ToLower(), lvi);
                }
            }

            try
            {
                foreach (string subdirectory in Directory.EnumerateDirectories(CurrentPath, "*", SearchOption.TopDirectoryOnly))
                {
                    string key = Path.GetFileName(subdirectory).ToLower();

                    if (!directoriesInView.ContainsKey(key))
                    {
                        // New directory to add
                        directoriesToAdd.Add(subdirectory);
                    }
                    else
                    {
                        // Mark as still present on disk by setting the ListViewItem to null
                        directoriesInView[key] = null;
                    }
                }
            }
            catch (Exception)
            {
            }

            try
            {
                foreach (string file in Directory.EnumerateFiles(CurrentPath, "*", SearchOption.TopDirectoryOnly))
                {
                    string key = Path.GetFileName(file).ToLower();

                    if (!filesInView.ContainsKey(key))
                    {
                        filesToAdd.Add(file);
                    }
                    else
                    {
                        // Mark as still present on disk by setting the ListViewItem to null
                        filesInView[key] = null;
                    }
                }
            }
            catch (Exception)
            {
            }

            // Find all of the items that have been deleted
            List<ListViewItem> itemsToDelete = filesInView.Where(x => x.Value != null).Select(x => x.Value).ToList();
            itemsToDelete.AddRange(directoriesInView.Where(x => x.Value != null).Select(x => x.Value).ToList());

            FileListView.BeginUpdate();
            FileListView.SuspendLayout();

            foreach (ListViewItem item in itemsToDelete)
            {
                FileListView.Items.Remove(item);
            }

            string pathToSelect = null;
            if (m_pendingPathToSelect != null)
            {
                pathToSelect = m_pendingPathToSelect;
                m_pendingPathToSelect = null;

                // If we had a previous selection we need to clear it as we want to select a new item.
                foreach (ListViewItem item in FileListView.SelectedItems)
                {
                    item.Selected = false;
                }
            }

            ListViewItem dummy;
            AddPathsToView(filesToAdd, directoriesToAdd, pathToSelect, out dummy, ParentDotDisplay.Disabled, DirectoryListType.Normal);

            FileListView.EndUpdate();
            FileListView.ResumeLayout();

            UpdateStatusBar(true);
        }

        private void AddPathsToView(IEnumerable<string> files, 
                                    IEnumerable<string> subdirectories, 
                                    string pathToSelect, 
                                    out ListViewItem firstSelected, 
                                    ParentDotDisplay parentDots,
                                    DirectoryListType directoryListType)
        {
            firstSelected = null;

            try
            {
                ListViewItem pendingItemToEdit = null;

                if (m_thumbnailCreator != null)
                {
                    m_thumbnailCreator.Abort();
                }

                var oldImageList = FileListView.LargeImageList;
                FileListView.LargeImageList = null;

                if (oldImageList != null)
                {
                    oldImageList.Dispose();
                }

                // Directories
                {
                    List<DirectoryInfo> directoryInfos = new List<DirectoryInfo>();

                    foreach (string subdirectory in subdirectories)
                    {
                        try
                        {
                            DirectoryInfo di = new DirectoryInfo(subdirectory);
                            directoryInfos.Add(di);
                        }
                        catch
                        {
                            // We may not be able to create info objects for private network shares 
                        }
                    }

                    switch (m_currentSortStyle)
                    {
                        case SortStyle.Name:
                        case SortStyle.SizeAscending:
                        case SortStyle.SizeDescending:
                            directoryInfos.Sort((a, b) => { return a.FullName.CompareTo(b.FullName); });
                            break;
                        case SortStyle.DateAscending:
                            directoryInfos.Sort((a, b) => { return a.LastWriteTime.CompareTo(b.LastWriteTime); });
                            break;
                        case SortStyle.DateDescending:
                            directoryInfos.Sort((a, b) => { return b.LastWriteTime.CompareTo(a.LastWriteTime); });
                            break;
                    }

                    /*
                    if (parentDots == ParentDotDisplay.Enabled)
                    {
                        int folderIconIndex = IconListManager.AddFolderIcon(CurrentPath, false);
                        string[] parts = CurrentPath.TrimEnd(new char[] { Path.DirectorySeparatorChar }).Split(Path.DirectorySeparatorChar);
                        string parentPath = string.Join(Path.DirectorySeparatorChar.ToString(), parts, 0, parts.Length - 1);

                        FileListView.Items.Add(new ListViewItem(new string[] { "..", string.Empty, string.Empty, string.Empty }, folderIconIndex)
                        {
                            Tag = new ParentDirectoryTag(parentPath)
                        });
                    }
                    */

                    // Only create file icon if we're not viewing thumbnails
                    Dictionary<string, int> iconIndexes = null;

                    if (FileListView.View != View.LargeIcon)
                    {
                        iconIndexes = m_iconListManager.BulkAddFolderIcons(directoryInfos);
                    }

                    foreach (var di in directoryInfos)
                    {
                        FileAttributes directoryAttribs = 0;
                        string dateString = "";
                        if (directoryListType == DirectoryListType.Normal)
                        {
                            try
                            {
                                directoryAttribs = di.Attributes;
                                dateString = di.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss");
                            }
                            catch
                            {
                            }
                        }

                        string leafName = Path.GetFileName(di.FullName);
                        Color itemColor;
                        if (ViewFilter.FilterViewByAttributes(directoryAttribs, m_viewMask, leafName.StartsWith("."), out itemColor))
                        {
                            string sizeString = "<folder>";
                            string attribsString = AttribsToString(directoryAttribs);
                            //bool isShortcut = Path.GetExtension(di.FullName).ToLower() == ".lnk";
                            //int folderIconIndex = IconListManager.AddFolderIcon(di.FullName, isShortcut);
                            int folderIconIndex = (iconIndexes != null) ? iconIndexes[di.FullName] : -1;

                            var lvi = new ListViewItem(new string[] { leafName, sizeString, attribsString, dateString }, folderIconIndex)
                            {
                                Tag = new DirectoryTag(di.FullName),
                                ForeColor = itemColor
                            };

                            if (di.FullName == pathToSelect)
                            {
                                lvi.Selected = true;
                                firstSelected = firstSelected ?? lvi;
                            }

                            if (di.FullName == m_pendingPathToEdit)
                            {
                                pendingItemToEdit = lvi;
                            }

                            FileListView.Items.Add(lvi);
                        }
                    }
                }

                // Files
                {
                    List<FileInfo> fileInfos = new List<FileInfo>();

                    foreach (string file in files)
                    {
                        try
                        {
                            fileInfos.Add(new FileInfo(file));
                        }
                        catch
                        {
                            // Ignore files we don't have permission to read
                        }
                    }

                    switch (m_currentSortStyle)
                    {
                        case SortStyle.Name:
                            fileInfos.Sort((a, b) => { return a.FullName.CompareTo(b.FullName); });
                            break;
                        case SortStyle.DateAscending:
                            fileInfos.Sort((a, b) => { return a.LastWriteTime.CompareTo(b.LastWriteTime); });
                            break;
                        case SortStyle.DateDescending:
                            fileInfos.Sort((a, b) => { return b.LastWriteTime.CompareTo(a.LastWriteTime); });
                            break;
                        case SortStyle.SizeAscending:
                            fileInfos.Sort((a, b) => { return a.Length.CompareTo(b.Length); });
                            break;
                        case SortStyle.SizeDescending:
                            fileInfos.Sort((a, b) => { return b.Length.CompareTo(a.Length); });
                            break;
                    }

                    // Only create file icon if we're not viewing thumbnails
                    Dictionary<string, int> iconIndexes = null;

                    if (FileListView.View != View.LargeIcon)
                    {
                        iconIndexes = m_iconListManager.BulkAddFileIcons(fileInfos);
                    }

                    foreach (var fi in fileInfos)
                    {
                        FileAttributes fileAttribs = 0;
                        string sizeString = "";
                        string dateString = "";
                        try
                        {
                            fileAttribs = fi.Attributes;
                            sizeString = string.Format("{0:N0}", fi.Length);
                            dateString = fi.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss");
                        }
                        catch
                        {
                        }

                        string leafName = Path.GetFileName(fi.FullName);
                        Color itemColour;
                        if (ViewFilter.FilterViewByAttributes(fileAttribs, m_viewMask, leafName.StartsWith("."), out itemColour))
                        {
                            string attribsString = AttribsToString(fileAttribs);
                            //bool isShortcut = Path.GetExtension(fi.FullName).ToLower() == ".lnk";
                            //int fileIconIndex = IconListManager.AddFolderIcon(di.FullName, isShortcut);
                            int fileIconIndex = (iconIndexes != null) ? iconIndexes[fi.FullName] : -1;

                            var lvi = new ListViewItem(new string[] { leafName, sizeString, attribsString, dateString }, fileIconIndex)
                            {
                                Tag = new FileTag(fi),
                                ForeColor = itemColour
                            };

                            if (fi.FullName == pathToSelect)
                            {
                                lvi.Selected = true;
                                firstSelected = firstSelected ?? lvi;
                            }

                            if (fi.FullName == m_pendingPathToEdit)
                            {
                                pendingItemToEdit = lvi;
                            }

                            FileListView.Items.Add(lvi);
                        }
                    }
                }

                // Thumbnail view
                if (FileListView.View == View.LargeIcon)
                {
                    var imageList = new ImageList();
                    imageList.ImageSize = new Size(c_thumbnailWidth, c_thumbnailHeight);
                    imageList.ColorDepth = ColorDepth.Depth32Bit;

                    FileListView.LargeImageList = imageList;

                    // When we're doing an update rather than a full scan we'll mess up the thumbnail image list if
                    // we kick off thumbnail creation on only the newly added files. Re calculate the whole image list
                    // It should be quick as all of the previous items will still be in the cache.
                    var allFiles = new List<string>();
                    foreach (ListViewItem lvi in FileListView.Items)
                    {
                        lvi.ImageIndex = -1;

                        if (lvi.Tag is FileTag)
                        {
                            allFiles.Add((lvi.Tag as FileTag).Path);
                        }
                        else
                        {
                            // Null will translate into a folder icon in the thumbnail creator.
                            allFiles.Add(null);
                        }
                    }

                    m_thumbnailCreator = new ThumbnailCreator(allFiles, this, c_thumbnailWidth, c_thumbnailHeight, imageList, m_thumbnailCache);
                }
                else
                {
                    m_thumbnailCreator = null;
                }

                if (pendingItemToEdit != null)
                {
                    pendingItemToEdit.BeginEdit();
                }

                m_pendingPathToEdit = null;

                if (firstSelected != null)
                {
                    firstSelected.EnsureVisible();
                    firstSelected.Focused = true;
                }
            }
            catch (Exception e)
            {
                var lvi = new ListViewItem(e.Message, 0);
                FileListView.Items.Add(lvi);
            }
        }

        public void Back()
        {
            SetDirectory(m_history.Back(), null, GrabFocusType.GrabFocus, HistoryUpdateType.NoHistoryUpdate);
        }

        public void Forward()
        {
            SetDirectory(m_history.Forward(), null, GrabFocusType.GrabFocus, HistoryUpdateType.NoHistoryUpdate);
        }

        public bool SetDirectory(string directory, string pathToSelect = null, GrabFocusType grabFocus = GrabFocusType.GrabFocus, HistoryUpdateType historyUpdate = HistoryUpdateType.UpdateHistory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return false;
            }

            if (File.Exists(directory))
            {
                pathToSelect = directory;
                directory = Path.GetDirectoryName(directory);
            }

            bool isRootUNCPath = false;
            bool isRootUNCOrShare = false;
            if (!directory.StartsWith(@"\\"))
            {
                if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                    !directory.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                {
                    directory += Path.DirectorySeparatorChar;
                }
            }
            else
            {
                // If this is the first part of a UNC path it's not a real directory, it's a share - we need to skip some checks
                string[] UNCParts = directory.Split('\\');
                if (UNCParts.Length <= 4)
                {
                    isRootUNCOrShare = true;

                    if (UNCParts.Length == 3)
                    {
                        isRootUNCPath = true;
                    }
                }
            }

            if (!isRootUNCOrShare)
            {
                try
                {
                    // Fix any relative paths
                    directory = Path.GetFullPath(directory);
                }
                catch (Exception)
                {
                    OnPathChangedEvent?.Invoke(this, new EventArgs());
                    return false;
                }

                if (!Directory.Exists(directory))
                {
                    // Fire the event to change the path so the UI refreshes the text back to the correct one.
                    OnPathChangedEvent?.Invoke(this, new EventArgs());
                    return false;
                }
            }

            FileListView.BeginUpdate();
            FileListView.SuspendLayout();
            FileListView.Items.Clear();

            if (historyUpdate == HistoryUpdateType.UpdateHistory)
            {
                m_history.SetPreviouslySelectedDirectoryForPath(CurrentPath, directory);
                m_history.PushPath(directory);
            }

            if (pathToSelect == null)
            {
                pathToSelect = m_history.GetPreviouslySelectedDirectoryForPath(directory);
            }

            CurrentPath = directory;

            ListViewItem firstSelected = null;

            try
            {
                if (isRootUNCPath)
                {
                    var networkShares = Trinet.Networking.ShareCollection.GetShares(CurrentPath);
                    List<string> sharePaths = new List<string>();
                    foreach (Trinet.Networking.Share networkShare in networkShares)
                    {
                        if (!networkShare.NetName.EndsWith("$") && (networkShare.ShareType.HasFlag(Trinet.Networking.ShareType.Device) ||
                                                                    networkShare.ShareType.HasFlag(Trinet.Networking.ShareType.Disk)))
                        {
                            // Don't use ToString() as the TriNet code seems to add a random number of leading slashes.
                            sharePaths.Add(networkShare.Server + '\\' + networkShare.NetName);
                        }
                    }

                    AddPathsToView(new string[0], sharePaths, pathToSelect, out firstSelected, ParentDotDisplay.Disabled, DirectoryListType.NetworkShares);
                }
                else
                {
                    IEnumerable<string> directories = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly);
                    IEnumerable<string> files = Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly);
                    bool isRoot = directory.Length < 4;

                    AddPathsToView(files, directories, pathToSelect, out firstSelected, ParentDotDisplay.Disabled, DirectoryListType.Normal);
                    //AddPathsToView(files, directories, pathToSelect, out firstSelected, isRoot ? ParentDotDisplay.Disabled : ParentDotDisplay.Enabled, DirectoryListType.Normal);
                }

                FileListView.Select();
            }
            catch(Exception e)
            {
                var lvi = new ListViewItem(e.Message, 0);
                FileListView.Items.Add(lvi);
            }

            FileListView.EndUpdate();
            FileListView.ResumeLayout();

            // Fix up the multi-part link label
            BuildBreadcrumbs(directory);

            try
            {
                m_fileWatcher.Path = CurrentPath;
                m_fileWatcher.EnableRaisingEvents = true;
            }
            catch
            {
            }

            OnPathChangedEvent?.Invoke(this, new EventArgs());

            if (grabFocus == GrabFocusType.GrabFocus)
            {
                FileListView.Select();

                if (firstSelected != null)
                {
                    firstSelected.EnsureVisible();
                    firstSelected.Focused = true;
                }
            }

            UpdateStatusBar(true);

            return true;
        }

        private void BuildBreadcrumbs(string directory)
        {
            string[] parts;
            if (directory.StartsWith(@"\\"))
            {
                parts = directory.Substring(2).Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                parts[0] = @"\\" + parts[0];
            }
            else
            {
                parts = directory.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            }

            int ignoreIndex = 0;
            Size stringSize;
            int maxWidth = PathLinkLabel.Width - 30;

            do
            {
                PathLinkLabel.Links.Clear();

                string fullPath = parts[0];
                string fullLinkPath = parts[0];
                PathLinkLabel.Links.Add(0, fullPath.Length, fullLinkPath);

                for (int i = 1; i < parts.Length; i++)
                {
                    int currentStartIndex = fullPath.Length + 1;    // +1 for '\' we're about to add.
                    fullLinkPath += Path.DirectorySeparatorChar + parts[i];

                    if (i > ignoreIndex)
                    {
                        fullPath += Path.DirectorySeparatorChar + parts[i];
                        PathLinkLabel.Links.Add(currentStartIndex, parts[i].Length, fullLinkPath);
                    }
                    else
                    {
                        fullPath += Path.DirectorySeparatorChar + "...";
                    }
                }

                stringSize = TextRenderer.MeasureText(fullPath, PathLinkLabel.Font);
                if (stringSize.Width > maxWidth)
                {
                    if (ignoreIndex < parts.Length - 2) // Keep the first and last parts and rely on the LinkLabel to add ellipsis at the end.
                    {
                        ignoreIndex++;
                    }
                    else
                    {
                        // We can't ignore any more, we'll just have to go with what we have
                        PathLinkLabel.Text = fullPath;
                        stringSize.Width = int.MinValue;
                    }
                }
                else
                {
                    PathLinkLabel.Text = fullPath;
                }

            } while (stringSize.Width > maxWidth);

            PathLinkLabel.TabStop = false;
        }

        private void UpdateStatusBar(bool updateFreeDriveSpace)
        {
            if (updateFreeDriveSpace)
            {
                m_currentDriveFreeSpace = FileOperations.GetDriveFreeBytes(CurrentPath);
            }

            DriveSpaceStatusLabel.Text = string.Format("Free Space: {0:N0} bytes", m_currentDriveFreeSpace);

            int fileCount = 0;
            int directoryCount = 0;
            long totalSelectedFileSize = 0;
            foreach (ListViewItem lvi in FileListView.SelectedItems)
            {
                if (lvi.Tag is FileTag)
                {
                    fileCount++;
                    totalSelectedFileSize += (lvi.Tag as FileTag).FileInfo?.Length ?? 0;
                }
                else if (lvi.Tag is DirectoryTag)
                {
                    directoryCount++;
                }
            }

            string fileText = string.Empty;
            if (fileCount > 0)
            {
                string filePluralText = fileCount == 1 ? "file" : "files";
                string sumText = fileCount == 1 ? "of" : "totalling";
                string bytesText = totalSelectedFileSize == 1 ? "byte" : "bytes";

                fileText = $"{fileCount} {filePluralText} {sumText} {totalSelectedFileSize:N0} {bytesText}";
            }

            string folderText = string.Empty;
            if (directoryCount > 0)
            {
                folderText = directoryCount == 1 ? "1 folder" : $"{directoryCount} folders";
            }

            SelectionStatusLabel.Text = (fileCount > 0 || directoryCount > 0) ? string.Format($"Selected: {folderText} {fileText}") : "";
        }

        private static string AttribsToString(FileAttributes attribs)
        {
            StringBuilder sb = new StringBuilder(3);

            if(attribs.HasFlag(FileAttributes.ReadOnly)) sb.Append('R');
            if(attribs.HasFlag(FileAttributes.Hidden)) sb.Append('H');
            if(attribs.HasFlag(FileAttributes.System)) sb.Append('S');

            return sb.ToString();
        }

        private void FileListView_ItemActivate(object sender, EventArgs e)
        {
            if (FileListView.SelectedItems.Count == 1)
            {
                var selectedItem = FileListView.SelectedItems[0];

                if (selectedItem.Tag is DirectoryTag)
                {
                    string newDirectory = (selectedItem.Tag as DirectoryTag).Path;
                    SetDirectory(newDirectory);
                }
                else if(selectedItem.Tag is FileTag)
                {
                    string path = (selectedItem.Tag as FileTag).Path;

                    // If this is an image file then use the internal viewer
                    if (m_supportedImageExtensions.Contains(Path.GetExtension(path).ToLower()))
                    {
                        m_windowManager?.OpenImageWindow(path);
                    }
                    else
                    {
                        FileOperations.ExecuteFile(path);
                    }
                }
            }
        }

        private void FileListView_Enter(object sender, EventArgs e)
        {
            OnFocusEvent?.Invoke(this, e);
        }

        public void ActivatePane()
        {
            FileListView.BackColor = SystemColors.Window;
        }

        public void DeactivatePane()
        {
            FileListView.BackColor = SystemColors.Control;
        }

        public void SetViewMask(ViewFilter.ViewMask newMask)
        {
            m_viewMask = newMask;
            SetDirectory(CurrentPath);
        }

        private void PathLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SetDirectory(e.Link.LinkData as string);
        }

        private void FileListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var dataObject = ClipboardManager.PathsToDataObject(SelectedItemsToPaths(), FileOperations.OperationType.Copy, ClipboardManager.ClipboardDataTypes.Data | 
                                                                                                                           ClipboardManager.ClipboardDataTypes.Files);
            if (dataObject != null)
            {
                dataObject.SetData(FileListView.SelectedItems);
                FileListView.DoDragDrop(dataObject, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
            }
        }

        private void FileListView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)) ||
                e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                DroppedFiles droppedData = PathsFromDragDropSource(e.Data, e.Effect);
                if (droppedData.Files != null &&
                    droppedData.Files.Count() > 0)
                {
                    string currentPathRoot = Path.GetPathRoot(CurrentPath);

                    // If all files are on the same drive, or ALT is pressed, move the files.
                    bool wouldMove = droppedData.Files.All((x) =>
                    {
                        return Path.GetPathRoot(x) == currentPathRoot;
                    });

                    // If Alt pressed, invert the operation
                    if ((e.KeyState & 32) == 32)
                    {
                        e.Effect = wouldMove ? DragDropEffects.Copy : DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effect = wouldMove ? DragDropEffects.Move : DragDropEffects.Copy;
                    }
                }
            }
            else if (e.Data.GetDataPresent("FileGroupDescriptor"))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private class DroppedFiles
        {
            public IEnumerable<string> Files;
            public FileOperations.OperationType FileOp;
        }

        private DroppedFiles PathsFromDragDropSource(IDataObject data, DragDropEffects effect)
        {
            if (data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)))
            {
                List<string> paths = new List<string>();
                foreach (ListViewItem current in (ListView.SelectedListViewItemCollection)data.GetData(typeof(ListView.SelectedListViewItemCollection)))
                {
                    if (current.Tag is PathTag)
                    {
                        paths.Add((current.Tag as PathTag).Path);
                    }
                }

                return new DroppedFiles()
                {
                    Files = paths,
                    FileOp = (effect == DragDropEffects.Move) ? FileOperations.OperationType.Move : FileOperations.OperationType.Copy
                };
            }
            else if (data.GetDataPresent(DataFormats.FileDrop))
            {
                return new DroppedFiles()
                {
                    Files = (string[])data.GetData(DataFormats.FileDrop),
                    FileOp = (effect == DragDropEffects.Move) ? FileOperations.OperationType.Move : FileOperations.OperationType.Copy
                };
            }
            else if (data.GetDataPresent("FileGroupDescriptor"))
            {
                // https://stackoverflow.com/questions/33019385/c-sharp-drag-and-drop-attached-file-from-outlook-email
                //
                // First step here is to get the filename of the attachment and build a full-path name so we can store it
                // in the temporary folder

                MemoryStream memoryStream = (MemoryStream)data.GetData("FileGroupDescriptor");
                byte[] fileGroupDescriptor = new byte[memoryStream.Length];
                memoryStream.Read(fileGroupDescriptor, 0, (int)memoryStream.Length);
                // used to build the filename from the FileGroupDescriptor block
                StringBuilder fileName = new StringBuilder();
                // this trick gets the filename of the passed attached file
                for (int i=76; fileGroupDescriptor[i] != 0; i++)
                {
                    fileName.Append(Convert.ToChar(fileGroupDescriptor[i]));
                }
                memoryStream.Close();

                // Second step:  we have the file name, now we need to get the actual raw data for the attached file and copy it to disk so we work on it.
                MemoryStream dataMemoryStream = (MemoryStream)data.GetData("FileContents", true);
                byte[] fileBytes = new byte[dataMemoryStream.Length];
                dataMemoryStream.Read(fileBytes, 0, (int)dataMemoryStream.Length);

                // Create a file and save the raw data to it
                string filePath = Path.Combine(Path.GetTempPath(), fileName.ToString());
                File.WriteAllBytes(filePath, fileBytes);

                FileInfo tempFile = new FileInfo(filePath);

                if (tempFile.Exists)
                {
                    return new DroppedFiles()
                    {
                        Files = new string[] { filePath },
                        FileOp = FileOperations.OperationType.Move  // Force move so we delete the temp file
                    };
                }
            }

            return new DroppedFiles();
        }

        private void FileListView_DragDrop(object sender, DragEventArgs e)
        {
            DroppedFiles droppedData = PathsFromDragDropSource(e.Data, e.Effect);
            if (droppedData.Files != null &&
                droppedData.Files.Count() > 0)
            {
                int fileCount = droppedData.Files.Count();

                var hitInfo = FileListView.HitTest(FileListView.PointToClient(new Point(e.X, e.Y)));
                    
                // If we dropped onto a column that wasn't the filename, assume we want to copy to the current directory
                if (hitInfo.Item != null)
                {
                    if (hitInfo.SubItem == hitInfo.Item.SubItems[0] || FileListView.View == View.LargeIcon)
                    {
                        if (hitInfo.Item.Tag is FileTag)
                        {
                            if (fileCount == 1)
                            {
                                // Dropped onto a file - execute item with dropped files as parameters.
                                // Abort if the dropped file is the same as the file it was dropped on
                                string targetPath = (hitInfo.Item.Tag as FileTag).Path;

                                List<string> args = new List<string>();
                                foreach (string arg in droppedData.Files)
                                {
                                    if (arg == targetPath)
                                        return;

                                    args.Add("\"" + arg + "\"");
                                }

                                FileOperations.ExecuteFile(targetPath, string.Join(" ", args));
                            }
                            else
                            {
                                // Multiple files dropped onto a file - assume we actually want to copy to that directory
                                OnDropOrPaste(droppedData.Files, CurrentPath, droppedData.FileOp, FileOperations.PasteOverSelfType.NotAllowed);
                            }
                        }
                        else if (hitInfo.Item.Tag is DirectoryTag)
                        {
                            // Dropped onto a directory - copy files into directory
                            string destinationPath = (hitInfo.Item.Tag as DirectoryTag).Path;

                            OnDropOrPaste(droppedData.Files, destinationPath, droppedData.FileOp, FileOperations.PasteOverSelfType.NotAllowed);
                        }
                    }
                }
                else
                {
                    // Dropped onto blank space - make sure we're not dropping into the same directory
                    OnDropOrPaste(droppedData.Files, CurrentPath, droppedData.FileOp, FileOperations.PasteOverSelfType.NotAllowed);
                }
            }
        }

        private static void OnDropOrPaste(IEnumerable<string> paths, string destinationDirectory, FileOperations.OperationType fileOp, FileOperations.PasteOverSelfType pasteOverSelf)
        {
            if (paths.Count() == 0)
            {
                return;
            }

            // Find the common root
            string[] firstPathSplit = null;
            string commonRoot;

            int lowestMatchIndex = int.MaxValue;
            foreach (string path in paths)
            {
                string[] pathSplit = path.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                if (firstPathSplit == null)
                {
                    // Assume all paths have a common root, so just pick the first path as the base to compare the others against
                    firstPathSplit = pathSplit;
                    lowestMatchIndex = firstPathSplit.Length - 1;
                }
                else
                {
                    // If this path matches fewer parts than the previous one, make it the new common root.
                    int currentMatchIndex = 0;
                    for (int i=0; i < pathSplit.Length && i < firstPathSplit.Length; i++)
                    {
                        if (pathSplit[i] == firstPathSplit[i])
                        {
                            currentMatchIndex++;
                        }
                    }

                    if (currentMatchIndex < lowestMatchIndex)
                    {
                        lowestMatchIndex = currentMatchIndex;
                    }
                }
            }

            if (lowestMatchIndex > 0)
            {
                commonRoot = string.Join(Path.DirectorySeparatorChar.ToString(), firstPathSplit, 0, lowestMatchIndex) + Path.DirectorySeparatorChar;
            }
            else
            {
                MessageBox.Show("The list of files to copy must all be on the same drive.", "Paste Files", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<string> copyFrom = new List<string>();
            List<string> copyTo = new List<string>();

            foreach (string path in paths)
            {
                // Remove the common root
                string relativeDestination = path.Substring(commonRoot.Length);

                string destinationPath = Path.Combine(destinationDirectory, relativeDestination);

                if (path == destinationPath)
                {
                    if (pasteOverSelf == FileOperations.PasteOverSelfType.Allowed)
                    {
                        // Duplicate the file or directory
                        bool isDirectory = Directory.Exists(path);
                        bool isFile = File.Exists(path);

                        if (isDirectory || isFile)
                        {
                            for (int i = 1; i < 10; i++)
                            {
                                string newDestinationPath = Path.Combine(Path.GetDirectoryName(destinationPath),
                                                                         Path.GetFileNameWithoutExtension(destinationPath) + " (" + i.ToString() + ")" + Path.GetExtension(destinationPath));
                                if ((isFile && !File.Exists(newDestinationPath)) || 
                                    (isDirectory && !Directory.Exists(newDestinationPath)))
                                {
                                    copyFrom.Add(path);
                                    copyTo.Add(newDestinationPath);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    copyFrom.Add(path);
                    copyTo.Add(destinationPath);
                }
            }

            if (copyFrom.Count > 0)
            {
                // If the common root is in the %temp% directory, don't thread the operation. This is probably a compressed
                // file extraction or other operation that's going to clean up temp files after we return from the DragDrop.

                string tempDirectory = Path.GetTempPath();
                string fullCommonRootPath = Path.GetFullPath(commonRoot);   // 7-Zip passes 8.3 version of temp directory

                FileOperations.OperationBlockingBehaviour blockingBehaviour = fullCommonRootPath.StartsWith(tempDirectory) ? FileOperations.OperationBlockingBehaviour.BlockUntilComplete :
                                                                                                                             FileOperations.OperationBlockingBehaviour.HandOffToThread;

                if (fileOp == FileOperations.OperationType.Move)
                {
                    FileOperations.MoveFiles(copyFrom, copyTo, blockingBehaviour);
                }
                else
                {
                    FileOperations.CopyFiles(copyFrom, copyTo, blockingBehaviour);
                }
            }
        }

        private void FileListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers.HasFlag(Keys.Control))
            {
                if (e.KeyCode == Keys.Left)
                {
                    Back();
                }
                else if (e.KeyCode == Keys.Right)
                {
                    Forward();
                }
                else if (e.KeyCode == Keys.R)
                {
                    SetDirectory(CurrentPath);
                }
                else if (e.KeyCode == Keys.F)
                {
                    OnSearchRequested?.Invoke(sender, e);
                }
                else if (e.KeyCode == Keys.A)
                {
                    foreach (ListViewItem item in FileListView.Items)
                    {
                        if (item.Tag is PathTag)
                        {
                            item.Selected = true;
                        }
                    }
                }
                else if (e.KeyCode == Keys.I)
                {
                    OnSyncronisationRequested?.Invoke(sender, e);
                }
                else if (e.KeyCode == Keys.C || e.KeyCode == Keys.X)
                {
                    HandleCopy(e.KeyCode == Keys.X);
                }
                else if (e.KeyCode == Keys.V)
                {
                    HandlePaste();
                }
            }
            else if (e.KeyCode == Keys.F2)
            {
                if (FileListView.SelectedItems.Count == 1)
                {
                    FileListView.SelectedItems[0].BeginEdit();
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                SetDirectory(CurrentPath);
            }
            else if (e.KeyCode == Keys.F7)
            {
                AddNewFile();
            }
            else if (e.KeyCode == Keys.F8)
            {
                AddNewFolder();
            }
            else if (e.KeyCode == Keys.Back)
            {
                ParentDirectory();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                var items = SelectedItemsToPaths();
                if (items.Count > 0)
                {
                    // Shift will delete permanently
                    bool toRecycleBin = !e.Modifiers.HasFlag(Keys.Shift);
                    FileOperations.DeleteFiles(items, toRecycleBin);
                }
            }
        }

        public void ParentDirectory()
        {
            SetDirectory(Path.Combine(CurrentPath, ".."));
        }

        private void HandleCopy(bool isCut)
        {
            var selectedPaths = SelectedItemsToPaths();
            if (selectedPaths.Count == 0)
            {
                // No files selected - copy the directory name to the clipboard as text
                ClipboardManager.CopyPathsToClipboard(new List<string>() { CurrentPath }, 
                                                      FileOperations.OperationType.Copy, 
                                                      ClipboardManager.ClipboardDataTypes.Text);
            }
            else
            {
                ClipboardManager.CopyPathsToClipboard(selectedPaths, isCut ? FileOperations.OperationType.Move :
                                                                             FileOperations.OperationType.Copy);
            }
        }

        private void HandlePaste()
        {
            FileOperations.OperationType fileOp;
            IEnumerable<string> paths = ClipboardManager.GetPathsFromClipboard(out fileOp);
            OnDropOrPaste(paths, CurrentPath, fileOp, FileOperations.PasteOverSelfType.Allowed);
        }

        private void AddNewFile()
        {
            string newFilePath = Path.Combine(CurrentPath, "New File.txt");
            int i = 0;

            while (File.Exists(newFilePath))
            {
                i++;
                if (i > 10)
                {
                    return;
                }

                newFilePath = Path.Combine(CurrentPath, $"New File {i}.txt");
            }

            try
            {
                File.WriteAllText(newFilePath, string.Empty);
                m_pendingPathToEdit = newFilePath;
                m_pendingPathToSelect = newFilePath;
                OnFilesChanged(null, null);
            }
            catch
            {
            }
        }

        private void AddNewFolder()
        {
            string newFolderPath = Path.Combine(CurrentPath, "New Folder");
            int i = 0;

            while (Directory.Exists(newFolderPath))
            {
                i++;
                if (i > 10)
                {
                    return;
                }

                newFolderPath = Path.Combine(CurrentPath, $"New Folder {i}");
            }

            try
            {
                Directory.CreateDirectory(newFolderPath);
                m_pendingPathToEdit = newFolderPath;
                m_pendingPathToSelect = newFolderPath;
                OnFilesChanged(null, null);
            }
            catch
            {
            }
        }

        private void AddNewShortcut()
        {
            if (FileListView.SelectedItems.Count == 1)
            {
                if (FileListView.SelectedItems[0].Tag is PathTag)
                {
                    PathTag pathTag = FileListView.SelectedItems[0].Tag as PathTag;

                    var shell = new IWshRuntimeLibrary.WshShell();
                    string shortcutPath = pathTag.Path + " - Shortcut.lnk";

                    if (!File.Exists(shortcutPath))
                    {
                        m_pendingPathToSelect = shortcutPath;
                        var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                        shortcut.Hotkey = "Ctrl+Shift+N";
                        shortcut.TargetPath = pathTag.Path;
                        shortcut.Save();
                    }
                }
            }
        }

        private List<string> SelectedItemsToPaths()
        {
            List<string> list = new List<string>();

            foreach (ListViewItem item in FileListView.SelectedItems)
            {
                if (item.Tag is PathTag)
                {
                    list.Add((item.Tag as PathTag).Path);
                }
            }

            return list;
        }

        private void FileListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 0)
            {
                m_currentSortStyle = SortStyle.Name;
                SetDirectory(CurrentPath);
            }
            else if (e.Column == 1)
            {
                switch (m_currentSortStyle)
                {
                    case SortStyle.Name:
                    case SortStyle.DateAscending:
                    case SortStyle.DateDescending:
                    case SortStyle.SizeAscending:
                        m_currentSortStyle = SortStyle.SizeDescending;
                        break;
                    case SortStyle.SizeDescending:
                        m_currentSortStyle = SortStyle.SizeAscending;
                        break;
                }

                SetDirectory(CurrentPath);
            }
            else if (e.Column == 3)
            {
                switch (m_currentSortStyle)
                {
                    case SortStyle.Name:
                    case SortStyle.SizeAscending:
                    case SortStyle.SizeDescending:
                    case SortStyle.DateAscending:
                        m_currentSortStyle = SortStyle.DateDescending;
                        break;
                    case SortStyle.DateDescending:
                        m_currentSortStyle = SortStyle.DateAscending;
                        break;
                }

                SetDirectory(CurrentPath);
            }
        }

        private void FileListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            // Recalculate the status info - we want to batch up refreshes if they happen too close together
            StatusBarRefreshTimer.Stop();
            StatusBarRefreshTimer.Start();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileListView_ItemActivate(null, null);
        }

        private void NewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewFile();
        }

        private void NewFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewFolder();
        }

        private void NewShortcutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewShortcut();
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HandlePaste();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HandleCopy(isCut: false);
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HandleCopy(isCut: true);
        }

        private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileListView.SelectedItems.Count == 1)
            {
                ListViewItem selectedItem = FileListView.SelectedItems[0];

                if (selectedItem.Tag is PathTag)
                {
                    FileOperations.ShowFileProperties((selectedItem.Tag as PathTag).Path);
                }
            }
            else if (FileListView.SelectedItems.Count > 1)
            {
                List<string> items = new List<string>();
                foreach (ListViewItem item in FileListView.SelectedItems)
                {
                    if (item.Tag is PathTag)
                    {
                        items.Add((item.Tag as PathTag).Path);
                    }
                }

                FileOperations.ShowMultiFileProperties(items);
            }
        }

        private void FileListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Label))
            {
                foreach (char c in m_invalidFilenmameChars)
                {
                    if (e.Label.Contains(c))
                    {
                        e.CancelEdit = true;
                        return;
                    }
                }

                ListViewItem item = FileListView.Items[e.Item];

                string path = null;

                if (item.Tag is PathTag)
                {
                    path = (item.Tag as PathTag).Path;
                }

                if (path != null)
                {
                    string newPath = Path.Combine(Path.GetDirectoryName(path), e.Label);

                    if (File.Exists(newPath))
                    {
                        MessageBox.Show("There is already an existing file with that name.", "Name Conflict", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        e.CancelEdit = true;
                    }
                    else if (Directory.Exists(newPath))
                    {
                        MessageBox.Show("There is already an existing directory with that name.", "Name Conflict", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        e.CancelEdit = true;
                    }
                    else
                    {
                        // Auto-select the new file after the rename when the view is redrawn due to the filewatcher.
                        m_pendingPathToSelect = newPath;

                        if (!FileOperations.RenameFiles(new List<string> { path }, new List<string> { newPath }))
                        {
                            e.CancelEdit = true;
                        }
                    }
                }
            }
        }

        private void FilePaneContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // Only allow copy if we have selected paths
            bool anyFilesSelected = FileListView.SelectedItems.Count > 0;
            bool singleFileSelected = FileListView.SelectedItems.Count == 1;

            newShortcutToolStripMenuItem.Enabled = singleFileSelected;
            copyToolStripMenuItem.Enabled = anyFilesSelected;
            cutToolStripMenuItem.Enabled = anyFilesSelected;
            openWithToolStripMenuItem.Enabled = singleFileSelected;
            propertiesToolStripMenuItem.Enabled = anyFilesSelected;

            if (singleFileSelected)
            {
                if (FileListView.SelectedItems[0].Tag is PathTag)
                {
                    PathTag pathTag = FileListView.SelectedItems[0].Tag as PathTag;
                    newShortcutToolStripMenuItem.Enabled = !pathTag.Path.ToLower().EndsWith(".lnk");
                }
            }

        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RangerForm.DefaultEditorPath == null || !File.Exists(RangerForm.DefaultEditorPath))
            {
                RangerForm.TriggerDefaultEditorSelect();
            }

            if (RangerForm.DefaultEditorPath != null && File.Exists(RangerForm.DefaultEditorPath))
            {
                List<string> selectedPaths = new List<string>();

                foreach (ListViewItem lvi in FileListView.SelectedItems)
                {
                    if (lvi.Tag is FileTag)
                    {
                        selectedPaths.Add("\"" + (lvi.Tag as FileTag).Path + "\"");
                    }
                }

                FileOperations.ExecuteFile(RangerForm.DefaultEditorPath, string.Join(" ", selectedPaths));
            }
        }

        public void SaveToConfig(Config config, string prefix)
        {
            config.SetValue(prefix + "filecolumnwidth", FileColumnHeader.Width.ToString());
            config.SetValue(prefix + "sizecolumnwidth", SizeColumnHeader.Width.ToString());
            config.SetValue(prefix + "attribcolumnwidth", AttribColumnHeader.Width.ToString());
            config.SetValue(prefix + "modifiedcolumnwidth", ModifiedColumnHeader.Width.ToString());
        }

        public void LoadFromConfig(Config config, string prefix)
        {
            FileColumnHeader.Width     = Convert.ToInt32(config.GetValue(prefix + "filecolumnwidth", FileColumnHeader.Width.ToString()));
            SizeColumnHeader.Width     = Convert.ToInt32(config.GetValue(prefix + "sizecolumnwidth", SizeColumnHeader.Width.ToString()));
            AttribColumnHeader.Width   = Convert.ToInt32(config.GetValue(prefix + "attribcolumnwidth", AttribColumnHeader.Width.ToString()));
            ModifiedColumnHeader.Width = Convert.ToInt32(config.GetValue(prefix + "modifiedcolumnwidth", ModifiedColumnHeader.Width.ToString()));
        }

        public void AddThumbnail(Image image, int index, ImageList currentImageList)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate { AddThumbnail(image, index, currentImageList); });
                }
                else
                {
                    // If the previous thumbnail creation was still going on we may get
                    // messages back from that thread. We don't want it messing with our
                    // new image list.
                    if (currentImageList == FileListView.LargeImageList)
                    {
                        FileListView.LargeImageList.Images.Add(image);
                        FileListView.Items[index].ImageIndex = index;
                    }

                    // Do not dispose the image as it's owned by the thumbnail cache
                }
            }
            catch
            {
            }
        }

        private void StatusBarRefreshTimer_Tick(object sender, EventArgs e)
        {
            StatusBarRefreshTimer.Stop();
            UpdateStatusBar(false);
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileListView.SelectedItems[0].Tag is FileTag)
            {
                FileTag fileTag = FileListView.SelectedItems[0].Tag as FileTag;
                // Do NOT add quotes around the path. This makes it not work.
                FileOperations.ExecuteFile("rundll32.exe", "shell32.dll, OpenAs_RunDLL " + fileTag.Path);
            }
        }

        private void openCommandPromptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileListView.SelectedItems.Count == 1 && FileListView.SelectedItems[0].Tag is DirectoryTag)
            {
                var path = (FileListView.SelectedItems[0].Tag as DirectoryTag).Path;
                FileOperations.ExecuteFile("cmd.exe", null, path);
            }
            else
            {
                FileOperations.ExecuteFile("cmd.exe", null, CurrentPath);
            }
        }

        private void recursiveFileListToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pathsToSearch = new List<string>();

            foreach (ListViewItem item in FileListView.SelectedItems)
            {
                if (item.Tag is DirectoryTag)
                {
                    pathsToSearch.Add((item.Tag as DirectoryTag).Path);
                }
            }

            if (pathsToSearch.Count == 0 && CurrentPath != null)
            {
                pathsToSearch.Add(CurrentPath);
            }

            var files = new List<string>();
            foreach (var path in pathsToSearch)
            {
                files.AddRange(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories));
            }

            if (files.Count > 0)
            {
                ClipboardManager.CopyPathsToClipboard(files,
                                                      FileOperations.OperationType.Copy,
                                                      ClipboardManager.ClipboardDataTypes.Text);
            }
        }

        private void thumbnailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem).Checked)
            {
                FileListView.View = View.LargeIcon;
            }
            else
            {
                FileListView.View = View.Details;
            }

            SetDirectory(CurrentPath);
        }
    }
}
