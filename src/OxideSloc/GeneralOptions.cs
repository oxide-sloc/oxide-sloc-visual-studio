using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace OxideSloc
{
    /// <summary>
    /// Tools &gt; Options &gt; Oxide SLOC page. Reachable in code via
    /// <c>(GeneralOptions)package.GetDialogPage(typeof(GeneralOptions))</c>.
    /// </summary>
    public sealed class GeneralOptions : DialogPage
    {
        [Category("Oxide SLOC")]
        [DisplayName("Binary path")]
        [Description("Path to the oxide-sloc executable. Leave empty to use the SLOC_BIN / OXIDE_SLOC environment variables, then oxide-sloc on PATH.")]
        public string BinaryPath { get; set; } = string.Empty;

        [Category("Oxide SLOC")]
        [DisplayName("Extra analyze flags")]
        [Description("Extra flags appended to `oxide-sloc analyze`, separated by spaces (for example: --per-file --activity-window 90).")]
        public string ExtraArgs { get; set; } = string.Empty;

        [Category("Oxide SLOC")]
        [DisplayName("Web UI port")]
        [Description("Port the oxide-sloc web UI listens on when started from Visual Studio.")]
        public int ServePort { get; set; } = 4317;
    }
}
