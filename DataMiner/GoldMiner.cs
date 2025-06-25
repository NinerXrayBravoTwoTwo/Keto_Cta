using Keto_Cta;
using LinearRegression;

namespace DataMiner;

public class Gold
{

    public Gold(string path)
    {
        var elements = ReadCsvFile(path);

        // Load elements into sets based on their MemberSet property

        Omega = elements.Where(e => e.MemberSet is SetName.Zeta or SetName.Gamma or SetName.Theta or SetName.Eta).ToArray();
        Alpha = elements.Where(e => e.MemberSet is SetName.Theta or SetName.Eta or SetName.Gamma).ToArray();
        Beta = elements.Where(e => e.MemberSet is SetName.Theta or SetName.Eta).ToArray();
        Zeta = elements.Where(e => e.MemberSet == SetName.Zeta).ToArray();
        Gamma = elements.Where(e => e.MemberSet == SetName.Gamma).ToArray();
        Theta = elements.Where(e => e.MemberSet == SetName.Theta).ToArray();
        Eta = elements.Where(e => e.MemberSet == SetName.Eta).ToArray();
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



}

public class Dust
{
    public Dust(SetName set, string title, RegressionPvalue regression)
    {
        _setName = set;
        Title = title;
        regression = regression ?? throw new ArgumentNullException(nameof(regression));
    }

    public readonly SetName _setName;
    public readonly string Title;
    public readonly Regression _Regression;

}