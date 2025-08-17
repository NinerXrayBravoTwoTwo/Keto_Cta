using DataMiner;
using Keto_Cta;

namespace MineReports;

public static class DustConfusion
{
    public static Dust[] EmptyDusts => [];

    public static Dust[] GetDusts(SetName setName, string? regressionName = null) =>
        Data.Dusts
            .Where(x => x.SetName == setName && (regressionName is null || x.RegressionName == regressionName))
            .ToArray();

    public static Dust[] GetInterestingDusts(SetName setName, string? regressionName = null) =>
        GetDusts(setName, regressionName).Where(x => x.IsInteresting).ToArray();

    public static Dust[] GetDustsByRegression(SetName setName, string? regressionName = null) =>
        GetDusts(setName, regressionName);

    public static IEnumerable<Dust> GetInterestingDustsWithRegressionName(SetName setName, string regressionName) =>
        GetDusts(setName, regressionName).Where(x => x.IsInteresting);

 }