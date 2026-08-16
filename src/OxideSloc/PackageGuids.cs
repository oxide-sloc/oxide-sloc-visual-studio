using System;

namespace OxideSloc
{
    /// <summary>
    /// GUIDs and command IDs shared between the package code and the .vsct command table.
    /// Keep these in sync with OxideSlocPackage.vsct.
    /// </summary>
    internal static class PackageGuids
    {
        public const string PackageGuidString = "8f3a1c2e-5b6d-4e7f-9a0b-1c2d3e4f5a6b";
        public const string CommandSetGuidString = "2b4d6f8a-0c1e-4a3b-8c5d-7e9f0a1b2c3d";
        public const string ToolWindowGuidString = "4c6e8a0b-2d3f-4b5c-9d6e-8f0a1b2c3d4e";

        public static readonly Guid CommandSet = new Guid(CommandSetGuidString);
    }

    /// <summary>Command IDs, matching the &lt;Button&gt; ids in the .vsct file.</summary>
    internal static class PackageIds
    {
        public const int AnalyzeSolutionCommand = 0x0100;
        public const int AnalyzeItemCommand = 0x0101;
        public const int OpenReportCommand = 0x0102;
        public const int ServeCommand = 0x0103;
        public const int ShowToolWindowCommand = 0x0104;
    }
}
