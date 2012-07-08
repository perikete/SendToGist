using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Editor;
using SendToGist.Publisher;

namespace SendToGist
{
    // package class
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // provide menu attribute.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    //[ProvideAutoLoad(VSConstants..UICONTEXT_CodeWindow)]
    [Guid(GuidList.guidSendToGistPkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    public sealed class SendToGistPackage : Package
    {
        private readonly GistPublisher _publisher;

        // can't use visual studio services in constructor -- do that in Initialize instead.
        public SendToGistPackage()
        {
            _publisher = new GistPublisher();
        }

        // package initialization -- called when package is sited in visual studio.
        protected override void Initialize()
        {
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null != mcs)
            {
                // Create the command for the menu item.
                var menuCommandID = new CommandID(GuidList.guidSendToGistCmdSet, (int)PkgCmdIDList.cmdidSendToGist);
                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += OnBeforeQueryStatus;
                mcs.AddCommand(menuItem);
            }

        }

        // enable if there is a selection in the current view.
        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var cmd = sender as OleMenuCommand;
            if (null != cmd)
            {
                var enable = false;
                var view = GetActiveTextView();

                if ((null != view) && !view.Selection.IsEmpty)
                {
                    enable = (view.Selection.SelectedSpans.Count > 0);
                }
                cmd.Enabled = enable;
            }
        }

        // get the active WpfTextView, if there is one.
        private IWpfTextView GetActiveTextView()
        {
            IWpfTextView view = null;
            IVsTextView vTextView;

            var txtMgr =
                (IVsTextManager)GetService(typeof(SVsTextManager));
            
            const int mustHaveFocus = 1;
            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);

            var userData = vTextView as IVsUserData;
            if (null != userData)
            {
                object holder;
                var guidViewHost = DefGuidList.guidIWpfTextViewHost;
                userData.GetData(ref guidViewHost, out holder);
                var viewHost = (IWpfTextViewHost)holder;
                view = viewHost.TextView;
            }

            return view;
        }

        public static string GetCurrentFilename(IWpfTextView wpfTextView)
        {
            Microsoft.VisualStudio.Text.ITextDocument document;
            if ((wpfTextView == null) ||
                    (!wpfTextView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(Microsoft.VisualStudio.Text.ITextDocument), out document)))
                return String.Empty;

            // If we have no document, just ignore it.
            if ((document == null) || (document.TextBuffer == null))
                return String.Empty;

            return Path.GetFileName(document.FilePath);
        }

        public void SetStatusBarText(string text)
        {
            var vsStatusBar = (IVsStatusbar)GetService(typeof(SVsStatusbar));
            vsStatusBar.SetText(text);
        }

        // called when the menu item is executed.
        private void MenuItemCallback(object sender, EventArgs e)
        {
            IWpfTextView view = GetActiveTextView();

            if ((null != view) && !view.Selection.IsEmpty)
            {
                if (view.Selection.SelectedSpans.Count > 0)
                {
                    string selectedText = view.Selection.SelectedSpans[0].GetText();

                    if (!String.IsNullOrWhiteSpace(selectedText))
                    {

                        var currentFilename = GetCurrentFilename(view);
                        var result = _publisher.Publish(new GistPublishRequest("Published", selectedText, currentFilename));

                        // bring up draft entry in browser.
                        System.Diagnostics.Process.Start(result.Url);
                        Clipboard.SetText(result.Url);
                        SetStatusBarText("Copied Gist's URL to clipboard.");
                    }
                }

            }
        }

    }
}



