// See https://aka.ms/new-console-template for more information

using DataMiner;
using Keto_Cta;
using LinearRegression;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.RegularExpressions;

var ctaDataPath = "TestData/keto-cta-quant-and-semi-quant.csv";


var Dust = new List<Dust>();
var MyMine = new GoldMiner(ctaDataPath);
var logMismatch = 0;


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
    //r selector = new CreateSelector(chart);
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

#region Print regession Csv table
// header
Console.WriteLine($"In Order of PValue:");
Console.WriteLine($"Index, Chart, Subset, N=, Slope, p-value, R^2, Y-intercept, X-mean, Y-mean, SD, CC");
// Print the regression data points

var index = 0;
var sortedDust = Dust.OrderBy(d => d.Regression.PValue());

foreach (var dust in sortedDust)
{
    var reg = dust.Regression;
    Console.WriteLine($"{index++}, {dust.Title}, {dust.SetName}, {reg.N}, {reg.Slope():F4}, "
                       + $"{reg.PValue():F4}, {reg.RSquared():F4}, "
                       + $"{reg.YIntercept():F4}, {reg.MeanX():F4}, {reg.MeanY():F4}, {reg.Qx():F4}, {reg.Correlation():F4}");
}

Console.WriteLine($"\nLog mismatch skipped: {logMismatch}");
#endregion


#region Set Order regression 

// Create a regression for each subset p-value to get mean p-value and count of regressions in each subset.

var histograms = new Dictionary<SetName, int[]>
{
    // This is a five bin histogram for p-values, each bucket represents a range of p-values,
    // the sixth bucket is for p-values that are 1.0, not predictive, typically NaN.
    { SetName.Omega, new int[6] },
    { SetName.Alpha, new int[6] },
    { SetName.Beta, new int[6] },
    { SetName.Zeta, new int[6] },
    { SetName.Gamma, new int[6] },
    { SetName.Theta, new int[6] },
    { SetName.Eta, new int[6] },
    { SetName.BetaUZeta, new int [6]}
};

var dataPoints = new Dictionary<SetName, List<(double x, double y)>>
{
    // This is a five bin histogram for p-values, each bucket represents a range of p-values,
    // the sixth bucket is for p-values that are 1.0, not predictive, typically NaN.
    { SetName.Omega, [] },
    { SetName.Alpha, [] },
    { SetName.Beta, [] },
    { SetName.Zeta, [] },
    { SetName.Gamma, [] },
    { SetName.Theta, [] },
    { SetName.Eta, [] },
    { SetName.BetaUZeta, []}
};

foreach (var dust in Dust)
{
    dataPoints[dust.SetName].Add((dust.Regression.PValue(), dust.Regression.Qx())); // Changed to AddDataPoint to match the method in PValueStat
    histograms[dust.SetName][(int)(dust.Regression.PValue() * 5)]++; // Increment the count for this bucket
}

Dictionary<SetName, RegressionPvalue> subsetRegressions = new();

Console.WriteLine("\nSubset Regressions:\nSet, N regressions=, Average p-value, subset-regressions p-value, Slope, 0-0.2, 0.2-0.4, 0.4-0.6, 0.6-0.8, 0.8-1.0, NaN");
foreach (var item in dataPoints)
{
    var data = dataPoints[item.Key];
    var regression = new RegressionPvalue(data);
    subsetRegressions.Add(item.Key, regression);
    
    var hist = histograms[item.Key];
    Console.WriteLine(
        $"{item.Key}, {regression.N}, {regression.MeanX():F6}, {regression.PValue():F6}, {regression.Slope():F4}, {string.Join(", ", hist)}");
}

#endregion

#region burn a graph please :)

// Print the regression data points for a specific regression
// Change the chartIdx to the index of the regression you want to print
//*****

const int chartIdx = 366;// 374; // 3,6,7Example index for the regression you want to print

ChartARegressionExcel(subsetRegressions, SetName.Theta);
ChartARegressionExcel(subsetRegressions, SetName.Eta);
ChartARegressionExcel(subsetRegressions, SetName.Alpha);
ChartARegressionExcel(subsetRegressions, SetName.Zeta);


void ChartARegressionExcel(Dictionary<SetName, RegressionPvalue> setRegressions, SetName set)
{
    var target = setRegressions[set];

//    Console.WriteLine($"\n-,-,Regression: {set} slope: {target.Slope():F4}");

    Console.WriteLine($"\n-,-,'regression - {set}' slope; {target.Slope():F4} N={target.N} R^2: {target.RSquared():F4} p-value: {target.PValue():F6}\n");

    var xxx = "p-value";
    var yyy = "p-value SD";

    Console.WriteLine($"{xxx}, {yyy}"); // not log-log split

    foreach (var point in target.DataPoints)
    {
        Console.WriteLine($"{point.x}, {point.y}");
    }
}

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