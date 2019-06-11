using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections.Specialized;
using Microsoft.Win32;

namespace Ranger
{
    public partial class FilePane : UserControl
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

        private SortStyle m_currentSortStyle = SortStyle.Name;

        private ViewFilter.ViewMask m_viewMask = 0;

        public ListView ListView { get { return FileListView; } }

        public string CurrentPath { get; private set; }
        private ulong m_currentDriveFreeSpace = 0;

        public event EventHandler OnFocusEvent;
        public event EventHandler OnPathChangedEvent;
        public event EventHandler OnSyncronisationRequested;

        private readonly FileSystemWatcher m_fileWatcher = new FileSystemWatcher();

        //public Etier.IconHelper.IconListManager IconListManager { private get; set; }
        public IconListManager IconListManager { private get; set; }

        private readonly char[] m_invalidFilenmameChars = Path.GetInvalidFileNameChars();

        private string m_pendingPathToEdit = null;
        private string m_pendingPathToSelect = null;

        private readonly PathHistory m_history = new PathHistory();

        public FilePane()
        {
            InitializeComponent();

            m_fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            m_fileWatcher.Filter = "*";
            m_fileWatcher.Changed += OnFilesChanged;
            m_fileWatcher.Created += OnFilesChanged;
            m_fileWatcher.Renamed += OnFilesChanged;
            m_fileWatcher.Deleted += OnFilesChanged;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                m_fileWatcher.Changed -= OnFilesChanged;
                m_fileWatcher.Created -= OnFilesChanged;
                m_fileWatcher.Renamed -= OnFilesChanged;
                m_fileWatcher.Deleted -= OnFilesChanged;
                m_fileWatcher.Dispose();
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
                else if (lvi.Tag is DirectoryTag)
                {
                    directoriesInView.Add(Path.GetFileName((lvi.Tag as DirectoryTag).Path).ToLower(), lvi);
                }
            }

            foreach (string subdirectory in Directory.GetDirectories(CurrentPath, "*", SearchOption.TopDirectoryOnly))
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

