using DataMiner;

namespace MineReports;

/// <summary>
/// One GoldMiner and the console's name-queue worker, plus a pump that keeps every dust
/// (not only the interesting ones) so the page can draw while mining continues.
/// </summary>
public sealed class BackgroundMinerSession : IDisposable
{
    private readonly object _gate = new();
    private readonly List<Dust> _dusts = [];
    private readonly GoldMiner _miner;
    private RegressionNamesProcessor? _names;
    private CancellationTokenSource? _dustCts;
    private Task? _dustPump;
    private long _titlesFinished;
    private long _dropped;
    private bool _running;

    public BackgroundMinerSession(string quantPath, string qangioPath, int maxTitles = 20000)
    {
        if (maxTitles < 1)
            throw new ArgumentOutOfRangeException(nameof(maxTitles));
        MaxTitles = maxTitles;
        _miner = new GoldMiner(quantPath, qangioPath);
    }

    public int MaxTitles { get; }

    public bool IsRunning
    {
        get { lock (_gate) return _running; }
    }

    public string[] CoolTitles() => new MineRegressionsWithGold(_miner).CoolMatrix();

    public int Enqueue(IReadOnlyList<string> titles)
    {
        var clean = titles
            .Select(title => title.Trim())
            .Where(title => title.Length > 0)
            .ToArray();
        if (clean.Length == 0)
            throw new ArgumentException("Paste at least one regression title.");

        var queued = _miner.RegressionNameQueue.Count;
        if (clean.Length > MaxTitles || queued + clean.Length > MaxTitles)
            throw new ArgumentException(
                $"That would put {queued + clean.Length} titles in the queue. Mining:MaxTitles in appsettings.json is {MaxTitles}. Raise that value to enqueue more at once.");

        foreach (var title in clean)
            _miner.RegressionNameQueue.Enqueue(title);
        return clean.Length;
    }

    public void Start()
    {
        lock (_gate)
        {
            if (_running)
                return;

            // A large batch size skips the processor's periodic CSV flush. The web session
            // keeps dusts in memory and does not want processed_strings.csv in the project folder.
            _names = new RegressionNamesProcessor(_miner, _miner.RegressionNameQueue, batchSize: int.MaxValue);
            _dustCts = new CancellationTokenSource();
            _dustPump = Task.Run(() => PumpDusts(_dustCts.Token));
            _running = true;
        }
    }

    public void Stop()
    {
        RegressionNamesProcessor? names;
        CancellationTokenSource? dustCts;
        Task? dustPump;
        lock (_gate)
        {
            if (!_running)
                return;
            names = _names;
            dustCts = _dustCts;
            dustPump = _dustPump;
            _names = null;
            _dustCts = null;
            _dustPump = null;
            _running = false;
        }

        names?.Stop(writeReport: false);
        dustCts?.Cancel();
        try
        {
            dustPump?.Wait();
        }
        catch (AggregateException)
        {
            // The pump treats cancellation as a normal exit.
        }

        dustCts?.Dispose();
        lock (_gate)
        {
            _titlesFinished += names?.Completed ?? 0;
            _dropped += names?.DroppedBecauseQueueFull ?? 0;
        }
    }

    /// <summary>
    /// Stops the workers and waits for the current batch to finish, then drops stored dusts.
    /// A running batch cannot refill the list after this returns.
    /// </summary>
    public void Clear()
    {
        Stop();
        lock (_gate)
        {
            _dusts.Clear();
            _titlesFinished = 0;
            _dropped = 0;
        }

        _miner.Clear();
    }

    public MinerStatus Status()
    {
        long done;
        long dropped;
        int stored;
        bool running;
        lock (_gate)
        {
            done = _titlesFinished + (_names?.Completed ?? 0);
            dropped = _dropped + (_names?.DroppedBecauseQueueFull ?? 0);
            stored = _dusts.Count;
            running = _running;
        }

        var warning = dropped > 0
            ? $"{dropped} titles were skipped because the dust queue was already over 40,000. Those results were not stored."
            : null;
        return new MinerStatus(running, _miner.RegressionNameQueue.Count, done, stored, dropped, MaxTitles, warning);
    }

    public DustQueryResponse Query(string? search, int limit, double? maxP, IReadOnlyCollection<string>? sets)
    {
        List<Dust> snapshot;
        int stored;
        lock (_gate)
        {
            snapshot = _dusts.ToList();
            stored = snapshot.Count;
        }

        var views = snapshot.Select(ToView).ToList();
        var matched = DustQueryFilter.Apply(views, search, int.MaxValue, maxP, sets, v => v.Title, v => v.Set, v => v.P);
        var shown = DustQueryFilter.Apply(views, search, limit, maxP, sets, v => v.Title, v => v.Set, v => v.P);
        var summary = Log10PHistogram.Summarize(shown.Select(row => row.P ?? double.NaN));
        var setsRows = Log10PHistogram.SummarizeBySet(shown.Select(row => (row.Set, row.P ?? double.NaN)))
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

        return new DustQueryResponse(
            stored,
            matched.Count,
            shown.Count,
            summary.Perfect,
            summary.Invalid,
            summary.Overflow,
            summary.BinWidth,
            summary.AxisMax,
            summary.Counts,
            summary.NegLog10Descending,
            setsRows,
            shown.Select(row => new DustRow(row.Title, row.Set, row.N, row.Slope, row.RSquared, row.P, row.NegLog10)).ToList());
    }

    public void Dispose() => Stop();

    private void PumpDusts(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (!DrainOnce())
            {
                try
                {
                    Task.Delay(40, token).Wait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        DrainOnce();
    }

    private bool DrainOnce()
    {
        var any = false;
        while (_miner.DustQueue.TryDequeue(out var dust))
        {
            lock (_gate)
                _dusts.Add(dust);
            any = true;
        }

        return any;
    }

    private static DustView ToView(Dust dust)
    {
        var p = dust.Regression.PValue;
        double? finiteP = double.IsFinite(p) ? p : null;
        double? score = finiteP is > 0 and <= 1 ? -Math.Log10(finiteP.Value) : null;
        return new DustView(
            dust.RegressionName,
            dust.SetName.ToString(),
            (int)dust.Regression.N,
            double.IsFinite(dust.Regression.Slope) ? dust.Regression.Slope : null,
            double.IsFinite(dust.Regression.RSquared) ? dust.Regression.RSquared : null,
            finiteP,
            score);
    }
}

public sealed record MinerStatus(
    bool Running,
    int TitlesQueued,
    long TitlesDone,
    int Dusts,
    long Dropped,
    int MaxTitles,
    string? Warning);

public sealed record DustView(
    string Title,
    string Set,
    int N,
    double? Slope,
    double? RSquared,
    double? P,
    double? NegLog10);

public sealed record DustRow(
    string Title,
    string Set,
    int N,
    double? Slope,
    double? RSquared,
    double? P,
    double? NegLog10);

public sealed record SetHistogramRow(
    string Name,
    int Dusts,
    IReadOnlyList<int> Bins,
    IReadOnlyList<int> Decades,
    int Overflow,
    int Perfect,
    int Invalid,
    IReadOnlyList<double> Curve);

public sealed record DustQueryResponse(
    int Stored,
    int Matched,
    int Shown,
    int Perfect,
    int Invalid,
    int Overflow,
    double BinWidth,
    double AxisMax,
    IReadOnlyList<int> Bins,
    IReadOnlyList<double> Curve,
    IReadOnlyList<SetHistogramRow> Sets,
    IReadOnlyList<DustRow> Dusts);
