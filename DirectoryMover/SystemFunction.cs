using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMover
{
    static class SystemFunction
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        static extern UInt32 SHGetSetFolderCustomSettings(ref LPSHFOLDERCUSTOMSETTINGS pfcs, string pszPath, UInt32 dwReadWrite);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct LPSHFOLDERCUSTOMSETTINGS
        {
            public UInt32 dwSize;
            public UInt32 dwMask;
            public IntPtr pvid;
            public string pszWebViewTemplate;
            public UInt32 cchWebViewTemplate;
            public string pszWebViewTemplateVersion;
            public string pszInfoTip;
            public UInt32 cchInfoTip;
            public IntPtr pclsid;
            public UInt32 dwFlags;
            public string pszIconFile;
            public UInt32 cchIconFile;
            public int iIconIndex;
            public string pszLogo;
            public UInt32 cchLogo;
        }

        static public void ChangeFolderIcon(string folderpath)
        {
            LPSHFOLDERCUSTOMSETTINGS FolderSettings = new LPSHFOLDERCUSTOMSETTINGS();
            FolderSettings.dwMask = 0x10;

            FolderSettings.pszIconFile = @"C:\windows\system32\SHELL32.dll";
            FolderSettings.iIconIndex = 0x40;

            UInt32 FCS_FORCEWRITE = 0x00000002;
            UInt32 FCS_WRITE = FCS_FORCEWRITE;

            string pszPath = folderpath;
            UInt32 HRESULT = SHGetSetFolderCustomSettings(ref FolderSettings, pszPath, FCS_WRITE);
        } 
    }
}
