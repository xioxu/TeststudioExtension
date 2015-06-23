using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ArtOfTest.Common;
using ArtOfTest.Common.Win32;
using ArtOfTest.WebAii.Core;
using ArtOfTest.WebAii.Win32.Dialogs;
using System.IO;

namespace TestStudioExtension
{
    public class CustomFileUploadDialog : FileUploadDialog
    {
        internal static readonly string SILVERLIGHT_TITLE = MUI.GetString(Path.Combine(MUI.MUIDirectory, "ieframe.dll.mui"), 12850, "Open");
        private string fileTitle = string.Empty;

        public CustomFileUploadDialog(Browser parentBrowser, string filePath, DialogButton dismissButton, string title) : base(parentBrowser, filePath, dismissButton, title)
        {
            fileTitle = title;
        }

        public override bool IsDialogActive(WindowCollection dialogs)
        {
            Window window = null;
            window = IsDialogActiveByTitleAndTextContent2(dialogs, this.fileTitle, true, null);

            if (window == null )
            {
                window = IsDialogActiveByTitleAndTextContent2(dialogs, SILVERLIGHT_TITLE, true, null);
            }

            if (window != null)
            {
                FieldInfo htmlTitle = typeof(FileUploadDialog).GetField("_htmlTitle", BindingFlags.Instance | BindingFlags.NonPublic); 
                htmlTitle.SetValue(this, window.Caption);
                return base.IsDialogActive(dialogs);
            }

            return false;
        }

        private bool IsWindowFromChildProcess(Window win)
        {
            MethodInfo method = typeof (BaseDialog).GetMethod("IsWindowFromChildProcess",
                BindingFlags.Instance | BindingFlags.NonPublic);

            return (bool)method.Invoke(this, new object[] { win });
        }

        protected Window IsDialogActiveByTitleAndTextContent2(WindowCollection dialogs, string title, bool partialTitle, string childWindowTextContent)
        {
            foreach (Window window in (ReadOnlyCollectionBase)dialogs)
            {
                bool flag1 = !partialTitle ? string.Equals(window.Caption, title, StringComparison.OrdinalIgnoreCase) : window.Caption != null && title != null && window.Caption.IndexOf(title, StringComparison.OrdinalIgnoreCase) > -1;
                if (flag1)
                {
                    if (!string.IsNullOrEmpty(childWindowTextContent))
                        flag1 = WindowManager.FindWindowRecursively(window.Handle, childWindowTextContent, true, 0) != null;
                    if (flag1)
                    {
                        if (this.ParentBrowser != null && this.ParentBrowser.BrowserType == BrowserType.InternetExplorer)
                        {
                            string[] strArray = this.ParentBrowser.Version.Split('.');
                            int result;
                            if (strArray.Length > 0 && int.TryParse(strArray[0], out result) && result >= 8)
                            {
                                this.SetDialogWindow(window);
                                return window;
                            }
                        }
                        bool flag2 = window.OwnerProcess != null && window.OwnerProcess.Id == this.ParentProcessId;
                        Window parentWindow = window.ParentWindow;
                        bool flag3 = parentWindow != null && parentWindow.OwnerProcess != null && parentWindow.OwnerProcess.Id == this.ParentProcessId;
                        if (flag2 || flag3 || IsWindowFromChildProcess(window))
                        {
                            this.SetDialogWindow(window);
                            return window;
                        }
                    }
                }
            }
            return null;
        }
    }
}
