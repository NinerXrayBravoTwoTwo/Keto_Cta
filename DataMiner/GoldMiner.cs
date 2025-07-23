using Keto_Cta;
using LinearRegression;

namespace DataMiner;

public class GoldMiner
{
    public GoldMiner(string path)
    {
        var elements = ReadCsvFile(path) ??
                       throw new ArgumentException("CSV file returned null elements.", nameof(path));

        Omega = elements.Where(e =>
            e.MemberSet is LeafSetName.Zeta or LeafSetName.Gamma or LeafSetName.Theta or LeafSetName.Eta).ToArray();
        Alpha = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta or LeafSetName.Gamma).ToArray();
        Beta = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta).ToArray();
        Zeta = elements.Where(e => e.MemberSet == LeafSetName.Zeta).ToArray();
        Gamma = elements.Where(e => e.MemberSet == LeafSetName.Gamma).ToArray();
        Theta = elements.Where(e => e.MemberSet == LeafSetName.Theta).ToArray();
        Eta = elements.Where(e => e.MemberSet == LeafSetName.Eta).ToArray();
        BetaUZeta = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta or LeafSetName.Zeta).ToArray();

        _setNameToData = new Dictionary<SetName, Element[]>
        {
            { SetName.Omega, Omega },
            { SetName.Alpha, Alpha },
            { SetName.Beta, Beta },
            { SetName.Zeta, Zeta },
            { SetName.Gamma, Gamma },
            { SetName.Eta, Eta },
            { SetName.Theta, Theta },
            { SetName.BetaUZeta, BetaUZeta }
        };
    }

    public Element[] Omega;
    public Element[] Alpha;
    public Element[] Beta;
    public Element[] Zeta;
    public Element[] Gamma;
    public Element[] Theta;
    public Element[] Eta;
    public Element[] BetaUZeta;

    private readonly Dictionary<SetName, Element[]> _setNameToData;
    private readonly Dictionary<string, CreateSelector> _selectorCache = new();
    // private readonly HashSet<string> _processedRatios = [];

    private static List<Element> ReadCsvFile(string path)
    {
        var list = new List<Element>();
        using var reader = new StreamReader(path);
        var index = 0;
        if (!reader.EndOfStream) reader.ReadLine();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
#pragma warning disable CS8602
            var values = line.Split(',');
#pragma warning restore CS8602

            try
            {
                var visit1 = new Visit("V1", null, int.Parse(values[0]), int.Parse(values[2]), double.Parse(values[4]),
                    double.Parse(values[6]), double.Parse(values[8]));
                var visit2 = new Visit("V2", null, int.Parse(values[1]), int.Parse(values[3]), double.Parse(values[5]),
                    double.Parse(values[7]), double.Parse(values[9]));

                index++;
                var element = new Element(index.ToString(), [visit1, visit2]);
                list.Add(element);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Skipping line {index + 1}: invalid number format ({ex.Message}).");
            }
        }

        return list;
    }

    private RegressionPvalue CalculateRegression(IEnumerable<Element> targetElements, string label,
        Func<Element, (double x, double y)> selector)
    {
        if (targetElements == null) throw new ArgumentNullException(nameof(targetElements));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var dataPoints = new List<(double x, double y)>();
        foreach (var element in targetElements)
        {
            try
            {
                dataPoints.Add(selector(element));
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Skipping data point in {label}: {ex.Message}");
                continue;
            }
        }

        return new RegressionPvalue(dataPoints);
    }

    private RegressionPvalue CalculateRegressionRatio(IEnumerable<Element> targetElements, string label,
        Func<Element, (double numerator, double denominator)> xSelector,
        Func<Element, double> ySelector)
    {
        var dataPoints = new List<(double x, double y)>();
        foreach (var element in targetElements)
        {
            try
            {
                var (numerator, denominator) = xSelector(element);
                double x = denominator != 0 ? numerator / denominator : 0;
                double y = ySelector(element);
                dataPoints.Add((x, y));
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Skipping data point in {label}: {ex.Message}");
                continue;
            }
        }

        return new RegressionPvalue(dataPoints);
    }

    public Dust[] GoldDust(string chartTitle)
    {
        return new List<Dust?>
        {
            AuDust(SetName.Omega, chartTitle),
            AuDust(SetName.Alpha, chartTitle),
            AuDust(SetName.Zeta, chartTitle),
            AuDust(SetName.Beta, chartTitle),
            AuDust(SetName.Gamma, chartTitle),
            AuDust(SetName.Theta, chartTitle),
            AuDust(SetName.Eta, chartTitle),
            AuDust(SetName.BetaUZeta, chartTitle)
        }.Where(d => d != null).Cast<Dust>().ToArray();
    }

    public List<string> RatioCharts(out int inverseIncluded)
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

        Dictionary<string, string> chartMap = new Dictionary<string, string>();
        var inverseDetected = 0;
        var dependentInRatio = 0;
        var numEqualDenom = 0;
        bool isSkipInverse = true;

        foreach (var numerator in allAttributes)
        {
            foreach (var denominator in allAttributes)
            {
                if (numerator == denominator) continue; // skip 

                foreach (var dependent in allAttributes)
                {
                    var chart = $"{numerator} / {denominator} vs. {dependent}";
                    string[] reg = [numerator, denominator];
                    var key = string.Join(',', reg.OrderBy(r => r)) + $",{dependent}";

                    if (chartMap.TryAdd(key, chart)) continue;

                    inverseDetected++;
                    chartMap.TryAdd(string.Join(',', reg) + $",{dependent}", chart);
                }
            }

        }


        inverseIncluded = inverseDetected;
        return chartMap.Select(kvp => kvp.Value).ToList();

    }


    public Dust? AuDust(SetName setName, string chartTitle)
    {
        if (!_setNameToData.TryGetValue(setName, out var data) || data.Length == 0)
        {
            Console.WriteLine($"No data for set {setName} in chart {chartTitle}");
            return null;
        }

        if (!_selectorCache.TryGetValue(chartTitle, out var selector))
        {
            try
            {
                selector = new CreateSelector(chartTitle);
                _selectorCache[chartTitle] = selector;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Invalid chart title {chartTitle}: {ex.Message}");
                return null;
            }
        }

        var regression = selector.IsRatio
            ? CalculateRegressionRatio(data, chartTitle, selector.XSelector, selector.YSelector)
            : CalculateRegression(data, chartTitle, selector.Selector);

        return regression.DataPointsCount() < 3 ? null : new Dust(setName, chartTitle, regression);
    }

    public string[] PrintBetaUZetaElements(SetName setName)
    {

        if (!_setNameToData.TryGetValue(setName, out var elements))
        {
            return [];
        }

        List<string> myData =
        [
            "index,DCac,DNCpv,LnDCac,LnDNcpv," +
            "Cac0,Cac1,LnCac0,LnCac1," +
            "Ncpv0,Ncpv1,LnNcpv0,LnNcpv1," +
            "Cac0/Ncpv0,Cac0/Ncpv1," +
            "Ln/LnNcpv0,LnCac0/LnNcpv1,Set"
        ];
        myData.AddRange(elements.Select(item => $"{item.Id},{item.DCac},{item.DNcpv},{item.LnDCac},{item.LnDNcpv},"
                    + $"{item.Visits[0].Cac},{item.Visits[1].Cac},{item.Visits[0].LnCac},{item.Visits[1].LnCac},"
                    + $"{item.Visits[0].Ncpv},{item.Visits[1].Ncpv},{item.Visits[0].LnNcpv},{item.Visits[1].LnNcpv},"
                    + $"{item.Visits[0].Cac / item.Visits[0].Ncpv},{item.Visits[0].Cac / item.Visits[1].Ncpv},"
                    + $"{item.Visits[0].LnCac / item.Visits[0].LnNcpv},{item.Visits[0].LnCac / item.Visits[1].LnNcpv},{item.MemberSet}"));

        return myData.ToArray();
    }

    public string[] PrintOmegaElementsFor3DGammaStudy(SetName setName)
    {
        // LnPav0 / LnNcpv0 vs. LnDPav -- Alpha
        // LnPav0 / LnNcpv1 vs. LnDPav -- Alpha
        // LnPav1 / LnNcpv1 vs. LnDPav -- Alpha

        // LnPav0 / LnNcpv0 vs. LnPav1 -- Alpha
        // LnPav0 / LnNcpv1 vs. LnPav1 -- Alpha

        if (!_setNameToData.TryGetValue(setName, out var elements))
        {
            return [];
        }

        List<string> myData =
        [
            "index,DPav,LnDPav,LnPav0,LnPav1,LnNcpv0,LnNcpv1," +
            "LnPav0/LnNcpv0,LnPav0/LnNcpv1,LnPav1/LnNcpv1,Set"
        ];


        myData.AddRange(elements.Select(item =>
            $"{item.Id},{item.DPav},{item.LnDPav},{item.Visits[0].LnPav},{item.Visits[1].LnPav},{item.Visits[0].LnNcpv},{item.Visits[1].LnNcpv}," +
            $"{item.Visits[0].Pav / item.Visits[0].Ncpv},{item.Visits[0].Pav / item.Visits[1].Ncpv}," +
            $"{item.Visits[1].LnPav / item.Visits[1].LnNcpv},{item.MemberSet}"));

        var ratio0_gamma = new List<string>();
        var ratio1_gamma = new List<string>();
        var ln_pav1_gamma = new List<string>();
        var x_gamma = new List<string>();
        var y_gamma = new List<string>();
        var z_gamma = new List<string>();

        var ratio0_theta = new List<string>();
        var ratio1_theta = new List<string>();
        var ln_pav1_theta = new List<string>();
        var x_theta = new List<string>();
        var y_theta = new List<string>();
        var z_theta = new List<string>();

        var ratio0_eta = new List<string>();
        var ratio1_eta = new List<string>();
        var ln_pav1_eta = new List<string>();
        var x_eta = new List<string>();
        var y_eta = new List<string>();
        var z_eta = new List<string>();

        var ratio0_zeta = new List<string>();
        var ratio1_zeta = new List<string>();
        var ln_pav1_zeta = new List<string>();
        var x_zeta = new List<string>();
        var y_zeta = new List<string>();
        var z_zeta = new List<string>();



        // Build 3D graphic
        //
        // # LnPav0 / LnNcpv0 vs. LnPav1 -- Alpha
        // # Slope; 6.7999 N=88 R^2: 0.9575 p-value: 0.000010 y-int -0.0016
        // 
        // # LnPav0 / LnNcpv1 vs. LnPav1 -- Alpha
        // # Slope; 7.0345 N=88 R^2: 0.9490 p-value: 0.000051 y-int 0.0004
        //
        //  Gamma data
        //# ratio0_gamma = np.array([0.5, 0.6, 0.7, 0.8, 0.9])
        //# ratio1_gamma = np.array([0.5, 0.6, 0.7, 0.8, 0.9])
        //# ln_pav1_gamma = np.array([1.0, 1.5, 2.0, 2.5, 3.0])  # Example data for Gamma
        foreach (var item in elements)
        {
            if (item.IsGamma)
            {
                ratio0_gamma.Add($"{item.Visits[0].LnPav / item.Visits[0].LnNcpv:F8}");
                ratio1_gamma.Add($"{item.Visits[0].LnPav / item.Visits[1].LnNcpv:F8}");
                ln_pav1_gamma.Add($"{item.Visits[1].LnPav:F8}");
                x_gamma.Add($"{item.Visits[0].LnPav / item.Visits[0].LnNcpv:F8}");
                y_gamma.Add($"{item.Visits[0].LnPav / item.Visits[1].LnNcpv:F8}");
                z_gamma.Add($"{item.Visits[1].LnPav:F8}");

                continue;
            }

            if (item.IsTheta)
            {
                ratio0_theta.Add($"{item.Visits[0].LnPav / item.Visits[0].LnNcpv:F8}");
                ratio1_theta.Add($"{item.Visits[0].LnPav / item.Visits[1].LnNcpv:F8}");
                ln_pav1_theta.Add($"{item.Visits[1].LnPav:F8}");
                x_theta.Add($"{item.Visits[0].LnPav / item.Visits[0].LnNcpv:F8}");
                y_theta.Add($"{item.Visits[0].LnPav / item.Visits[1].LnNcpv:F8}");
                z_theta.Add($"{item.Visits[1].LnPav:F8}");

                continue;
            }
            if (item.IsEta)
            {
                ratio0_eta.Add($"{item.Visits[0].LnPav / item.Visits[0].LnNcpv:F8}");
                ratio1_eta.Add($"{item.Visits[0].LnPav / item.Visits[1].LnNcpv:F8}");
                ln_pav1_eta.Add($"{item.Visits[1].LnPav:F8}");
                x_eta.Add($"{item.Visits[0].LnPav / item.Visits[0].LnNcpv:F8}");
                y_eta.Add($"{item.Visits[0].LnPav / item.Visits[1].LnNcpv:F8}");
                z_eta.Add($"{item.Visits[1].LnPav:F8}");
                continue;
            }
            if (item.IsZeta)
            {
                ratio0_zeta.Add($"{item.Visits[0].LnPav / item.Visits[0].LnNcpv:F8}");
                ratio1_zeta.Add($"{item.Visits[0].LnPav / item.Visits[1].LnNcpv:F8}");
                ln_pav1_zeta.Add($"{item.Visits[1].LnPav:F8}");
                x_zeta.Add($"{item.Visits[0].LnPav / item.Visits[0].LnNcpv:F8}");
                y_zeta.Add($"{item.Visits[0].LnPav / item.Visits[1].LnNcpv:F8}");
                z_zeta.Add($"{item.Visits[1].LnPav:F8}");
                continue;
            }

        }
        myData.Add("\n# 3D Gamma data");
        myData.Add("x_gamma = np.array([" + string.Join(", ", x_gamma) + "])");
        myData.Add("y_gamma = np.array([" + string.Join(", ", y_gamma) + "])");
        myData.Add("z_gamma = np.array([" + string.Join(", ", z_gamma) + "])");

        myData.Add("\n# 3D Theta data");
        myData.Add("x_theta = np.array([" + string.Join(", ", x_theta) + "])");
        myData.Add("y_theta = np.array([" + string.Join(", ", y_theta) + "])");
        myData.Add("z_theta = np.array([" + string.Join(", ", z_theta) + "])");

        myData.Add("\n# 3D Eta data");
        myData.Add("x_eta = np.array([" + string.Join(", ", x_eta) + "])");
        myData.Add("y_eta = np.array([" + string.Join(", ", y_eta) + "])");
        myData.Add("z_eta = np.array([" + string.Join(", ", z_eta) + "])");

        myData.Add("\n# 3D Zeta data");
        myData.Add("x_zeta = np.array([" + string.Join(", ", x_zeta) + "])");
        myData.Add("y_zeta = np.array([" + string.Join(", ", y_zeta) + "])");
        myData.Add("z_zeta = np.array([" + string.Join(", ", z_zeta) + "])");

        // add Gamma data
        myData.Add("\n# Gamma data");
        myData.Add("ratio0_gamma = np.array([" + string.Join(", ", ratio0_gamma) + "])");
        myData.Add("ratio1_gamma = np.array([" + string.Join(", ", ratio1_gamma) + "])");
        myData.Add("ln_pav1_gamma = np.array([" + string.Join(", ", ln_pav1_gamma) + "])");

        myData.Add("\n# Theta data");
        myData.Add("ratio0_theta = np.array([" + string.Join(", ", ratio0_theta) + "])");
        myData.Add("ratio1_theta = np.array([" + string.Join(", ", ratio1_theta) + "])");
        myData.Add("ln_pav1_theta = np.array([" + string.Join(", ", ln_pav1_theta) + "])");

        myData.Add("\n# Eta data");
        myData.Add("ratio0_eta = np.array([" + string.Join(", ", ratio0_eta) + "])");
        myData.Add("ratio1_eta = np.array([" + string.Join(", ", ratio1_eta) + "])");
        myData.Add("ln_pav1_eta = np.array([" + string.Join(", ", ln_pav1_eta) + "])");

        myData.Add("\n# Zeta data");
        myData.Add("ratio0_zeta = np.array([" + string.Join(", ", ratio0_zeta) + "])");
        myData.Add("ratio1_zeta = np.array([" + string.Join(", ", ratio1_zeta) + "])");
        myData.Add("ln_pav1_zeta = np.array([" + string.Join(", ", ln_pav1_zeta) + "])");

        return myData.ToArray();
    }

    public string[] PrintAllSetMatrix()
    {
        List<string> myData = [];

        myData.AddRange(PrintStatisticMatrix(SetName.Omega, true));
        myData.AddRange(PrintStatisticMatrix(SetName.Eta, false));
        myData.AddRange(PrintStatisticMatrix(SetName.Theta, false));
        myData.AddRange(PrintStatisticMatrix(SetName.Gamma, false));
        myData.AddRange(PrintStatisticMatrix(SetName.Zeta, false));

        return myData.ToArray();
    }

    public string[] PrintStatisticMatrix(SetName setName, bool header = true)
    {

        if (!_setNameToData.TryGetValue(setName, out var elements))
        {
            return [];
        }



        string[] chartTitles =
        [
            "Cac0 vs. Cac1", "Tps0 vs. Tps1", "Ncpv0 vs. Ncpv1", "Tcpv0 vs. Tcpv1", "Pav0 vs. Pav1",
            "LnCac0 vs. LnCac1", "LnTps0 vs. LnTps1", "LnNcpv0 vs. LnNcpv1", "LnTcpv0 vs. LnTcpv1", "LnPav0 vs. LnPav1",
        ];
        var localDust = chartTitles.Select(chart => AuDust(setName, chart)).OfType<Dust>().ToList();
        //var sortedDust = localDust.OrderBy(d => d.Regression.PValue());

        // if header is true, add header row
        List<string> myData = header
            ?
            [
                "MOE = Margin Of Error i.e. +/-\n" +
                "Regression,Set,Mean X,moe X,Mean Y,moe Y," +
                "Slope,R^2,p-value"
            ]
            : [];

        foreach (var dust in localDust)
        {
            var confidience = dust.Regression.ConfidenceInterval();
            var moeX = dust.Regression.MarginOfError();
            var moeY = dust.Regression.MarginOfError(true);

            myData.Add(
                $"{dust.ChartTitle},{setName},{moeX.Mean:F3},{moeX.MarginOfError:F3},{moeY.Mean:F3},{moeY.MarginOfError:F3}," +
                $"{dust.Regression.Slope():F5},{dust.Regression.RSquared():F4},{dust.Regression.PValue():F8}");
        }

        return myData.ToArray();
    }

    public string[] PrintKetoCta(bool header = true)
    {
        if (!_setNameToData.TryGetValue(SetName.Omega, out var elements))
        {
            return [];
        }
        //V1_Total_Plaque_Score,V2_Total_Plaque_Score,V1_CAC,V2_CAC,V1_Non_Calcified_Plaque_Volume,V2_Non_Calcified_Plaque_Volume,V1_Total_Calcified_Plaque_Volume,V2_Total_Calcified_Plaque_Volume,V1_Percent_Atheroma_Volume,V2_Percent_Atheroma_Volume
        // 
        List<string> myData = header
            ?
            [
                "Index,Set,"+
                "Tps0,Tps1,Cac0,Cac1,Ncpv0,Ncpv1,Tcpv0,Tcpv1,Pav0,Pav1," +
                "LnTps0,LnTps1,LnCac0,LnCac1,LnNcpv0,LnNcpv1,LnTcpv0,LnTcpv1,LnPav0,LnPav1," +
                "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav"
            ]
            : [];

        myData.AddRange(elements.Select(item =>
            $"{item.Id},{item.MemberSet}," +
            $"{item.Visits[0].Tps},{item.Visits[1].Tps},{item.Visits[0].Cac},{item.Visits[1].Cac},{item.Visits[0].Ncpv},{item.Visits[1].Ncpv},{item.Visits[0].Tcpv},{item.Visits[1].Tcpv},{item.Visits[0].Pav},{item.Visits[1].Pav}," +
            $"{item.Visits[0].LnTps},{item.Visits[1].LnTps},{item.Visits[0].LnCac},{item.Visits[1].LnCac},{item.Visits[0].LnNcpv},{item.Visits[1].LnNcpv},{item.Visits[0].Pav},{item.Visits[0].LnTcpv},{item.Visits[1].LnTcpv},{item.Visits[0].LnPav},{item.Visits[1].LnPav}" +
            $"{item.DTps},{item.DCac},{item.DNcpv},{item.DTcpv},{item.DPav},{item.LnDTps},{item.LnDCac},{item.LnDNcpv},{item.LnDTcpv},{item.LnDPav}"));

        return myData.ToArray();
    }
}