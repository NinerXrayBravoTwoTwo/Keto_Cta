using System.Data;

namespace DataMiner;

public class MineRegressionsWithGold(GoldMiner goldMiner)
{
    // Load dust  Element Delta vs. Element Delta
    private bool _doneMineOperation = false;
    public bool MineOperation()
    {
        if (_doneMineOperation)
            return false;

        _doneMineOperation = true;

        RatioVsDelta().ToList().ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
        RootComboRatio().ToList().ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
        RootRatioMatrix().ToList().ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
        V1vsV0matrix().ToList().ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
        CoolMatrix().ToList().ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));

        PermutationsA(VisitAttributes, ElementAttributes).ToList().ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
        PermutationsB(VisitAttributes, ElementAttributes).ToList().ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
        PermutationsCc(VisitAttributes, ElementAttributes).ToList().ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));

        Success = true;
        return Success;
    }

    public void Clear()
    {
        Success = _doneMineOperation = _isRatioMatrix = _isRatioVsDelta = _isV1vsV0matrix = _isCool = false;
    }

    public bool Success { get; internal set; }

    public static string[] VisitAttributes =
    [
        "Tps", "Cac", "Ncpv", "Tcpv", "Pav", "Qangio", "LnTps", "LnCac", "LnNcpv", "LnTcpv", "LnPav", "LnQangio"
    ];

    public static string[] ElementAttributes =
    [
        "DTps", "DCac", "DNcpv", "DTcpv", "DPav", "DQangio", "LnDTps", "LnDCac", "LnDNcpv", "LnDTcpv", "LnDPav", "LnDQangio",
        "TdCac", "TdNcpv", "TdQangio", "GeoMeanCac", "GeoMeanNcpv", "LnGeoMeanCac", "LnGeoMeanNcpv"
    ];

    public static string[] PermutationsA(string[] visitAttributes, string[] elementAttributes)
    {
        List<string> permutations = [];
        // VVV 
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

        //EVV
        foreach (var reg in elementAttributes) //regressor loop
            foreach (var num in visitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in visitAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    //for (var x = 0; x < 2; x++)
                    for (var y = 0; y < 2; y++)
                        for (var z = 0; z < 2; z++)
                        {
                            permutations.Add($"{num}/{den}{y} vs. {reg}{z}");
                            permutations.Add($"Ln({num}/{den}{y}) vs. {reg}{z}");
                        }
                }

        //VEE
        foreach (var reg in visitAttributes) //regressor loop
            foreach (var num in ElementAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    for (var x = 0; x < 2; x++)
                    //for (var y = 0; y < 2; y++)
                    //for (var z = 0; z < 2; z++)
                    {
                        permutations.Add($"{num}{x}/{den} vs. {reg}");
                        permutations.Add($"Ln({num}{x}/{den} vs. {reg}");
                    }
                }
        // EEE
        foreach (var reg in elementAttributes) //regressor loop
            foreach (var num in elementAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    if (!num.Equals(den))
                    {
                        //for (var x = 0; x < 2; x++)
                        //for (var y = 0; y < 2; y++)
                        //for (var z = 0; z < 2; z++)
                        if (!num.Equals(den))
                        {
                            permutations.Add($"{num}/{den} vs. {reg}");
                            permutations.Add($"Ln({num}/{den} vs. {reg}");
                        }
                    }
                }

        // EVE
        foreach (var reg in elementAttributes) //regressor loop
            foreach (var num in visitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {

                    //for (var x = 0; x < 2; x++)
                    for (var y = 0; y < 2; y++)
                    //for (var z = 0; z < 2; z++)
                    {
                        permutations.Add($"{num}/{den}{y} vs. {reg}");
                        permutations.Add($"Ln({num}/{den}{y}) vs. {reg}");
                    }
                }

        return permutations.ToArray();
    }

    public static string[] PermutationsB(string[] visitAttributes, string[] elementAttributes)
    {
        List<string> permutations = [];

        // EVE
        foreach (var reg in elementAttributes) //regressor loop
            foreach (var num in visitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {

                    //for (var x = 0; x < 2; x++)
                    for (var y = 0; y < 2; y++)
                    //for (var z = 0; z < 2; z++)

                    {
                        permutations.Add($"{num}/{den}{y} vs. {reg}");
                        permutations.Add($"Ln({num}/{den}{y} vs. {reg}");
                    }
                }
        // VEE
        foreach (var reg in ElementAttributes) //regressor loop
            foreach (var num in visitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in elementAttributes.Where(a => !a.StartsWith("Ln")))
                {

                    for (var x = 0; x < 2; x++)
                    //for (var y = 0; y < 2; y++)
                    //for (var z = 0; z < 2; z++)

                    {
                        permutations.Add($"{num}{x}/{den} vs. {reg}");
                        permutations.Add($"Ln({num}{x}/{den} vs. {reg}");
                    }
                }
        // EEE
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
        //VVV
        foreach (var reg in VisitAttributes) //regressor loop
            foreach (var num in VisitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var den in VisitAttributes.Where(a => !a.StartsWith("Ln")))
                {
                    for (var x = 0; x < 2; x++)
                        for (var y = 0; y < 2; y++)
                            for (var z = 0; z < 2; z++)
                                if (!$"{num}{x}".Equals("{den}{y}"))
                                {
                                    permutations.Add($"{num}{x}/{den}{y} vs. {reg}{z}");
                                    permutations.Add($"Ln({num}{x}/{den}{y}) vs. {reg}{z}");
                                }
                }

        return permutations.ToArray();
    }

    public static string[] PermutationsCc(string[] visitAttributes, string[] elementAttributes)
    {
        /*
         * VVVV
         * EEEE
         * VEVE
         * EVEV
         * VVEE
         * EEVV
         */
        List<string> permutations = [];

        // Visit attributes permutations VVVV
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

        // Element attributes permutations EEEE
        foreach (var depD in elementAttributes.Where(a => !a.StartsWith("Ln")))
            foreach (var depN in elementAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var regN in elementAttributes.Where(a => !a.StartsWith("Ln")))
                    foreach (var regD in elementAttributes.Where(a => !a.StartsWith("Ln")))
                    {
                        if (!depD.Equals(depN) && !regN.Equals(regD))
                        {
                            permutations.Add($"{depN}/{depD} vs. {regN}/{regD}");
                            permutations.Add($"Ln({depN}/{depD}) vs. Ln({regN}/{regD})");
                        }
                    }

        // Element and Visit attributes permutations VEVE
        foreach (var depD in visitAttributes.Where(a => !a.StartsWith("Ln")))
            foreach (var depN in elementAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var regN in visitAttributes.Where(a => !a.StartsWith("Ln")))
                    foreach (var regD in elementAttributes.Where(a => !a.StartsWith("Ln")))
                    {
                        for (var x = 0; x < 2; x++)
                            //for (var y = 0; y < 2; y++)
                            for (var z = 0; z < 2; z++)
                            //  for (var o = 0; o < 2; o++)
                            {
                                if (!$"{depD}{x}".Equals($"{depN}") && !$"{regN}{z}".Equals($"{regD}"))
                                {
                                    var dependent = $"{depN}{x}/{depD}";
                                    var regressor = $"{regN}{z}/{regD}";
                                    if (dependent.Equals(regressor)) continue; // skip if regressor is the same as dependent

                                    permutations.Add($"{depN}{x}/{depD} vs. {regN}{z}/{regD}");
                                    permutations.Add($"Ln({depN}{x}/{depD}) vs. Ln({regN}{z}/{regD})");
                                }
                            }
                    }

        // Visit attributes permutations EVEV
        foreach (var depD in elementAttributes.Where(a => !a.StartsWith("Ln"))) //regressor loop
            foreach (var depN in visitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var regN in elementAttributes.Where(a => !a.StartsWith("Ln")))
                    foreach (var regD in visitAttributes.Where(a => !a.StartsWith("Ln")))
                    {
                        //for (var x = 0; x < 2; x++)
                        for (var y = 0; y < 2; y++)
                            //for (var z = 0; z < 2; z++)
                            for (var o = 0; o < 2; o++)
                            {
                                if (!$"{depD}".Equals($"{depN}{y}") && !$"{regN}".Equals($"{regD}{o}"))
                                {
                                    var dependent = $"{depN}/{depD}{y}";
                                    var regressor = $"{regN}/{regD}{o}";
                                    if (dependent.Equals(regressor)) continue; // skip if regressor is the same as dependent

                                    permutations.Add($"{depN}/{depD}{y} vs. {regN}/{regD}{o}");
                                    permutations.Add($"Ln({depN}/{depD}{y}) vs. Ln({regN}/{regD}{o})");
                                }
                            }
                    }


        // Visit attributes permutations VVEE
        foreach (var depD in visitAttributes.Where(a => !a.StartsWith("Ln"))) //regressor loop
            foreach (var depN in visitAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var regN in ElementAttributes.Where(a => !a.StartsWith("Ln")))
                    foreach (var regD in ElementAttributes.Where(a => !a.StartsWith("Ln")))
                    {
                        for (var x = 0; x < 2; x++)
                            for (var y = 0; y < 2; y++)
                            //for (var z = 0; z < 2; z++)
                            //for (var o = 0; o < 2; o++)
                            {
                                if (!$"{depD}{x}".Equals($"{depN}{y}") && !$"{regN}".Equals($"{regD}"))
                                {
                                    var dependent = $"{depN}{x}/{depD}{y}";
                                    var regressor = $"{regN}/{regD}";
                                    if (dependent.Equals(regressor)) continue; // skip if regressor is the same as dependent

                                    permutations.Add($"{depN}{x}/{depD}{y} vs. {regN}/{regD}");
                                    permutations.Add($"Ln({depN}{x}/{depD}{y}) vs. Ln({regN}/{regD})");
                                }
                            }

                    }

        // Visit attributes permutations EEVV
        foreach (var depD in ElementAttributes.Where(a => !a.StartsWith("Ln"))) //regressor loop
            foreach (var depN in elementAttributes.Where(a => !a.StartsWith("Ln")))
                foreach (var regN in visitAttributes.Where(a => !a.StartsWith("Ln")))
                    foreach (var regD in visitAttributes.Where(a => !a.StartsWith("Ln")))
                    {
                        //for (var x = 0; x < 2; x++)
                        //for (var y = 0; y < 2; y++)
                        for (var z = 0; z < 2; z++)
                            for (var o = 0; o < 2; o++)
                            {
                                permutations.Add($"{depN}/{depD} vs. {regN}{z}/{regD}{o}");
                                permutations.Add($"Ln({depN}/{depD}) vs. Ln({regN}{z}/{regD}{o})");
                            }

                    }

        return permutations.ToArray();
    }

    public string[] RootComboRatio()
    {
        return MineRegressionsWithGold.PermutationsCc(MineRegressionsWithGold.VisitAttributes,
                MineRegressionsWithGold.ElementAttributes)
            .ToArray();

    }

    private bool _isRatioMatrix;
    public string[] RootRatioMatrix()
    {
        if (_isRatioMatrix) return [];
        _isRatioMatrix = true;

        var names = MineRegressionsWithGold.
            PermutationsA(MineRegressionsWithGold.VisitAttributes, MineRegressionsWithGold.ElementAttributes);

        return names.ToArray();
    }

    private bool _isV1vsV0matrix;
    /// <summary>
    /// #1
    /// </summary>0
    /// <returns></returns>
    public string[] V1vsV0matrix()
    {
        if (_isV1vsV0matrix) return [];
        _isV1vsV0matrix = true;

        var names = MineRegressionsWithGold.VisitAttributes
            .Select(visit => $"{visit}1 vs. {visit}0").ToList();

        return names.ToArray();
    }

    private bool _isRatioVsDelta;

    /// <summary>
    /// #2 Ratio vs Delta
    /// </summary>
    /// <returns></returns>
    public string[] RatioVsDelta()
    {
        if (_isRatioVsDelta)
            return [];
        _isRatioVsDelta = true;

        var names = new List<string>();
        foreach (var vAttr in MineRegressionsWithGold.VisitAttributes
                    .Where(v => !v.StartsWith("Ln", StringComparison.OrdinalIgnoreCase)))

            foreach (var eAttr in MineRegressionsWithGold.ElementAttributes
                         .Where(e => !e.StartsWith("Ln", StringComparison.OrdinalIgnoreCase)))
            {
                names.Add($"{vAttr}1/{vAttr}0 vs. {eAttr}");
                names.Add(eAttr.Contains("growth", StringComparison.InvariantCultureIgnoreCase) // growth or half-life is already log transformed, compounded log transforms are futile, you will become noise.
                    ? $"Ln({vAttr}1/{vAttr}0) vs. {eAttr}"
                    : $"Ln({vAttr}1/{vAttr}0) vs. Ln{eAttr}");
            }

        return names.ToArray();
    }

    /// <summary>
    /// MonoVarient mine
    /// </summary>
    private bool _isRank;
    public string[] ElemMono()
    {
        if (_isRank)
            return [];

        _isRank = true;

        var names = ElementAttributes.Select(e => $"{e} vs. RankA").ToList();
        names.AddRange(VisitAttributes.Select(v => $"{v}1/{v}0 vs. RankA"));
        names.AddRange(VisitAttributes.Select(v => $"Ln({v}1/{v}0) vs. RankA"));

        return names.ToArray();
    }

    private bool _isCool;
    public string[] CoolMatrix()
    {
        if (_isCool)
            return [];

        _isCool = true;

        string[] names =
        [
            "Cac1/Cac0 vs. Ncpv1/Ncpv0",
            "Cac0/Cac1 vs. Ncpv0/Ncpv1",
            "Qangio1/Qangio0 vs. Ncpv1/Ncpv0",
            "Qangio0/Qangio1 vs. Cac1/Cac0",

            "Ln(Cac1/Cac0) vs. Ln(Ncpv1/Ncpv0)",
            "Ln(Cac0/Cac1) vs. Ln(Ncpv0/Ncpv1)",
            "Ln(Qangio1/Qangio0) vs. Ln(Ncpv1/Ncpv0)",
            "Ln(Qangio0/Qangio1) vs. Ln(Cac1/Cac0)",

        ];
        return names.ToArray();
    }

    private bool _isLnStudy;
    public string[] LnStudy()
    {
        if (_isLnStudy)
            return [];

        _isCool = true;

        string[] names =
        [
            "Cac1 vs. Cac0",
            "LnCac1 vs. LnCac0",
            "Cac1/Cac0 vs. DCac",
            "Cac0/Cac1 vs. DCac",
            "Ln(Cac1/LnCac0) vs. LnDCac",
            "Ln(Cac0/LnCac1) vs. LnDCac",

            "Ncpv1 vs. Ncpv0",
            "LnNcpv1 vs. LnNcpv0",
            "Ncpv1/Ncpv0 vs. DNcpv",
            "Ncpv0/Ncpv1 vs. DNcpv",
            "Ln(Ncpv1/LnNcpv0) vs. LnDNcpv",
            "Ln(Ncpv0/LnNcpv1) vs. LnDNcpv",
        ];
        return names.ToArray();
    }
}
// end of class MineRegressionsWithGold
