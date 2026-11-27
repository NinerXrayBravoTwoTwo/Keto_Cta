using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DataMiner;
using Keto_Cta;
using LinearRegression;
using MineReports;

namespace KetoCtaTest;

public class ScatterSeriesTests
{
    private static readonly string Quant = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "KetoCtaRegressions", "TestData", "keto-cta-quant-and-semi-quant.csv"));

    private static readonly string Qangio = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "KetoCtaRegressions", "TestData", "keto-cta-qangio.csv"));

    [Fact]
    public void FitAndPointsMatchTheDustPipeline()
    {
        var miner = new GoldMiner(Quant, Qangio);
        const string title = "LnNcpv1 vs. LnCac0";
        var dust = miner.GoldDust(title).Single(item => item.SetName == SetName.Beta);
        var series = ScatterSeries.Build(miner, title, "Beta");

        Assert.Equal(dust.Regression.Slope, series.Fit.Slope);
        Assert.Equal(dust.Regression.YIntercept, series.Fit.Intercept);
        Assert.Equal(dust.Regression.RSquared, series.Fit.RSquared);
        Assert.Equal(dust.Regression.PValue, series.Fit.P);
        Assert.Equal((int)dust.Regression.N, series.Fit.N);
        Assert.Equal(dust.Regression.ConfidenceIntervalPlus().StandardError, series.Fit.SlopeStandardError);
        Assert.Equal("LnNcpv1", series.YLabel);
        Assert.Equal("LnCac0", series.XLabel);
        Assert.Equal(dust.Regression.DataPoints.Select(point => point.x), series.Points.Select(point => point.X));
        Assert.Equal(dust.Regression.DataPoints.Select(point => point.y), series.Points.Select(point => point.Y));
        Assert.Equal(dust.Regression.IdPoints, series.Points.Select(point => point.Id));
        Assert.Equal(series.Fit.N, series.Points.Count);
    }

    [Fact]
    public void ZetaMatchesItsOwnDustAndIsNotBeta()
    {
        var miner = new GoldMiner(Quant, Qangio);
        const string title = "LnNcpv1 vs. LnCac0";
        var beta = ScatterSeries.Build(miner, title, "Beta");
        var zeta = ScatterSeries.Build(miner, title, "zeta");
        var dust = miner.AuDust(SetName.Zeta, title);

        Assert.NotNull(dust);
        Assert.Equal(dust.Regression.Slope, zeta.Fit.Slope);
        Assert.Equal(dust.Regression.RSquared, zeta.Fit.RSquared);
        Assert.Equal(dust.Regression.PValue, zeta.Fit.P);
        Assert.NotEqual(beta.Fit.N, zeta.Fit.N);
    }

    [Fact]
    public void UnknownSetAndBadTitleAreRefused()
    {
        var miner = new GoldMiner(Quant, Qangio);
        var unknown = Assert.Throws<ArgumentException>(() => ScatterSeries.Build(miner, "LnNcpv1 vs. LnCac0", "Nope"));
        var badTitle = Assert.Throws<ArgumentException>(() => ScatterSeries.Build(miner, "not a title", "Omega"));

        Assert.Contains("Unknown set", unknown.Message);
        Assert.Contains("No regression", badTitle.Message);
    }

    [Fact]
    public void BetaBandMatchesTheFormulaAndIsNarrowestAtTheMean()
    {
        var miner = new GoldMiner(Quant, Qangio);
        var series = ScatterSeries.Build(miner, "LnNcpv1 vs. LnCac0", "Beta");
        Assert.Equal(0.95, series.Level);
        Assert.NotNull(series.MeanBand);
        Assert.NotNull(series.PredictionBand);
        Assert.Equal(ScatterSeries.BandSteps, series.MeanBand.Count);
        Assert.Equal(40, series.Points.Count);

        var xs = series.Points.Select(point => point.X).ToArray();
        var ys = series.Points.Select(point => point.Y).ToArray();
        var n = xs.Length;
        var xBar = xs.Average();
        var yBar = ys.Average();
        var sxx = xs.Sum(x => (x - xBar) * (x - xBar));
        var sxy = 0.0;
        var rss = 0.0;
        for (var i = 0; i < n; i++)
            sxy += (xs[i] - xBar) * (ys[i] - yBar);
        var slope = sxy / sxx;
        var intercept = yBar - slope * xBar;
        for (var i = 0; i < n; i++)
        {
            var residual = ys[i] - (intercept + slope * xs[i]);
            rss += residual * residual;
        }

        var s = Math.Sqrt(rss / (n - 2));
        const double tCrit = 2.0243941639119694; // scipy.stats.t.ppf(0.975, 38)

        var nearest = series.MeanBand.MinBy(row => Math.Abs(row.X - xBar))!;
        foreach (var row in new[] { series.MeanBand[0], nearest, series.MeanBand[^1] })
            AssertBand(row, MeanHalfWidth(row.X));

        var predNearest = series.PredictionBand.MinBy(row => Math.Abs(row.X - xBar))!;
        foreach (var row in new[] { series.PredictionBand[0], predNearest, series.PredictionBand[^1] })
            AssertBand(row, PredictionHalfWidth(row.X));

        double Width(ScatterBandPoint row) => row.Upper - row.Lower;
        var narrowest = series.MeanBand.MinBy(Width)!;
        var step = (series.MeanBand[^1].X - series.MeanBand[0].X) / (ScatterSeries.BandSteps - 1);
        Assert.True(Math.Abs(narrowest.X - xBar) <= step / 2 + 1e-9);
        Assert.True(Width(series.MeanBand[0]) > Width(narrowest));
        Assert.True(Width(series.MeanBand[^1]) > Width(narrowest));
        Assert.True(Width(predNearest) > Width(nearest));

        CompareWithStatsModels(series, nearest.X);

        void AssertBand(ScatterBandPoint row, double halfWidth)
        {
            var fit = intercept + slope * row.X;
            Assert.Equal(fit, row.Fit, 8);
            Assert.Equal(fit - halfWidth, row.Lower, 8);
            Assert.Equal(fit + halfWidth, row.Upper, 8);
        }

        double MeanHalfWidth(double x) => tCrit * s * Math.Sqrt(1.0 / n + (x - xBar) * (x - xBar) / sxx);
        double PredictionHalfWidth(double x) => tCrit * s * Math.Sqrt(1 + 1.0 / n + (x - xBar) * (x - xBar) / sxx);
    }

    [Fact]
    public void BandIsNullWhenXDoesNotVary()
    {
        var regression = new RegressionPvalue(new List<(double x, double y)> { (1, 2), (1, 3), (1, 5) });
        var (mean, prediction) = ScatterSeries.Intervals(regression, 0.95);
        Assert.Null(mean);
        Assert.Null(prediction);
    }

    [Fact]
    public void LevelOutsideZeroToOneIsRefused()
    {
        var miner = new GoldMiner(Quant, Qangio);
        var error = Assert.Throws<ArgumentException>(() => ScatterSeries.Build(miner, "LnNcpv1 vs. LnCac0", "Beta", 1.2));
        Assert.Contains("between 0 and 1", error.Message);
    }

    private static void CompareWithStatsModels(ScatterResult series, double xAtMean)
    {
        var probe = Process.Start(new ProcessStartInfo("python3", "-c \"import statsmodels\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });
        probe!.WaitForExit();
        if (probe.ExitCode != 0)
            return;

        var xs = new[] { series.MeanBand![0].X, xAtMean, series.MeanBand[^1].X };
        var script = """
            import json, sys
            import numpy as np
            import statsmodels.api as sm
            rows = [line.split(",") for line in sys.stdin.read().splitlines() if line]
            x = np.array([float(row[0]) for row in rows])
            y = np.array([float(row[1]) for row in rows])
            model = sm.OLS(y, sm.add_constant(x)).fit()
            out = []
            for raw in sys.argv[1:]:
                frame = model.get_prediction([[1.0, float(raw)]]).summary_frame(alpha=0.05)
                out.append({
                    "mean": float(frame["mean"].iloc[0]),
                    "mean_lo": float(frame["mean_ci_lower"].iloc[0]),
                    "mean_hi": float(frame["mean_ci_upper"].iloc[0]),
                    "obs_lo": float(frame["obs_ci_lower"].iloc[0]),
                    "obs_hi": float(frame["obs_ci_upper"].iloc[0]),
                })
            print(json.dumps(out))
            """;
        var start = new ProcessStartInfo("python3")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add(script);
        foreach (var x in xs)
            start.ArgumentList.Add(x.ToString("R"));
        using var process = Process.Start(start)!;
        process.StandardInput.Write(string.Join("\n", series.Points.Select(point => $"{point.X.ToString("R")},{point.Y.ToString("R")}")));
        process.StandardInput.Close();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Assert.True(process.ExitCode == 0, error);
        var frames = JsonSerializer.Deserialize<List<StatsFrame>>(output)!;
        for (var i = 0; i < xs.Length; i++)
        {
            var mean = series.MeanBand.Single(row => row.X == xs[i]);
            var prediction = series.PredictionBand!.Single(row => row.X == xs[i]);
            Assert.Equal(frames[i].Mean, mean.Fit, 6);
            Assert.Equal(frames[i].MeanLo, mean.Lower, 6);
            Assert.Equal(frames[i].MeanHi, mean.Upper, 6);
            Assert.Equal(frames[i].ObsLo, prediction.Lower, 6);
            Assert.Equal(frames[i].ObsHi, prediction.Upper, 6);
        }
    }

    private sealed record StatsFrame(
        [property: JsonPropertyName("mean")] double Mean,
        [property: JsonPropertyName("mean_lo")] double MeanLo,
        [property: JsonPropertyName("mean_hi")] double MeanHi,
        [property: JsonPropertyName("obs_lo")] double ObsLo,
        [property: JsonPropertyName("obs_hi")] double ObsHi);
}
