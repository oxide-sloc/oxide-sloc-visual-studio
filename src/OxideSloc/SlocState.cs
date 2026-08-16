using System;

namespace OxideSloc
{
    /// <summary>
    /// Process-wide state shared between the analyze commands and the tool window:
    /// the most recent report path and metrics. The tool window subscribes to
    /// <see cref="MetricsUpdated"/> to refresh itself.
    /// </summary>
    internal static class SlocState
    {
        public static string LastReportHtml { get; private set; }
        public static PlainMetrics LastMetrics { get; private set; }

        /// <summary>Raised on the UI thread whenever a new analysis completes.</summary>
        public static event EventHandler MetricsUpdated;

        public static void Publish(PlainMetrics metrics, string reportHtml)
        {
            LastMetrics = metrics;
            if (!string.IsNullOrEmpty(reportHtml))
            {
                LastReportHtml = reportHtml;
            }
            MetricsUpdated?.Invoke(null, EventArgs.Empty);
        }
    }
}
