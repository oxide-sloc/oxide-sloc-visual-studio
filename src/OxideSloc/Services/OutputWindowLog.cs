using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace OxideSloc
{
    /// <summary>Thin wrapper around a named Output-window pane.</summary>
    internal sealed class OutputWindowLog
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _title;
        private IVsOutputWindowPane _pane;
        private readonly Guid _paneGuid = new Guid("6d1c2f4a-3b7e-4c8d-9f0a-1b2c3d4e5f60");

        public OutputWindowLog(IServiceProvider serviceProvider, string title)
        {
            _serviceProvider = serviceProvider;
            _title = title;
        }

        /// <summary>Append text to the pane, creating and activating it on first use.</summary>
        public void Write(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnsurePane();
            _pane?.OutputStringThreadSafe(text);
        }

        public void WriteLine(string text) => Write(text + Environment.NewLine);

        public void Activate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnsurePane();
            _pane?.Activate();
        }

        private void EnsurePane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_pane != null)
            {
                return;
            }
            if (_serviceProvider.GetService(typeof(SVsOutputWindow)) is IVsOutputWindow outputWindow)
            {
                Guid guid = _paneGuid;
                outputWindow.CreatePane(ref guid, _title, 1, 1);
                outputWindow.GetPane(ref guid, out _pane);
            }
        }
    }
}
