namespace MineReports;

/// <summary>
/// Bins slope p-values on a −log10 scale. Finite scores in [0, 10) use bins of width 0.2.
/// Scores at or above 10 are counted as overflow. p = 0 and non-probabilities are counted
/// separately and are not given a bar.
/// </summary>
public static class Log10PHistogram
{
    public const double BinWidth = 0.2;
    public const double AxisMax = 10.0;
    public const int BinCount = 50;

    public static Log10PSummary Summarize(IEnumerable<double> pValues)
    {
        ArgumentNullException.ThrowIfNull(pValues);

        var counts = new int[BinCount];
        var curve = new List<double>();
        var perfect = 0;
        var invalid = 0;
        var overflow = 0;

        foreach (var p in pValues)
        {
            if (double.IsNaN(p) || double.IsInfinity(p) || p < 0 || p > 1)
            {
                invalid++;
                continue;
            }

            if (p == 0)
            {
                perfect++;
                continue;
            }

            var score = -Math.Log10(p);
            var bin = BinOfScore(score);
            if (bin < 0)
                overflow++;
            else
                counts[bin]++;

            curve.Add(score);
        }

        curve.Sort(static (a, b) => b.CompareTo(a));
        return new Log10PSummary(counts, overflow, perfect, invalid, curve);
    }

    /// <summary>
    /// Bin index for a −log10 score, or −1 when the score belongs in the ≥10 tail.
    /// </summary>
    public static int BinOfScore(double score)
    {
        if (double.IsNaN(score) || score < 0)
            return 0;

        if (score >= AxisMax)
            return -1;

        var index = (int)Math.Floor((decimal)score / (decimal)BinWidth);
        if (index < 0)
            return 0;
        if (index >= BinCount)
            return -1;
        return index;
    }
}

public sealed class Log10PSummary(
    int[] counts,
    int overflow,
    int perfect,
    int invalid,
    IReadOnlyList<double> negLog10Descending)
{
    public double BinWidth => Log10PHistogram.BinWidth;
    public double AxisMax => Log10PHistogram.AxisMax;
    public IReadOnlyList<int> Counts { get; } = counts;
    public int Overflow { get; } = overflow;
    public int Perfect { get; } = perfect;
    public int Invalid { get; } = invalid;

    /// <summary>Finite −log10(p) values, most significant first. Includes the ≥10 tail. Excludes p = 0 and invalid p.</summary>
    public IReadOnlyList<double> NegLog10Descending { get; } = negLog10Descending;
}
