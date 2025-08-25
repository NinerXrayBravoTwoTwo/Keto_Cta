using DataMiner;
using Keto_Cta;

// ReSharper disable FormatStringProblem

namespace MineReports;

public static class DustRegressionList
{

    private const string HeaderFormat = "{0,-41}{1,-10}{2,10:F3}{3,8:F3}{4,10:F3}{5,8:F3}{6,10:F4}{7,10:F3}{8,13:F8}";
    private const string RowFormat = HeaderFormat;

    public static string[] Build(IEnumerable<Dust>? dusts, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = dusts
            .Where(d => !double.IsNaN(d.Regression.PValue))
            .OrderByDescending(d => d.Regression.PValue);

        return ReportBuffer(notNaN, orderedDusts).ToArray();
    }

    public static string[] Build(IEnumerable<Dust>? dusts, int limit, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = dusts
            .Where(d => !double.IsNaN(d.Regression.PValue))
            .OrderByDescending(d => d.Regression.PValue)
            .TakeLast(limit);

        return ReportBuffer(notNaN, orderedDusts).ToArray();
    }

    public static string[] Build(IEnumerable<Dust>? dusts, Token depToken, Token regToken, int limit, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = dusts.Where(d =>
                (!double.IsNaN(d.Regression.PValue) || !notNaN)
                && IsTokenMatch(d.DepToken, d.RegToken, depToken, regToken))
            .OrderByDescending(d => d.Regression.PValue)
            .TakeLast(limit);

        return ReportBuffer(notNaN, orderedDusts).ToArray();
    }


    public static string[] Build(
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
        return ReportBuffer(notNaN, dustBuffer
                             .OrderByDescending(d => d.Regression.PValue)
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

    private static List<string> ReportBuffer(bool notNaN, IEnumerable<Dust> orderedDusts)
    {
        var reportBuffer = new List<string>
            {
            string.Format(
            HeaderFormat,
            "Regression",
            "Set",
            "Mean X",
            "moe X",
            "Mean Y",
            "moe Y",
            "Slope",
            "xSD",
            "p-value")
        };

        foreach (var dust in orderedDusts)
        {
            if (notNaN && double.IsNaN(dust.Regression.PValue))
            {
                continue;
            }

            reportBuffer.Add(FormatRow(dust));
        }

        return reportBuffer;
    }

    private static string FormatRow(Dust dust)
    {
        string regressionName = dust.RegressionName;
        string setName = dust.SetName.ToString();

        // Truncate strings to avoid overflow, compatible with older C#
        regressionName = regressionName.Length > 41 ? regressionName.Substring(0, 41) : regressionName;
        setName = setName.Length > 10 ? setName.Substring(0, 10) : setName;

        var moeX = dust.Regression.MarginOfError();
        var moeY = dust.Regression.MarginOfError(true);
        return string.Format(
            RowFormat,
            regressionName,
            setName,
            FormatNumber(moeX.Mean, 3),
            FormatNumber(moeX.MarginOfError, 3),
            FormatNumber(moeY.Mean, 3),
            FormatNumber(moeY.MarginOfError, 3),
            FormatNumber(dust.Regression.Slope, 4),
            FormatNumber(dust.Regression.StdDevX, 3),
            FormatNumber(dust.Regression.PValue, 8));
    }

    private static string FormatRowB(Dust dust)
    {
        string regressionName = dust.RegressionName;
        string setName = dust.SetName.ToString();

        // Truncate strings to avoid overflow, compatible with older C#
        regressionName = regressionName.Length > 41 ? regressionName.Substring(0, 41) : regressionName;
        setName = setName.Length > 10 ? setName.Substring(0, 10) : setName;

        var moeX = dust.Regression.MarginOfError();
        var moeY = dust.Regression.MarginOfError(true);
        return string.Format(
            RowFormat,
            regressionName,
            setName,
            FormatNumber(moeX.Mean, 3),
            FormatNumber(moeX.MarginOfError, 3),
            FormatNumber(moeY.Mean, 3),
            FormatNumber(moeY.MarginOfError, 3),
            FormatNumber(dust.Regression.Slope, 4),
            FormatNumber(dust.Regression.StdDevX, 3),
            FormatNumber(dust.Regression.PValue, 8));
           //var xxx =  dust.Regression.ConfidenceIntervalPlus();
    }

    private static string FormatNumber(double value, int precision)
    {
        if (double.IsNaN(value))
        {
            return "NaN".PadLeft(precision + 2); // +2 for decimal point and sign
        }

        if (double.IsInfinity(value))
        {
            return (value > 0 ? "Inf" : "-Inf").PadLeft(precision + 2);
        }

        return value.ToString($"F{precision}").PadLeft(precision + 2);
    }
}