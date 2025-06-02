using Keto_Cta;
using Xunit.Abstractions;

namespace KetoCtaTest;

public class ParseKetoCtaTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void ReadCsvFile_ValidFile_ShouldReturnListOfElements()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);

        // Example output
        foreach (var element in elements) _testOutputHelper.WriteLine($"Name: {element}");
    }

    private static List<Element> ReadCsvFile(string path)
    {
        var list = new List<Element>();

        using var reader = new StreamReader(path);
        var index = 0;

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