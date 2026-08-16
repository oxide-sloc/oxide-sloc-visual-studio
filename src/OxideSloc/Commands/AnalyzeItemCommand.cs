using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace OxideSloc
{
    /// <summary>Context-menu "Analyze with oxide-sloc" on a selected file or project node.</summary>
    internal sealed class AnalyzeItemCommand
    {
        public static async Task InitializeAsync(OxideSlocPackage package)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (await package.GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var id = new CommandID(PackageGuids.CommandSet, PackageIds.AnalyzeItemCommand);
                mcs.AddCommand(new MenuCommand((s, e) => Execute(package), id));
            }
        }

        private static void Execute(OxideSlocPackage package)
        {
            package.JoinableTaskFactory.RunAsync(async () =>
            {
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();
                string path = null;
                if (await package.GetServiceAsync(typeof(SDTE)) is DTE dte)
                {
                    path = ResolveSelectedPath(dte);
                }
                if (string.IsNullOrEmpty(path))
                {
                    package.Log.Activate();
                    package.Log.WriteLine("Oxide SLOC: no file or project is selected.");
                    return;
                }
                await AnalyzeHelper.RunAnalyzeAsync(package, path, Path.GetFileName(path.TrimEnd('\\', '/')));
            });
        }

        /// <summary>Resolve the path of the first Solution Explorer selection.</summary>
        private static string ResolveSelectedPath(DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SelectedItems items = dte.SelectedItems;
            if (items == null || items.Count == 0)
            {
                return null;
            }
            SelectedItem item = items.Item(1);

            // A project node: scan the project directory.
            if (item.Project != null && !string.IsNullOrEmpty(item.Project.FullName))
            {
                return Path.GetDirectoryName(item.Project.FullName);
            }

            // A file/folder node: scan that file (or its containing folder).
            ProjectItem projectItem = item.ProjectItem;
            if (projectItem != null && projectItem.FileCount > 0)
            {
                try
                {
                    return projectItem.FileNames[1];
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }
    }
}
