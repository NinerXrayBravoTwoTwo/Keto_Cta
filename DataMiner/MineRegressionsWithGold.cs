//using MineReports;

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

        var elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,DQangio"; //.Split(",");
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
        //    _dust.Clear();
        var productionDusts = Deduplication
            .RemoveDuplicatesByGuid(_dust
                .ToArray())
            .OrderByDescending(d => d.Regression.PValue);

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
        results.AddRange(GenerateRatioPermutations(dependVisitAtt, dependElemAtt, independVisitAtt,
            independElementAtt));
        results.AddRange(GenerateRatioPermutations(dependVisitAtt, dependElemAtt, independVisitLn, independElementLn));

        return results.OrderBy(n => n).ToArray();

    }

    public static string[] VisitAttributes =
    {
        "Tps", "Cac", "Ncpv", "Tcpv", "Pav", "Qangio", "LnTps", "LnCac", "LnNcpv", "LnTcpv", "LnPav", "LnQangio"
    };

    public static string[] ElementAttributes =
    {
        "DTps", "DCac", "DNcpv", "DTcpv", "DPav", "DQangio", "LnDTps", "LnDCac", "LnDNcpv", "LnDTcpv", "LnDPav", "LnDQangio"
    };

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
                if (numerator == denominator)
                    continue; // ToDo: Sanity filter method, skip, regressor of 1 is not very exciting :) 

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

    public static List<string> GenerateRatioVsVisitElementPermutations()
    {

        var permutations = PermutationsA(VisitAttributes, ElementAttributes)
            .Concat(PermutationsB(VisitAttributes, ElementAttributes));

        return permutations.OrderBy(p => p).ToList();
    }

    public static List<string> PermutationsA(string[] visitAttributes, string[] elementAttributes)
    {
        List<string> permutations = [];

        foreach (var reg in visitAttributes) //regressor loop z
            foreach (var num in visitAttributes.Where(a => !a.StartsWith("Ln"))) // y
                foreach (var den in visitAttributes.Where(a => !a.StartsWith("Ln"))) //x
                {
                    for (var x = 0; x < 2; x++)
                        for (var y = 0; y < 2; y++)
                            for (var z = 0; z < 2; z++)
                                if (!$"{num}{x}".Equals($"{den}{y}")) // skip if numerator and denominator are the same
                                {
                                    permutations.Add($"{num}{x}/{den}{y} vs. {reg}{z}");
                                    permutations.Add($"Ln({num}{x}/{den}{y}) vs. {reg}{z}");
                                }
                }


        foreach (var reg in elementAttributes) //regressor loop
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

        foreach (var reg in elementAttributes) //regressor loop
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

        foreach (var reg in elementAttributes) //regressor loop
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

        foreach (var reg in elementAttributes) //regressor loop
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

        return permutations;
    }

    public static List<string> PermutationsB(string[] visitAttributes, string[] elementAttributes)
    {
        List<string> permutations = [];

        foreach (var reg in elementAttributes) //regressor loop
            foreach (var num in visitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    if (!num.Equals(den))
                    {
                        for (var x = 0; x < 2; x++)
                            //for (var y = 0; y < 2; y++)
                            for (var z = 0; z < 2; z++)
                            {
                                permutations.Add($"{num}{x}/{den} vs. {reg}{z}");
                                permutations.Add($"Ln({num}{x}/{den} vs. {reg}{z}");
                            }
                    }
                }

        foreach (var reg in elementAttributes) //regressor loop
            foreach (var num in elementAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    if (!num.Equals(den))
                    {
                        //for (var x = 0; x < 2; x++)
                        //for (var y = 0; y < 2; y++)
                        for (var z = 0; z < 2; z++)
                        {
                            permutations.Add($"{num}/{den} vs. {reg}{z}");
                            permutations.Add($"Ln({num}/{den} vs. {reg}{z}");
                        }
                    }
                }

        foreach (var reg in elementAttributes) //regressor loop
            foreach (var num in elementAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    if (!num.Equals(den))
                    {
                        //for (var x = 0; x < 2; x++)
                        for (var y = 0; y < 2; y++)
                            for (var z = 0; z < 2; z++)
                            {
                                permutations.Add($"{num}/{den}{y} vs. {reg}{z}");
                                permutations.Add($"Ln({num}/{den}{y}) vs. {reg}{z}");
                            }
                    }
                }

        return permutations;
    }

    public static List<string> PermutationsCc(string[] visitAttributes, string[] elementAttributes)
    {
        List<string> permutations = [];

        foreach (var depD in visitAttributes.Where(a => !a.StartsWith("Ln"))) //regressor loop
        foreach (var depN in visitAttributes.Where(a => !a.StartsWith("Ln")))
        foreach (var regN in visitAttributes.Where(a => !a.StartsWith("Ln")))
        foreach (var regD in visitAttributes.Where(a => !a.StartsWith("Ln")))
        {
            for (var x = 0; x < 2; x++)
            for (var y = 0; y < 2; y++)
            for (var z = 0; z < 2; z++)
            for (var o = 0; o < 2; o++)
            {
                if (!$"{depD}{x}".Equals($"{depN}{y}") && !$"{regN}{z}".Equals($"{regD}{o}"))
                {
                    var dependent = $"{depN}{x}/{depD}{y}";
                    var regressor = $"{regN}{z}/{regD}{o}";
                    if (dependent.Equals(regressor)) continue; // skip if regressor is the same as dependent

                    permutations.Add($"{depN}{x}/{depD}{y} vs. {regN}{z}/{regD}{o}");
                    permutations.Add($"Ln({depN}{x}/{depD}{y}) vs. Ln({regN}{z}/{regD}{o})");
                }
            }

        }

        return permutations;
    }
} // end of class MineRegressionsWithGold
