using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace OxideSloc
{
    /// <summary>Tools &gt; Oxide SLOC &gt; Analyze Solution.</summary>
    internal sealed class AnalyzeSolutionCommand
    {
        public static async Task InitializeAsync(OxideSlocPackage package)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (await package.GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var id = new CommandID(PackageGuids.CommandSet, PackageIds.AnalyzeSolutionCommand);
                mcs.AddCommand(new MenuCommand((s, e) => Execute(package), id));
            }
        }

        private static void Execute(OxideSlocPackage package)
        {
            package.JoinableTaskFactory.RunAsync(async () =>
            {
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();
                string solutionDir = null;
                if (await package.GetServiceAsync(typeof(SDTE)) is DTE dte &&
                    !string.IsNullOrEmpty(dte.Solution?.FullName))
                {
                    solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
                }
                if (string.IsNullOrEmpty(solutionDir))
                {
                    package.Log.Activate();
                    package.Log.WriteLine("Oxide SLOC: no solution is open.");
                    return;
                }
                await AnalyzeHelper.RunAnalyzeAsync(package, solutionDir, "solution");
            });
        }
    }
}
