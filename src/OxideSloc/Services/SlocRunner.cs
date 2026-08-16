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
        public static string DescribeExit(int? code)
        {
            switch (code)
            {
                case 0: return "Analysis complete.";
                case 2: return "Warnings gate failed (--fail-on-warnings).";
                case 3: return "Code lines below threshold (--fail-below).";
                case 4: return "SLOC budget exceeded (--fail-on-budget).";
                case 5: return "Growth exceeded baseline (--fail-above-baseline).";
                case 6: return "Cyclomatic complexity exceeded (--max-complexity).";
                default: return "oxide-sloc exited with code " + (code?.ToString() ?? "unknown") + ".";
            }
        }

        /// <summary>Run oxide-sloc with the given argument list off the UI thread.</summary>
        public static Task<RunResult> RunAsync(string binary, string[] args, string workingDirectory)
        {
            return Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = binary,
                    Arguments = BuildArguments(args),
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

        /// <summary>
        /// Join an argument vector into a single command-line string with Windows
        /// (CommandLineToArgvW) quoting rules. net472 has no ProcessStartInfo.ArgumentList.
        /// </summary>
        private static string BuildArguments(string[] args)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(QuoteArgument(args[i]));
            }
            return sb.ToString();
        }

        private static string QuoteArgument(string arg)
        {
            if (arg.Length > 0 && arg.IndexOfAny(new[] { ' ', '\t', '"', '\\' }) < 0)
            {
                return arg;
            }
            var sb = new StringBuilder();
            sb.Append('"');
            int backslashes = 0;
            foreach (char c in arg)
            {
                if (c == '\\')
                {
                    backslashes++;
                }
                else if (c == '"')
                {
                    sb.Append('\\', backslashes * 2 + 1);
                    backslashes = 0;
                    sb.Append('"');
                }
                else
                {
                    sb.Append('\\', backslashes);
                    backslashes = 0;
                    sb.Append(c);
                }
            }
            sb.Append('\\', backslashes * 2);
            sb.Append('"');
            return sb.ToString();
        }

        /// <summary>Build the argv for `oxide-sloc analyze`.</summary>
        public static string[] AnalyzeArgs(string path, string jsonOut, string htmlOut, string extraArgs)
        {
            var list = new System.Collections.Generic.List<string> { "analyze", path, "--plain" };
            if (!string.IsNullOrEmpty(jsonOut))
            {
                list.Add("--json-out");
                list.Add(jsonOut);
            }
            if (!string.IsNullOrEmpty(htmlOut))
            {
                list.Add("--html-out");
                list.Add(htmlOut);
            }
            AppendExtra(list, extraArgs);
            return list.ToArray();
        }

        private static void AppendExtra(System.Collections.Generic.List<string> list, string extraArgs)
        {
            if (string.IsNullOrWhiteSpace(extraArgs))
            {
                return;
            }
            // Simple whitespace split; users needing quoted args can set them individually.
            foreach (var token in extraArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                list.Add(token);
            }
        }
    }
}
