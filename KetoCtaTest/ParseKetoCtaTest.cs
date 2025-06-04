using System.Collections;
using System.Runtime.Intrinsics;
using Keto_Cta;
using Xunit.Abstractions;
using LinearRegression;

namespace KetoCtaTest;

public class ParseKetoCtaTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void ReadCsvFile_ValidFile_ShouldReturnListOfElements()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var elements = ReadCsvFile(filePath);
        var x = new RegressionPvalue();
        Assert.Equal("1", elements[0].Id);
        Assert.Equal("100", elements[99].Id);

        var omegas = elements.Where(e => e.MemberSet is SetName.Zeta or SetName.Gamma or SetName.Theta or SetName.Eta);
        var alphas = elements.Where(e => e.MemberSet is SetName.Theta or SetName.Eta or SetName.Gamma);
        var zetas = elements.Where(e => e.MemberSet == SetName.Zeta);
        var gammas = elements.Where(e => e.MemberSet == SetName.Gamma);
        var thetas = elements.Where(e => e.MemberSet == SetName.Theta);
        var etas = elements.Where(e => e.MemberSet == SetName.Eta);

        testOutputHelper.WriteLine($"omega count: {omegas.Count()}");
        testOutputHelper.WriteLine($"alpha count: {alphas.Count()}");
        testOutputHelper.WriteLine($"zeta count: {zetas.Count()}");
        testOutputHelper.WriteLine($"gamma count: {gammas.Count()}");
        testOutputHelper.WriteLine($"Theta count: {thetas.Count()}");
        testOutputHelper.WriteLine($"Eta count: {etas.Count()}\n");

        var r_omega = RegressiondLnNcpvLnDcac(omegas, "Omega");
        var r_alphas = RegressiondLnNcpvLnDcac(alphas, "Alpha");
        var r_zeta = RegressiondLnNcpvLnDcac(zetas, "Zeta");
        var r_gamma = RegressiondLnNcpvLnDcac(gammas, "Gamma");
        var r_theta = RegressiondLnNcpvLnDcac(thetas, "Theta");
        var r_eta = RegressiondLnNcpvLnDcac(etas, "Etas");

        var s_alphas=RegressLnV1V2Ncpv(alphas, "Alpha V1 vs V2 Ncpv");
        var s_zetas=RegressLnV1V2Ncpv(zetas, "Zeta V1 vs V2 Ncpv");
        
        // Generate a .csv list for daves favorite stacked plaque graph
        // X= col 1 - id + v1.ncpv +v2.deltaNcpv 

        //// Stacked NCPV bar graph dumper
        //testOutputHelper.WriteLine("\n\nindex, v1.Ncpv, v2.Ncpv");
        //foreach (var item in omegas)
        //    testOutputHelper.WriteLine($"{item.Id},{item.Ln(item.Visits[0].Ncpv):F4}, {item.LnDNcpv:F4}");

        testOutputHelper.WriteLine("\n\nindex, v1.Ncpv, v2.Ncpv");
        // Regression of Lnv1.ncpv vs Lnv2.v2.Ncpv
        foreach (var item in alphas)
            testOutputHelper.WriteLine($"{item.Id},{item.Ln(item.Visits[0].Ncpv):F4}, {item.Ln(item.Visits[1].Ncpv):F4}");


        RegressionPvalue RegressiondLnNcpvLnDcac(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(item => (item.LnDNcpv, LnDCp: item.LnDCac)));
            var regression = new LinearRegression.RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine($"'{label}' slope: {regression.Slope():F5}  R2: {regression.RSquared():F5} PValue: {regression.PValue():F4} N: {regression.NumberSamples}");

            return regression;
        }

        RegressionPvalue RegressLnV1V2Ncpv(IEnumerable<Element> targetElements, string label)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(item => (item.Ln(item.Visits[0].Ncpv), item.Ln(item.Visits[1].Ncpv) )));
            var regression = new LinearRegression.RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine($"'{label}' slope: {regression.Slope():F5}  R2: {regression.RSquared():F5} PValue: {regression.PValue():F4} N: {regression.NumberSamples}");

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