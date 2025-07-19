using DataMiner;
using Keto_Cta;
using LinearRegression;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.Regex;

var ctaDataPath = "TestData/keto-cta-quant-and-semi-quant.csv";
var MyMine = new GoldMiner(ctaDataPath);
var logMismatch = 0;
var uninterestingSkip = 0;
var Dust = new List<Dust>();

#region Load dust  Element Delta vs. Element Delta
var elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

for (var x = 0; x < elementDelta.Length; x++)
{
    for (var y = 0; y < elementDelta.Length; y++)
    {
        if (x != y)
        {
            var chart = $"{elementDelta[x]} vs. {elementDelta[y]}";
            try
            {
                var selector = new CreateSelector(chart);
                if (selector.IsLogMismatch || selector.IsUninteresting)
                {
                    if (selector.IsLogMismatch) logMismatch++;
                    if (selector.IsUninteresting) uninterestingSkip++;
                    continue;
                }
                Dust.AddRange(MyMine.GoldDust(chart));
            }
            catch (ArgumentException error)
            {
                logMismatch++; // technically this is a regression against self error  
            }
        }
    }
}
#endregion

#region Load dust Baseline vs. Year later  
var visit = "Tps,Cac,Ncpv,Tcpv,Pav,LnTps,LnCac,LnNcpv,LnTcpv,LnPav".Split(",");
for (var x = 0; x < visit.Length; x++)
{
    var chart = $"{visit[x]}0 vs. {visit[x]}1";
    try
    {
        var selector = new CreateSelector(chart);
        if (selector.IsLogMismatch || selector.IsUninteresting)
        {
            if (selector.IsLogMismatch) logMismatch++;
            if (selector.IsUninteresting) uninterestingSkip++;
            continue;
        }
        Dust.AddRange(MyMine.GoldDust(chart));
    }
    catch (ArgumentException error)
    {
        logMismatch++;
    }
}
#endregion

#region dust Baseline vs. Year delta
var eDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

foreach (var visit0 in visit)
{
    foreach (var delta in eDelta)
    {
        var chart = $"{visit0}0 vs. {delta}";
        try
        {
            var selector = new CreateSelector(chart);
            if (selector.IsLogMismatch || selector.IsUninteresting)
            {
                if (selector.IsLogMismatch) logMismatch++;
                if (selector.IsUninteresting) uninterestingSkip++;
                continue;
            }
            Dust.AddRange(MyMine.GoldDust(chart));
        }
        catch (ArgumentException error)
        {
            logMismatch++;
        }
    }
}
#endregion

#region Add regression charts

var inverseRatiosIncluded = 0;
foreach (var chart in MyMine.RatioCharts(out  inverseRatiosIncluded))
{
    try
    {
        var selector = new CreateSelector(chart);
        if (selector.IsLogMismatch || selector.IsUninteresting)
        {
            if (selector.IsLogMismatch) logMismatch++;
            if (selector.IsUninteresting) uninterestingSkip++;
            continue;
        }
        Dust.AddRange(MyMine.GoldDust(chart));
    }
    catch (ArgumentException error)
    {
        logMismatch++;
    }
}
#endregion

#region Print regression Csv table
Console.WriteLine($"In Order of PValue (Interesting Regressions Highlighted):");
Console.WriteLine($"Index, Chart, Subset, N=, Slope, p-value, R^2, Y-intercept, X-mean, Y-mean, SD, CC");
var totalRegressions = 0;
var index = 0;
var sortedDust = Dust.OrderBy(d => d.Regression.PValue());
foreach (var dust in sortedDust)
{
    totalRegressions++;
    if (dust.IsInteresting)
    {
        var reg = dust.Regression;
        var interestingString = dust.IsInteresting ? "Yes" : "-";
        Console.WriteLine($"{index++}, {dust.ChartTitle}, {dust.SetName}, {reg.N}, {reg.Slope():F4}, "
                          + $"{reg.PValue():F4}, {reg.RSquared():F4}, "
                          + $"{reg.YIntercept():F4}, {reg.MeanX():F4}, {reg.MeanY():F4}, {reg.Qx():F4}, {reg.Correlation():F4}");
    }
}
Console.WriteLine($"\nTotal regressions calculated {totalRegressions}");
Console.WriteLine($"Log mismatch regressions skipped: {logMismatch}");
Console.WriteLine($"Inverse Ratio regressions included: {inverseRatiosIncluded}");
Console.WriteLine($"Uninteresting regressions included in calculated (See Dust.IsInteresting flag): {uninterestingSkip}");
Console.WriteLine($"Total interesting regressions: {Dust.Count(d => d.IsInteresting)}");
Console.WriteLine($"Interesting remaining regressions: {index}");
#endregion

