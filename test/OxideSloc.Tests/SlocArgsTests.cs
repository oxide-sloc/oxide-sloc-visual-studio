using System.Linq;
using OxideSloc;
using Xunit;

namespace OxideSloc.Tests
{
    public class SlocArgsTests
    {
        [Theory]
        [InlineData(0, "Analysis complete.")]
        [InlineData(2, "Warnings gate failed (--fail-on-warnings).")]
        [InlineData(3, "Code lines below threshold (--fail-below).")]
        [InlineData(4, "SLOC budget exceeded (--fail-on-budget).")]
        [InlineData(5, "Growth exceeded baseline (--fail-above-baseline).")]
        [InlineData(6, "Cyclomatic complexity exceeded (--max-complexity).")]
        public void DescribeExit_maps_known_codes(int code, string expected)
        {
            Assert.Equal(expected, SlocArgs.DescribeExit(code));
        }

        [Fact]
        public void DescribeExit_handles_unknown_code()
        {
            Assert.Contains("99", SlocArgs.DescribeExit(99));
            Assert.Contains("unknown", SlocArgs.DescribeExit(null));
        }

        [Theory]
        [InlineData(4, true)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(0, false)]
        [InlineData(2, false)]
        [InlineData(3, false)]
        public void IsError_flags_hard_failures(int code, bool expected)
        {
            Assert.Equal(expected, SlocArgs.IsError(code));
        }

        [Fact]
        public void AnalyzeArgs_always_has_analyze_plain_and_path()
        {
            var args = SlocArgs.AnalyzeArgs("C:/proj", null, null, null);
            Assert.Equal("analyze", args[0]);
            Assert.Contains("C:/proj", args);
            Assert.Contains("--plain", args);
        }

        [Fact]
        public void AnalyzeArgs_threads_outputs_and_extra_flags()
        {
            var args = SlocArgs.AnalyzeArgs("p", "a.json", "b.html", "--per-file --activity-window 90");
            Assert.Contains("--json-out", args);
            Assert.Contains("a.json", args);
            Assert.Contains("--html-out", args);
            Assert.Contains("b.html", args);
            Assert.Contains("--per-file", args);
            Assert.Contains("90", args);
        }

        [Fact]
        public void QuoteArgument_leaves_simple_tokens_untouched()
        {
            Assert.Equal("analyze", SlocArgs.QuoteArgument("analyze"));
            Assert.Equal("--plain", SlocArgs.QuoteArgument("--plain"));
        }

        [Fact]
        public void QuoteArgument_quotes_paths_with_spaces()
        {
            Assert.Equal("\"C:\\my code\\src\"", SlocArgs.QuoteArgument(@"C:\my code\src"));
        }

        [Fact]
        public void QuoteArgument_escapes_embedded_quotes()
        {
            // a"b -> "a\"b"
            Assert.Equal("\"a\\\"b\"", SlocArgs.QuoteArgument("a\"b"));
        }

        [Fact]
        public void BuildArguments_round_trips_via_split()
        {
            var argv = new[] { "analyze", @"C:\my code\src", "--plain" };
            string line = SlocArgs.BuildArguments(argv);
            Assert.Equal("analyze \"C:\\my code\\src\" --plain", line);
        }
    }
}
