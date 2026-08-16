using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace OxideSloc
{
    internal struct RunResult
    {
        public int? ExitCode;
        public string StdOut;
        public string StdErr;
        public Exception SpawnError;

        public bool Started => SpawnError == null;
    }

    /// <summary>
    /// Locates and runs the oxide-sloc executable. Binary resolution order:
    ///   1. the extension's Binary Path option, if set
    ///   2. the SLOC_BIN / OXIDE_SLOC environment variables
    ///   3. "oxide-sloc" on PATH
    /// </summary>
    internal static class SlocRunner
    {
        public static string ResolveBinary(GeneralOptions options)
        {
            var configured = options?.BinaryPath?.Trim();
            if (!string.IsNullOrEmpty(configured))
            {
                return configured;
            }
            var fromEnv = Environment.GetEnvironmentVariable("SLOC_BIN")
                ?? Environment.GetEnvironmentVariable("OXIDE_SLOC");
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return fromEnv.Trim();
            }
            return "oxide-sloc";
        }

        /// <summary>Human-facing meaning of an oxide-sloc exit code.</summary>
        public static string DescribeExit(int? code) => SlocArgs.DescribeExit(code);

        /// <summary>Build the argv for `oxide-sloc analyze`.</summary>
        public static string[] AnalyzeArgs(string path, string jsonOut, string htmlOut, string extraArgs)
            => SlocArgs.AnalyzeArgs(path, jsonOut, htmlOut, extraArgs);

        /// <summary>Run oxide-sloc with the given argument list off the UI thread.</summary>
        public static Task<RunResult> RunAsync(string binary, string[] args, string workingDirectory)
        {
            return Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = binary,
                    Arguments = SlocArgs.BuildArguments(args),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory ?? string.Empty,
                };

                try
                {
                    using (var process = new Process { StartInfo = psi })
                    {
                        var stdout = new StringBuilder();
                        var stderr = new StringBuilder();
                        process.OutputDataReceived += (s, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
                        process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        return new RunResult
                        {
                            ExitCode = process.ExitCode,
                            StdOut = stdout.ToString(),
                            StdErr = stderr.ToString(),
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new RunResult { SpawnError = ex };
                }
            });
        }
    }
}
