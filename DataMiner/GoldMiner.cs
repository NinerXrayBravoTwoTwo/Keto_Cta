using System.Text;
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
    private readonly HashSet<string> _processedRatios = new();

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
            Dust(SetName.Omega, chartTitle),
            Dust(SetName.Alpha, chartTitle),
            Dust(SetName.Zeta, chartTitle),
            Dust(SetName.Beta, chartTitle),
            Dust(SetName.Gamma, chartTitle),
            Dust(SetName.Theta, chartTitle),
            Dust(SetName.Eta, chartTitle),
            Dust(SetName.BetaUZeta, chartTitle)
        }.Where(d => d != null).Cast<Dust>().ToArray();
    }

    public List<string> ChartsPredictDelta()
    {
        var visitBaseline = "Tps0,Cac0,Ncpv0,Tcpv0,Pav0,LnTps0,LnCac0,LnNcpv0,LnTcpv0,LnPav0".Split(",");
        var elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

        Console.WriteLine("Index, Title, Set, Slope, P-value, Correlation");
        Console.WriteLine($"Processing {visitBaseline.Length * elementDelta.Length} combinations...");

        List<string> chartList = new List<string>();

        for (var x = 0; x < visitBaseline.Length; x++)
        {
            for (var y = 0; y < elementDelta.Length; y++)
            {
                if (x == y && !visitBaseline[x].StartsWith("Ln") && !elementDelta[y].StartsWith("Ln")) continue;
                {
                    chartList.Add($"{x} vs. {y}");
                }

            }
        }

        return chartList;
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
                    // no ratios with dependent in regressor
                    //if (dependent == numerator || dependent == denominator)
                    //{
                    //    dependentInRatio++;
                    //    continue;
                    //}

                    //if (numerator.Equals(denominator))
                    //{
                    //    numEqualDenom++;
                    //    continue; // skip self-ratio of identity
                    //}

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


    public Dust? Dust(SetName setName, string chartTitle)
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
            "index, DCac, DNCpv, LnDCac, LnDNcpv, " +
            "Cac0, Cac1, LnCac0, LnCac1, " +
            "Ncpv0, Ncpv1, LnNcpv0, LnNcpv1, " +
            "Cac0/Ncpv0, Cac0/Ncpv1, " +
            "Ln/LnNcpv0, LnCac0/LnNcpv1, Set"
        ];

        foreach (var item in elements)
        {
            myData.Add(
                $"{item.Id}, {item.DCac}, {item.DNcpv}, {item.LnDCac}, {item.LnDNcpv}, " +
                $"{item.Visits[0].Cac}, {item.Visits[1].Cac}, {item.Visits[0].LnCac}, {item.Visits[1].LnCac}, " +
                $"{item.Visits[0].Ncpv}, {item.Visits[1].Ncpv}, {item.Visits[0].LnNcpv}, {item.Visits[1].LnNcpv}, " +
                $"{item.Visits[0].Cac / item.Visits[0].Ncpv},  {item.Visits[0].Cac / item.Visits[1].Ncpv}, " +
                $"{item.Visits[0].LnCac / item.Visits[0].LnNcpv},  {item.Visits[0].LnCac / item.Visits[1].LnNcpv}, {item.MemberSet}"
            );
        }
        return myData.ToArray();
    }

    public string[] PrintOmegaElements(SetName setName)
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
            "index, DPav, LnDPav, LnPav0, LnPav1, LnNcpv0, LnNcpv1, " +
            "LnPav0/LnNcpv0, LnPav0/LnNcpv1, LnPav1/LnNcpv1, Set"
        ];


        myData.AddRange(elements.Select(item =>
            $"{item.Id}, {item.DPav}, {item.LnDPav}, {item.Visits[0].LnPav}, {item.Visits[1].LnPav}, {item.Visits[0].LnNcpv}, {item.Visits[1].LnNcpv}, " +
            $"{item.Visits[0].Pav / item.Visits[0].Ncpv}, {item.Visits[0].Pav / item.Visits[1].Ncpv}, " +
            $"{item.Visits[1].LnPav / item.Visits[1].LnNcpv}, {item.MemberSet}"));
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
        // if header is true, add header row
        List<string> myData = header
            ?
            [
                "Chart, Set, N=, Mean X, Mean Y, SD X, SD Y, " +
                "Annual Change (Slope), Y-Intercept, Max X, Max Y, Min X, Min Y, p-value"
            ]
            : [];


        string[] chartTitles = ["Cac0 vs. Cac1", "Tps0 vs. Tps1", "Ncpv0 vs. Ncpv1", "Tcpv0 vs. Tcpv1", "Pav0 vs. Pav1"];

        RegressionPvalue regression;
        foreach (var chart in chartTitles)
        {

            switch (chart)
            {
                case "Cac0 vs. Cac1":
                    regression = CalculateRegression(elements, chart, e => (e.Visits[0].Cac, e.Visits[1].Cac));
                    break;
                case "Tps0 vs. Tps1":
                    regression = CalculateRegression(elements, chart, e => (e.Visits[0].Tps, e.Visits[1].Tps));
                    break;
                case "Ncpv0 vs. Ncpv1":
                    regression = CalculateRegression(elements, chart, e => (e.Visits[0].Ncpv, e.Visits[1].Ncpv));
                    break;
                case "Tcpv0 vs. Tcpv1":
                    regression = CalculateRegression(elements, chart, e => (e.Visits[0].Tcpv, e.Visits[1].Tcpv));
                    break;
                case "Pav0 vs. Pav1":
                    regression = CalculateRegression(elements, chart, e => (e.Visits[0].Pav, e.Visits[1].Pav));
                    break;
                default:
                    regression = new RegressionPvalue(); // Default empty regression
                    break;

            }
            

            myData.Add($"{chart}, {setName}, {regression.N}, {regression.MeanX():F5}, {regression.MeanY():F5}, {regression.Qx():F5},  {regression.Qy():F5}, " +
                       $"{regression.Slope():F5}, {regression.YIntercept():F5}, " +
                       $"{regression.MaxX:F5}, {regression.MaxY:F5}, {regression.MinX:F5}, {regression.MinY:F5}, {regression.PValue():F5}");
        }



        return myData.ToArray();
    }

    public IEnumerable<string> RatioCharts()
    {
        return RatioCharts(out _);
    }
}