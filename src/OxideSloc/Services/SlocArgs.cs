using System;
using System.Collections.Generic;
using System.Text;

namespace OxideSloc
{
    /// <summary>
    /// Pure argument-building and exit-code helpers with no Visual Studio dependencies,
    /// so they can be unit-tested standalone (see the OxideSloc.Tests project, which
    /// links this file directly and runs on Windows and Linux).
    /// </summary>
    internal static class SlocArgs
    {
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

        /// <summary>Is this exit code a hard failure (as opposed to a warning-level gate)?</summary>
        public static bool IsError(int? code) => code == 4 || code == 5 || code == 6;

        /// <summary>Build the argv for `oxide-sloc analyze`.</summary>
        public static string[] AnalyzeArgs(string path, string jsonOut, string htmlOut, string extraArgs)
        {
            var list = new List<string> { "analyze", path, "--plain" };
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

        private static void AppendExtra(List<string> list, string extraArgs)
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

        /// <summary>
        /// Join an argument vector into a single command-line string with Windows
        /// (CommandLineToArgvW) quoting rules. net472 has no ProcessStartInfo.ArgumentList.
        /// </summary>
        public static string BuildArguments(string[] args)
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

        public static string QuoteArgument(string arg)
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
    }
}
