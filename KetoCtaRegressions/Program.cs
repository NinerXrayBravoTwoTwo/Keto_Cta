// See https://aka.ms/new-console-template for more information

using DataMiner;
using LinearRegression;

var labels = new[] { "Omega", "Alpha", "Zeta", "Beta", "Gamma", "Theta", "Eta" };

var myMine = new SaltMiner("TestData/keto-cta-quant-and-semi-quant-empty.csv");
var regressions = myMine.MineLnNcpLnDcac();

Console.WriteLine("Ln(Delta Ncpv) vs Ln(Delta Cac)\n");
var i = 0;
foreach (var item in regressions)
{
    Console.WriteLine($"{labels[i++]}, Slope: {item.Slope():F3}, N={item.NumberSamples}, P-value: {item.PValue():F5}, Intercept: {item.YIntercept():F5}");
}
