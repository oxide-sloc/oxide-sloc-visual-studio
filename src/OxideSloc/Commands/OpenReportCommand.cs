using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace OxideSloc
{
    /// <summary>Tools &gt; Oxide SLOC &gt; Open HTML Report.</summary>
    internal sealed class OpenReportCommand
    {
        public static async Task InitializeAsync(OxideSlocPackage package)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (await package.GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var id = new CommandID(PackageGuids.CommandSet, PackageIds.OpenReportCommand);
                mcs.AddCommand(new MenuCommand((s, e) => Execute(package), id));
            }
        }

        private static void Execute(OxideSlocPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string html = SlocState.LastReportHtml;
            if (string.IsNullOrEmpty(html) || !File.Exists(html))
            {
                package.Log.Activate();
                package.Log.WriteLine("Oxide SLOC: no report yet. Run \"Analyze Solution\" first.");
                return;
            }
            // Open in the default browser.
            Process.Start(new ProcessStartInfo(html) { UseShellExecute = true });
        }
    }
}