            foreach (string file in Directory.GetFiles(CurrentPath, "*", SearchOption.TopDirectoryOnly))
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
            }

            AddPathsToView(filesToAdd, directoriesToAdd, pathToSelect, out ListViewItem dummy, false);

            FileListView.EndUpdate();
            FileListView.ResumeLayout();
        }

        private void AddPathsToView(IEnumerable<string> files, IEnumerable<string> subdirectories, string pathToSelect, out ListViewItem firstSelected, bool addParentDirDots)
        {
            firstSelected = null;

            try
            {
                StringBuilder attribs = new StringBuilder("---");
                ListViewItem pendingItemToEdit = null;

                // Directories
                {
                    List<DirectoryInfo> directoryInfos = new List<DirectoryInfo>();

                    foreach (string subdirectory in subdirectories)
                    {
                        directoryInfos.Add(new DirectoryInfo(subdirectory));
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

                    if (addParentDirDots)
                    {
                        int folderIconIndex = IconListManager.AddFolderIcon(CurrentPath, false);
                        string[] parts = CurrentPath.TrimEnd(new char[] { Path.DirectorySeparatorChar }).Split(Path.DirectorySeparatorChar);
                        string parentPath = string.Join(Path.DirectorySeparatorChar.ToString(), parts, 0, parts.Length - 1);

                        FileListView.Items.Add(new ListViewItem(new string[] { "..", string.Empty, string.Empty, string.Empty }, folderIconIndex)
                        {
                            Tag = new DirectoryTag(parentPath)
                        });
                    }

                    foreach (var di in directoryInfos)
                    {
                        if (ViewFilter.FilterViewByAttributes(di.Attributes, m_viewMask, out Color itemColor))
                        {
                            string sizeString = "<folder>";
                            string dateString = di.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss");
                            string attribsString = AttribsToString(di.Attributes);

                            int folderIconIndex = IconListManager.AddFolderIcon(di.FullName, false);

                            var lvi = new ListViewItem(new string[] { Path.GetFileName(di.FullName), sizeString, attribsString, dateString }, folderIconIndex)
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
                        fileInfos.Add(new FileInfo(file));
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

                    foreach (var fi in fileInfos)
                    {
                        if (ViewFilter.FilterViewByAttributes(fi.Attributes, m_viewMask, out Color itemColour))
                        {
                            string sizeString = string.Format("{0:N0}", fi.Length);
                            string dateString = fi.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss");
                            string attribsString = AttribsToString(fi.Attributes);

                            var lvi = new ListViewItem(new string[] { Path.GetFileName(fi.FullName), sizeString, attribsString, dateString }, IconListManager.AddFileIcon(fi.FullName, false))
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

                if (pendingItemToEdit != null)
                {
                    pendingItemToEdit.BeginEdit();
                }

                m_pendingPathToEdit = null;
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

            m_currentDriveFreeSpace = FileOperations.GetDriveFreeBytes(CurrentPath);

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

                    AddPathsToView(new string[0], sharePaths, pathToSelect, out firstSelected, false);
                }
                else
                {
                    string[] directories = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);
                    string[] files = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly);

                    AddPathsToView(files, directories, pathToSelect, out firstSelected, !isRootUNCPath);
                }
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

            UpdateStatusBar();

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

        private void UpdateStatusBar()
        {
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
                    FileOperations.ExecuteFile(path);
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
            FileListView.DoDragDrop(FileListView.SelectedItems, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
        }

        private void FileListView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)) ||
                e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                IEnumerable<string> path = PathsFromDragDropSource(e.Data);

                string currentPathRoot = Path.GetPathRoot(CurrentPath);

                // If all files are on the same drive, or ALT is pressed, move the files.
                bool wouldMove = path.All((x) =>
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

        private IEnumerable<string> PathsFromDragDropSource(IDataObject data)
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

                return paths;
            }
            else if (data.GetDataPresent(DataFormats.FileDrop))
            {
                return (string[])data.GetData(DataFormats.FileDrop);
            }

            return null;
        }

        private void FileListView_DragDrop(object sender, DragEventArgs e)
        {
            IEnumerable<string> paths = PathsFromDragDropSource(e.Data);
            if (paths != null && 
                paths.Count() > 0 && 
                (e.Effect == DragDropEffects.Copy || 
                 e.Effect == DragDropEffects.Move))
            {
                var fileOp = (e.Effect == DragDropEffects.Move) ? FileOperations.OperationType.Move :
                                                                  FileOperations.OperationType.Copy;

                var hitInfo = FileListView.HitTest(FileListView.PointToClient(new Point(e.X, e.Y)));
                if (hitInfo.Item != null)
                {
                    if (hitInfo.Item.Tag is FileTag)
                    {
                        // Dropped onto a file - execute item with dropped files as parameters.
                        // Abort if the dropped file is the same as the file it was dropped on
                        string targetPath = (hitInfo.Item.Tag as FileTag).Path;

                        List<string> args = new List<string>();
                        foreach (string arg in paths)
                        {
                            if (arg == targetPath)
                                return;

                            args.Add("\"" + arg + "\"");
                        }

                        FileOperations.ExecuteFile(targetPath, string.Join(" ", args));
                    }
                    else if (hitInfo.Item.Tag is DirectoryTag)
                    {
                        // Dropped onto a directory - copy files into directory
                        string destinationPath = (hitInfo.Item.Tag as DirectoryTag).Path;

                        OnDropOrPaste(paths, destinationPath, fileOp, FileOperations.PasteOverSelfType.NotAllowed);
                    }
                }
                else
                {
                    // Dropped onto blank space - make sure we're not dropping into the same directory
                    OnDropOrPaste(paths, CurrentPath, fileOp, FileOperations.PasteOverSelfType.NotAllowed);
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
                if (fileOp == FileOperations.OperationType.Move)
                {
                    FileOperations.MoveFiles(copyFrom, copyTo);
                }
                else
                {
                    FileOperations.CopyFiles(copyFrom, copyTo);
                }
            }
        }

        private void FileListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers.HasFlag(Keys.Control))
            {
                if (e.KeyCode == Keys.R)
                {
                    SetDirectory(CurrentPath);
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
                    FileOperations.DeleteFiles(items);
                }
            }
        }

        public void ParentDirectory()
        {
            SetDirectory(Path.Combine(CurrentPath, ".."));
        }

        private void HandleCopy(bool isCut)
        {
            List<string> paths = new List<string>();
            foreach (ListViewItem selectedItem in FileListView.SelectedItems)
            {
                if (selectedItem.Tag is PathTag)
                {
                    paths.Add((selectedItem.Tag as PathTag).Path);
                }
            }

            ClipboardManager.CopyPathsToClipboard(paths, isCut ? FileOperations.OperationType.Move :
                                                                 FileOperations.OperationType.Copy);
        }

        private void HandlePaste()
        {
            IEnumerable<string> paths = ClipboardManager.GetPathsFromClipboard(out FileOperations.OperationType fileOp);
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
            }
            catch
            {
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

        private void NewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewFile();
        }

        private void NewFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewFolder();
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
            openWithToolStripMenuItem.DropDownItems.Clear();

            // Only allow copy if we have selected paths
            bool filesSelected = FileListView.SelectedItems.Count > 0;
            copyToolStripMenuItem.Enabled = filesSelected;
            cutToolStripMenuItem.Enabled = filesSelected;
            propertiesToolStripMenuItem.Enabled = FileListView.SelectedItems.Count == 1;

            if (filesSelected)
            {
                if (FileListView.SelectedItems[0].Tag is FileTag fileTag)
                {
                    string extension = Path.GetExtension(fileTag.Path);
                    IEnumerable<FileOperations.RegisteredHandler> handlers = FileOperations.GetRegisteredExtensionHandlers(extension);
                    foreach (var handler in handlers)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem(handler.Name);
                        item.Tag = handler.Command;
                        item.Click += OnOpenWithClick;
                        openWithToolStripMenuItem.DropDownItems.Add(item);
                    }
                }
            }

            openWithToolStripMenuItem.Enabled = openWithToolStripMenuItem.HasDropDownItems;
        }

        private void OnOpenWithClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string handler)
            {
                if (FileListView.SelectedItems[0].Tag is FileTag fileTag)
                {
                    string pathToRun = handler.Replace("%1", fileTag.Path);
                    string exe;
                    string args;

                    if (pathToRun.StartsWith("\""))
                    {
                        int endQuote = pathToRun.IndexOf("\"", 1);
                        exe = pathToRun.Substring(1, endQuote-1);
                        args = pathToRun.Substring(endQuote+1);
                    }
                    else
                    {
                        string[] pathParts = pathToRun.Split();
                        exe = pathParts[0];
                        args = string.Join(" ", pathParts, 1, pathParts.Length - 1);
                    }

                    Console.WriteLine($"Handler exe:{exe} with args:{args}");
                    FileOperations.ExecuteFile(exe, args);
                }
            }
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RangerMainForm.DefaultEditorPath == null || !File.Exists(RangerMainForm.DefaultEditorPath))
            {
                RangerMainForm.TriggerDefaultEditorSelect();
            }

            if (RangerMainForm.DefaultEditorPath != null && File.Exists(RangerMainForm.DefaultEditorPath))
            {
                List<string> selectedPaths = new List<string>();

                foreach (ListViewItem lvi in FileListView.SelectedItems)
                {
                    if (lvi.Tag is FileTag)
                    {
                        selectedPaths.Add("\"" + (lvi.Tag as FileTag).Path + "\"");
                    }
                }

                FileOperations.ExecuteFile(RangerMainForm.DefaultEditorPath, string.Join(" ", selectedPaths));
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

        private void StatusBarRefreshTimer_Tick(object sender, EventArgs e)
        {
            StatusBarRefreshTimer.Stop();
            UpdateStatusBar();
        }
    }
}
