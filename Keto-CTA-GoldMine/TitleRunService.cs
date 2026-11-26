using System.Diagnostics;
using DataMiner;
using MineReports;

namespace Keto_CTA_GoldMine;

public sealed class TitleRunService(IWebHostEnvironment environment)
{
    public const int MaxTitles = 2000;

    public string[] CoolTitles()
    {
        var (quant, qangio) = ResolveData();
        var miner = new GoldMiner(quant, qangio);
        return new MineRegressionsWithGold(miner).CoolMatrix();
    }

    public RunResponse Run(IReadOnlyList<string> titles)
    {
        if (titles.Count == 0)
            throw new ArgumentException("Paste at least one regression title.");
        if (titles.Count > MaxTitles)
            throw new ArgumentException($"Refusing {titles.Count} titles. The page cap is {MaxTitles}. Full matrix and mine sweeps stay on the console.");

        var (quant, qangio) = ResolveData();
        var miner = new GoldMiner(quant, qangio);
        var names = new RegressionNamesProcessor(miner, miner.RegressionNameQueue);
        var clock = Stopwatch.StartNew();
        try
        {
            foreach (var title in titles)
                miner.RegressionNameQueue.Enqueue(title);

            var deadline = DateTime.UtcNow + TimeSpan.FromMinutes(2);
            while (names.Completed < titles.Count)
            {
                if (DateTime.UtcNow > deadline)
                    throw new TimeoutException("The run did not finish within 2 minutes.");
                Thread.Sleep(50);
            }

            var dusts = new List<Dust>();
            while (miner.DustQueue.TryDequeue(out var dust))
                dusts.Add(dust);

            var summary = Log10PHistogram.Summarize(dusts.Select(d => d.Regression.PValue));
            var sets = Log10PHistogram.SummarizeBySet(dusts.Select(d => (d.SetName.ToString(), d.Regression.PValue)))
                .Select(set => new SetHistogramRow(
                    set.Name,
                    set.Summary.Counts.Sum() + set.Summary.Overflow + set.Summary.Perfect + set.Summary.Invalid,
                    set.Summary.Counts,
                    set.Summary.Decades,
                    set.Summary.Overflow,
                    set.Summary.Perfect,
                    set.Summary.Invalid,
                    set.Summary.NegLog10Descending))
                .ToList();
            var rows = dusts
                .Select(ToRow)
                .OrderByDescending(row => row.NegLog10 ?? double.NegativeInfinity)
                .ToList();

            return new RunResponse(
                titles.Count,
                (int)clock.ElapsedMilliseconds,
                summary.Perfect,
                summary.Invalid,
                summary.Overflow,
                summary.BinWidth,
                summary.AxisMax,
                summary.Counts,
                summary.NegLog10Descending,
                sets,
                rows);
        }
        finally
        {
            names.Stop(writeReport: false);
        }
    }

    private static DustRow ToRow(Dust dust)
    {
        var p = dust.Regression.PValue;
        double? score = double.IsNaN(p) || double.IsInfinity(p) || p <= 0 || p > 1
            ? null
            : -Math.Log10(p);
        return new DustRow(
            dust.RegressionName,
            dust.SetName.ToString(),
            (int)dust.Regression.N,
            Finite(dust.Regression.Slope),
            Finite(dust.Regression.RSquared),
            Finite(p),
            score);
    }

    private static double? Finite(double value) => double.IsFinite(value) ? value : null;

    private (string Quant, string Qangio) ResolveData()
    {
        return (Find("keto-cta-quant-and-semi-quant.csv"), Find("keto-cta-qangio.csv"));
    }

    private string Find(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Data", fileName),
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "KetoCtaRegressions", "TestData", fileName))
        };
        var found = candidates.FirstOrDefault(File.Exists);
        if (found is null)
            throw new FileNotFoundException($"Dataset file '{fileName}' was not next to the app or in KetoCtaRegressions/TestData.");
        return found;
    }
}

public sealed record RunResponse(
    int Titles,
    int ElapsedMs,
    int Perfect,
    int Invalid,
    int Overflow,
    double BinWidth,
    double AxisMax,
    IReadOnlyList<int> Bins,
    IReadOnlyList<double> Curve,
    IReadOnlyList<SetHistogramRow> Sets,
    IReadOnlyList<DustRow> Dusts);

public sealed record SetHistogramRow(
    string Name,
    int Dusts,
    IReadOnlyList<int> Bins,
    IReadOnlyList<int> Decades,
    int Overflow,
    int Perfect,
    int Invalid,
    IReadOnlyList<double> Curve);

public sealed record DustRow(
    string Title,
    string Set,
    int N,
    double? Slope,
    double? RSquared,
    double? P,
    double? NegLog10);
