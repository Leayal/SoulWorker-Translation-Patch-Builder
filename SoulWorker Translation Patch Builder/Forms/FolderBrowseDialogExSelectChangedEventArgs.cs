using System;
using System.Runtime.InteropServices;

namespace Leayal.Forms
{
    public class FolderBrowseDialogExSelectChangedEventArgs : EventArgs
    {
        const int WM_USER = 0x400;
        const int BFFM_ENABLEOK = WM_USER + 101;
        const int BFFM_SETOKTEXT = WM_USER + 105; // Unicode only

        public string CurrentPath { get; }

        private HandleRef hr;
        public FolderBrowseDialogExSelectChangedEventArgs(HandleRef h, string _path) : base()
        {
            this.CurrentPath = _path;
            this.hr = h;
        }

        public void SetOKEnabled(bool _enabled)
        {
            if (_enabled)
                FolderBrowseDialogEx.SendMessage(hr, BFFM_ENABLEOK, 0, FolderBrowseDialogEx.BUTTONOK_ENABLE);
            else
                FolderBrowseDialogEx.SendMessage(hr, BFFM_ENABLEOK, 0, FolderBrowseDialogEx.BUTTONOK_DISABLE);
        }

        public void SetOKButtonText(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                IntPtr textPtr = Marshal.StringToHGlobalUni(text);
                FolderBrowseDialogEx.SendMessage(hr, BFFM_SETOKTEXT, 0, textPtr);
                Marshal.FreeHGlobal(textPtr);
            }
        }
    }
}
