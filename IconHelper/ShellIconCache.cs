using System;
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

        private List<ImageList> m_imageLists = new List<ImageList>();

        public IconListManager(ImageList smallImageList, ImageList largeImageList)
        {
            m_imageLists.Add(smallImageList);
            m_imageLists.Add(largeImageList);
        }

        public int AddPathIcon(string path, PathType pathType, bool isOverlay)
        {
            Dictionary<string, int> cacheToUse = null;
            string cacheKey = null;

            // Check that we haven't already got this path, if we have, then return back its index
            if (pathType == PathType.File)
            {
                string fileExtention = System.IO.Path.GetExtension(path).ToLower();
                if (path == "exe")
                {
                    cacheToUse = isOverlay ? m_cachedFullPathsOverlay : m_cachedFullPaths;
                    cacheKey = fileExtention;
                }
                else
                {
                    cacheToUse = isOverlay ? m_cachedExtensionsOverlay : m_cachedExtentions;
                    cacheKey = path;
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
                cacheToUse = isOverlay ? m_cachedFullPathsOverlay : m_cachedFullPaths;
                cacheKey = path;
            }

            Dictionary<int, int> systemindicesToUse = isOverlay ? m_systemOverlayIconindicies : m_systemIconIndices;

            if (cacheToUse.TryGetValue(cacheKey, out int imageIndex))
            {
                return imageIndex;
            }
            else
            {
                int index;
                bool isDirectory = pathType != PathType.File;

                var icon = Etier.IconHelper.IconReader.GetFileIcon(path, Etier.IconHelper.IconReader.IconSize.Small, isOverlay, isDirectory, out int iIcon);
                if (systemindicesToUse.TryGetValue(iIcon, out int existingIndex))
                {
                    index = existingIndex;
                }
                else
                {
                    index = m_imageLists[0].Images.Count;
                    m_imageLists[0].Images.Add(icon);
                    m_imageLists[1].Images.Add(Etier.IconHelper.IconReader.GetFileIcon(path, Etier.IconHelper.IconReader.IconSize.Large, isOverlay, isDirectory, out int dummyiIcon));
                    systemindicesToUse.Add(iIcon, index);
                }

                cacheToUse.Add(cacheKey, index);

                return index;
            }
        }

        public int AddFileIcon(string drivePath, bool overlay)
        {
            return AddPathIcon(drivePath, PathType.File, overlay);
        }

        public int AddDriveIcon(string drivePath, bool overlay)
        {
            return AddPathIcon(drivePath, PathType.Drive, overlay);
        }

        public int AddFolderIcon(string dirPath, bool overlay)
        {
            return AddPathIcon(dirPath, PathType.Directory, overlay);
        }

    }
}
