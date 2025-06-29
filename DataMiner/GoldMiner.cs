using Keto_Cta;
using LinearRegression;

namespace DataMiner;

public class GoldMiner
{
    public GoldMiner(string path)
    {
        var elements = ReadCsvFile(path);

        // Load elements into sets based on their MemberSet property
        Omega = elements.Where(e =>
            e.MemberSet is LeafSetName.Zeta or LeafSetName.Gamma or LeafSetName.Theta or LeafSetName.Eta).ToArray();
        Alpha = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta or LeafSetName.Gamma).ToArray();
        Beta = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta).ToArray();
        Zeta = elements.Where(e => e.MemberSet == LeafSetName.Zeta).ToArray();
        Gamma = elements.Where(e => e.MemberSet == LeafSetName.Gamma).ToArray();
        Theta = elements.Where(e => e.MemberSet == LeafSetName.Theta).ToArray();
        Eta = elements.Where(e => e.MemberSet == LeafSetName.Eta).ToArray();
        BetaUZeta = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta or LeafSetName.Zeta).ToArray();

        // Initialize _setNameToData after datasets are assigned
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

    private static List<Element> ReadCsvFile(string path)
    {
        var list = new List<Element>();
        using var reader = new StreamReader(path);
        if (!reader.EndOfStream) reader.ReadLine(); // Skip header
        var index = 0;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrEmpty(line)) continue;

            var values = line.Split(',');
            if (values.Length < 10)
            {
                Console.WriteLine($"Skipping line {index + 1}: insufficient values.");
                continue;
            }

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
        var dataPoints = new List<(double x, double y)>();
        dataPoints.AddRange(targetElements.Select(selector));
        var regression = new RegressionPvalue(dataPoints);
        return regression;
    }

    private RegressionPvalue CalculateRegressionRatio(IEnumerable<Element> targetElements, string label,
        Func<Element, (double numerator, double denominator)> xSelector,
        Func<Element, double> ySelector)
    {
        var dataPoints = new List<(double x, double y)>();
        dataPoints.AddRange(targetElements.Select(e =>
        {
            var (numerator, denominator) = xSelector(e);
            double x = denominator != 0 ? numerator / denominator : 0; // Handle division by zero
            double y = ySelector(e);
            return (x, y);
        }));
        var regression = new RegressionPvalue(dataPoints);
        return regression;
    }

    /// <summary>
    /// Mines the data to create a regression for each set based on LnNcp and LnDcac values.
    /// </summary>
    /// <returns>An array of <see cref="Dust"/> objects for supported sets.</returns>
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

    /// <summary>
    /// Generates a collection of <see cref="Dust"/> objects representing the relationships between baseline visit
    /// metrics and delta metrics based on predefined selectors.
    /// </summary>
    /// <remarks>This method evaluates combinations of baseline visit metrics and delta metrics, excluding
    /// cases where the metrics are mismatched in logarithmic scale. For each valid combination, a <see cref="Dust"/> 
    /// object is created and added to the result set. The method outputs diagnostic information to the console during
    /// execution.</remarks>
    /// <returns>An array of <see cref="Dust"/> objects representing the valid metric combinations.</returns>
    public Dust[] BaselinePredictDelta()
    {
        var visitBaseline = "Tps0,Cac0,Ncpv0,Tcpv0,Pav0,LnTps0,LnCac0,LnNcpv0,LnTcpv0,LnPav0".Split(",");
        var elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

        Console.WriteLine("Index, Title, Set, Slope, p-value, Correlation");

        var dusts = new List<Dust>();

        for (var x = 0; x < visitBaseline.Length; x++)
        {
            for (var y = 0; y < elementDelta.Length; y++)
            {
                if (x == y) continue;

                var chart = $"{visitBaseline[x]} vs. {elementDelta[y]}";
                try
                {
                    var dust = Dust(SetName.Omega, chart);
                    if (dust != null)
                    {
                        dusts.Add(dust);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create Dust for {chart}: {ex.Message}");
                }
            }
        }

        return dusts.ToArray();
    }

    /// <summary>
    /// Creates a <see cref="Dust"/> object for the specified set and chart title,
    /// or returns <see langword="null"/> if the set is not supported.
    /// </summary>
    /// <param name="setName">The name of the set for which the <see cref="Dust"/> object is created. Supported values are <see cref="SetName.Omega"/>, <see cref="SetName.Alpha"/>, <see cref="SetName.Beta"/>, <see cref="SetName.Zeta"/>, <see cref="SetName.Gamma"/>, <see cref="SetName.Eta"/>, <see cref="SetName.Theta"/>, and <see cref="SetName.BetaUZeta"/>.</param>
    /// <param name="chartTitle">The title of the chart associated with the <see cref="Dust"/> object.</param>
    /// <returns>A <see cref="Dust"/> object initialized with the specified set name, chart title, and regression, or <see langword="null"/> if <paramref name="setName"/> is not supported.</returns>
    /// <exception cref="ArgumentException">Thrown when the chart title is invalid (e.g., missing 'vs.', same variables, or invalid variable names).</exception>
    /// <exception cref="InvalidOperationException">Thrown when the dataset for the specified set is null or empty.</exception>
    public Dust? Dust(SetName setName, string chartTitle)
    {
        CreateSelector selector;
        try
        {
            selector = new CreateSelector(chartTitle);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid chart title: {ex.Message}", nameof(chartTitle), ex);
        }

        if (selector.IsLogMismatch)
        {
            throw new ArgumentException("Cannot create regression with mismatched logarithmic properties.", nameof(chartTitle));
        }

        if (!_setNameToData.TryGetValue(setName, out var data))
        {
            return null;
        }

        if (data == null || data.Length == 0)
        {
            throw new InvalidOperationException($"Dataset for {setName} is null or empty.");
        }

        var regression = CalculateRegression(data, chartTitle, selector.Selector);
        return new Dust(setName, chartTitle, regression);
    }
}