using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace OxideSloc
{
    /// <summary>
    /// The oxide-sloc Visual Studio package. Registers the commands and the metrics
    /// tool window, and hosts the shared output pane.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("oxide-sloc", "Run oxide-sloc code-metrics reports from Visual Studio.", "0.1.0")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SlocToolWindow))]
    [ProvideOptionPage(typeof(GeneralOptions), "Oxide SLOC", "General", 0, 0, true)]
    [Guid(PackageGuids.PackageGuidString)]
    public sealed class OxideSlocPackage : AsyncPackage
    {
        /// <summary>Shared "Oxide SLOC" output pane, created lazily.</summary>
        internal OutputWindowLog Log { get; private set; }

        /// <summary>Current values from the Tools &gt; Options &gt; Oxide SLOC page.</summary>
        internal GeneralOptions Options => (GeneralOptions)GetDialogPage(typeof(GeneralOptions));

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            Log = new OutputWindowLog(this, "Oxide SLOC");

            await AnalyzeSolutionCommand.InitializeAsync(this);
            await AnalyzeItemCommand.InitializeAsync(this);
            await OpenReportCommand.InitializeAsync(this);
            await ServeCommand.InitializeAsync(this);
            await ShowToolWindowCommand.InitializeAsync(this);
        }
    }
}
