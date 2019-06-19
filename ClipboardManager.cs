using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ranger
{
    public static class ClipboardManager
    {
        [Serializable]
        public class FileOperationPathList
        {
            public IEnumerable<string> Paths = new List<string>();
            public FileOperations.OperationType FileOperation;
        }

        public static IDataObject PathsToDataObject(IEnumerable<string> paths, FileOperations.OperationType fileOp)
        {
            if (paths.Count() == 0)
                return null;

            FileOperationPathList pathList = new FileOperationPathList()
            {
                Paths = paths,
                FileOperation = fileOp
            };

            StringBuilder sb = new StringBuilder();
            StringCollection fileDroplist = new StringCollection();

            foreach (string path in paths)
            {
                sb.AppendLine(path);
                fileDroplist.Add(path);
            }

            DataObject dataObject = new DataObject();
            dataObject.SetData(pathList);
            dataObject.SetFileDropList(fileDroplist);
            dataObject.SetText(sb.ToString());

            return dataObject;
        }

        public static void CopyPathsToClipboard(IEnumerable<string> paths, FileOperations.OperationType fileOp)
        {
            var dataObject = PathsToDataObject(paths, fileOp);
            if (dataObject != null)
            {
                Clipboard.SetDataObject(dataObject);
            }
        }

        public static IEnumerable<string> GetPathsFromClipboard(out FileOperations.OperationType fileOp)
        {
            fileOp = FileOperations.OperationType.Copy;

            IDataObject clipData = Clipboard.GetDataObject();
            if (clipData != null)
            {
                if (clipData.GetDataPresent(typeof(FileOperationPathList)))
                {
                    FileOperationPathList opList = clipData.GetData(typeof(FileOperationPathList)) as FileOperationPathList;
                    if (opList != null)
                    {
                        fileOp = opList.FileOperation;
                        return opList.Paths;
                    }
                }
            }

            if (Clipboard.ContainsFileDropList())
            {
                StringCollection clipPaths = Clipboard.GetFileDropList();

                string[] paths = new string[clipPaths.Count];
                clipPaths.CopyTo(paths, 0);
                return paths;
            }

            if (Clipboard.ContainsText())
            {
                // See if this is a list of files and dirs
                List<string> paths = new List<string>();
                StringReader sr = new StringReader(Clipboard.GetText());
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        line = line.Trim();
                        if (File.Exists(line) || Directory.Exists(line))
                        {
                            paths.Add(line);
                        }
                    }
                }

                return paths;
            }

            return new List<string>();
        }
    }

}
