using System.ComponentModel.Design;
using DataMiner;
using Keto_Cta;
using LinearRegression;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Text.RegularExpressions.Regex;

var ctaDataPath = "TestData/keto-cta-quant-and-semi-quant.csv";
var MyMine = new GoldMiner(ctaDataPath);
var logMismatch = 0;
var uninterestingSkip = 0;
var localDusts = new List<Dust>();

#region Set Order regression histogram
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
    { SetName.Omega, [] },
    { SetName.Alpha, [] },
    { SetName.Beta, [] },
    { SetName.Zeta, [] },
    { SetName.Gamma, [] },
    { SetName.Theta, [] },
    { SetName.Eta, [] },
    { SetName.BetaUZeta, [] }
};

foreach (var dust in localDusts)
{
    dataPoints[dust.SetName].Add((dust.Regression.PValue(), dust.Regression.StdDevX()));
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
                          $"\n-,-,Slope: {target.Slope():F4} N={target.N} R^2: {target.RSquared():F4} p-value: {target.PValue():F6} y-int {target.YIntercept():F4}");
        Console.WriteLine($"{regressor}, {dependent}");
        foreach (var point in target.DataPoints)
        {
            Console.WriteLine($"{point.x}, {point.y}");
        }
    }
}
#endregion

#region Mine!

var mineRegressions= new MineRegressionsWithGold();
string []  Mine(MineRegressionsWithGold miner)
{
    localDusts.AddRange(miner.GenerateGoldRegression(MyMine));
    var dusts = miner.GenerateGoldRegression(MyMine);
    var report = miner.Report();

    return report;
}



#endregion

//ChartToExcel(Dust.Where(d => d.ChartTitle.Equals("LnDPav / LnTps0 vs. LnDTcpv".Trim()))); // for 'command?' extension 


// Wait for user input before closing the console window
Console.WriteLine("\nPress Enter to exit or type a Chart Title to view its regression data (e.g., 'Cac0 vs. Cac1'):");

while (true)
{
    Console.Write("> ");
    var command = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(command)) continue;
    // Check for exit commands
    if ( IsMatch(command, @"^(exit|quit|end|q)$", RegexOptions.IgnoreCase))
        break;

    if (!string.IsNullOrWhiteSpace(command))
    {
        if (IsMatch(command, @"cac", RegexOptions.IgnoreCase))
        {
            var myData = MyMine.PrintBetaUZetaElements(SetName.BetaUZeta);
            foreach (var item in myData)
            {
                Console.WriteLine(item);
            }
        }
        else if (IsMatch(command, @"mine", RegexOptions.IgnoreCase))
        {
            localDusts.Clear();
            localDusts.AddRange(mineRegressions.GenerateGoldRegression(MyMine));
            var myData = mineRegressions.Report();
            foreach (var item in myData)
            {
                Console.WriteLine(item);
            }

        }
        else if (IsMatch(command, @"gamma", RegexOptions.IgnoreCase))
        {
            var myData = MyMine.PrintOmegaElementsFor3DGammaStudy(SetName.Omega);
            foreach (var item in myData)
            {
                Console.WriteLine(item);
            }
        }
        else if (IsMatch(command, @"all\s*matrix", RegexOptions.IgnoreCase))
        {
            var myData = MyMine.PrintAllSetMatrix();
            foreach (var item in myData)
            {
                Console.WriteLine(item);
            }
        }
        else if (IsMatch(command, @"matrix", RegexOptions.IgnoreCase))
        {
            var myData = MyMine.PrintStatisticMatrix(SetName.Omega);
            foreach (var item in myData)
            {
                Console.WriteLine(item);
            }
        }
        else if (IsMatch(command, @"keto.*", RegexOptions.IgnoreCase))
        {
            var myData = MyMine.PrintKetoCta();
            foreach (var item in myData)
            {
                Console.WriteLine(item);
            }

        }
        else if (IsMatch(command, @"help", RegexOptions.IgnoreCase))
        {
            Console.WriteLine("Possible Commands: 'independent vs. regressor', cac, mine, gamma, dust, matrix, "+
                              "all matrix, keto, q|exit|quit|end|help");
        }
        else if (IsMatch(command, @"dust", RegexOptions.IgnoreCase))
        {
            foreach (var item in localDusts)
            {
                Console.WriteLine($"{item.ChartTitle},{item.SetName},{item.Regression.RSquared():F3}");
            }
        }
        else // Get the data for a chart
        {
            var dust = localDusts.Where(d => d.ChartTitle.Equals(command, StringComparison.OrdinalIgnoreCase)).ToArray();
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