using System.ComponentModel.Design.Serialization;
using DataMiner;
using Keto_Cta;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using LinearRegression;
using static System.Text.RegularExpressions.Regex;

var ctaDataPath = "TestData/keto-cta-quant-and-semi-quant.csv";
var ctaQangioPath = "TestData/keto-cta-qangio.csv";
var goldMiner = new GoldMiner(ctaDataPath, ctaQangioPath);
var miner = new MineRegressionsWithGold();

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

var dataPoints = new Dictionary<SetName, List<(string id, double x, double y)>>
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

foreach (var dust in miner.Dusts)
{
    dataPoints[dust.SetName].Add(("NA", dust.Regression.PValue, dust.Regression.StdDevX));
    var bucket = (int)(dust.Regression.PValue * 5);
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

void DustsToRegressionList(IEnumerable<Dust> dusts)
{
    // header
    Console.WriteLine($"Regression,Phenotype,Mean X,moe X,Mean Y,moe Y,Slope,xSD,p-value");
    var myDusts = dusts as Dust[] ?? dusts.ToArray();
    var orderBypVal = myDusts.OrderBy(d => d.Regression.PValue);
    foreach (var item in orderBypVal)
    {
        var moeX = item.Regression.MarginOfError();
        var moeY = item.Regression.MarginOfError(true);
        Console.WriteLine($"{item.RegressionName},{item.SetName}," +
                          $"{moeX.Mean:F3},{moeX.MarginOfError:F3}," +
                          $"{moeY.Mean:F3},{moeY.MarginOfError:F3}," +
                          $"{item.Regression.Slope:F4},{item.Regression.StdDevX:F3}," +
                          $"{item.Regression.PValue:F8}");
    }
}

void DustsToCvs(IEnumerable<Dust> dust)
{
    foreach (var dust1 in dust)
    {
        var target = dust1.Regression;
        var parts = Split(dust1.RegressionName, @"\s+vs.\s*");
        if (parts.Length < 2) continue; // Handle invalid titles
        var regressor = parts[1];
        var dependent = parts[0];

        Console.WriteLine($"\n-,-,{dust1.RegressionName} -- {dust1.SetName}" +
                          $"\n-,-,Slope: {target.Slope:F4} N={target.N} R^2: {target.RSquared:F4} p-value: {target.PValue:F6} y-int {target.YIntercept:F4}");
        Console.WriteLine($"{regressor}, {dependent}");
        foreach (var point in target.DataPoints)
        {
            Console.WriteLine($"{point.x}, {point.y}");
        }
    }
}
#endregion

#region Mine!

//string[] Mine(MineRegressionsWithGold miner, bool isIncludeRatioCharts = false)
//{
//    localDusts.AddRange(miner.GenerateGoldRegression(goldMiner, isIncludeRatioCharts));
//    var dusts = miner.GenerateGoldRegression(goldMiner, isIncludeRatioCharts);
//    var report = miner.Report(dusts);

//    return report;
//}

#endregion

//ChartToExcel(Dust.Where(d => d.ChartTitle.Equals("LnDPav / LnTps0 vs. LnDTcpv".Trim()))); // for 'command?' extension 

var vsPattern = @"^([A-Z\d\s\(/\(\)]+)\s(vs\.?)\s([A-Z\d\s\(/\(\)]+)\s*(-.+)?$";

// Wait for user input before closing the console window
Console.WriteLine("\nPress Enter to exit or type a Chart Title to view its regression data (e.g., 'Cac0 vs. Cac1'):");

while (true)
{
    Console.Write("> ");
    var command = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(command)) continue;

    // Check for exit commands
    if (IsMatch(command, @"^(exit|quit|end|q)$", RegexOptions.IgnoreCase))
        break;

    if (string.IsNullOrWhiteSpace(command)) continue;

    if (IsMatch(command, @"^beta", RegexOptions.IgnoreCase))
    {
        var myData = goldMiner.PrintBetaElements(SetName.Beta);
        foreach (var item in myData)
        {
            Console.WriteLine(item);
        }
    }
    else if (IsMatch(command, @"^Ele\w+ (\d{1,3}|\w*)", RegexOptions.IgnoreCase))
    {
        Match match = Regex.Match(command, @"^Ele\w+ (\d{1,3}|\w*)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            List<LeafSetName> leafSets = [];

            //Element Id ?
            var isElementId = int.TryParse(match.Groups[1].Value, out int elementId);
            if (!isElementId)
            {
                // Or phenotype/leafSet name

                var inst = match.Groups[1].Value.ToLower();

                // Scan decorations for set names
                foreach (var item in Enum.GetNames(typeof(LeafSetName)))
                {
                    if (inst.Contains(item.ToLower()))
                    {
                        if (Enum.TryParse(item, true, out LeafSetName result))
                            leafSets.Add(result);

                        break;
                    }
                }
            }

            IEnumerable<Element> elements;
            if (match.Groups[1].Value.Contains("ang"))
            {
                elements = goldMiner.Elements.Where(e => !double.IsNaN(e.DQangio)).OrderBy(e => e.MemberSet);
            }
            else
            {
                elements = isElementId
                    ? goldMiner.Elements.Where(e => e.Id.Equals(elementId.ToString()))
                    : goldMiner.Elements.Where(e => e.MemberSet.Equals(leafSets.FirstOrDefault()));
            }

            var enumerable = elements as Element[] ?? elements.ToArray();
            if (enumerable.Any())
            {
                foreach (var item in enumerable)
                    Console.WriteLine(item + "\n");
            }
            else
            {
                var msg = isElementId
                    ? $"Element with ID {elementId} not found."
                    : $"Only leaf sets 'zeta', 'gamma', 'theta', 'eta' can be requested here. Plus QAngino, for temporarily testing.";
                Console.WriteLine(msg);
            }
        }
    }
    else if (IsMatch(command, @"^mine", RegexOptions.IgnoreCase))
    {
        var tokens = Match(command, @"^Mine\s*([A-Z]+)?\s*(\d{1}k)?$", RegexOptions.IgnoreCase); //G2 = size, G1=Type of Mining
        if (!tokens.Success)
        {
            Console.WriteLine("Invalid 'mine' syntax. Try 'mine root','mine ratios', 'mine deltas' ...");
            continue;
        }
        var start = DateTime.Now;

        miner.GenerateGoldRegression(goldMiner);

        var end = DateTime.Now;
        var interval = end - start;

        Console.WriteLine(
            $"{miner.DustCount} regressions in {interval.TotalMinutes:F3} min.  Regressions/ms: {miner.DustCount / interval.Milliseconds}");

        var myData = miner.Report(2);

        foreach (var speck in myData)
        {
            Console.WriteLine(speck);
        }

        Console.WriteLine(
            $"{miner.DustCount} regressions in {interval.TotalMinutes:F3} min.  Regressions/ms: {miner.DustCount / interval.Milliseconds}");
    }
    else if (IsMatch(command, @"clear*", RegexOptions.IgnoreCase))
    {
        miner.Clear();
        goldMiner.Clear();

    }
    else if (IsMatch(command, @"gamma", RegexOptions.IgnoreCase))
    {
        var myData = goldMiner.PrintOmegaElementsFor3DGammaStudy(SetName.Omega);
        foreach (var item in myData)
        {
            Console.WriteLine(item);
        }
    }
    else if (IsMatch(command, @"^all\s*matrix", RegexOptions.IgnoreCase))
    {
        List<Dust> dusts = [];
        dusts.AddRange(goldMiner.RootAllSetMatrix());
        Console.WriteLine(string.Join('\n', GoldMiner.ToStringFormatRegressionsInDusts(dusts.ToArray()), true));

    }
    else if (IsMatch(command, @"^matrix\s*(\w+)?\s*$", RegexOptions.IgnoreCase))
    {
        var tokens = Match(command, @"^matrix\s*(\w+)?\s*$", RegexOptions.IgnoreCase);
        if (!tokens.Groups[1].Success)
        {
            var rootDusts = goldMiner.RootStatisticMatrix(SetName.Omega);
            Console.WriteLine(string.Join('\n', GoldMiner.ToStringFormatRegressionsInDusts(rootDusts, true)));
            continue;
        }

        //else G1 is there :) !!!
        {
            var setName = tokens.Groups[1].Value;
            if (Enum.TryParse(setName, true, out SetName result))
            {
                var rootDusts = goldMiner.RootStatisticMatrix(result);
                Console.WriteLine(string.Join('\n', GoldMiner.ToStringFormatRegressionsInDusts(rootDusts, true)));
                continue;
            }

            // ... but G1 did not parse as  a valid set soo ... try to do something useful     

            var dusts = goldMiner.RootAllSetMatrix();
            Console.WriteLine(string.Join('\n', GoldMiner.ToStringFormatRegressionsInDusts(dusts, true)));
        }
    }
    else if (IsMatch(command, @"keto.*", RegexOptions.IgnoreCase))
    {
        var myData = goldMiner.PrintKetoCta();
        foreach (var item in myData)
        {
            Console.WriteLine(item);
        }
    }
    else if (IsMatch(command, @"help", RegexOptions.IgnoreCase))
    {
        Console.WriteLine("Possible Commands: 'independent vs. regressor', BetaUZeta, mine, gamma, dust, matrix, " +
                          "all matrix, keto, clear dusts, q|exit|quit|end|help");
    }
    else if (IsMatch(command, @"^dust$", RegexOptions.IgnoreCase))
    {
        //GoldMiner.ToStringFormatRegressionsInDusts(localDusts.ToArray());
        //foreach (var speck in localDusts.OrderBy(d => d.Regression.PValue))
        //{
        //    Console.WriteLine(speck);
        //}
    }
    // Explore the regression for a single regression title across sub-phenotypes Zeta, Gamma, Theta, Eta
    else if (IsMatch(command, vsPattern, RegexOptions.IgnoreCase))
    {
        // Parse for subset name, if none default to Omega
        var tokens = Match(command, vsPattern, RegexOptions.IgnoreCase);
        var title = tokens.Groups[4].Success ? command.Split('-') : [command];

        if (!tokens.Success)
        {
            Console.WriteLine($"Please submit regressions in the form of 'Dependent vs Regressor' for example 'Cac1 vs Cac0, (Omega|Beta)'");
            continue;
        }

        IEnumerable<Dust> dusts = [];
        try
        {
            dusts = goldMiner.GoldDust(title[0]);
        }
        catch (Exception error)
        {
            Console.WriteLine($"Error generating regression for '{title[0]}': {error.Message}");
            continue;
        }

        //localDusts.AddRange(dusts);

        List<SetName> useSets = [];

        // User wants specific LMHR sub phenotype set

        var inst = tokens.Groups[4].Success ? title[1].ToLower() : string.Empty;
        var isAllPhenotypes = inst.Contains("all");

        // Scan decorations for set names
        foreach (var item in Enum.GetNames(typeof(SetName)))
        {
            if (isAllPhenotypes | inst.Contains(item.ToLower()))
            {
                if (Enum.TryParse(item, true, out SetName result))
                    useSets.Add(result);
            }
        }

        // Look up existing chart
        if (dusts.Any())
        {
            foreach (var item in useSets)
                DustsToCvs(dusts.Where(d => d.SetName.Equals(item)));

            DustsToRegressionList(dusts);
            Console.WriteLine("Enter another Chart Title or 'exit' to quit:");
        }
        else // create new dusts
        {
            Console.WriteLine($"Generating regression for '{title[0]}'...");
            try
            {
                var newDusts = goldMiner.GoldDust(title[0]);
                if (newDusts.Length == 0)
                {
                    Console.WriteLine($"Regression '{title[0]}' can not be generated.");
                    continue;
                }

                //localDusts.AddRange(newDusts);

                DustsToRegressionList(newDusts);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Please us valid data names");
            }
        }
    }
}