using System.Text.RegularExpressions;

namespace DataMiner;

public class MineRegressionsWithGold()
{
    private List<Dust> _dust = [];

    public IEnumerable<Dust> Dusts => _dust;

    public int DustCount => _dust.Count;

    private int _uninterestingSkip = 0;

    public void Clear()
    {
        _dust.Clear();
        _uninterestingSkip = 0;
    }

    public void AddRange(IEnumerable<Dust> dusts)
    {
        _dust.AddRange(dusts);
        var productionDusts = Deduplication.RemoveDuplicatesByGuid(_dust.ToArray());
        _dust.Clear();
        _dust = productionDusts.ToList();
    }

    // Load dust  Element Delta vs. Element Delta
    public bool GenerateGoldRegressions(GoldMiner myMine, int limit = 1000) // that is 10k by the way
    {
        if (DustCount > 200)
        {
            Success = true;
            return Success;
        }

        #region Load dust  Element Delta vs. Element Delt
        var elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,DQangio,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav,LnDQangio".Split(",");
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
        var visit = "Tps,Cac,Ncpv,Tcpv,Pav,Qangio,LnTps,LnCac,LnNcpv,LnTcpv,LnPav,LnQangio".Split(",");
        for (var x = 0; x < visit.Length; x++)
        {
            var chart = $"{visit[x]}1 vs. {visit[x]}0";
            _dust.AddRange(myMine.GoldDust(chart));

        }
        #endregion

        #region dust x Baseline, y Year delta
        var eDelta = "DTps,DCac,DNcpv,DTcpv,DPav,DQangio,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav,LnDQangio".Split(",");


        foreach (var visit0 in visit)
        {
            foreach (var delta in eDelta)
            {
                var chart = $"{delta} vs. {visit}0";
                _dust.AddRange(myMine.GoldDust(chart));
            }
        }
        #endregion

        #region Add Ratio  charts

        //  var listOfcacRatios = new List<string>();

        var ratioCharts = RatioCharts();

        foreach (var chart in ratioCharts)
        {
            //if (Regex.IsMatch(chart, @"LnCac1 vs. Ln\(Cac0", RegexOptions.IgnoreCase))
            //    listOfcacRatios.Add(chart);

            _dust.AddRange(myMine.GoldDust(chart));
        }
        #endregion

        var productionDusts = Deduplication.RemoveDuplicatesByGuid(_dust.ToArray()).OrderBy(d => d.Regression.PValue);

        Success = true;
        return Success;
    }

    public bool Success { get; internal set; }

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
        var elementAttributes = "DTps,DCac,DNcpv,DTcpv,DPav,DQangio,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav,LnDQangio".Split(',');
        var visitAttributes = "Tps,Cac,Ncpv,Tcpv,Pav,Qangio,LnTps,LnCac,LnNcpv,LnTcpv,LnPav,LnQangio".Split(',');

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

                    var charta = $"{dependent} vs. {numerator}/{denominator}";
                    chartMapa.Add(charta);

                    if (denominator.StartsWith("Ln") || numerator.StartsWith("Ln")) continue;

                    // skip Ln / Ln, should use Ln(numerator/denominator) instead
                    var chartb = $"{dependent} vs. Ln({numerator}/{denominator})";

                    // todo: add Ln(depNum/depDen) vs Ln(num/den)
                    // todo: Ln(depnum/deDen) vs regressor
                    // todo: Ln(Sqrt(a*b)) vs regressor
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
    public string[] Report(string match, double kiloLimit = 10)
    {
        _report.Clear();
        //var myDusts = _dust.ToArray(); // Fixes CS0039: Convert List<Dust> to array directly
        _report.Add($"Total Regressions: {_dust.Count}");

        #region Print regression Csv table
        _report.Add($"In Order of PValue (Interesting Regressions Highlighted):");
        _report.Add($"Index, Regression,sub-phenotype N,MeanX,moeX,MeanY,moeY,Slope,R^2,p-value");
        var totalRegressions = 0;
        var index = 0;
        //
        foreach (var dust in _dust)
        {
            if (index > kiloLimit * 1000.0)
                break;

            totalRegressions++;
            if (!dust.IsInteresting) continue;

            if (string.IsNullOrEmpty(match)
                || match.Contains("any", StringComparison.InvariantCultureIgnoreCase)
                || Regex.IsMatch(dust.ToString(), match.Trim(), RegexOptions.IgnoreCase))
            {
                var reg = dust.Regression;
                var moeX = reg.MarginOfError();
                var moeY = reg.MarginOfError(true);
                _report.Add($"{index++},{dust.RegressionName},{dust.SetName} {reg.N}," +
                            $"{moeX.Mean:F3},{moeX.MarginOfError:F3}," +
                            $"{moeY.Mean:F3},{moeY.MarginOfError:F3}," +
                            $"{reg.Slope:F4},{reg.RSquared:F3},{reg.PValue:F6}");
            }
        }

        _report.Add($"\nTotal regressions calculated {totalRegressions}");
        _report.Add($"Uninteresting regressions included in calculated (See Dust.IsInteresting flag): {_uninterestingSkip}");
        _report.Add($"Total interesting regressions: {_dust.Count(d => d.IsInteresting)}");
        _report.Add($"Interesting remaining regressions: {index}");
        #endregion

        return _report.ToArray();
    }
}
