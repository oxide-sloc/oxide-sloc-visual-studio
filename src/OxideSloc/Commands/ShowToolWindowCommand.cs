using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace OxideSloc
{
    /// <summary>Tools &gt; Oxide SLOC &gt; Metrics Window. Shows the metrics tool window.</summary>
    internal sealed class ShowToolWindowCommand
    {
        public static async Task InitializeAsync(OxideSlocPackage package)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (await package.GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var id = new CommandID(PackageGuids.CommandSet, PackageIds.ShowToolWindowCommand);
                mcs.AddCommand(new MenuCommand((s, e) => Execute(package), id));
            }
        }

        private static void Execute(OxideSlocPackage package)
        {
            package.JoinableTaskFactory.RunAsync(async () =>
            {
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();
                ToolWindowPane window = await package.ShowToolWindowAsync(
                    typeof(SlocToolWindow), 0, create: true, cancellationToken: package.DisposalToken);
                if (window?.Frame == null)
                {
                    throw new NotSupportedException("Cannot create the Oxide SLOC tool window.");
                }
            });
        }
    }
}
