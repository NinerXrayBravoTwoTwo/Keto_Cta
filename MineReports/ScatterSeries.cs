using System.Text.RegularExpressions;
using DataMiner;
using Keto_Cta;

namespace MineReports;

/// <summary>
/// One regression the way the console <c>title</c> command prints it:
/// <see cref="GoldMiner.AuDust"/> for the set, then that dust's points and fit.
/// </summary>
public static class ScatterSeries
{
    public static ScatterResult Build(GoldMiner miner, string title, string setName)
    {
        ArgumentNullException.ThrowIfNull(miner);
        var chartTitle = (title ?? "").Trim();
        if (chartTitle.Length == 0)
            throw new ArgumentException("Enter a chart title, for example LnNcpv1 vs. LnCac0.");

        if (!Enum.TryParse<SetName>((setName ?? "").Trim(), ignoreCase: true, out var set))
            throw new ArgumentException($"Unknown set '{setName}'. Use {string.Join(", ", Enum.GetNames<SetName>())}.");

        Dust? dust;
        try
        {
            dust = miner.AuDust(set, chartTitle);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"No regression for '{chartTitle}' in {set}. {ex.Message}", ex);
        }

        if (dust is null)
            throw new ArgumentException($"No regression for '{chartTitle}' in {set}. The title needs the form 'Y vs. X' and at least 3 finite points.");

        var regression = dust.Regression;
        var ids = regression.IdPoints.ToArray();
        var points = regression.DataPoints
            .Select((point, index) => new ScatterPoint(index < ids.Length ? ids[index] : "", point.x, point.y))
            .ToArray();
        var labels = AxisLabels(chartTitle);
        var interval = regression.ConfidenceIntervalPlus();

        return new ScatterResult(
            chartTitle,
            set.ToString(),
            labels.X,
            labels.Y,
            points,
            new ScatterFit(
                Finite(regression.Slope),
                Finite(regression.YIntercept),
                Finite(regression.RSquared),
                Finite(regression.PValue),
                (int)regression.N,
                Finite(interval.StandardError)));
    }

    public static (string Y, string X) AxisLabels(string title)
    {
        var parts = Regex.Split(title.Trim(), @"\s+vs\.?\s+", RegexOptions.IgnoreCase);
        if (parts.Length < 2)
            return (title.Trim(), "");
        return (parts[0].Trim(), parts[1].Trim());
    }

    private static double? Finite(double value) => double.IsFinite(value) ? value : null;
}

public sealed record ScatterPoint(string Id, double X, double Y);

public sealed record ScatterFit(
    double? Slope,
    double? Intercept,
    double? RSquared,
    double? P,
    int N,
    double? SlopeStandardError);

public sealed record ScatterResult(
    string Title,
    string Set,
    string XLabel,
    string YLabel,
    IReadOnlyList<ScatterPoint> Points,
    ScatterFit Fit);
