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
        public string Path { get; }
        public PathTag(string path) { Path = path; }
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
        public string DisplayName { get; set; }
        public BookmarkTag(string path) : base(path) { DisplayName = System.IO.Path.GetFileName(path); }
    }

    public class BookmarkFileTag : BookmarkTag
    {
        public BookmarkFileTag(string path) : base(path) { }
    }

    public class BookmarkDirectoryTag : BookmarkTag
    {
        public BookmarkDirectoryTag(string path) : base(path) { }
    }
}
