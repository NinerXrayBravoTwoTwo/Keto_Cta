using MineReports;

namespace KetoCtaTest;

public class DustQueryFilterTests
{
    private static readonly (string Title, string Set, double? P)[] Sample =
    [
        ("Cac1 vs. Cac0", "Zeta", 0.01),
        ("Cac1 vs. Cac0", "Eta", 0.2),
        ("Cac1 vs. Cac0", "BetaUZeta", 0.04),
        ("Ln Zeta ratio vs. Cac0", "Eta", 0.001),
        ("Cac0/Cac1 vs. Ncpv0/Ncpv1", "Gamma", null),
        ("DCac vs. RankA", "Omega", 0)
    ];

    [Fact]
    public void SearchMatchesTheTitleAndNotTheSetName()
    {
        var hits = Apply(search: "Zeta");

        Assert.Equal(["Ln Zeta ratio vs. Cac0"], hits.Select(row => row.Title));
    }

    [Fact]
    public void SetFilterIsExact()
    {
        var hits = Apply(sets: ["Zeta"]);

        Assert.Single(hits);
        Assert.Equal("Zeta", hits[0].Set);
    }

    [Fact]
    public void MaxPKeepsPerfectFitsAndDropsInvalidAndWeakerOnes()
    {
        var hits = Apply(maxP: 0.05);

        Assert.Equal(["Omega", "Eta", "Zeta", "BetaUZeta"], hits.Select(row => row.Set).ToArray());
        Assert.Equal([0, 0.001, 0.01, 0.04], hits.Select(row => row.P).ToArray());
    }

    [Fact]
    public void LimitKeepsTheMostSignificantRows()
    {
        var hits = Apply(limit: 2);

        Assert.Equal(2, hits.Count);
        Assert.Equal(0, hits[0].P);
        Assert.Equal(0.001, hits[1].P);
    }

    [Fact]
    public void EmptySetListMatchesNothing()
    {
        var hits = Apply(sets: []);

        Assert.Empty(hits);
    }

    private static IReadOnlyList<(string Title, string Set, double? P)> Apply(
        string? search = null,
        int limit = 100,
        double? maxP = null,
        IReadOnlyCollection<string>? sets = null)
    {
        return DustQueryFilter.Apply(
            Sample,
            search,
            limit,
            maxP,
            sets,
            row => row.Title,
            row => row.Set,
            row => row.P);
    }
}
