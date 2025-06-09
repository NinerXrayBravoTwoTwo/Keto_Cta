// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Text.RegularExpressions;
using DataMiner;
using LinearRegression;

var labels = new[] { "Omega", "Alpha", "Zeta", "Beta", "Gamma", "Theta", "Eta", "BetaUZeta" };

var myMine = new SaltMiner("TestData/keto-cta-quant-and-semi-quant-empty.csv");

//Headers
Console.WriteLine("index, ChartLabel, SetName, Slope, NumberSamples, RSquared, PValue, YIntercept, MeanX, MeanY");

var count = 0;

var allRegressions = new List<RegressionPvalue>();
var allChartLabels = new List<string>();
var allSetNames = new List<string>();

string FormatRegression(RegressionPvalue item, string set, string chartLabel, int index)
{
    allRegressions.Add(item);
    allChartLabels.Add(chartLabel);
    allSetNames.Add(set);
    return $"{count}, {chartLabel}, {set}, {item.Slope():F3}, {item.N}, {item.RSquared():F5}, {item.PValue():F9}, {item.YIntercept():F5}, {item.MeanX():F2}, {item.MeanY():F5}";
}

#region Ln-Ln CTA vs Ln cac
var regressions = myMine.MineLnDNcpLnDCac();

var chartLabel = "ln-ln (D Ncpv vs. D Cac)";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineLnDTpsLnDCac();

chartLabel = "ln-ln (D Tps vs. D Cac)";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineLnDPavLnDCac();

chartLabel = "ln-ln (D Pav vs. D Cac)";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineLnDTcpvLnDCac();

chartLabel = "ln-ln (D Tcpv) vs. D Cac)";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));
#endregion

#region CTA vs cac
regressions = myMine.MineDNcpvDCac();
chartLabel = "D Ncpv vs D Cac";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineDTpsDCac();
chartLabel = "D Tps vs D Cac";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineDPavDCac();
chartLabel = "D Pav vs D Cac";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineDTcpvDCac();
chartLabel = "D Tcpv vs D Cac";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

#endregion

#region CTA1 vs. CTA2

regressions = myMine.MineNcpv1Ncpv2();
chartLabel = "Ncpv0 vs Ncpv1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineTps1Tps2();
chartLabel = "Tps0 vs Tps1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MinePav1Pav2();
chartLabel = "Pav0 vs Pav1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineTcpv1Tcpv2();
chartLabel = "Tcpv0 vs Tcpv1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineCac1Cac2();
chartLabel = "Cac0 vs Cac1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

#endregion

#region ln-ln CTA1 vs. CTA2

regressions = myMine.MineLnNcpv1LnNcpv2();
chartLabel = "ln-ln Ncpv0 vs. Ncpv1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineLnTps1LnTps2();
chartLabel = "ln-ln Tps0 vs. Tps1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineLnPav1LnPav2();
chartLabel = "ln-ln Pav0 vs. Pav1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineLnTcpv1LnTcpv2();
chartLabel = "ln-ln Tcpv0 vs. Tcpv1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

regressions = myMine.MineLnCac1LnCac2();
chartLabel = "ln-ln Cac0 vs. Cac1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel, count++))));

#endregion

// Print the regression data points for a specific regression

const int chartIdx = 134; // Example index for the regression you want to print
var target = allRegressions[chartIdx - 1];
var iamThis = allChartLabels[chartIdx - 1];
var iamInSet = allSetNames[chartIdx - 1];

Console.WriteLine($"\n-,-,Regression: {chartIdx} slope: {target.Slope():F4}");

Console.WriteLine($"-,-,'{iamThis} - {iamInSet}' slope; {target.Slope():F4} N={target.N} R^2: {target.RSquared():F4} p-value: {target.PValue():F6}\n");

var regSplit = Regex.Split(iamThis, "\\s+vs.\\s*", RegexOptions.IgnoreCase);

//var isLogLog = Regex.IsMatch(iamThis, "log.log|ln.ln", RegexOptions.IgnoreCase);

var xxx = regSplit[0];
var yyy = regSplit[1];

if (Regex.IsMatch(iamThis, "log.log|ln.ln", RegexOptions.IgnoreCase))
{
    xxx = Regex.Replace(regSplit[0], "(log.log|ln.ln)\\s*", "");

    xxx = $"Ln(|{xxx}|+1)";
    yyy = $"Ln(|{regSplit[1]}|+1)";
}

Console.WriteLine($"{xxx}, {yyy}"); // not log-log split

foreach (var point in target.DataPoints)
{
    Console.WriteLine($"{point.x}, {point.y}");
}
