using System.Text.RegularExpressions;
using DataMiner;
using Keto_Cta;
using LinearRegression;

namespace MineReports;

/// <summary>
/// One regression the way the console <c>title</c> command prints it:
/// <see cref="GoldMiner.AuDust"/> for the set, then that dust's points and fit.
/// </summary>
public static class ScatterSeries
{
    public const int BandSteps = 50;

    public static ScatterResult Build(GoldMiner miner, string title, string setName, double level = 0.95)
    {
        ArgumentNullException.ThrowIfNull(miner);
        var chartTitle = (title ?? "").Trim();
        if (chartTitle.Length == 0)
            throw new ArgumentException("Enter a chart title, for example LnNcpv1 vs. LnCac0.");
        if (!double.IsFinite(level) || level <= 0 || level >= 1)
            throw new ArgumentException("Confidence level must be between 0 and 1.");

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
        // A fresh regression from the plotted points. Add/Dec would change the sums and leave DataPoints behind.
        var fromPoints = new RegressionPvalue(points.Select(point => (point.X, point.Y)).ToList());
        var (meanBand, predictionBand) = Intervals(fromPoints, level);

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
                Finite(interval.StandardError)),
            level,
            meanBand,
            predictionBand);
    }

    /// <summary>
    /// Mean-response and prediction bands at <paramref name="level"/>, sampled at
    /// <see cref="BandSteps"/> x values from min to max. A NaN from the library drops that band.
    /// </summary>
    public static (IReadOnlyList<ScatterBandPoint>? Mean, IReadOnlyList<ScatterBandPoint>? Prediction) Intervals(
        RegressionPvalue regression, double level)
    {
        ArgumentNullException.ThrowIfNull(regression);
        var xs = regression.DataPoints.Select(point => point.x).ToArray();
        if (xs.Length < 2)
            return (null, null);

        var min = xs.Min();
        var max = xs.Max();
        var mean = new ScatterBandPoint[BandSteps];
        var prediction = new ScatterBandPoint[BandSteps];
        var meanOk = true;
        var predictionOk = true;
        for (var i = 0; i < BandSteps; i++)
        {
            var x = min + (max - min) * i / (BandSteps - 1d);
            var meanInterval = regression.MeanResponseInterval(x, level);
            var predictionInterval = regression.PredictionInterval(x, level);
            if (!IsFinite(meanInterval))
                meanOk = false;
            else
                mean[i] = new ScatterBandPoint(x, meanInterval.Fit, meanInterval.Lower, meanInterval.Upper);
            if (!IsFinite(predictionInterval))
                predictionOk = false;
            else
                prediction[i] = new ScatterBandPoint(x, predictionInterval.Fit, predictionInterval.Lower, predictionInterval.Upper);
        }

        return (meanOk ? mean : null, predictionOk ? prediction : null);
    }

    private static bool IsFinite((double Fit, double Lower, double Upper) interval) =>
        double.IsFinite(interval.Fit) && double.IsFinite(interval.Lower) && double.IsFinite(interval.Upper);

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

public sealed record ScatterBandPoint(double X, double Fit, double Lower, double Upper);

public sealed record ScatterResult(
    string Title,
    string Set,
    string XLabel,
    string YLabel,
    IReadOnlyList<ScatterPoint> Points,
    ScatterFit Fit,
    double Level,
    IReadOnlyList<ScatterBandPoint>? MeanBand,
    IReadOnlyList<ScatterBandPoint>? PredictionBand);
