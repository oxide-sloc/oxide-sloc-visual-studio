using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace OxideSloc
{
    /// <summary>Dockable tool window that displays the latest oxide-sloc metrics.</summary>
    [Guid(PackageGuids.ToolWindowGuidString)]
    public sealed class SlocToolWindow : ToolWindowPane
    {
        public SlocToolWindow() : base(null)
        {
            Caption = "Oxide SLOC";
            Content = new SlocToolWindowControl();
        }
    }
}
