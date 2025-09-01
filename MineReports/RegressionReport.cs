using DataMiner;
using Keto_Cta;

namespace MineReports;

public enum RegressionReport
{
    PValue = 1,
    ConfInterval = 2,
    StandardError = 3,
    QValue = 4,
}

public class DustRegressionList(RegressionReport reportType)
{
    public IRegressionReport Report = reportType == RegressionReport.ConfInterval
        ? ListRegressionCi.CreateInstance()
        : ListRegressionBasic.CreateInstance();

    public RegressionReport ReportType = reportType;

    private IOrderedEnumerable<Dust> ApplySorting(IEnumerable<Dust> dusts)
    {
        return ReportType switch
        {
            //RegressionReport.StandardError => dusts.OrderBy( d => d.Regression.ConfidenceIntervalPlus().StandardError),
            RegressionReport.PValue => dusts.OrderByDescending(d => d.Regression.PValue),
            RegressionReport.ConfInterval => dusts
                .OrderByDescending(d => d.Regression.ConfidenceIntervalPlus().StandardError)
                .ThenByDescending(d => d.Regression.PValue),
            _ => dusts.OrderByDescending(d => d.Regression.PValue) // Default
        };
    }

    public string[] Build(IEnumerable<Dust>? dusts, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = ApplySorting(
            dusts.Where(d => !double.IsNaN(d.Regression.PValue)));

        return Report.ReportBuffer(orderedDusts).ToArray();
    }

    public string[] Build(IEnumerable<Dust>? dusts, int limit, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = ApplySorting(
            dusts.Where(d => !double.IsNaN(d.Regression.PValue)))
            .TakeLast(limit);

        return Report.ReportBuffer(orderedDusts).ToArray();
    }

    public string[] Build(IEnumerable<Dust>? dusts, Token depToken, Token regToken, int limit, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = ApplySorting(
            dusts.Where(d =>
                (!double.IsNaN(d.Regression.PValue) || !notNaN)
                && IsTokenMatch(d.DepToken, d.RegToken, depToken, regToken)))
            .TakeLast(limit);

        return Report.ReportBuffer(orderedDusts).ToArray();
    }

    public string[] Build(
        IEnumerable<Dust>? dusts,
        string[]? matchName,
        Token depToken, Token regToken,
        SetName[] setNames,
        int limit = 500, bool notNaN = false)
    {
        if (dusts == null) return [];

        var filteredDusts = dusts
            .Where(d =>
                (!double.IsNaN(d.Regression.PValue) || !notNaN)
                && IsTokenMatch(d.DepToken, d.RegToken, depToken, regToken)
                && (matchName == null || IsFilterMatch(d.RegressionName, matchName))
                && IsSetNameMatch(d.SetName, setNames));

        var orderedDusts = ApplySorting(filteredDusts)
            .TakeLast(limit);

        return Report.ReportBuffer(orderedDusts).ToArray();
    }

    private static bool IsSetNameMatch(SetName dustSetName, SetName[] setNames)
    {
        return setNames.Length == 0 || setNames.Contains(dustSetName);
    }

    private static bool IsFilterMatch(string thisTitle, string[] findMe)
    {
        if (findMe.Length == 0) return true;
        return findMe.Any(token => thisTitle.Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsTokenMatch(Token itemDepToken, Token itemRegToken, Token depToken, Token regToken)
    {
        return (depToken == Token.None || itemDepToken == depToken)
               && (regToken == Token.None || itemRegToken == regToken);
    }
}