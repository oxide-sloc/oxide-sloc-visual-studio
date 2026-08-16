using System;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace OxideSloc
{
    /// <summary>Shared implementation behind the "Analyze Solution" and "Analyze Item" commands.</summary>
    internal static class AnalyzeHelper
    {
        public static async Task RunAnalyzeAsync(OxideSlocPackage package, string path, string label)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (string.IsNullOrEmpty(path))
            {
                ShowMessage(package, "Oxide SLOC", "Nothing to analyze.", OLEMSGICON.OLEMSGICON_INFO);
                return;
            }

            var options = package.Options;
            string binary = SlocRunner.ResolveBinary(options);

            // Reports go to a temp directory, never the user's source tree.
            string outDir = Path.Combine(Path.GetTempPath(), "oxide-sloc");
            Directory.CreateDirectory(outDir);
            string safe = MakeSafe(label);
            string jsonOut = Path.Combine(outDir, safe + ".json");
            string htmlOut = Path.Combine(outDir, safe + ".html");

            string[] args = SlocRunner.AnalyzeArgs(path, jsonOut, htmlOut, options?.ExtraArgs);
            string workingDir = Directory.Exists(path) ? path : Path.GetDirectoryName(path);

            package.Log.Activate();
            package.Log.WriteLine(Environment.NewLine + "$ " + binary + " analyze " + path);

            RunResult result = await SlocRunner.RunAsync(binary, args, workingDir);

            await package.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!result.Started)
            {
                package.Log.WriteLine("[error] " + result.SpawnError.Message);
                ShowMessage(package, "Oxide SLOC",
                    "Could not run oxide-sloc (" + result.SpawnError.Message +
                    "). Set the Binary Path in Tools > Options > Oxide SLOC, or add oxide-sloc to PATH.",
                    OLEMSGICON.OLEMSGICON_CRITICAL);
                return;
            }

            if (!string.IsNullOrEmpty(result.StdOut))
            {
                package.Log.Write(result.StdOut);
            }
            if (!string.IsNullOrEmpty(result.StdErr))
            {
                package.Log.Write(result.StdErr);
            }

            var metrics = PlainMetrics.Parse(result.StdOut);
            string reportHtml = File.Exists(htmlOut) ? htmlOut : null;
            SlocState.Publish(metrics, reportHtml);

            string summary = SlocRunner.DescribeExit(result.ExitCode);
            if (metrics.WarningCount > 0)
            {
                summary += " " + metrics.WarningCount + " warning(s).";
            }

            if (result.ExitCode == 0 && metrics.WarningCount == 0)
            {
                // Quiet success - the tool window and output pane already reflect it.
                return;
            }

            var icon = result.ExitCode == 4 || result.ExitCode == 5 || result.ExitCode == 6
                ? OLEMSGICON.OLEMSGICON_CRITICAL
                : OLEMSGICON.OLEMSGICON_WARNING;
            ShowMessage(package, "Oxide SLOC", summary, icon);
        }

        private static string MakeSafe(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return "report";
            }
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                label = label.Replace(c, '_');
            }
            return label.Length > 60 ? label.Substring(0, 60) : label;
        }

        private static void ShowMessage(OxideSlocPackage package, string title, string text, OLEMSGICON icon)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            VsShellUtilities.ShowMessageBox(
                package,
                text,
                title,
                icon,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
