// See https://aka.ms/new-console-template for more information

using DataMiner;
using LinearRegression;

var labels = new[] { "Omega", "Alpha", "Zeta", "Beta", "Gamma", "Theta", "Eta" };

var myMine = new SaltMiner("TestData/keto-cta-quant-and-semi-quant-empty.csv");

//Headers
Console.WriteLine("ChartLabel, SetName, Slope, NumberSamples, RSquared, PValue, YIntercept");

//
var regressions = myMine.MineLnDNcpLnDCac();

var chartLabel = "Ln(Delta Ncpv) vs Ln(Delta Cac)";
var i = 0;
foreach (var item in regressions)
{
    Console.WriteLine($" {chartLabel}, {labels[i++]}, {item.Slope():F3}, {item.NumberSamples}, {item.RSquared():F5}, {item.PValue():F5}, {item.YIntercept():F5}");
}

regressions = myMine.MineLnDTpsLnDCac();

chartLabel = "Ln(Delta Tps) vs Ln(Delta Cac)";
i = 0;
foreach (var item in regressions)
{
    Console.WriteLine($" {chartLabel}, {labels[i++]}, {item.Slope():F3}, {item.NumberSamples}, {item.RSquared():F5}, {item.PValue():F5}, {item.YIntercept():F5}");
}

regressions = myMine.MineLnDPavLnDCac();

chartLabel = "Ln(Delta Pav) vs Ln(Delta Cac)";
i = 0;
foreach (var item in regressions)
{
    Console.WriteLine($" {chartLabel}, {labels[i++]}, {item.Slope():F3}, {item.NumberSamples}, {item.RSquared():F5}, {item.PValue():F5}, {item.YIntercept():F5}");
}

regressions = myMine.MineLnDTcpvLnDCac();

chartLabel = "Ln(Delta Tcpv) vs Ln(Delta Cac)";
i = 0;
foreach (var item in regressions)
{
    Console.WriteLine($" {chartLabel}, {labels[i++]}, {item.Slope():F3}, {item.NumberSamples}, {item.RSquared():F5}, {item.PValue():F5}, {item.YIntercept():F5}");
}

