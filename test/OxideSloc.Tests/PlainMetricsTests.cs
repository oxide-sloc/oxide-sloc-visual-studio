using OxideSloc;
using Xunit;

namespace OxideSloc.Tests
{
    public class PlainMetricsTests
    {
        private const string Sample =
            "files_analyzed=8\n" +
            "code_lines=470\n" +
            "comment_lines=53\n" +
            "blank_lines=66\n" +
            "cyclomatic_complexity=12\n" +
            "unit_tests=0\n" +
            "warning_count=2\n" +
            "warning=first problem\n" +
            "warning=second problem\n";

        [Fact]
        public void Parse_extracts_totals()
        {
            var m = PlainMetrics.Parse(Sample);
            Assert.Equal(8, m.FilesAnalyzed);
            Assert.Equal(470, m.CodeLines);
            Assert.Equal(53, m.CommentLines);
            Assert.Equal(66, m.BlankLines);
            Assert.Equal(12, m.Complexity);
            Assert.Equal(0, m.UnitTests);
        }

        [Fact]
        public void Parse_collects_warnings_separately()
        {
            var m = PlainMetrics.Parse(Sample);
            Assert.Equal(2, m.Warnings.Count);
            Assert.Equal(2, m.WarningCount);
            Assert.Equal("first problem", m.Warnings[0]);
            Assert.Equal("second problem", m.Warnings[1]);
        }

        [Fact]
        public void Parse_is_resilient_to_blank_and_malformed_lines()
        {
            var m = PlainMetrics.Parse("code_lines=5\n\nnot-a-pair\r\n");
            Assert.Equal(5, m.CodeLines);
            Assert.Null(m.FilesAnalyzed);
        }

        [Fact]
        public void WarningCount_falls_back_to_line_count_when_absent()
        {
            var m = PlainMetrics.Parse("warning=only one\n");
            Assert.Equal(1, m.WarningCount);
        }

        [Fact]
        public void Parse_handles_empty_input()
        {
            var m = PlainMetrics.Parse("");
            Assert.Null(m.CodeLines);
            Assert.Equal(0, m.WarningCount);
        }
    }
}
