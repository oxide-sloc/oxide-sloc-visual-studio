using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace OxideSloc
{
    /// <summary>
    /// WPF content for the Oxide SLOC tool window, built in code (no XAML) so the
    /// project needs no XAML build configuration. Shows the latest metrics and
    /// refreshes whenever an analysis completes.
    /// </summary>
    internal sealed class SlocToolWindowControl : UserControl
    {
        private readonly TextBlock _headline = new TextBlock();
        private readonly StackPanel _rows = new StackPanel();
        private readonly TextBlock _hint = new TextBlock();

        public SlocToolWindowControl()
        {
            var root = new StackPanel { Margin = new Thickness(12) };

            _headline.FontSize = 22;
            _headline.FontWeight = FontWeights.Bold;
            _headline.Margin = new Thickness(0, 0, 0, 8);
            root.Children.Add(_headline);

            root.Children.Add(_rows);

            _hint.Opacity = 0.7;
            _hint.TextWrapping = TextWrapping.Wrap;
            _hint.Margin = new Thickness(0, 12, 0, 0);
            root.Children.Add(_hint);

            Content = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = root,
            };

            Render(SlocState.LastMetrics);

            SlocState.MetricsUpdated += OnMetricsUpdated;
            Unloaded += (s, e) => SlocState.MetricsUpdated -= OnMetricsUpdated;
        }

        private void OnMetricsUpdated(object sender, EventArgs e)
        {
            // Publish is raised on the UI thread, but marshal defensively.
            Dispatcher.Invoke(() => Render(SlocState.LastMetrics));
        }

        private void Render(PlainMetrics metrics)
        {
            _rows.Children.Clear();
            if (metrics == null)
            {
                _headline.Text = "No analysis yet";
                _hint.Text = "Run \"Analyze Solution\" from the Tools > Oxide SLOC menu, "
                    + "or right-click a file or project and choose \"Analyze with oxide-sloc\".";
                return;
            }

            long code = metrics.CodeLines ?? 0;
            _headline.Text = Compact(code) + " SLOC";

            AddRow("Code lines", metrics.CodeLines);
            AddRow("Comment lines", metrics.CommentLines);
            AddRow("Blank lines", metrics.BlankLines);
            AddRow("Files analyzed", metrics.FilesAnalyzed);
            AddRow("Complexity", metrics.Complexity);
            AddRow("Unit tests", metrics.UnitTests);
            if (metrics.WarningCount > 0)
            {
                AddRow("Warnings", metrics.WarningCount);
            }

            _hint.Text = SlocState.LastReportHtml != null
                ? "Open the full report from Tools > Oxide SLOC > Open HTML Report."
                : string.Empty;
        }

        private void AddRow(string label, long? value)
        {
            if (value == null)
            {
                return;
            }
            var row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var name = new TextBlock { Text = label, Margin = new Thickness(0, 2, 8, 2), Opacity = 0.85 };
            var val = new TextBlock
            {
                Text = value.Value.ToString("N0", CultureInfo.InvariantCulture),
                Margin = new Thickness(0, 2, 0, 2),
                FontWeight = FontWeights.SemiBold,
            };
            Grid.SetColumn(name, 0);
            Grid.SetColumn(val, 1);
            row.Children.Add(name);
            row.Children.Add(val);
            _rows.Children.Add(row);
        }

        /// <summary>Compact number formatting matching the oxide-sloc UI convention.</summary>
        private static string Compact(long n)
        {
            double a = Math.Abs((double)n);
            if (a >= 1e6)
            {
                return StripZero((n / 1e6).ToString("0.0", CultureInfo.InvariantCulture)) + "M";
            }
            if (a >= 1e4)
            {
                return StripZero((n / 1e3).ToString("0.0", CultureInfo.InvariantCulture)) + "K";
            }
            return n.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string StripZero(string s) => s.EndsWith(".0") ? s.Substring(0, s.Length - 2) : s;
    }
}
