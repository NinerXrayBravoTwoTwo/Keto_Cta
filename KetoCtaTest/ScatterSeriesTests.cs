using DataMiner;
using Keto_Cta;
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
}
