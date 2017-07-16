using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Leayal.Forms
{
    public sealed class FolderBrowseDialogEx : IDisposable
    {
        // Constants for sending and receiving messages in BrowseCallBackProc
        const int WM_USER = 0x400;
        const int BFFM_INITIALIZED = 1;
        const int BFFM_SELCHANGED = 2;
        const int BFFM_VALIDATEFAILEDA = 3;
        const int BFFM_VALIDATEFAILEDW = 4;
        const int BFFM_IUNKNOWN = 5; // provides IUnknown to client. lParam: IUnknown*
        const int BFFM_SETSTATUSTEXTA = WM_USER + 100;
        const int BFFM_ENABLEOK = WM_USER + 101;
        const int BFFM_SETSELECTIONA = WM_USER + 102;
        const int BFFM_SETSELECTIONW = WM_USER + 103;
        const int BFFM_SETSTATUSTEXTW = WM_USER + 104;
        const int BFFM_SETOKTEXT = WM_USER + 105; // Unicode only
        const int BFFM_SETEXPANDED = WM_USER + 106; // Unicode only

        // Browsing for directory.
#pragma warning disable 414
        internal static IntPtr BUTTONOK_DISABLE = new IntPtr(0);
        internal static IntPtr BUTTONOK_ENABLE = new IntPtr(1);
        private uint BIF_RETURNONLYFSDIRS = 0x0001;  // For finding a folder to start document searching
        private uint BIF_DONTGOBELOWDOMAIN = 0x0002;  // For starting the Find Computer
        private uint BIF_STATUSTEXT = 0x0004;  // Top of the dialog has 2 lines of text for BROWSEINFO.lpszTitle and one line if
        // this flag is set.  Passing the message BFFM_SETSTATUSTEXTA to the hwnd can set the
        // rest of the text.  This is not used with BIF_USENEWUI and BROWSEINFO.lpszTitle gets
        // all three lines of text.
        private uint BIF_RETURNFSANCESTORS = 0x0008;
        private uint BIF_EDITBOX = 0x0010;   // Add an editbox to the dialog
        private uint BIF_VALIDATE = 0x0020;   // insist on valid result (or CANCEL)

        private uint BIF_NEWDIALOGSTYLE = 0x0040;   // Use the new dialog layout with the ability to resize
        // Caller needs to call OleInitialize() before using this API
        private uint BIF_USENEWUI = 0x0040 + 0x0010; //(BIF_NEWDIALOGSTYLE | BIF_EDITBOX);

        private uint BIF_BROWSEINCLUDEURLS = 0x0080;   // Allow URLs to be displayed or entered. (Requires BIF_USENEWUI)
        private uint BIF_UAHINT = 0x0100;   // Add a UA hint to the dialog, in place of the edit box. May not be combined with BIF_EDITBOX
        private uint BIF_NONEWFOLDERBUTTON = 0x0200;   // Do not add the "New Folder" button to the dialog.  Only applicable with BIF_NEWDIALOGSTYLE.
        private uint BIF_NOTRANSLATETARGETS = 0x0400;  // don't traverse target as shortcut

        private uint BIF_BROWSEFORCOMPUTER = 0x1000;  // Browsing for Computers.
        private uint BIF_BROWSEFORPRINTER = 0x2000;// Browsing for Printers
        private uint BIF_BROWSEINCLUDEFILES = 0x4000; // Browsing for Everything
        private uint BIF_SHAREABLE = 0x8000;  // sharable resources displayed (remote shares, requires BIF_USENEWUI)
#pragma warning restore 414

        [DllImport("shell32.dll")]
        static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

        // Note that the BROWSEINFO object's pszDisplayName only gives you the name of the folder.
        // To get the actual path, you need to parse the returned PIDL
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        // static extern uint SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)]
        //StringBuilder pszPath);
        static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

        [DllImport("user32.dll", PreserveSig = true)]
        internal static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);

        public delegate int BrowseCallBackProc(IntPtr hwnd, int msg, IntPtr lp, IntPtr wp);
        struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public string pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public BrowseCallBackProc lpfn;
            public IntPtr lParam;
            public int iImage;
        }
        public int OnBrowseEvent(IntPtr hWnd, int msg, IntPtr lp, IntPtr lpData)
        {
            HandleRef h = new HandleRef(null, hWnd);
            switch (msg)
            {
                case BFFM_INITIALIZED: // Required to set initialPath
                    {
                        //Win32.SendMessage(new HandleRef(null, hWnd), BFFM_SETSELECTIONA, 1, lpData);
                        // Use BFFM_SETSELECTIONW if passing a Unicode string, i.e. native CLR Strings.
                        if (!string.IsNullOrWhiteSpace(this.SelectedDirectory) && System.IO.Directory.Exists(this.SelectedDirectory))
                        {
                            SendMessage(h, BFFM_SETSELECTIONW, 1, this.SelectedDirectory);
                            SendMessage(h, BFFM_SETEXPANDED, 1, this.SelectedDirectory);
                            this.OnFolderBrowseDialogExSelectChanged(new FolderBrowseDialogExSelectChangedEventArgs(h, this.SelectedDirectory));
                        }
                        if (!string.IsNullOrWhiteSpace(this.OKButtonText) && this.OKButtonText != "OK")
                            this.SetOKButtonText(h, this.OKButtonText);
                        break;
                    }
                case BFFM_SELCHANGED:
                    {
                        IntPtr pathPtr = Marshal.AllocHGlobal((int)(260 * Marshal.SystemDefaultCharSize));
                        if (SHGetPathFromIDList(lp, pathPtr))
                        {
                            this.SetOKEnabled(h, true);
                            SendMessage(h, BFFM_SETSTATUSTEXTW, 0, pathPtr);
                        }
                        else
                            this.SetOKEnabled(h, false);
                        string _path = Marshal.PtrToStringAuto(pathPtr);
                        Marshal.FreeHGlobal(pathPtr);
                        this.OnFolderBrowseDialogExSelectChanged(new FolderBrowseDialogExSelectChangedEventArgs(h, _path));
                        break;
                    }
            }

            return 0;
        }

        internal void SetOKEnabled(HandleRef h, bool _enabled)
        {
            if (_enabled)
                SendMessage(h, BFFM_ENABLEOK, 0, BUTTONOK_ENABLE);
            else
                SendMessage(h, BFFM_ENABLEOK, 0, BUTTONOK_DISABLE);
        }

        internal void SetOKButtonText(HandleRef h, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                IntPtr textPtr = Marshal.StringToHGlobalUni(text);
                SendMessage(h, BFFM_SETOKTEXT, 0, textPtr);
                Marshal.FreeHGlobal(textPtr);
            }
        }

        public bool UseNewStyle { get; set; }
        public bool ValidateTextBox { get; set; }
        public bool ShowTextBox { get; set; }
        public bool ShowNewFolderButton { get; set; }
        public bool ShowFiles { get; set; }
        public string Description { get; set; }
        public string RootDirectory { get; set; }
        public string SelectedDirectory { get; set; }
        public string OKButtonText { get; set; }

        public MessageBoxResult ShowDialog()
        {
            return this.ShowDialog(null);
        }

        public MessageBoxResult ShowDialog(Window parent)
        {
            if (_disposed)
                throw new ObjectDisposedException("FolderBrowseDialogEx");
            MessageBoxResult result = MessageBoxResult.None;
            string re = this.SelectFolder(this.Description, parent);
            if (!string.IsNullOrWhiteSpace(re))
            {
                result = MessageBoxResult.OK;
                this.SelectedDirectory = re;
            }
            this.Dispose();
            return result;
        }

        private StringBuilder sb;

        public FolderBrowseDialogEx()
        {
            this.sb = new StringBuilder(256);
            this.OKButtonText = "OK";
            this.UseNewStyle = true;
            this.ShowFiles = false;
            this.ShowNewFolderButton = true;
            this.ShowTextBox = true;
            this.ValidateTextBox = true;
            this.Description = "Select destination folder";
            this.RootDirectory = string.Empty;
            this.SelectedDirectory = string.Empty;
        }

        private string SelectFolder(string caption, Window parent)
        {
            IntPtr pidl = IntPtr.Zero;
            BROWSEINFO bi = new BROWSEINFO();
            bi.hwndOwner = new System.Windows.Interop.WindowInteropHelper(parent).Handle;
            bi.pidlRoot = IntPtr.Zero;
            bi.lpszTitle = caption;
            bi.ulFlags = 0;
            if (this.UseNewStyle)
                bi.ulFlags |= BIF_NEWDIALOGSTYLE;
            if (this.ShowFiles)
                bi.ulFlags |= BIF_BROWSEINCLUDEFILES;
            if (!this.ShowNewFolderButton)
                bi.ulFlags |= BIF_NONEWFOLDERBUTTON;
            if (this.ValidateTextBox)
                bi.ulFlags |= BIF_VALIDATE;
            if (this.ShowTextBox)
                bi.ulFlags |= BIF_EDITBOX;
            bi.lpfn = new BrowseCallBackProc(OnBrowseEvent);
            bi.lParam = IntPtr.Zero;
            bi.iImage = 0;

            IntPtr bufferAddress = IntPtr.Zero;
            try
            {
                /* Why 520 ???
                 * Longest path possible prior to Windows 10 is 260 characters. and Because of Unicode so 260 * 2 = 520
                 * On Windows 10, there is a policy/settings which enable "Win32 long path"
                 */
                bufferAddress = Marshal.AllocHGlobal(1024);
                this.sb.Clear();
                pidl = SHBrowseForFolder(ref bi);
                if (!SHGetPathFromIDList(pidl, bufferAddress))
                {
                    return null;
                }
                this.sb.Append(Marshal.PtrToStringAuto(bufferAddress));
            }
            catch (Exception ex)
            {
                MessageBox.Show(parent, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Caller is responsible for freeing this memory.
                Marshal.FreeCoTaskMem(pidl);
                if (bufferAddress != IntPtr.Zero)
                    Marshal.FreeHGlobal(bufferAddress);
            }

            return this.sb.ToString();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            this.sb.Clear();
        }

        public event EventHandler<FolderBrowseDialogExSelectChangedEventArgs> FolderBrowseDialogExSelectChanged;
        private void OnFolderBrowseDialogExSelectChanged(FolderBrowseDialogExSelectChangedEventArgs e)
        {
            this.FolderBrowseDialogExSelectChanged?.Invoke(this, e);
        }
    }
}
