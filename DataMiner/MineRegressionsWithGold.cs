using System.Text.RegularExpressions;

namespace DataMiner;

public class MineRegressionsWithGold()
{
    private List<Dust> _dust = [];

    private int _logMismatch = 0;

    private int _uninterestingSkip = 0;

    private int _inverseRatiosIncluded = 0;

    public void ClearDust()
    {
        _dust.Clear();
        _logMismatch =
            _uninterestingSkip =
                _inverseRatiosIncluded = 0;
    }


    // Load dust  Element Delta vs. Element Delta
    /// <summary>
    /// 
    /// </summary>
    /// <param name="myMine"></param>
    /// <param name="isIncludeRatioCharts"></param>
    /// <returns></returns>
    public Dust[] GenerateGoldRegression(GoldMiner myMine, bool isIncludeRatioCharts = false)
    {
        if (_dust.Count > 20) return _dust.ToArray();

        #region Load dust  Element Delta vs. Element Delt
        var elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");
        for (var x = 0; x < elementDelta.Length; x++)
        {
            for (var y = 0; y < elementDelta.Length; y++)
            {
                if (x != y)
                {
                    var chart = $"{elementDelta[y]} vs. {elementDelta[x]}";
                    try
                    {
                        var selector = new CreateSelector(chart);

                        _dust.AddRange(myMine.GoldDust(chart));
                    }
                    catch (ArgumentException)
                    {
                        _logMismatch++; // technically this is a regression against self error  
                    }
                }
            }
        }
        #endregion

        #region Load dust x Baseline, y Year later  
        var visit = "Tps,Cac,Ncpv,Tcpv,Pav,LnTps,LnCac,LnNcpv,LnTcpv,LnPav".Split(",");
        for (var x = 0; x < visit.Length; x++)
        {
            var chart = $"{visit[x]}1 vs. {visit[x]}0";
            try
            {
                var selector = new CreateSelector(chart);

                _dust.AddRange(myMine.GoldDust(chart));
            }
            catch (ArgumentException)
            {
                _logMismatch++;
            }
        }
        #endregion

        #region dust x Baseline, y Year delta
        var eDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

        foreach (var visit0 in visit)
        {
            foreach (var delta in eDelta)
            {

                var chart = $"{visit0}0 vs. {delta}";
                try
                {
                    var selector = new CreateSelector(chart);

                    _dust.AddRange(myMine.GoldDust(chart));
                }
                catch (ArgumentException)
                {
                    _logMismatch++;
                }
            }
        }
        #endregion

        #region Add Ratio  charts

        //if (isIncludeRatioCharts)
        var listOfcacRatios = new List<string>();

        var ratioCharts = RatioCharts();
        foreach (var chart in ratioCharts)
        {

            if (Regex.IsMatch(chart, @"LnCac1 vs. Ln\(Cac0", RegexOptions.IgnoreCase))
                listOfcacRatios.Add(chart);

            try
            {
                var selector = new CreateSelector(chart);

                //System.Diagnostics.Debug.WriteLine($"Accept: {chart}, LogErc-{selector.IsLogMismatch}, ComponentOverlap={selector.HasComponentOverlap}");

                _dust.AddRange(myMine.GoldDust(chart));
            }
            catch (ArgumentException)
            {
                _logMismatch++;
            }
        }
        #endregion


        _dust = _dust.Distinct().OrderBy(d => d.Regression.PValue()).ToList();
        return _dust.ToArray();
    }

