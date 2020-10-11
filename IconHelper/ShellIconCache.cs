using System.Collections.Generic;
using System.Windows.Forms;

namespace Ranger
{
    public class IconListManager
    {
        public enum PathType
        {
            File,
            Directory,
            Drive
        }

        // Cache the icon index in the system image list. This prevents us creating duplicates of identical icons
        private Dictionary<int, int> m_systemIconIndices = new Dictionary<int, int>();
        private Dictionary<int, int> m_systemOverlayIconindicies = new Dictionary<int, int>();
        // Cache the icons for the paths we view. Doing this by extension is cheaper, but we lose out on exe icons.
        private Dictionary<string, int> m_cachedExtentions = new Dictionary<string, int>();
        private Dictionary<string, int> m_cachedExtensionsOverlay = new Dictionary<string, int>();
        private Dictionary<string, int> m_cachedFullPaths = new Dictionary<string, int>();
        private Dictionary<string, int> m_cachedFullPathsOverlay = new Dictionary<string, int>();
        private Dictionary<string, int> m_cachedDirectoryPaths = new Dictionary<string, int>();
        private Dictionary<string, int> m_cachedDirectoryPathsOverlay = new Dictionary<string, int>();
        private Dictionary<string, int> m_cachedDrivePaths = new Dictionary<string, int>();
        private Dictionary<StockIconID, int> m_cachedStockIcons = new Dictionary<StockIconID, int>();

        private ImageList m_imageList = null;
        private int m_imageListCount = 0;

        public IconListManager(ImageList smallImageList)
        {
            m_imageList = smallImageList;
            m_imageListCount = m_imageList.Images.Count;
        }

        public int AddStockIcon(StockIconID iconID, List<System.Drawing.Image> pendingIconsForBulkAdd = null)
        {
            int existingImageListID;
            if (m_cachedStockIcons.TryGetValue(iconID, out existingImageListID))
            {
                return existingImageListID;
            }
            else
            {
                int index;

                int iIcon;
                var icon = Etier.IconHelper.IconReader.GetStockIcon((Etier.IconHelper.Shell32.SHSTOCKICONID)iconID, out iIcon);

                int existingImageListIndex;
                if (m_systemIconIndices.TryGetValue(iIcon, out existingImageListIndex))
                {
                    index = existingImageListIndex;
                    icon.Dispose(); // Not going to keep this - we're using an existing one.
                }
                else
                {
                    index = m_imageListCount++;

                    if (pendingIconsForBulkAdd == null)
                    {
                        m_imageList.Images.Add(icon);
                    }
                    else
                    {
                        pendingIconsForBulkAdd.Add(icon.ToBitmap());
                    }

                    m_systemIconIndices.Add(iIcon, index);
                }

                m_cachedStockIcons.Add(iconID, index);

                return index;
            }
        }

        private int AddPathIcon(string path, PathType pathType, bool isOverlay, List<System.Drawing.Image> pendingIconsForBulkAdd = null)
        {
            Dictionary<string, int> cacheToUse = null;
            string cacheKey = null;

            // Make sure we're not passing a drive in as a directory
            if (pathType == PathType.Directory && path.Length < 4)
            {
                pathType = PathType.Drive;
            }

            // Check that we haven't already got this path, if we have, then return back its index
            if (pathType == PathType.File)
            {
                string fileExtension = System.IO.Path.GetExtension(path).ToLower();
                if (fileExtension == ".exe" || fileExtension == ".ico")
                {
                    cacheToUse = isOverlay ? m_cachedFullPathsOverlay : m_cachedFullPaths;
                    cacheKey = path;
                }
                else
                {
                    cacheToUse = isOverlay ? m_cachedExtensionsOverlay : m_cachedExtentions;
                    cacheKey = fileExtension;
                }
            }
            else if (pathType == PathType.Directory)
            {
                cacheToUse = isOverlay ? m_cachedDirectoryPathsOverlay : m_cachedDirectoryPaths;

                if (path.StartsWith(@"\\"))
                {
                    string[] split = path.Substring(2).Split('\\');
                    if (split.Length == 2)
                    {
                        // Network share off a root UNC path
                        cacheKey = "NET";
                    }
                    else
                    {
                        cacheKey = "DIR";  // Keep dir icons to a minimum.
                    }
                }
                else
                {
                    cacheKey = "DIR";  // Keep dir icons to a minimum.
                }
            }
            else if (pathType == PathType.Drive)
            {
                cacheToUse = m_cachedDrivePaths;
                cacheKey = "DRV" + path;
            }

            Dictionary<int, int> systemindicesToUse = isOverlay ? m_systemOverlayIconindicies : m_systemIconIndices;

            int imageIndex;
            if (cacheToUse.TryGetValue(cacheKey, out imageIndex))
            {
                return imageIndex;
            }
            else
            {
                int index;
                bool isDirectory = pathType != PathType.File;

                int iIcon;
                var icon = Etier.IconHelper.IconReader.GetFileIcon(path, Etier.IconHelper.IconReader.IconSize.Small, isOverlay, isDirectory, out iIcon);

                int existingIndex;
                if (systemindicesToUse.TryGetValue(iIcon, out existingIndex))
                {
                    index = existingIndex;
                    icon.Dispose(); // Not going to keep this - we're using an existing one.
                }
                else
                {
                    index = m_imageListCount++;

                    if (pendingIconsForBulkAdd == null)
                    {
                        m_imageList.Images.Add(icon);
                    }
                    else
                    {
                        pendingIconsForBulkAdd.Add(icon.ToBitmap());
                    }

                    systemindicesToUse.Add(iIcon, index);
                }

                cacheToUse.Add(cacheKey, index);

                return index;
            }
        }

        public int AddFileIcon(string filePath, bool overlay)
        {
            return AddPathIcon(filePath, PathType.File, overlay);
        }

        public Dictionary<string, int> BulkAddFileIcons(IEnumerable<System.IO.FileInfo> fileInfos)
        {
            List<System.Drawing.Image> pendingIcons = new List<System.Drawing.Image>();
            Dictionary<string, int> iconIndices = new Dictionary<string, int>();

            foreach (var fi in fileInfos)
            {
                string path = fi.FullName;
                bool isShortcut = System.IO.Path.GetExtension(path).ToLower() == ".lnk";

                iconIndices.Add(path, AddPathIcon(path, PathType.File, isShortcut, pendingIcons));
            }

            if (pendingIcons.Count > 0)
            {
                m_imageList.Images.AddRange(pendingIcons.ToArray());
            }

            return iconIndices;
        }

        public int AddDriveIcon(string drivePath, bool overlay)
        {
            return AddPathIcon(drivePath, PathType.Drive, overlay);
        }

        public int AddFolderIcon(string dirPath, bool overlay)
        {
            return AddPathIcon(dirPath, PathType.Directory, overlay);
        }

        public Dictionary<string, int> BulkAddFolderIcons(IEnumerable<System.IO.DirectoryInfo> dirInfos)
        {
            List<System.Drawing.Image> pendingIcons = new List<System.Drawing.Image>();
            Dictionary<string, int> iconIndices = new Dictionary<string, int>();

            foreach (var di in dirInfos)
            {
                string path = di.FullName;
                bool isShortcut = System.IO.Path.GetExtension(path).ToLower() == ".lnk";

                iconIndices.Add(path, AddPathIcon(path, PathType.Directory, isShortcut, pendingIcons));
            }

            if (pendingIcons.Count > 0)
            {
                m_imageList.Images.AddRange(pendingIcons.ToArray());
            }

            return iconIndices;
        }
    }
}
