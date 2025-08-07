using System.Text.RegularExpressions;

namespace DataMiner;

public class MineRegressionsWithGold()
{
    private List<Dust> _dust = [];

    private int _uninterestingSkip = 0;


    public void ClearDust()
    {
        _dust.Clear();
        _uninterestingSkip = 0;
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
                if (x == y) continue;
                var chart = $"{elementDelta[y]} vs. {elementDelta[x]}";
                _dust.AddRange(myMine.GoldDust(chart));
            }
        }
        #endregion

        #region Load dust x Baseline, y Year later  
        var visit = "Tps,Cac,Ncpv,Tcpv,Pav,LnTps,LnCac,LnNcpv,LnTcpv,LnPav".Split(",");
        for (var x = 0; x < visit.Length; x++)
        {
            var chart = $"{visit[x]}1 vs. {visit[x]}0";
            _dust.AddRange(myMine.GoldDust(chart));

        }
        #endregion

        #region dust x Baseline, y Year delta
        var eDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

        foreach (var visit0 in visit)
        {
            foreach (var delta in eDelta)
            {

                var chart = $"{visit0}0 vs. {delta}";
                //var selector = new CreateSelector(chart);
                _dust.AddRange(myMine.GoldDust(chart));
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

            // var selector = new CreateSelector(chart);

            //System.Diagnostics.Debug.WriteLine($"Accept: {chart}, LogErc-{selector.IsLogMismatch}, ComponentOverlap={selector.HasComponentOverlap}");

            _dust.AddRange(myMine.GoldDust(chart));

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
    public string[] Report(IEnumerable<Dust> dusts)
    {
        if (_report.Count > 10)
            return _report.ToArray();

        _report.Clear();
        _report.Add($"Total Regressions: {dusts.Count()}");

        #region Print regression Csv table
        _report.Add($"In Order of PValue (Interesting Regressions Highlighted):");
        _report.Add($"Index, Regression,sub-phenotype,N,MeanX, moeX,MeanY,moeY, Slope, p-value");
        var totalRegressions = 0;
        var index = 0;
        var sortedDust = dusts.OrderBy(d => d.Regression.PValue());
        foreach (var dust in sortedDust)
        {
            totalRegressions++;
            if (dust.IsInteresting)
            {
                var reg = dust.Regression;
                var moeX = reg.MarginOfError();
                var moeY = reg.MarginOfError(true);
                _report.Add($"{index++}, {dust.RegressionName}, {dust.SetName} {reg.N}," +
                            $"{moeX.Mean},{moeX.MarginOfError},{moeY.Mean},{moeY.MarginOfError}" +
                            $"{reg.Slope():F4},{reg.PValue():F6}");
            }
        }
        _report.Add($"\nTotal regressions calculated {totalRegressions}");
        _report.Add($"Uninteresting regressions included in calculated (See Dust.IsInteresting flag): {_uninterestingSkip}");
        _report.Add($"Total interesting regressions: {dusts.Count(d => d.IsInteresting)}");
        _report.Add($"Interesting remaining regressions: {index}");
        #endregion

        return _report.ToArray();
    }
}
