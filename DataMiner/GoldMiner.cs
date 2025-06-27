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
    }

    public Element[] Omega;
    public Element[] Alpha;
    public Element[] Beta;
    public Element[] Zeta;
    public Element[] Gamma;
    public Element[] Theta;
    public Element[] Eta;

    private static List<Element> ReadCsvFile(string path)
    {
        var list = new List<Element>();
        if (list == null) throw new ArgumentNullException(nameof(list));

        using var reader = new StreamReader(path);
        var index = 0;
        // Skip the header line
        if (!reader.EndOfStream) reader.ReadLine();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var values = line.Split(',');
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            var visit1 = new Visit("V1", null, int.Parse(values[0]), int.Parse(values[2]), double.Parse(values[4]),
                double.Parse(values[6]), double.Parse(values[8]));
            var visit2 = new Visit("V2", null, int.Parse(values[1]), int.Parse(values[3]), double.Parse(values[5]),
                double.Parse(values[7]), double.Parse(values[9]));

            index++;
            var element = new Element(index.ToString(), [visit1, visit2]);

            list.Add(element);
        }

        return list;
    }

    RegressionPvalue CalculateRegression(IEnumerable<Element> targetElements, string label,
        Func<Element, (double x, double y)> selector)
    {
        var dataPoints = new List<(double x, double y)>();
        dataPoints.AddRange(targetElements.Select(selector));
        var regression = new RegressionPvalue(dataPoints);
        return regression;
    }

    /// <summary>
    /// Mines the data to create a regression for each set based on LnNcp and LnDcac values.
    /// </summary>
    /// <returns></returns>
    public Dust[] GoldDust(string chartTitle)
    {
        var dataPoints = new List<(double x, double y)>();

        // var selector = GenSelectorFromString(string chartTitle);

        //dataPoints.AddRange(targetElements.Select(selector));
        var regression = new RegressionPvalue(dataPoints);

        return new List<Dust>
        {
            //new Dust(SetName.Omega, regressionTitle, new Regression(Omega.Select(e => e.DNcpv), Omega.Select(e => e.LnDCac))),
            //new Dust(SetName.Alpha, "Alpha", new Regression(Alpha.Select(e => e.Visit1.LnNcp), Alpha.Select(e => e.Visit1.LnDcac))),
            //new Dust(SetName.Zeta, "Zeta", new Regression(Zeta.Select(e => e.Visit1.LnNcp), Zeta.Select(e => e.Visit1.LnDcac))),
            //new Dust(SetName.Beta, "Beta", new Regression(Beta.Select(e => e.Visit1.LnNcp), Beta.Select(e => e.Visit1.LnDcac))),
            //new Dust(SetName.Gamma, "Gamma", new Regression(Gamma.Select(e => e.Visit1.LnNcp), Gamma.Select(e => e.Visit1.LnDcac))),
            //new Dust(SetName.Theta, "Theta", new Regression(Theta.Select(e => e.Visit1.LnNcp), Theta.Select(e => e.Visit1.LnDcac))),
            //new Dust(SetName.Eta, "Eta", new Regression(Eta.Select(e => e.Visits[0].LnNcpv), Eta.Select(e => e.Visit1.LnDcac)))
        }.ToArray();
    }

    public string[] ChartsForVisitVsDelement()
    {
        var deltaAttributes = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");
        var visitAttributes = "Tps,Cac,Ncpv,Tcpv,DPav,LnTps,LnCac,LnNcpv,LnTcpv,LnPav".Split(",");

        var charts = new List<string>();
        for (var dVisit = 0; dVisit < 2; dVisit++)
            for (var x = 0; x < visitAttributes.Length; x++)
                for (var y = 0; y < deltaAttributes.Length; y++)
                    if (x != y)
                        charts.Add($"{visitAttributes[x]}{dVisit} vs. {deltaAttributes[y]}");

        return charts.ToArray();

    }

    public Dust Dust(SetName setName, string chartTitle)
    {
        var selector = new CreateSelector(chartTitle);

        if (setName == SetName.Omega)
        {
            var regression = CalculateRegression(Omega, chartTitle, selector.Selector);
            return new Dust(setName, chartTitle, regression);
        }

        return null;
    }
} 