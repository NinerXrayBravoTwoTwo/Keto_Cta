using System.Text;

namespace DataMiner;

public class MineRegressionsWithGold
{
    private List<Dust> _dust = [];

    public IEnumerable<Dust> Dusts => _dust;

    public int DustCount => _dust.Count;

    public void Clear()
    {
        _dust.Clear();
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

        var elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,DQangio";//.Split(",");
        var elementLnDelta = "LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav,LnDQangio";

        var deltaAttCharts = GenerateElementDeltaCharts(elementDelta.Split(','));
        var deltaLLnAttCharts = GenerateElementDeltaCharts(elementLnDelta.Split(','));

        foreach (var ttl in deltaLLnAttCharts.Concat(deltaAttCharts))
            _dust.AddRange(myMine.GoldDust(ttl));

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
                var chart = $"{delta} vs. {visit0}0";
                _dust.AddRange(myMine.GoldDust(chart));
            }
        }
        #endregion

        #region Add Ratio  charts

        var ratioCharts = RatioCharts();

        foreach (var chart in ratioCharts)
            _dust.AddRange(myMine.GoldDust(chart));

        #endregion

        DeduplicateAndSortDusts();

        Success = true;
        return Success;

        string[] GenerateElementDeltaCharts(string[] elementAttributes)
        {
            List<string> titles = [];
            for (var x = 0; x < elementAttributes.Length; x++)
            {
                for (var y = 0; y < elementAttributes.Length; y++)
                {
                    if (x == y) continue;
                    titles.Add($"{elementAttributes[y]} vs. {elementAttributes[x]}");

                }
            }

            return titles.ToArray();
        }
    }

    protected void DeduplicateAndSortDusts()
    {
        var productionDusts = Deduplication.RemoveDuplicatesByGuid(_dust.ToArray()).OrderBy(d => d.Regression.PValue);
        _dust.Clear();
        _dust.AddRange(productionDusts);
    }

    public bool Success { get; internal set; }

    /// <summary>
    /// Generates a collection of ratio chart titles based on permutations of independent 
    /// and dependent attributes for regression analysis.
    /// </summary>
    /// <remarks>
    /// This method combines various independent attributes (both visit-based and element-based) 
    /// to create ratio chart titles. The generated titles are sorted alphabetically before being returned.
    /// </remarks>
    /// <returns>
    /// An array of strings representing the titles of ratio charts.
    /// </returns>
    public string[] RatioCharts()
    {
        string elementAttString = "DTps,DCac,DNcpv,DTcpv,DPav,DQangio";
        string elementLnString = "LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav,LnDQangio";

        string visitAtt = "Tps,Cac,Ncpv,Tcpv,Pav,Qangio";
        string visitLn = "LnTps,LnCac,LnNcpv,LnTcpv,LnPav,LnQangio";

        var independElementAtt = elementAttString.Split(',');
        var independElementLn = elementLnString.Split(',');

        var independVisitAtt = visitAtt.Split(',');
        var independVisitLn = visitLn.Split(',');

        var dependElemAtt = elementAttString.Split(',').Concat(elementLnString.Split(',')).ToArray();
        var dependVisitAtt = visitAtt.Split(',').Concat(visitLn.Split(',')).ToArray();

        List<string> results = [];
        results.AddRange(GenerateRatioVsVisitElementPermutations());
        results.AddRange(GenerateRatioPermutations(dependVisitAtt, dependElemAtt, independVisitAtt, independElementAtt));
        results.AddRange(GenerateRatioPermutations(dependVisitAtt, dependElemAtt, independVisitLn, independElementLn));

        return results.OrderBy(n => n).ToArray();

    }

    private static List<string> GenerateRatioPermutations(
        string[] depVisitAtt, string[] depElementAtt,
        string[] regVisitAttributes, string[] regElementAttributes)
    {
        var regBothVisits = BothVisits(regVisitAttributes);
        var allRegAttributes = regElementAttributes.Concat(regBothVisits).ToList();

        var depBothVisits = BothVisits(depVisitAtt);
        var depAllAttributes = depElementAtt.Concat(depBothVisits).ToArray();

        List<string> chartMapA = [];
        List<string> chartMapB = [];

        foreach (var numerator in allRegAttributes)
        {
            foreach (var denominator in allRegAttributes)
            {
                if (numerator == denominator) continue; // ToDo: Sanity filter method, skip, regressor of 1 is not very exciting :) 

                foreach (var dependent in depAllAttributes)
                {
                    if (DependentInRegressor(dependent, $"{numerator}/{denominator}")) // ToDo: Sanity method
                        continue;

                    bool DependentInRegressor(string dep, string regressor)
                    {
                        return
                            regressor.ToLower()
                            .Contains(dep.ToLower());
                    }

                    var chartA = $"{dependent} vs. {numerator}/{denominator}";
                    chartMapA.Add(chartA);

                    if (denominator.StartsWith("Ln") || numerator.StartsWith("Ln"))
                        continue; // ToDo: Sanity Filter method, ln(LnVar/var) ?? reject  

                    // skip Ln / Ln, should use Ln(numerator/denominator) instead
                    var chartB = $"{dependent} vs. Ln({numerator}/{denominator})";

                    // todo: add Ln(depNum/depDen) vs Ln(num/den)
                    // todo: Ln(dep_num/dep_den) vs regressor
                    // todo: Ln(Sqrt(a*b)) vs regressor
                    chartMapB.Add(chartB);
                }
            }
        }

        List<string> result = [];
        result.AddRange(chartMapB);
        result.AddRange(chartMapA);
        return result;

        List<string> BothVisits(string[] visitAttributes)
        {
            var list = new List<string>();
            foreach (var visit in visitAttributes)
            {
                list.Add($"{visit}0");
                list.Add($"{visit}1");
            }

            return list;
        }
    }

    private static List<string> GenerateRatioVsVisitElementPermutations()
    {
        string[] visitAttributes =
        {
            "Tps", "Cac", "Ncpv", "Tcpv", "Pav", "Qangio", "LnTps", "LnCac", "LnNcpv", "LnTcpv", "LnPav", "LnQangio"
        };
        string[] elementAttributes =
        {
            "DTps", "DCac", "DNcpv", "DTcpv", "DPav", "DQangio", "LnDTps", "LnDCac", "LnDNcpv", "LnDTcpv", "LnDPav", "LnDQangio"
        };

        List<string> permutations = [];

        foreach (var reg in visitAttributes)   //regressor loop z
            foreach (var num in visitAttributes.Where(a => !a.StartsWith("Ln"))) // y
                foreach (var den in visitAttributes.Where(a => !a.StartsWith("Ln"))) //x
                {
                    if (!num.Equals(den))
                    {
                        for (var x = 0; x < 2; x++)
                            for (var y = 0; y < 2; y++)
                                for (var z = 0; z < 2; z++)
                                {
                                    permutations.Add($"{num}{x}/{den}{y} vs. {reg}{z}");
                                    permutations.Add($"Ln({num}{x}/{den}{y}) vs. {reg}{z}");
                                }
                    }
                }

        foreach (var reg in elementAttributes)   //regressor loop
            foreach (var num in visitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in visitAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    if (!num.Equals(den))
                    {
                        for (var x = 0; x < 2; x++)
                            for (var y = 0; y < 2; y++)
                            //for (var z = 0; z < 2; z++)
                            {
                                permutations.Add($"{num}{x}/{den}{y} vs. {reg}");
                                permutations.Add($"Ln({num}{x}/{den}{y}) vs. {reg}");
                            }
                    }
                }

        foreach (var reg in elementAttributes)   //regressor loop
            foreach (var num in visitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    if (!num.Equals(den))
                    {
                        for (var x = 0; x < 2; x++)
                        //for (var y = 0; y < 2; y++)
                        //for (var z = 0; z < 2; z++)
                        {
                            permutations.Add($"{num}{x}/{den} vs. {reg}");
                            permutations.Add($"Ln({num}{x}/{den} vs. {reg}");
                        }
                    }
                }

        foreach (var reg in elementAttributes)   //regressor loop
            foreach (var num in elementAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    if (!num.Equals(den))
                    {
                        //for (var x = 0; x < 2; x++)
                        //for (var y = 0; y < 2; y++)
                        //for (var z = 0; z < 2; z++)
                        {
                            permutations.Add($"{num}/{den} vs. {reg}");
                            permutations.Add($"Ln({num}/{den} vs. {reg}");
                        }
                    }
                }

        foreach (var reg in elementAttributes)   //regressor loop
            foreach (var num in elementAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    if (!num.Equals(den))
                    {
                        //for (var x = 0; x < 2; x++)
                        for (var y = 0; y < 2; y++)
                        //for (var z = 0; z < 2; z++)
                        {
                            permutations.Add($"{num}/{den}{y} vs. {reg}");
                            permutations.Add($"Ln({num}/{den}{y}) vs. {reg}");
                        }
                    }
                }

        return permutations.OrderBy(p => p).ToList();
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
        _report.Add("index".PadRight(6) +
                    "regression".PadRight(33) +
                    "set".PadRight(10) +
                    "mean X".PadLeft(10) +
                    "moe X".PadLeft(10) +
                    "mean Y".PadLeft(10) +
                    "moe Y".PadLeft(10) +
                    "slope".PadLeft(10) +
                    "R^2".PadLeft(10) +
                    "p-value".PadLeft(12));
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
                || dust.ToString().ToLower().Contains(match.ToLower()))
            {
                var reg = dust.Regression;
                var moeX = reg.MarginOfError();
                var moeY = reg.MarginOfError(true);
                var sb = new StringBuilder();
                sb.Append($"{index++}".PadRight(6));
                sb.Append($"{dust.RegressionName}".PadRight(33));
                sb.Append($"{dust.SetName}".PadRight(10));
                sb.Append($"{reg.MeanX:F3}".PadLeft(10));
                sb.Append($"{moeX.MarginOfError:F3}".PadLeft(10));
                sb.Append($"{reg.MeanY:F3}".PadLeft(10));
                sb.Append($"{moeY.MarginOfError:F3}".PadLeft(10));
                sb.Append($"{reg.Slope:F4}".PadLeft(10));
                sb.Append($"{reg.RSquared:F3}".PadLeft(10));
                sb.Append($"{reg.PValue:F6}".PadLeft(12));
                _report.Add(sb.ToString());
            }
        }

        _report.Add($"\nTotal regressions calculated {totalRegressions}");
        _report.Add($"Total interesting regressions: {_dust.Count(d => d.IsInteresting)}");
        _report.Add($"Interesting remaining regressions: {index}");
        #endregion

        return _report.ToArray();
    }

    public string[] RegressionHistogram()
    {
        DeduplicateAndSortDusts();
        return HistogramTool.Build(Dusts.ToArray());
    }
}