#region Set Order regression
var histograms = new Dictionary<SetName, int[]>
{
    { SetName.Omega, new int[6] },
    { SetName.Alpha, new int[6] },
    { SetName.Beta, new int[6] },
    { SetName.Zeta, new int[6] },
    { SetName.Gamma, new int[6] },
    { SetName.Theta, new int[6] },
    { SetName.Eta, new int[6] },
    { SetName.BetaUZeta, new int[6] }
};

var dataPoints = new Dictionary<SetName, List<(double x, double y)>>
{
    { SetName.Omega, new List<(double, double)>() },
    { SetName.Alpha, new List<(double, double)>() },
    { SetName.Beta, new List<(double, double)>() },
    { SetName.Zeta, new List<(double, double)>() },
    { SetName.Gamma, new List<(double, double)>() },
    { SetName.Theta, new List<(double, double)>() },
    { SetName.Eta, new List<(double, double)>() },
    { SetName.BetaUZeta, new List<(double, double)>() }
};

foreach (var dust in Dust)
{
    dataPoints[dust.SetName].Add((dust.Regression.PValue(), dust.Regression.Qx()));
    var bucket = (int)(dust.Regression.PValue() * 5);
    histograms[dust.SetName][Math.Min(bucket, 5)]++; // Clamp to 5 for NaN bin
}

var subsetRegressions = new Dictionary<SetName, RegressionPvalue>();
Console.WriteLine("\nCalculated Subset Regressions:\nSet, 0-0.2, 0.2-0.4, 0.4-0.6, 0.6-0.8, 0.8-1.0, NaN");
foreach (var item in dataPoints)
{
    var data = dataPoints[item.Key];
    if (data.Count != 0)
    {
        var regression = new RegressionPvalue(data);
        subsetRegressions[item.Key] = regression;
        var hist = histograms[item.Key];
        Console.WriteLine(
            $"{item.Key}, {hist[0]}, {hist[1]}, {hist[2]}, {hist[3]}, {hist[4]}, {hist[5]}");
    }
}
#endregion

#region Chart Specific Regression
void ChartARegressionExcel(Dictionary<SetName, RegressionPvalue> setRegressions, SetName set)
{
    if (!setRegressions.TryGetValue(set, out var target) || !target.DataPoints.Any()) return;

    Console.WriteLine($"\n-,-,'regression - {set}' slope; {target.Slope():F4} N={target.N} R^2: {target.RSquared():F4} p-value: {target.PValue():F6}\n");
    Console.WriteLine($"p-value, p-value SD");
    foreach (var point in target.DataPoints)
    {
        Console.WriteLine($"{point.x}, {point.y}");
    }
}

void ChartToCvs(IEnumerable<Dust> dust)
{
    foreach (var dust1 in dust)
    {
        var target = dust1.Regression;
        var parts = Split(dust1.ChartTitle, @"\s+vs.\s*");
        if (parts.Length < 2) continue; // Handle invalid titles
        var regressor = parts[0];
        var dependent = parts[1];

        Console.WriteLine($"\n-,-,{dust1.ChartTitle} -- {dust1.SetName}" +
                          $"\n-,-,Slope; {target.Slope():F4} N={target.N} R^2: {target.RSquared():F4} p-value: {target.PValue():F6} y-int {target.YIntercept():F4}");
        Console.WriteLine($"{regressor}, {dependent}");
        foreach (var point in target.DataPoints)
        {
            Console.WriteLine($"{point.x}, {point.y}");
        }
    }
}
#endregion

//ChartToExcel(Dust.Where(d => d.ChartTitle.Equals("LnDPav / LnTps0 vs. LnDTcpv".Trim()))); // for 'command?' extension 


// Wait for user input before closing the console window
Console.WriteLine("\nPress Enter to exit or type a Chart Title to view its regression data (e.g., 'Cac0 vs. Cac1'):");

while (true)
{
    var command = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(command) || IsMatch(command, @"^(exit|quit|end|q)$", RegexOptions.IgnoreCase))
        break;

    if (!string.IsNullOrWhiteSpace(command))
    {
        if (IsMatch(command, @"print\s*cac", RegexOptions.IgnoreCase))
        {
            var myData = MyMine.PrintBetaUZetaElements(SetName.BetaUZeta);
            foreach (var item in myData)
            {
                Console.WriteLine(item);
            }
        }
        if (IsMatch(command, @"print\s*gamma", RegexOptions.IgnoreCase))
        {
            var myData = MyMine.PrintOmegaElements(SetName.Omega);
            foreach (var item in myData)
            {
                Console.WriteLine(item);
            }
        }
        else
        {
            var dust = Dust.Where(d => d.ChartTitle.Equals(command, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (dust.Any())
            {
                ChartToCvs(dust);
                Console.WriteLine("Enter another Chart Title or 'exit' to quit:");
            }
            else
            {
                Console.WriteLine($"Chart '{command}' not found. Try again or 'exit' to quit:");
            }
        }
    }
}