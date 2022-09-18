using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ranger
{
    public class RootTag
    {
    }

    public class PathTag
    {
        public string Path { get; set; }
        public PathTag(string path) { Path = path; }
    }

    public class ParentDirectoryTag : DirectoryTag
    {
        public ParentDirectoryTag(string path) : base(path) { }
    }

    public class DirectoryTag : PathTag
    {
        public DirectoryTag(string path) : base(path) { }
    }

    public class LazyDirectoryTag : PathTag
    {
        public LazyDirectoryTag(string path) : base(path) { }
    }

    public class FileTag : PathTag
    {
        public System.IO.FileInfo FileInfo { get; }
        public FileTag(string path) : base(path) { }
        public FileTag(System.IO.FileInfo fi) : base(fi.FullName) { FileInfo = fi; }
    }

    public class BookmarkTag : PathTag
    {
        public bool OpenWithExplorer { get; set; }
        public string DisplayName { get; set; }
        public BookmarkTag(string path, bool openWithExplorer) : base(path) 
        { 
            DisplayName = System.IO.Path.GetFileName(path);
            OpenWithExplorer = openWithExplorer;
        }
    }

    public class BookmarkFileTag : BookmarkTag
    {
        public BookmarkFileTag(string path, bool openWithExplorer) : base(path, openWithExplorer) { }
    }

    public class BookmarkDirectoryTag : BookmarkTag
    {
        public BookmarkDirectoryTag(string path, bool openWithExplorer) : base(path, openWithExplorer) { }
    }

    public class RecycleBinTag
    {
    }
}
