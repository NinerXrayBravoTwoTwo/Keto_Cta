// See https://aka.ms/new-console-template for more information

using DataMiner;
using LinearRegression;

var labels = new[] { "Omega", "Alpha", "Zeta", "Beta", "Gamma", "Theta", "Eta" };

var myMine = new SaltMiner("TestData/keto-cta-quant-and-semi-quant-empty.csv");

//Headers
Console.WriteLine("ChartLabel, SetName, Slope, NumberSamples, RSquared, PValue, YIntercept, MeanX, MeanY");

string FormatRegression(RegressionPvalue item, string label, string chartLabel) =>
    $"{chartLabel}, {label}, {item.Slope():F3}, {item.NumberSamples}, {item.RSquared():F5}, {item.PValue():F9}, {item.YIntercept():F5}, {item.MeanX():F2}, {item.MeanY():F5}";

#region Ln CTA vs Ln cac
var regressions = myMine.MineLnDNcpLnDCac();

var chartLabel = "log-log (D Ncpv vs. D Cac)";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

regressions = myMine.MineLnDTpsLnDCac();

chartLabel = "log-log (D Tps vs. D Cac)";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

regressions = myMine.MineLnDPavLnDCac();

chartLabel = "log-log (D Pav vs. D Cac)";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

regressions = myMine.MineLnDTcpvLnDCac();

chartLabel = "log-log (D Tcpv) vs. D Cac)";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));
#endregion

#region CTA vs cac
regressions = myMine.MineDNcpvDCac();
chartLabel = "D Ncpv vs D Cac";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

regressions = myMine.MineDTpsDCac();
chartLabel = "D Tps vs D Cac";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

regressions = myMine.MineDPavDCac();
chartLabel = "D Pav vs D Cac";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

regressions = myMine.MineDTcpvDCac();
chartLabel = "D Tcpv vs D Cac";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

#endregion

#region log-log v1.CTA vs. v2.CTA

regressions = myMine.MineLnNcpv1LnNcpv2();
chartLabel = "Ncpv0 vs Ncpv1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));


regressions = myMine.MineLnTps1LnTps2();
chartLabel = "Tps0 vs Tps1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));


regressions = myMine.MineLnPav1LnPav2();
chartLabel = "Pav0 vs Pav1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

regressions = myMine.MineLnTcpv1LnTcpv2();
chartLabel = "Tcpv0 vs Tcpv1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

regressions = myMine.MineLnCac1LnCac2();
chartLabel = "Cac0 vs Cac1";
Console.WriteLine(string.Join(Environment.NewLine,
    regressions.Zip(labels, (item, label) =>
        FormatRegression(item, label, chartLabel))));

#endregion