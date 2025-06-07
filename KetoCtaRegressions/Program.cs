// See https://aka.ms/new-console-template for more information

using DataMiner;
using LinearRegression;

var myMine = new SaltMiner("TestData/keto-cta-quant-and-semi-quant-empty.csv");
var regressions = myMine.MineLnNcpLnDcac();

var i = 0;
foreach (var item in regressions)
{
    i++;
    Console.WriteLine($"[{i}], Slope: {item.Slope():F3}, Intercept: {item.YIntercept():F5}, P-value: {item.PValue():F5}");
}


