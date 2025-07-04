using Keto_Cta;
using LinearRegression;
using System.Collections.Generic;

namespace DataMiner;

public class GoldMiner
{
    public GoldMiner(string path)
    {
        var elements = ReadCsvFile(path) ?? throw new ArgumentException("CSV file returned null elements.", nameof(path));

        Omega = elements.Where(e => e.MemberSet is LeafSetName.Zeta or LeafSetName.Gamma or LeafSetName.Theta or LeafSetName.Eta).ToArray() ?? Array.Empty<Element>();
        Alpha = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta or LeafSetName.Gamma).ToArray() ?? Array.Empty<Element>();
        Beta = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta).ToArray() ?? Array.Empty<Element>();
        Zeta = elements.Where(e => e.MemberSet == LeafSetName.Zeta).ToArray() ?? Array.Empty<Element>();
        Gamma = elements.Where(e => e.MemberSet == LeafSetName.Gamma).ToArray() ?? Array.Empty<Element>();
        Theta = elements.Where(e => e.MemberSet == LeafSetName.Theta).ToArray() ?? Array.Empty<Element>();
        Eta = elements.Where(e => e.MemberSet == LeafSetName.Eta).ToArray() ?? Array.Empty<Element>();
        BetaUZeta = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta or LeafSetName.Zeta).ToArray() ?? Array.Empty<Element>();

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

    public (Dust[] Dusts, Dictionary<SetName, int[]> Histograms) BaselinePredictDelta()
    {
        var visitBaseline = "Tps0,Cac0,Ncpv0,Tcpv0,Pav0,LnTps0,LnCac0,LnNcpv0,LnTcpv0,LnPav0".Split(",");
        var elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

        Console.WriteLine("Index, Title, Set, Slope, P-value, Correlation");
        Console.WriteLine($"Processing {visitBaseline.Length * elementDelta.Length} combinations...");

        var dusts = new List<Dust>();
        var skipped = new List<(string chart, string reason)>();
        var histograms = new Dictionary<SetName, int[]>
        {
            { SetName.Omega, new int[5] },
            { SetName.Alpha, new int[5] },
            { SetName.Beta, new int[5] },
            { SetName.Zeta, new int[5] },
            { SetName.Gamma, new int[5] },
            { SetName.Theta, new int[5] },
            { SetName.Eta, new int[5] },
            { SetName.BetaUZeta, new int [5]}    
        };
        _processedRatios.Clear();

        for (var x = 0; x < visitBaseline.Length; x++)
        {
            for (var y = 0; y < elementDelta.Length; y++)
            {
                if (x == y && !visitBaseline[x].StartsWith("Ln") && !elementDelta[y].StartsWith("Ln")) continue;

                var chart = $"{visitBaseline[x]} vs. {elementDelta[y]}";
                try
                {
                    if (!_selectorCache.TryGetValue(chart, out var selector))
                    {
                        selector = new CreateSelector(chart);
                        _selectorCache[chart] = selector;
                    }

                    if (selector.IsLogMismatch)
                    {
                        skipped.Add((chart, "Logarithmic mismatch"));
                        continue;
                    }

                    var dust = Dust(SetName.Omega, chart);
                    if (dust != null)
                    {
                        dusts.Add(dust);
                        Console.WriteLine($"Generated: {chart}, Set: {dust.SetName}, Slope: {dust.Regression.Slope:F2}, P-value: {dust.Regression.PValue():F4}, DataPoints: {dust.Regression.DataPointsCount()}");

                        // Add to histogram
                        var pValue = dust.Regression.PValue();
                        if (!double.IsNaN(pValue) && pValue >= 0 && pValue <= 1)
                        {
                            var bucket = Math.Min((int)(pValue * 5), 4); // Clamp to 0-4
                            histograms[dust.SetName][bucket]++;
                        }
                    }
                    else
                    {
                        skipped.Add((chart, "Null dust (insufficient data points)"));
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Failed to create Dust for {chart}: {ex.Message}");
                    skipped.Add((chart, $"Invalid chart title: {ex.Message}"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error for {chart}: {ex.Message}");
                    skipped.Add((chart, $"Unexpected error: {ex.Message}"));
                }
            }
        }

        // Process ratio charts
        var ratioCombinations = new List<(string num, string denom)>();
        foreach (var num in visitBaseline.Concat(elementDelta))
        {
            foreach (var denom in visitBaseline.Concat(elementDelta))
            {
                if (num != denom) ratioCombinations.Add((num, denom));
            }
        }

        foreach (var (num, denom) in ratioCombinations)
        {
            foreach (var dep in elementDelta)
            {
                var ratio = $"{num}/{denom}";
                var inverseRatio = $"{denom}/{num}";
                var chart = $"{ratio} vs. {dep}";

                if (_processedRatios.Contains(inverseRatio))
                {
                    skipped.Add((chart, "Inverse ratio already processed"));
                    continue;
                }

                try
                {
                    if (!_selectorCache.TryGetValue(chart, out var selector))
                    {
                        selector = new CreateSelector(chart);
                        _selectorCache[chart] = selector;
                    }

                    _processedRatios.Add(ratio);

                    if (selector.IsLogMismatch)
                    {
                        skipped.Add((chart, "Logarithmic mismatch"));
                        continue;
                    }

                    var dust = Dust(SetName.Omega, chart);
                    if (dust != null)
                    {
                        dusts.Add(dust);
                        Console.WriteLine($"Generated: {chart}, Set: {dust.SetName}, Slope: {dust.Regression.Slope:F2}, P-value: {dust.Regression.PValue():F4}, DataPoints: {dust.Regression.DataPointsCount()}");

                        // Add to histogram
                        var pValue = dust.Regression.PValue();
                        if (!double.IsNaN(pValue) && pValue >= 0 && pValue <= 1)
                        {
                            var bucket = Math.Min((int)(pValue * 5), 4); // Clamp to 0-4
                            histograms[dust.SetName][bucket]++;
                        }
                    }
                    else
                    {
                        skipped.Add((chart, "Null dust (insufficient data points)"));
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Failed to create Dust for {chart}: {ex.Message}");
                    skipped.Add((chart, $"Invalid chart title: {ex.Message}"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error for {chart}: {ex.Message}");
                    skipped.Add((chart, $"Unexpected error: {ex.Message}"));
                }
            }
        }

        Console.WriteLine($"Generated {dusts.Count} regressions, skipped {skipped.Count}: {string.Join("; ", skipped.Select(s => $"{s.chart} ({s.reason})"))}");
        return (dusts.ToArray(), histograms);
    }

    public Dust? Dust(SetName setName, string chartTitle)
    {
        if (!_setNameToData.TryGetValue(setName, out var data) || data == null || data.Length == 0)
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
}