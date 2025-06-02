using Keto_Cta;
using Xunit.Abstractions;
using System.Linq;

namespace KetoCtaTest;

public class ParseKetoCtaTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void ReadCsvFile_ValidFile_ShouldReturnListOfElements()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);
        var omegas = elements.Where(e => e.MemberSet is SetName.Zeta or SetName.Gamma or SetName.Theta or SetName.Eta);
        var zetas = elements.Where(e => e.MemberSet == SetName.Zeta);
        var gammas = elements.Where(e => e.MemberSet == SetName.Gamma);
        var  thetas= elements.Where(e => e.MemberSet == SetName.Theta) ;
        var etas = elements.Where(e => e.MemberSet == SetName.Eta);

        _testOutputHelper.WriteLine($"omega count: {omegas.Count()})");
        _testOutputHelper.WriteLine($"zeta count: {zetas.Count()}");
        _testOutputHelper.WriteLine($"gamma count: {gammas.Count()}");
        _testOutputHelper.WriteLine($"Theta count: {thetas.Count()}");
        _testOutputHelper.WriteLine($"Eta count: {etas.Count()}");

        Assert.Equal(elements[0].Id, "1" );
        Assert.Equal(elements[99].Id, "100");

    }

    private static List<Element> ReadCsvFile(string path)
    {
        var list = new List<Element>();

        using var reader = new StreamReader(path);
        var index = 0;
        // Skip the header line
        if (!reader.EndOfStream)
        {
            reader.ReadLine();
        }   
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