    /// <summary>
    /// Generates a list of ratio-based chart descriptions by combining attributes as numerators, denominators, and
    /// dependents.
    /// </summary>
    /// <remarks>The method iterates through combinations of attributes to create unique ratio-based chart
    /// descriptions.  If duplicate or inverse relationships are detected, they are counted and included in the
    /// output.</remarks>
    /// <param name="inverseIncluded">Outputs the number of inverse relationships detected and included in the chart descriptions.</param>
    /// <returns>A list of strings representing ratio-based chart descriptions in the format  "<c>numerator / denominator vs.
    /// dependent</c>".</returns>
    /// 
    public string[] RatioCharts()
    {
        var elementAttributes = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(',');
        var visitAttributes = "Tps,Cac,Ncpv,Tcpv,Pav,LnTps,LnCac,LnNcpv,LnTcpv,LnPav".Split(',');

        var bothVisits = new List<string>();
        foreach (var visit in visitAttributes)
        {
            bothVisits.Add($"{visit}0");
            bothVisits.Add($"{visit}1");
        }

        var allAttributes = elementAttributes.Concat(bothVisits).ToList();
        List<string> chartMapa = [];
        List<string> chartMapb = [];
        var inverseDetected = 0;
        var dependentInRatio = 0;
        var numEqualDenom = 0;
        var isSkipInverse = true;

        List<string> showme = [];
        foreach (var numerator in allAttributes)
        {
            foreach (var denominator in allAttributes)
            {
                if (numerator == denominator) continue; // skip 

                foreach (var dependent in allAttributes)
                {
                    // if (!dependent.StartsWith("LnCac1")) continue; // skip, only interested in lnCac0 for now

                    string[] reg = [numerator, denominator];
                    var key = string.Join(',', reg) + $",{dependent}";

                    var charta = $"{dependent} vs. {numerator} / {denominator}";
                    chartMapa.Add(charta);


                    // skip Ln / Ln, should use Ln(numerator / denominator) instead
                    var chartb = $"{dependent} vs. Ln({numerator} / {denominator})";

                    chartMapb.Add(chartb);

                    //"Chart: LnCac1 vs. Ln(Cac0 / Ncpv0)"
                    //"Chart: LnCac1 vs. Ln(Cac0 / Ncpv1)"

                    //if (chartb.StartsWith("LnCac1 vs. Ln(Cac0"))
                    //    showme.Add($"Chart: {chartb}"); // debug
                }
            }
        }

        List<string> result = [];
        result.AddRange(chartMapb);
        result.AddRange(chartMapa);

        return result.Distinct().ToArray();

    }

    private readonly List<string> _report = [];
    /// <summary>
    /// Generates a detailed report of regression analysis results.
    /// </summary>
    /// <remarks>The report includes a summary of total regressions, a CSV table of interesting regressions 
    /// sorted by p-value, and various statistics about the analysis. If the number of existing  report entries exceeds
    /// 10, the current report is returned without modification. Otherwise,  the report is cleared and
    /// regenerated.</remarks>
    /// <returns>An array of strings containing the generated report. Each string represents a line in the report.</returns>
    public string[] Report()
    {
        if (_report.Count > 10)
            return _report.ToArray();
        _report.Clear();
        _report.Add($"Total Regressions: {_dust.Count}");

        #region Print regression Csv table
        _report.Add($"In Order of PValue (Interesting Regressions Highlighted):");
        _report.Add($"Index, Chart, Subset, N, Slope, p-value, R^2, Y-intercept, X-mean, Y-mean, SD, CC");
        var totalRegressions = 0;
        var index = 0;
        var sortedDust = _dust.OrderBy(d => d.Regression.PValue());
        foreach (var dust in sortedDust)
        {
            totalRegressions++;
            if (dust.IsInteresting)
            {
                var reg = dust.Regression;
                _report.Add($"{index++}, {dust.ChartTitle}, {dust.SetName}, {reg.N}, {reg.Slope():F4}, "
                                  + $"{reg.PValue():F4}, {reg.RSquared():F4}, "
                                  + $"{reg.YIntercept():F4}, {reg.MeanX():F4}, {reg.MeanY():F4}, {reg.StdDevX():F4}, {reg.Correlation():F4}");
            }
        }
        _report.Add($"\nTotal regressions calculated {totalRegressions}");
        _report.Add($"Log mismatch regressions skipped: {_logMismatch}");
        _report.Add($"Inverse Ratio regressions included: {_inverseRatiosIncluded}");
        _report.Add($"Uninteresting regressions included in calculated (See Dust.IsInteresting flag): {_uninterestingSkip}");
        _report.Add($"Total interesting regressions: {_dust.Count(d => d.IsInteresting)}");
        _report.Add($"Interesting remaining regressions: {index}");
        #endregion

        return _report.ToArray();
    }
}
