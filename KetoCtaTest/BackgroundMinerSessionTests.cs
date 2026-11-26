using MineReports;

namespace KetoCtaTest;

public class BackgroundMinerSessionTests
{
    private static readonly string Quant = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "KetoCtaRegressions", "TestData", "keto-cta-quant-and-semi-quant.csv"));

    private static readonly string Qangio = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "KetoCtaRegressions", "TestData", "keto-cta-qangio.csv"));

    [Fact]
    public void ClearStopsWorkersAndDustsStayAtZero()
    {
        using var session = new BackgroundMinerSession(Quant, Qangio, maxTitles: 50);
        session.Start();
        session.Enqueue(["Cac1 vs. Cac0"]);
        WaitUntil(session, status => status.Dusts > 0);

        session.Clear();
        var cleared = session.Status();

        Assert.False(cleared.Running);
        Assert.Equal(0, cleared.Dusts);
        Assert.Equal(0, cleared.TitlesQueued);
        Assert.Equal(0, cleared.TitlesDone);

        Thread.Sleep(300);
        var later = session.Status();
        Assert.False(later.Running);
        Assert.Equal(0, later.Dusts);
        Assert.Equal(0, later.TitlesQueued);
    }

    [Fact]
    public void StopThenStartResumesTheQueue()
    {
        using var session = new BackgroundMinerSession(Quant, Qangio, maxTitles: 50);
        session.Start();
        session.Enqueue(["LnNcpv1 vs. LnCac0"]);
        WaitUntil(session, status => status.TitlesDone >= 1 && status.TitlesQueued == 0);

        session.Stop();
        var stopped = session.Status();
        Assert.False(stopped.Running);
        var dusts = stopped.Dusts;

        session.Enqueue(["Cac0 vs. Ncpv1"]);
        Assert.Equal(1, session.Status().TitlesQueued);
        Assert.Equal(dusts, session.Status().Dusts);

        session.Start();
        WaitUntil(session, status => status.TitlesDone >= 2 && status.Dusts > dusts);
        Assert.True(session.Status().Running);
    }

    private static void WaitUntil(BackgroundMinerSession session, Func<MinerStatus, bool> done)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(20);
        while (DateTime.UtcNow < deadline)
        {
            if (done(session.Status()))
                return;
            Thread.Sleep(30);
        }

        var status = session.Status();
        throw new TimeoutException(
            $"Timed out. running={status.Running} queued={status.TitlesQueued} done={status.TitlesDone} dusts={status.Dusts}");
    }
}
