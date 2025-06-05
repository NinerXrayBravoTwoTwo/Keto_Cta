using System.Text;
using Keto_Cta;
using LinearRegression;
using Xunit.Abstractions;

namespace KetoCtaTest;

public class ParseKetoCtaTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void ReadCsvFile_ValidFile_ShouldReturnListOfElements()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);
        Assert.Equal("1", elements[0].Id);
        Assert.Equal("100", elements[99].Id);

        var omegas = elements.Where(e => e.MemberSet is SetName.Zeta or SetName.Gamma or SetName.Theta or SetName.Eta)
            .ToArray();
        var alphas = elements.Where(e => e.MemberSet is SetName.Theta or SetName.Eta or SetName.Gamma).ToArray();
        var zetas = elements.Where(e => e.MemberSet == SetName.Zeta).ToArray();
        var gammas = elements.Where(e => e.MemberSet == SetName.Gamma).ToArray();
        var thetas = elements.Where(e => e.MemberSet == SetName.Theta).ToArray();
        var etas = elements.Where(e => e.MemberSet == SetName.Eta).ToArray();

        testOutputHelper.WriteLine($"omega count: {omegas.Length}");
        testOutputHelper.WriteLine($"alpha count: {alphas.Length}");
        testOutputHelper.WriteLine($"zeta count: {zetas.Length}");
        testOutputHelper.WriteLine($"gamma count: {gammas.Length}");
        testOutputHelper.WriteLine($"Theta count: {thetas.Length}");
        testOutputHelper.WriteLine($"Eta count: {etas.Length}\n");

        var rOmega = RegressionLnNcpvLnDcac(omegas, "Omega");
        var rAlphas = RegressionLnNcpvLnDcac(alphas, "Alpha");
        var rZeta = RegressionLnNcpvLnDcac(zetas, "Zeta");
        var rGamma = RegressionLnNcpvLnDcac(gammas, "Gamma");
        var rTheta = RegressionLnNcpvLnDcac(thetas, "Theta");
        var rEta = RegressionLnNcpvLnDcac(etas, "Etas");
        //
        var sOmegas = RegressLnV1V2Ncpv(omegas, "\nOmega V1 vs V2 Ncpv");
        var sAlphas = RegressLnV1V2Ncpv(alphas, "Alpha V1 vs V2 Ncpv");
        var sZetas = RegressLnV1V2Ncpv(zetas, "Zeta V1 vs V2 Ncpv");
        var sGammas = RegressLnV1V2Ncpv(gammas, "Gamma V1 vs V2 Ncpv");
        var sThetas = RegressLnV1V2Ncpv(thetas, "Theta V1 vs V2 Ncpv");
        var sEtas = RegressLnV1V2Ncpv(etas, "Eta V1 vs V2 Ncpv");

        // Generate a .csv list for favorite stacked plaque graph
        // X= col 1 - id + v1.ncpv +v2.deltaNcpv 

        //// Stacked NCPV bar graph dumper
        //testOutputHelper.WriteLine("\n\nindex, v1.Ncpv, DNcpv,v1.LnNcpv, LnDNcpv");
        //foreach (var item in omegas)
        //    testOutputHelper.WriteLine($"{item.Id},{item.Visits[0].Ncpv:F4}, {item.DNcpv:F4}, {item.Ln(item.Visits[0].Ncpv):F4}, {item.LnDNcpv:F4}");

        testOutputHelper.WriteLine(
            "\nindex, "
            + "v1.LnNcpv, v2.LnCpv, "
            + "v1.Ncpv, v2.Ncpv, "
            + "v1.Tps, v2.Tps, "
            + "v1.Cac, v2.Cac, "
            + "DTps, DCac, DNcpv, DTcpv, DPav, Why?"); // if (cac2 < cac1 || tps2 < tps1) then zeta   

        // Regression of Lnv1.ncpv vs Lnv2.v2.Ncpv
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
                $"{item.Id},{item.Ln(v1.Ncpv):F4}, {item.Ln(v2.Ncpv):F4}, "
                + $"{v1.Ncpv:F4}, {v2.Ncpv:F4}, "
                + $"{v1.Tps:F4}, {v2.Tps:F4}, "
                + $"{v1.Cac:F4}, {v2.Cac:F4}, "
                + $"{item.DTps:F4}, {item.DCac:F4}, {item.DNcpv:F4}, {item.DTcpv:F4}, {item.DPav:F4}, {sb}"
            );
        }

        RegressionPvalue RegressionLnNcpvLnDcac(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(item => (item.LnDNcpv, LnDCp: item.LnDCac)));
            var regression = new RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine(
                $"'{label}' slope: {regression.Slope():F5}  R2: {regression.RSquared():F5} PValue: {regression.PValue():F4} N: {regression.NumberSamples}");

            return regression;
        }

        RegressionPvalue RegressLnV1V2Ncpv(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(item =>
                (item.Ln(item.Visits[0].Ncpv), item.Ln(item.Visits[1].Ncpv))));
            var regression = new RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine(
                $"'{label}' slope: {regression.Slope():F5}  R2: {regression.RSquared():F5} PValue: {regression.PValue():F4} N: {regression.NumberSamples}");

            return regression;
        }
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