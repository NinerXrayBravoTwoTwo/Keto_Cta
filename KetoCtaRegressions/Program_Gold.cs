using DataMiner;
using Keto_Cta;
using LinearRegression;
using System.Text.Json;
using System.Text.RegularExpressions;

var ctaDataPath = "TestData/keto-cta-quant-and-semi-quant.csv";
var MyMine = new GoldMiner(ctaDataPath);
var logMismatch = 0;
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
                if (selector.IsLogMismatch)
                {
                    logMismatch++;
                    continue;
                }
                Dust.AddRange(MyMine.GoldDust(chart));
            }
            catch (ArgumentException)
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
    Dust.AddRange(MyMine.GoldDust(chart));
}
#endregion

#region dust Baseline vs. Year delta
var eDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

foreach (var visit0 in visit)
{
    foreach (var delta in eDelta)
    {
        var chart = $"{visit0}0 vs. {delta}";
        var selector = new CreateSelector(chart);
        if (selector.IsLogMismatch)
        {
            logMismatch++;
            continue;
        }
        Dust.AddRange(MyMine.GoldDust(chart));
    }
}
#endregion

#region Add regression charts
foreach (var chart in MyMine.RatioCharts())
{
    Dust.AddRange(MyMine.GoldDust(chart));
}
#endregion

#region Print regression Csv table
Console.WriteLine($"In Order of PValue:");
Console.WriteLine($"Index, Chart, Subset, N=, Slope, p-value, R^2, Y-intercept, X-mean, Y-mean, SD, CC");
var index = 0;
var sortedDust = Dust.OrderBy(d => d.Regression.PValue());
foreach (var dust in sortedDust)
{
    var reg = dust.Regression;
    Console.WriteLine($"{index++}, {dust.ChartTitle}, {dust.SetName}, {reg.N}, {reg.Slope():F4}, "
                      + $"{reg.PValue():F4}, {reg.RSquared():F4}, "
                      + $"{reg.YIntercept():F4}, {reg.MeanX():F4}, {reg.MeanY():F4}, {reg.Qx():F4}, {reg.Correlation():F4}");
}
Console.WriteLine($"\nLog mismatch skipped: {logMismatch}");
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
Console.WriteLine("\nSubset Regressions:\nSet, N regressions=, Average p-value, subset-regressions p-value, Slope, 0-0.2, 0.2-0.4, 0.4-0.6, 0.6-0.8, 0.8-1.0, NaN");
foreach (var item in dataPoints)
{
    var data = dataPoints[item.Key];
    var regression = new RegressionPvalue(data);
    subsetRegressions[item.Key] = regression;
    var hist = histograms[item.Key];
    Console.WriteLine(
        $"{item.Key}, {regression.N}, {regression.MeanX():F6}, {regression.PValue():F6}, {regression.Slope():F4}, {hist[0]}, {hist[1]}, {hist[2]}, {hist[3]}, {hist[4]}, {hist[5]}");
}
#endregion

#region Chart Specific Regression

void ChartARegressionExcel(Dictionary<SetName, RegressionPvalue> setRegressions, SetName set)
{
    var target = setRegressions[set];
    Console.WriteLine($"\n-,-,'regression - {set}' slope; {target.Slope():F4} N={target.N} R^2: {target.RSquared():F4} p-value: {target.PValue():F6}\n");
    Console.WriteLine($"p-value, p-value SD");
    foreach (var point in target.DataPoints)
    {
        Console.WriteLine($"{point.x}, {point.y}");
    }
}
#endregion

#region burn a graph please :)

ChartARegressionExcel(subsetRegressions, SetName.Alpha);
ChartARegressionExcel(subsetRegressions, SetName.Theta);
ChartARegressionExcel(subsetRegressions, SetName.Eta);


// Print the regression data points for a specific regression
// Change the chartIdx to the index of the regression you want to print
//*****






void ChartARegressionGrok(List<RegressionPvalue> regressionPvalues, int i, List<string> list,
    List<string> allSetNames1)
{
    var target = regressionPvalues[i - 1];
    var iamThis = list[i - 1];
    var iamInSet = allSetNames1[i - 1];

    // Extract x and y variable names from the regression label
    var regSplit = Regex.Split(iamThis, @"\s+vs.\s*", RegexOptions.IgnoreCase);
    var xxx = regSplit[0];
    var yyy = regSplit[1];

    // Handle log-log transformation if present
    if (Regex.IsMatch(iamThis, "log.log|ln.ln", RegexOptions.IgnoreCase))
    {
        xxx = Regex.Replace(regSplit[0], @"(log.log|ln.ln)\s*", "");
        xxx = $"Ln(|{xxx}|+1)";
        yyy = $"Ln(|{regSplit[1]}|+1)";
    }

    // Build the dataset with x,y points
    var dataPoints = target.DataPoints.Select(point => new { x = point.x, y = point.y }).ToList();

    // Create the JSON configuration object
    var chartConfig = new
    {
        type = "scatter",
        data = new
        {
            datasets = new[]
            {
                new
                {
                    label =
                        $"'{iamThis} - {iamInSet}' (slope: {target.Slope():F4}, N={target.N}, R^2: {target.RSquared():F4}, p-value: {target.PValue():F6})",
                    data = dataPoints,
                    backgroundColor = "#FF6B6B",
                    borderColor = "#FF6B6B",
                    pointRadius = 5
                }
            }
        },
        options = new
        {
            scales = new
            {
                x = new { title = new { display = true, text = xxx } },
                y = new { title = new { display = true, text = yyy } }
            }
        }
    };

    // Serialize to JSON and output
    string jsonOutput = JsonSerializer.Serialize(chartConfig, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine(jsonOutput);
}

#endregion

// Wait for user input before closing the console window
Console.ReadLine();