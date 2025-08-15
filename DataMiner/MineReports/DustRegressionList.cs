using DataMiner;

namespace MineReports;

public static class DustRegressionList
{

    private const string HeaderFormat = "{0,-32}{1,-10}{2,10:F3}{3,8:F3}{4,10:F3}{5,8:F3}{6,10:F4}{7,10:F3}{8,13:F8}";
    private const string RowFormat = HeaderFormat;

    public static string[] Build(IEnumerable<Dust>? dusts, string[] matchName, int limit=500, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = dusts
            .Where(d => matchName.Length == 0 || PassesFilters(d.RegressionName, matchName))
            .OrderByDescending(d => d.Regression.PValue)
            .TakeLast(limit);

        return ReportBuffer(notNaN, orderedDusts).ToArray();

        bool PassesFilters(string thisTitle, string[] findMe)
        {
            var result = true;

            foreach (var token in findMe)
                if (!thisTitle.Contains(token, StringComparison.OrdinalIgnoreCase))
                    result = false;

            return result;
        }


        bool FilterTokens(Token itemDepToken, Token itemRegToken, Token depToken, Token regToken)
        {
            return (depToken == Token.None || itemDepToken == depToken)
                   && (regToken == Token.None || itemRegToken == regToken);
        }
    }

    public static string[] Build(IEnumerable<Dust>? dusts, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = dusts.OrderByDescending(d => d.Regression.PValue);

        return ReportBuffer(notNaN, orderedDusts).ToArray();
    }

    public static string[] Build(IEnumerable<Dust>? dusts, Token depToken, Token regToken, int limit = 500, bool notNaN = false)
    {
        if (dusts == null) return [];

        var orderedDusts = dusts.Where(d =>
                                               (regToken == Token.None || d.RegToken == regToken)
                                            && (depToken == Token.None || d.DepToken == regToken))
            .OrderByDescending(d => d.Regression.PValue)
            .TakeLast(limit);

        return ReportBuffer(notNaN, orderedDusts).ToArray();
    }

    private static List<string> ReportBuffer(bool notNaN, IEnumerable<Dust> orderedDusts)
    {
        var reportBuffer = new List<string>
        {
            "regression".PadRight(33) +
            "set".PadRight(10) +
            "mean X".PadLeft(10) +
            "moe X".PadLeft(8) +
            "mean Y".PadLeft(10) +
            "moe Y".PadLeft(8) +
            "slope".PadLeft(10) +
            "R^2".PadLeft(10) +
            "p-value".PadLeft(13)

            //string.Format(
            //    HeaderFormat,
            //    "Regression",
            //    "Set",
            //    0.0, // Placeholder for Mean X
            //    0.0, // Placeholder for moe X
            //    0.0, // Placeholder for Mean Y
            //    0.0, // Placeholder for moe Y
            //    0.0, // Placeholder for Slope
            //    0.0, // Placeholder for xSD
            //    0.0  // Placeholder for p-value
            //)
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
        string regressionName = dust.RegressionName ?? string.Empty;
        string setName = dust.SetName.ToString();

        // Truncate strings to avoid overflow, compatible with older C#
        regressionName = regressionName.Length > 32 ? regressionName.Substring(0, 32) : regressionName;
        setName = setName.Length > 10 ? setName.Substring(0, 10) : setName;

        var moeX = dust.Regression.MarginOfError();
        var moeY = dust.Regression.MarginOfError(true);
        return string.Format(
            RowFormat,
            regressionName,
            setName,
            double.IsNaN(moeX.Mean) || double.IsInfinity(moeX.Mean) ? 0.0 : moeX.Mean,
            double.IsNaN(moeX.MarginOfError) || double.IsInfinity(moeX.MarginOfError) ? 0.0 : moeX.MarginOfError,
            double.IsNaN(moeY.Mean) || double.IsInfinity(moeY.Mean) ? 0.0 : moeY.Mean,
            double.IsNaN(moeY.MarginOfError) || double.IsInfinity(moeY.MarginOfError) ? 0.0 : moeY.MarginOfError,
            double.IsNaN(dust.Regression.Slope) || double.IsInfinity(dust.Regression.Slope) ? 0.0 : dust.Regression.Slope,
            double.IsNaN(dust.Regression.StdDevX) || double.IsInfinity(dust.Regression.StdDevX) ? 0.0 : dust.Regression.StdDevX,
            double.IsNaN(dust.Regression.PValue) || double.IsInfinity(dust.Regression.PValue) ? 0.0 : dust.Regression.PValue);
    }
}