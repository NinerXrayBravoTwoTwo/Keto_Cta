using MineReports;

namespace KetoCtaTest;

public class Log10PHistogramTests
{
    [Fact]
    public void EmptyInputHasFiftyEmptyBins()
    {
        var summary = Log10PHistogram.Summarize([]);

        Assert.Equal(50, summary.Counts.Count);
        Assert.Equal(0.2, summary.BinWidth);
        Assert.Equal(10, summary.AxisMax);
        Assert.All(summary.Counts, count => Assert.Equal(0, count));
        Assert.Equal(0, summary.Overflow);
        Assert.Equal(0, summary.Perfect);
        Assert.Equal(0, summary.Invalid);
        Assert.Empty(summary.NegLog10Descending);
    }

    [Fact]
    public void ReferenceLevelsFallInTheExpectedBins()
    {
        // −log10(0.05) ≈ 1.301, −log10(0.2) ≈ 0.699, −log10(1) = 0.
        var summary = Log10PHistogram.Summarize([0.05, 0.2, 1]);

        Assert.Equal(1, summary.Counts[Log10PHistogram.BinOfScore(-Math.Log10(0.05))]);
        Assert.Equal(1, summary.Counts[Log10PHistogram.BinOfScore(-Math.Log10(0.2))]);
        Assert.Equal(1, summary.Counts[0]);
        Assert.Equal(3, summary.NegLog10Descending.Count);
        Assert.True(summary.NegLog10Descending[0] > summary.NegLog10Descending[1]);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.2, 1)]
    [InlineData(1.301, 6)]
    [InlineData(0.699, 3)]
    [InlineData(9.8, 49)]
    [InlineData(9.999, 49)]
    public void BinEdgesAreHalfOpenIntervalsOfWidthPointTwo(double score, int expectedBin)
    {
        Assert.Equal(expectedBin, Log10PHistogram.BinOfScore(score));
    }

    [Fact]
    public void TailPerfectAndInvalidAreKeptOffTheBars()
    {
        var tiny = Math.Pow(10, -10); // −log10 = 10 exactly, on the open tail
        var summary = Log10PHistogram.Summarize([tiny, 0, double.NaN, -0.1, 1.5, double.PositiveInfinity]);

        Assert.Equal(1, summary.Overflow);
        Assert.Equal(1, summary.Perfect);
        Assert.Equal(4, summary.Invalid);
        Assert.All(summary.Counts, count => Assert.Equal(0, count));
        Assert.Single(summary.NegLog10Descending);
        Assert.Equal(10, summary.NegLog10Descending[0], 6);
    }
}
