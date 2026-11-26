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

    [Fact]
    public void EachSetKeepsItsOwnBinsAndInvalidCounts()
    {
        var rows = new (string Set, double P)[]
        {
            ("Omega", 0.05),
            ("Gamma", double.NaN),
            ("Gamma", 0),
            ("Zeta", Math.Pow(10, -5.5))
        };

        var bySet = Log10PHistogram.SummarizeBySet(rows);

        Assert.Equal(Log10PHistogram.SetOrder, bySet.Select(row => row.Name).ToArray());

        var omega = bySet.Single(row => row.Name == "Omega").Summary;
        var gamma = bySet.Single(row => row.Name == "Gamma").Summary;
        var zeta = bySet.Single(row => row.Name == "Zeta").Summary;
        var alpha = bySet.Single(row => row.Name == "Alpha").Summary;

        Assert.Equal(1, omega.Counts[Log10PHistogram.BinOfScore(-Math.Log10(0.05))]);
        Assert.Equal(0, omega.Invalid);
        Assert.Equal(0, omega.Perfect);
        Assert.Equal(1, omega.Decades[1]);

        Assert.Equal(1, gamma.Invalid);
        Assert.Equal(1, gamma.Perfect);
        Assert.All(gamma.Counts, count => Assert.Equal(0, count));
        Assert.Empty(gamma.NegLog10Descending);

        Assert.Equal(1, zeta.Decades[5]);
        Assert.Equal(0, zeta.Invalid);
        Assert.Equal(0, alpha.Counts.Sum());
        Assert.Equal(0, alpha.Invalid);
    }

    [Fact]
    public void UnknownSetIsAppendedAfterTheConsoleOrder()
    {
        var bySet = Log10PHistogram.SummarizeBySet([("Nope", 1)]);

        Assert.Equal(Log10PHistogram.SetOrder.Append("Nope"), bySet.Select(row => row.Name));
        Assert.Equal(1, bySet[^1].Summary.Counts[0]);
    }
}
