using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace OxideSloc
{
    /// <summary>Tools &gt; Oxide SLOC &gt; Start Web UI. Launches `oxide-sloc serve` and opens the browser.</summary>
    internal sealed class ServeCommand
    {
        private static Process _serveProcess;

        public static async Task InitializeAsync(OxideSlocPackage package)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (await package.GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var id = new CommandID(PackageGuids.CommandSet, PackageIds.ServeCommand);
                mcs.AddCommand(new MenuCommand((s, e) => Execute(package), id));
            }
        }

        private static void Execute(OxideSlocPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_serveProcess != null && !_serveProcess.HasExited)
            {
                package.Log.Activate();
                package.Log.WriteLine("Oxide SLOC: web UI is already running.");
                return;
            }

            var options = package.Options;
            int port = options?.ServePort > 0 ? options.ServePort : 4317;
            string binary = SlocRunner.ResolveBinary(options);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = binary,
                    Arguments = "serve --bind 127.0.0.1:" + port,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                _serveProcess = Process.Start(psi);
                package.Log.Activate();
                package.Log.WriteLine("Oxide SLOC: started web UI on http://127.0.0.1:" + port);

                // Give the server a moment to bind, then open the browser.
                System.Threading.Thread.Sleep(800);
                Process.Start(new ProcessStartInfo("http://127.0.0.1:" + port) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                package.Log.WriteLine("Oxide SLOC: failed to start web UI: " + ex.Message);
            }
        }
    }
}
