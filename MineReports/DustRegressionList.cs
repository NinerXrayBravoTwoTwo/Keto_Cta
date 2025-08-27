using DataMiner;
using Keto_Cta;

// ReSharper disable FormatStringProblem


namespace MineReports;

public enum RegressionReport
{
    PValue = 1,
    ConfInterval = 2
}

public class DustRegressionList(RegressionReport reportType)
{
    public IRegressionReport Report = reportType == RegressionReport.ConfInterval
        ? ListRegressionCi.CreateInstance()
        : ListRegressionBasic.CreateInstance();

    public string[] Build(IEnumerable<Dust>? dusts, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = dusts
            .Where(d => !double.IsNaN(d.Regression.PValue))
            .OrderByDescending(d => d.Regression.PValue);

        return Report.ReportBuffer(notNaN, orderedDusts).ToArray();
    }

    public string[] Build(IEnumerable<Dust>? dusts, int limit, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = dusts
            .Where(d => !double.IsNaN(d.Regression.PValue))
            .OrderByDescending(d => d.Regression.PValue)
            .TakeLast(limit);

        return Report.ReportBuffer(notNaN, orderedDusts).ToArray();
    }

    public string[] Build(IEnumerable<Dust>? dusts, Token depToken, Token regToken, int limit, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = dusts
            .Where(d =>
                (!double.IsNaN(d.Regression.PValue) || !notNaN)
                && IsTokenMatch(d.DepToken, d.RegToken, depToken, regToken))
            .OrderByDescending(d => d.Regression.PValue)
            .TakeLast(limit);

        return Report.ReportBuffer(notNaN, orderedDusts).ToArray();
    }


    public string[] Build(
        IEnumerable<Dust>? dusts,
        string[]? matchName,
        Token depToken, Token regToken,
        SetName[] setNames,
        int limit = 500, bool notNaN = false)
    {
        if (dusts == null) return [];

        var dustBuffer = new List<Dust>();
        var enumerable = dusts as Dust[] ?? dusts.ToArray();
        foreach (var dust in enumerable)
        {
            if (!double.IsNaN(dust.Regression.PValue)
                && IsTokenMatch(dust.DepToken, dust.RegToken, depToken, regToken)
                && matchName != null
                && IsFilterMatch(dust.RegressionName, matchName)
                && IsSetNameMatch(dust.SetName, setNames))

                dustBuffer.Add(dust);
        }
        return Report.ReportBuffer(notNaN, dustBuffer
                             .OrderByDescending(d => d.Regression.ConfidenceIntervalPlus().StandardError)
                             .TakeLast(limit))
                             .ToArray();
    }

    private static bool IsSetNameMatch(SetName dustSetName, SetName[] setNames1)
    {
        return setNames1.Length == 0 || setNames1.Contains(dustSetName);
    }

    private static bool IsFilterMatch(string thisTitle, string[] findMe)
    {
        if (findMe.Length == 0) return true;
        var result = false;
        foreach (var token in findMe)
            if (thisTitle.Contains(token, StringComparison.OrdinalIgnoreCase))
                result = true;

        return result;
    }

    private static bool IsTokenMatch(Token itemDepToken, Token itemRegToken, Token depToken, Token regToken)
    {
        return (depToken == Token.None || itemDepToken == depToken)
               && (regToken == Token.None || itemRegToken == regToken);
    }
}