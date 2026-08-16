using System;
using System.Collections.Generic;
using System.Globalization;

namespace OxideSloc
{
    /// <summary>
    /// Parsed representation of oxide-sloc <c>--plain</c> output (one key=value per line).
    /// Repeated <c>warning=</c> lines are collected separately.
    /// </summary>
    internal sealed class PlainMetrics
    {
        private readonly Dictionary<string, string> _values =
            new Dictionary<string, string>(StringComparer.Ordinal);

        public IReadOnlyList<string> Warnings { get; }

        private PlainMetrics(Dictionary<string, string> values, List<string> warnings)
        {
            _values = values;
            Warnings = warnings;
        }

        public static PlainMetrics Parse(string stdout)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            var warnings = new List<string>();
            if (!string.IsNullOrEmpty(stdout))
            {
                foreach (var rawLine in stdout.Split('\n'))
                {
                    var line = rawLine.TrimEnd('\r');
                    int eq = line.IndexOf('=');
                    if (eq < 0)
                    {
                        continue;
                    }
                    string key = line.Substring(0, eq);
                    string value = line.Substring(eq + 1);
                    if (key == "warning")
                    {
                        warnings.Add(value);
                    }
                    else
                    {
                        values[key] = value;
                    }
                }
            }
            return new PlainMetrics(values, warnings);
        }

        public string GetString(string key) => _values.TryGetValue(key, out var v) ? v : null;

        public long? GetLong(string key)
        {
            if (_values.TryGetValue(key, out var v) &&
                long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
            {
                return n;
            }
            return null;
        }

        public long? FilesAnalyzed => GetLong("files_analyzed");
        public long? CodeLines => GetLong("code_lines");
        public long? CommentLines => GetLong("comment_lines");
        public long? BlankLines => GetLong("blank_lines");
        public long? Complexity => GetLong("cyclomatic_complexity");
        public long? UnitTests => GetLong("unit_tests");
        public long WarningCount => GetLong("warning_count") ?? Warnings.Count;
    }
}
