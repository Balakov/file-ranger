using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

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

        public static void MoveFiles(List<string> fromList, List<string> toList)
        {
            new System.Threading.Thread(() =>
            {
                CopyFilesInternal(fromList, toList, FILE_OP_TYPE.FO_MOVE);
            }).Start();
        }

        public static void CopyFiles(List<string> fromList, List<string> toList)
        {
            new System.Threading.Thread(() =>
            {
                CopyFilesInternal(fromList, toList, FILE_OP_TYPE.FO_COPY);
            }).Start();
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

        public static void DeleteFiles(List<string> fromList)
        {
            new System.Threading.Thread(() =>
            {
                string from = FilenamesToShellString(fromList);

                SHFILEOPSTRUCT lpFileOp = new SHFILEOPSTRUCT()
                {
                    wFunc = FILE_OP_TYPE.FO_DELETE,
                    pFrom = from,
                    fFlags = FILE_OP_FLAGS.FOF_WANTNUKEWARNING | FILE_OP_FLAGS.FOF_ALLOWUNDO
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
            // MAX_PATH is 255 chars
            StringBuilder result = new StringBuilder(255 * filenames.Count);

            foreach (string file in filenames)
            {
                result.Append(file);
                result.Append("\0");
            }

            result.Append("\0");

            return result.ToString();
        }

        public static void ExecuteFile(string path, string args = null)
        {
            System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = path,
                Arguments = args,
                WorkingDirectory = System.IO.Path.GetDirectoryName(path),
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(si);
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

            if (GetDiskFreeSpaceEx(path, out ulong freeBytes,  out ulong dummy1,  out ulong dummy2))
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

    }
}
