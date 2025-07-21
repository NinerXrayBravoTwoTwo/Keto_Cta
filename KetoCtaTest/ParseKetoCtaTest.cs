using Keto_Cta;
using LinearRegression;
using System.Text;
using Xunit.Abstractions;

namespace KetoCtaTest;

public class ParseKetoCtaTest(ITestOutputHelper testOutputHelper)
{
    //[Fact]
    //public void ReadCsvFile_InvalidFile_ShouldThrowFileNotFoundException()
    //{
    //    const string filePath = "TestData/invalid-file.csv";
    //    Assert.Throws<FileNotFoundException>(() => ReadCsvFile(filePath));
    //}

    [Fact]
    public void ReadCsvFile_ValidFile_ShouldReturnElementsList()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);
        Assert.NotEmpty(elements);
        Assert.Equal(100, elements.Count);
        Assert.Equal("1", elements[0].Id);
        Assert.Equal("100", elements[99].Id);
    }

    [Theory]
    [InlineData("TestData/keto-cta-quant-and-semi-quant.csv", 100)]
    //[InlineData("TestData/keto-cta-quant-and-semi-quant-empty.csv", 0)]

    public void ReadCsvFile_ValidFile_ShouldReturnCorrectElementCount(string filePath, int expectedCount)
    {
        var elements = ReadCsvFile(filePath);
        Assert.Equal(expectedCount, elements.Count);
    }

    #region DCleery vs DCac
    [Fact]
    public void DoRegressionDNcpvDCac()
    {
        const string path = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(path);

        var omegas = Elements(elements,
            out var alphas,
            out var zetas,
            out var gammas,
            out var thetas,
            out var etas);

        testOutputHelper.WriteLine($"\nDNcpv vs DCac\n");
        var rOmega = RegressionDNcpvDCac(omegas, "DNcpv vs DCac - Omega");
        var rAlphas = RegressionDNcpvDCac(alphas, "DNcpv vs DCac - Alpha");
        var rZeta = RegressionDNcpvDCac(zetas, "DNcpv vs DCac - Zeta");
        var rGamma = RegressionDNcpvDCac(gammas, "DNcpv vs DCac - Gamma");
        var rTheta = RegressionDNcpvDCac(thetas, "DNcpv vs DCac - Theta");
        var rEta = RegressionDNcpvDCac(etas, "DNcpv vs DCac - Etas");
        RegressionPvalue RegressionDNcpvDCac(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(item => (item.DNcpv, item.DCac)));
            var regression = new RegressionPvalue(dataPoints);

            testOutputHelper.WriteLine(Message(label, regression));

            return regression;
        }
    }

    [Fact]
    public void DoRegressionLnTpsLnDCac()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);
        var omegas = Elements(elements,
            out var alphas,
            out var zetas,
            out var gammas,
            out var thetas,
            out var etas);
        testOutputHelper.WriteLine($"\nLnDNcpv vs LnDCac\n");
        var rOmega = RegressionLnDTpsLnDCac(omegas, "Omega");
        var rAlphas = RegressionLnDTpsLnDCac(alphas, "Alpha");
        var rZeta = RegressionLnDTpsLnDCac(zetas, "Zeta");
        var rGamma = RegressionLnDTpsLnDCac(gammas, "Gamma");
        var rTheta = RegressionLnDTpsLnDCac(thetas, "Theta");
        var rEta = RegressionLnDTpsLnDCac(etas, "Etas");
        return;
        RegressionPvalue RegressionLnDTpsLnDCac(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(item => (item.LnDTps, item.LnDCac)));
            var regression = new RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine(Message(label, regression));
            return regression;
        }
    }

    [Fact]
    public void DoRegressionLnDPavLnDCac()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);
        var omegas = Elements(elements,
            out var alphas,
            out var zetas,
            out var gammas,
            out var thetas,
            out var etas);
        testOutputHelper.WriteLine($"\nLnDNcpv vs LnDCac\n");
        var rOmega = RegressionLnDNcpvLnDCac(omegas, "Omega");
        var rAlphas = RegressionLnDNcpvLnDCac(alphas, "Alpha");
        var rZeta = RegressionLnDNcpvLnDCac(zetas, "Zeta");
        var rGamma = RegressionLnDNcpvLnDCac(gammas, "Gamma");
        var rTheta = RegressionLnDNcpvLnDCac(thetas, "Theta");
        var rEta = RegressionLnDNcpvLnDCac(etas, "Etas");
        return;
        RegressionPvalue RegressionLnDNcpvLnDCac(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(item => (item.LnDPav, item.LnDCac)));
            var regression = new RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine(Message(label, regression));
            return regression;
        }
    }

    [Fact]
    public void DoRegressionLnDTcpvLnDCac()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);
        var omegas = Elements(elements,
            out var alphas,
            out var zetas,
            out var gammas,
            out var thetas,
            out var etas);
        testOutputHelper.WriteLine($"\nLnDNcpv vs LnDCac\n");
        var rOmega = RegressionLnDTcpvLnDCac(omegas, "Omega");
        var rAlphas = RegressionLnDTcpvLnDCac(alphas, "Alpha");
        var rZeta = RegressionLnDTcpvLnDCac(zetas, "Zeta");
        var rGamma = RegressionLnDTcpvLnDCac(gammas, "Gamma");
        var rTheta = RegressionLnDTcpvLnDCac(thetas, "Theta");
        var rEta = RegressionLnDTcpvLnDCac(etas, "Etas");
        return;
        RegressionPvalue RegressionLnDTcpvLnDCac(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(item => (item.LnDTcpv, item.LnDCac)));
            var regression = new RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine(Message(label, regression));
            return regression;
        }
    }

    #endregion

    #region v1-Log  vs. v2-Log
    [Fact]
    public void DoRegressionLnDNcpvLnDCac()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);

        var omegas = Elements(elements,
            out var alphas,
            out var zetas,
            out var gammas,
            out var thetas,
            out var etas);

        testOutputHelper.WriteLine($"\nLnNcpv vs LnCac\n");
        var rOmega = RegressionLnNcpvLnDCac(omegas, "Omega");
        var rAlphas = RegressionLnNcpvLnDCac(alphas, "Alpha");
        var rZeta = RegressionLnNcpvLnDCac(zetas, "Zeta");
        var rGamma = RegressionLnNcpvLnDCac(gammas, "Gamma");
        var rTheta = RegressionLnNcpvLnDCac(thetas, "Theta");
        var rEta = RegressionLnNcpvLnDCac(etas, "Etas");

        PrintTable(thetas, "Set Theta");
        PrintTable(etas, "Set Eta");

        return;

        RegressionPvalue RegressionLnNcpvLnDCac(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();

            dataPoints.AddRange(targetElements.Select(item => (item.LnDNcpv, LnDCp: item.LnDCac)));

            var regression = new RegressionPvalue(dataPoints);

            testOutputHelper.WriteLine(Message(label, regression));

            return regression;
        }
    }

    [Fact]
    public void DoRegressLnV1V2Ncpv()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);

        var omegas = Elements(elements,
            out var alphas,
            out var zetas,
            out var gammas,
            out var thetas,
            out var etas);

        //LnNcpv1 vs LnNcpv2
        testOutputHelper.WriteLine("\nLnNcpv vs LnCac\n");
        var sOmegas = RegressLnV1V2Ncpv(omegas, "Omega V1 vs V2 Ncpv");
        var sAlphas = RegressLnV1V2Ncpv(alphas, "Alpha V1 vs V2 Ncpv");
        var sZetas = RegressLnV1V2Ncpv(zetas, "Zeta V1 vs V2 Ncpv");
        var sGammas = RegressLnV1V2Ncpv(gammas, "Gamma V1 vs V2 Ncpv");
        var sThetas = RegressLnV1V2Ncpv(thetas, "Theta V1 vs V2 Ncpv");
        var sEtas = RegressLnV1V2Ncpv(etas, "Eta V1 vs V2 Ncpv");

        // Generate a .csv list for favorite stacked plaque graph
        // X= col 1 - id + v1.ncpv +v2.deltaNcpv 

        //// Stacked NCPV bar graph dumper
        //testOutputHelper.WriteLine("\n\index, v1.Ncpv, DNcpv,v1.LnNcpv, LnDNcpv");
        //foreach (var item in omegas)
        //    testOutputHelper.WriteLine($"{item.Id},{item.Visits[0].Ncpv:F4}, {item.DNcpv:F4}, {item.Ln(item.Visits[0].Ncpv):F4}, {item.LnDNcpv:F4}");

        testOutputHelper.WriteLine("\nZeta\n");

        testOutputHelper.WriteLine(
             "index, "
             + "v1.LnNcpv, v2.LnCpv, "
             + "v1.Ncpv, v2.Ncpv, "
             + "v1.Tps, v2.Tps, "
             + "v1.Cac, v2.Cac, "
             + "DTps, DCac, DNcpv, DTcpv, DPav, Regressed Metrics"); // if (cac2 < cac1 || tps2 < tps1 || ncpv1 < ncpv2 || ... ) then zeta   

        // Lnv1.ncpv vs Lnv2.v2.Ncpv
        foreach (var item in zetas)
        {
            var v1 = item.Visits[0];
            var v2 = item.Visits[1];
            var sb = new StringBuilder();

            if (item.DTps < 0) sb.Append("Tps ");
            if (item.DCac < 0) sb.Append("Cac ");
            if (item.DNcpv < 0) sb.Append("Ncpv ");
            if (item.DTcpv < 0) sb.Append("Tcpv ");
            if (item.DPav < 0) sb.Append("Pav ");

            testOutputHelper.WriteLine(
                $"{item.Id},{item.Visits[0].LnNcpv:F4}, {item.Visits[1].LnNcpv:F4}, "
                + $"{v1.Ncpv:F4}, {v2.Ncpv:F4}, "
                + $"{v1.Tps:F4}, {v2.Tps:F4}, "
                + $"{v1.Cac:F4}, {v2.Cac:F4}, "
                + $"{item.DTps:F4}, {item.DCac:F4}, {item.DNcpv:F4}, {item.DTcpv:F4}, {item.DPav:F4}, {sb}"
            );
        }

        return;


        RegressionPvalue RegressLnV1V2Ncpv(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();

            dataPoints.AddRange(targetElements.Select(
                item => (item.Visits[0].LnNcpv, item.Visits[1].LnNcpv)));

            var regression = new RegressionPvalue(dataPoints);

            testOutputHelper.WriteLine(Message(label, regression));

            return regression;
        }
    }
    #endregion

    #region v1 vs. v2 
    [Fact]
    public void DoRegressionV1V2Tps()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);

        var omegas = Elements(elements,
            out var alphas,
            out var zetas,
            out var gammas,
            out var thetas,
            out var etas);

        testOutputHelper.WriteLine("\nV1 vs V2, Tps\n");
        var rOmega = RegressionV1V2Tps(omegas, "Omega");
        var rAlphas = RegressionV1V2Tps(alphas, "Alpha");
        var rZeta = RegressionV1V2Tps(zetas, "Zeta");
        var rGamma = RegressionV1V2Tps(gammas, "Gamma");
        var rTheta = RegressionV1V2Tps(thetas, "Theta");
        var rEta = RegressionV1V2Tps(etas, "Etas");

        return;


        RegressionPvalue RegressionV1V2Tps(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(item =>
                ((double)item.Visits[0].Tps, (double)item.Visits[1].Tps)));

            var regression = new RegressionPvalue(dataPoints);

            testOutputHelper.WriteLine(Message(label, regression));

            return regression;
        }
    }

    [Fact]
    public void DoRegressionV1V2Cac()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);

        var omegas = Elements(elements,
            out var alphas,
            out var zetas,
            out var gammas,
            out var thetas,
            out var etas);

        testOutputHelper.WriteLine($"\nV1 vs V2, Cac\n");
        var rOmega = RegressionV1V2Cac(omegas, "Omega");
        var rAlphas = RegressionV1V2Cac(alphas, "Alpha");
        var rZeta = RegressionV1V2Cac(zetas, "Zeta");
        var rGamma = RegressionV1V2Cac(gammas, "Gamma");
        var rTheta = RegressionV1V2Cac(thetas, "Theta");
        var rEta = RegressionV1V2Cac(etas, "Etas");

        return;


        RegressionPvalue RegressionV1V2Cac(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();

            dataPoints.AddRange(targetElements.Select(item =>
               ((double)item.Visits[0].Cac, (double)item.Visits[1].Cac)));

            var regression = new RegressionPvalue(dataPoints);

            testOutputHelper.WriteLine(Message(label, regression));


            return regression;
        }
    }

    [Fact]
    public void DoRegressionV1V2Tcpv()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);

        var omegas = Elements(elements,
            out var alphas,
            out var zetas,
            out var gammas,
            out var thetas,
            out var etas);

        testOutputHelper.WriteLine($"\nV1 vs V2, Tcp\n");
        var rOmega = RegressionV1V2Tcpv(omegas, "Omega");
        var rAlphas = RegressionV1V2Tcpv(alphas, "Alpha");
        var rZeta = RegressionV1V2Tcpv(zetas, "Zeta");
        var rGamma = RegressionV1V2Tcpv(gammas, "Gamma");
        var rTheta = RegressionV1V2Tcpv(thetas, "Theta");
        var rEta = RegressionV1V2Tcpv(etas, "Etas");

        return;


        RegressionPvalue RegressionV1V2Tcpv(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();

            dataPoints.AddRange(targetElements.Select(item =>
                (item.Visits[0].Tcpv, item.Visits[1].Tcpv)));

            var regression = new RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine(Message(label, regression));


            return regression;
        }
    }
    #endregion

    private Element[] Elements(List<Element> elements, out Element[] alphas, out Element[] zetas, out Element[] gammas,
        out Element[] thetas, out Element[] etas)
    {
        var omegas = elements.Where(e => e.MemberSet is LeafSetName.Zeta or LeafSetName.Gamma or LeafSetName.Theta or LeafSetName.Eta).ToArray();

        alphas = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta or LeafSetName.Gamma).ToArray();
        zetas = elements.Where(e => e.MemberSet == LeafSetName.Zeta).ToArray();
        gammas = elements.Where(e => e.MemberSet == LeafSetName.Gamma).ToArray();
        thetas = elements.Where(e => e.MemberSet == LeafSetName.Theta).ToArray();
        etas = elements.Where(e => e.MemberSet == LeafSetName.Eta).ToArray();

        testOutputHelper.WriteLine($"omega count: {omegas.Length}");
        testOutputHelper.WriteLine($"alpha count: {alphas.Length}");
        testOutputHelper.WriteLine($"zeta count: {zetas.Length}");
        testOutputHelper.WriteLine($"gamma count: {gammas.Length}");
        testOutputHelper.WriteLine($"Theta count: {thetas.Length}");
        testOutputHelper.WriteLine($"Eta count: {etas.Length}");
        return omegas;
    }

    private void PrintTable(Element[] setElements, string label)
    {
        testOutputHelper.WriteLine(
            $"\n{label}\n"
            + "index, "
            + "LnNcpv, LnCac, "

            + "v1.Ncpv, v2.Ncpv, "
            + "v1.Cac, v2.Cac, "
            + "v1.Tps, v2.Tps, "
            + "v1.Pav, v2.Pav, "
            + "v1.Tcpv, v2.Tcpv, "
            + "DTps, DCac, DNcpv, DTcpv, DPav"); // if (cac2 < cac1 || tps2 < tps1 || ncpv1 < ncpv2 || ... ) then zeta   


        // Lnv1.ncpv vs Lnv2.v2.Ncpv
        foreach (var item in setElements)
        {
            var v1 = item.Visits[0];
            var v2 = item.Visits[1];

            testOutputHelper.WriteLine(
                $"{item.Id},{item.LnDNcpv:F4}, {item.LnDCac:F4}, "
                + $"{v1.Ncpv:F4}, {v2.Ncpv:F4}, "
                + $"{v1.Cac:F4}, {v2.Cac:F4}, "
                + $"{v1.Tps:F4}, {v2.Tps:F4}, "
                + $"{v1.Pav:F4}, {v2.Pav:F4}, "
                + $"{v1.Tcpv:F4}, {v2.Tcpv:F4}, "
                + $"{item.DTps:F4}, {item.DCac:F4}, {item.DNcpv:F4}, {item.DTcpv:F4}, {item.DPav:F4}"
            );
        }
    }

    private static string Message(string label, RegressionPvalue regression)
    {
        return $"'{label}' slope: {regression.Slope():F5} N: {regression.N} R2: {regression.RSquared():F5} PValue: {regression.PValue():F5} Y-intercept: {regression.YIntercept():F4}";
    }

    private static List<Element> ReadCsvFile(string path)
    {
        var list = new List<Element>();

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