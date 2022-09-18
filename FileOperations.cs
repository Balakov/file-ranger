using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Ranger
{
    public static class FileOperations
    {
        public enum OperationType
        {
            Copy,
            Move
        }

        public enum PasteOverSelfType
        {
            Allowed,
            NotAllowed
        }

        public enum OperationBlockingBehaviour
        {
            HandOffToThread,
            BlockUntilComplete
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FILE_OP_TYPE wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public FILE_OP_FLAGS fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }

        private enum FILE_OP_TYPE : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004,
        }

        [Flags]
        private enum FILE_OP_FLAGS : ushort
        {
            FOF_MULTIDESTFILES = 0x0001,
            FOF_CONFIRMMOUSE = 0x0002,
            FOF_SILENT = 0x0004,
            FOF_RENAMEONCOLLISION = 0x0008,
            FOF_NOCONFIRMATION = 0x0010,
            FOF_WANTMAPPINGHANDLE = 0x0020,
            FOF_ALLOWUNDO = 0x0040,
            FOF_FILESONLY = 0x0080,
            FOF_SIMPLEPROGRESS = 0x0100,
            FOF_NOCONFIRMMKDIR = 0x0200,
            FOF_NOERRORUI = 0x0400,
            FOF_NOCOPYSECURITYATTRIBS = 0x0800,
            FOF_NORECURSION = 0x1000,
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,
            FOF_WANTNUKEWARNING = 0x4000,
            FOF_NORECURSEREPARSE = 0x8000,
        }

        public static void MoveFiles(List<string> fromList, List<string> toList, OperationBlockingBehaviour blockingBehaviour)
        {
            var thread = new System.Threading.Thread(() =>
            {
                CopyFilesInternal(fromList, toList, FILE_OP_TYPE.FO_MOVE);
            });

            thread.Start();

            if (blockingBehaviour == OperationBlockingBehaviour.BlockUntilComplete)
            {
                thread.Join();
            }
        }

        public static void CopyFiles(List<string> fromList, List<string> toList, OperationBlockingBehaviour blockingBehaviour)
        {
            var thread = new System.Threading.Thread(() =>
            {
                CopyFilesInternal(fromList, toList, FILE_OP_TYPE.FO_COPY);
            });

            thread.Start();

            if (blockingBehaviour == OperationBlockingBehaviour.BlockUntilComplete)
            {
                thread.Join();
            }
        }

        private static bool CopyFilesInternal(List<string> fromList, List<string> toList, FILE_OP_TYPE opType)
        {
            bool success = false;

            string from = FilenamesToShellString(fromList);
            string to = FilenamesToShellString(toList);

            SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT()
            {
                wFunc = opType,
                pFrom = from,
                pTo = to,
                fFlags = FILE_OP_FLAGS.FOF_NOCONFIRMMKDIR | FILE_OP_FLAGS.FOF_MULTIDESTFILES
            };

            if (SHFileOperation(ref lpFileOp) == 0)
            {
                success = !lpFileOp.fAnyOperationsAborted;
            }

            return success;
        }

        public static void DeleteFiles(List<string> fromList, bool toRecycleBin)
        {
            new System.Threading.Thread(() =>
            {
                string from = FilenamesToShellString(fromList);

                SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT()
                {
                    wFunc = FILE_OP_TYPE.FO_DELETE,
                    pFrom = from,
                    fFlags = FILE_OP_FLAGS.FOF_WANTNUKEWARNING | (toRecycleBin ? FILE_OP_FLAGS.FOF_ALLOWUNDO : 0)
                };

                SHFileOperation(ref lpFileOp);
            }).Start();
        }

        public static bool RenameFiles(List<string> fromList, List<string> toList)
        {
            bool success = false;

            string from = FilenamesToShellString(fromList);
            string to = FilenamesToShellString(toList);

            SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT()
            {
                wFunc = FILE_OP_TYPE.FO_RENAME,
                pFrom = from,
                pTo = to,
                fFlags = 0
            };

            if (SHFileOperation(ref lpFileOp) == 0)
            {
                success = !lpFileOp.fAnyOperationsAborted;
            }

            return success;
        }

        private static string FilenamesToShellString(List<string> filenames)
        {
            // MAX_PATH is 260 chars
            StringBuilder result = new StringBuilder(260 * filenames.Count);

            foreach (string file in filenames)
            {
                result.Append(file);
                result.Append("\0");
            }

            result.Append("\0");

            return result.ToString();
        }

        public static void ExecuteFile(string path, string args = null, string cwd = null)
        {
            System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = path,
                Arguments = args,
                WorkingDirectory = !string.IsNullOrEmpty(cwd) ? cwd : Path.GetDirectoryName(path),
                UseShellExecute = true
            };

            try
            {
                System.Diagnostics.Process.Start(si);
            }
            catch
            {
            }
        }

        public static void ExecuteWithExplorer(string path)
        {
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        public static ulong GetDriveFreeBytes(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }

            if (!path.EndsWith(@"\"))
            {
                path += @"\";
            }

            ulong freeBytes;
            ulong dummy1;
            ulong dummy2;
            if (GetDiskFreeSpaceEx(path, out freeBytes, out dummy1, out dummy2))
            {
                return freeBytes;
            }

            return 0;
        }


        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;

        public static bool ShowFileProperties(string Filename)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            return ShellExecuteEx(ref info);
        }

        public class RegisteredHandler
        {
            public string Name;
            public string Command;
        }

        [DllImport("shell32.dll", SetLastError = true)]
        static extern int SHMultiFileProperties(IDataObject pdtobj, int flags);

        //	ILCreateFromPath
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ILCreateFromPath(string pwszPath);

        //	ILFree
        [DllImport("shell32.dll", CharSet = CharSet.None)]
        public static extern void ILFree(IntPtr pidl);

        //	ILGetSize
        [DllImport("shell32.dll", CharSet = CharSet.None)]
        public static extern uint ILGetSize(IntPtr pidl);

        //
        // http://www.codeproject.com/KB/files/JFileManager.aspx
        //

        public static void ShowMultiFileProperties(List<string> paths)
        {
            var pdtobj = new DataObject();
            MemoryStream msLinks = FileListToShellIDListArray(paths);
            pdtobj.SetData("Shell IDList Array", true, msLinks);

            SHMultiFileProperties(pdtobj, 0);
        }

        private static object[] FileListToIDListArray(List<string> listObjects)
        {
            IntPtr pidl;
            int cbPidl;

            // Allocate an array of objects (byte arrays) to store the 
            // pidl of each item in the file list
            object[] objectArray = new object[listObjects.Count];
            for (int count = 0; count < listObjects.Count; count++)
            {
                // Obtain a pidl for the file
                pidl = ILCreateFromPath(listObjects[count]);

                cbPidl = (int)ILGetSize(pidl);

                // Convert the pidl to a byte array
                objectArray[count] = (object)new byte[cbPidl];
                Marshal.Copy(pidl, (byte[])objectArray[count], 0, cbPidl);

                // Free the pidl
                ILFree(pidl);
            }
            return objectArray;
        }

        private static MemoryStream FileListToShellIDListArray(List<string> listObjects)
        {
            int cb = 0;
            int offset = 0;
            int count;
            uint pidlDesktop = 0;

            object[] objectArray = null;
            MemoryStream stream = null;
            BinaryWriter streamWriter = null;

            // Convert the array of paths to an array of pidls
            objectArray = FileListToIDListArray(listObjects);

            // Determine the amount of memory required for the CIDA
            // The 2 in the statement below is for the offset to the 
            // folder pidl and the count field in the CIDA structure
            cb = offset = Marshal.SizeOf(typeof(uint)) * (objectArray.Length + 2);
            for (count = 0; count < objectArray.Length; count++)
            {
                cb += ((byte[])objectArray[count]).Length; ;
            }

            // Create a memory stream that we will write the CIDA into
            stream = new MemoryStream();

            // Wrap the memory stream with a BinaryWriter object
            streamWriter = new BinaryWriter(stream);

            // Write the cidl member of the CIDA structure
            streamWriter.Write(objectArray.Length);

            // Write the array of offsets for each pidl. Calculate each offset as we go
            streamWriter.Write(offset);
            offset += Marshal.SizeOf(pidlDesktop);

            for (count = 0; count < objectArray.Length; count++)
            {
                streamWriter.Write(offset);
                offset += ((byte[])objectArray[count]).Length;
            }

            // Write the parent folder pidl
            streamWriter.Write(pidlDesktop);

            // Write the item pidls
            for (count = 0; count < objectArray.Length; count++)
            {
                streamWriter.Write((byte[])objectArray[count]);
            }

            // Return the memory stream that contains the CIDA
            return stream;
        }



    }
}
