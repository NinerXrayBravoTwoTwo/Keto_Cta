using DataMiner;
using Keto_Cta;
using System.Text;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.Regex;

var ctaDataPath = "TestData/keto-cta-quant-and-semi-quant.csv";
var ctaQangioPath = "TestData/keto-cta-qangio.csv";
var goldMiner = new GoldMiner(ctaDataPath, ctaQangioPath);
var miner = new MineRegressionsWithGold();

#region Chart Specific Regression

void DustsToRegressionList(IEnumerable<Dust> dusts, bool isDoNotPrintNaNregressions = false)
{
    // header
    Console.WriteLine($"Regression,Phenotype,Mean X,moe X,Mean Y,moe Y,Slope,xSD,p-value");
    var myDusts = dusts as Dust[] ?? dusts.ToArray();
    var orderBypVal = myDusts.OrderBy(d => d.Regression.PValue);
    foreach (var item in orderBypVal)
    {
        if (isDoNotPrintNaNregressions && double.IsNaN(item.Regression.PValue)) continue;

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
        var ids = dust1.Regression.IdPoints.ToArray();

        Console.WriteLine($"\n-,-,{dust1.RegressionName} -- {dust1.SetName}" +
                          $"\n-,-,Slope: {target.Slope:F4} N={target.N} R^2: {target.RSquared:F4} p-value: {target.PValue:F6} y-int {target.YIntercept:F4}");
        Console.WriteLine($"Id, {regressor}, {dependent}");
        var n = 0;
        foreach (var point in target.DataPoints)
        {
            Console.WriteLine($"{ids[n++]}, {point.x}, {point.y}");
        }
    }
}
#endregion

var vsPattern = @"^([A-Z\d\s\(/\(\)]+)\s(vs\.?)\s([A-Z\d\s\(/\(\)]+)\s*(-.+)?$";

// Wait for user input before closing the console window
Console.WriteLine("\nPress Enter to exit or type a Chart Title to view its regression data (e.g., 'Cac0 vs. Cac1'):");

// Preload a small set
miner.AddRange(goldMiner.RootAllSetMatrix());

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
    else if (IsMatch(command, @"^title\s*(((?:\s(,?\w+\)?\,?)\s*)+)?)?$", RegexOptions.IgnoreCase))
    {

        // parse filters and limit
        var tokens = Regex.Match(command, @"^title\s*(((?:\s(,?\w+\)?\,?)\s*)+)?)?$", RegexOptions.IgnoreCase);

        if (!tokens.Success)
        {
            Console.WriteLine("Syntax error");
        }

        int limit = 0;
        var filters = new Dictionary<string, int>();
        var depToken = Token.None;
        var regToken = Token.None;

        var isFoundALimit = false;
        for (var x = 0; x < tokens.Groups[3].Captures.Count; x++)
        {
            var param = tokens.Groups[3].Captures[x].Value;
            if (int.TryParse(param, out limit))
            {
                isFoundALimit = true;
            }
            else
            {
                if (regToken == Token.None && param.StartsWith("reg"))
                {
                    var xxxx = param.Substring("reg".Length);
                    if (Enum.TryParse(param.Substring("reg".Length), true, out regToken))
                        continue;
                }

                if (depToken == Token.None && param.StartsWith("dep"))
                    if (Enum.TryParse(param.Substring("dep".Length), true, out depToken))
                        continue;

                _ = filters.TryAdd(param.ToLower(), 1);
            }
        }

        if (!isFoundALimit || limit < 1) limit = 30;

        Console.WriteLine($"Limit: {limit}, Filters: {string.Join(',', filters.ToArray())}");

        // Regression names
        var names = new Dictionary<string, (string title, int n, double sumPvalue, double minPvalue, Token depToken, Token regToken)>();

        // load dust strings
        foreach (var dust in miner.Dusts)
        {
            var key = dust.RegressionName.ToLower();
            if (names.ContainsKey(key))
            {
                var pv = double.IsNaN(dust.Regression.PValue)
                    ? 1
                    : dust.Regression.PValue;

                var rec = names[key];
                rec.n += 1;
                rec.minPvalue = double.Min(rec.minPvalue, pv);
                rec.sumPvalue += Math.Abs(pv - 1) < 0.999999 ? 0 : pv;
                names[key] = rec;
            }
            else
            {
                var pValue = double.IsNaN(dust.Regression.PValue) ? 1 : dust.Regression.PValue;
                names.Add(key, (dust.RegressionName, 1, pValue, pValue, dust.DepToken, dust.RegToken));
            }
        }

        var uniqueName = 0;
        var reportBuffer = new List<string>();

        foreach (var item in names.Values.OrderByDescending(l => l.minPvalue))
        {
            if (filters.Count == 0 && depToken == Token.None && regToken == Token.None
                || PassesFilters(item.title, filters.Keys.ToArray())
                && FilterTokens(item.depToken, item.regToken, depToken, regToken))
            {
                var sb = new StringBuilder();
                sb.Append($"{item.title}".PadRight(32));
                sb.Append($"{item.depToken} vs {item.regToken}".PadRight(24));
                sb.Append($"{item.sumPvalue / item.n:F4}".PadRight(14));
                sb.Append($"{item.minPvalue:F6}".PadRight(14));
                reportBuffer.Add(sb.ToString());

                uniqueName++;
            }

            continue;

            bool PassesFilters(string thisTitle, string[] findMe)
            {
                var result = true;

                foreach (var token in findMe)
                    if (!thisTitle.Contains(token, StringComparison.OrdinalIgnoreCase))
                        result = false;

                return result;
            }


            bool FilterTokens(Token itemDepToken, Token itemRegToken, Token depToken, Token regToken)
            {
                return (depToken == Token.None || itemDepToken == depToken)
                       && (regToken == Token.None || itemRegToken == regToken);
            }


        }

        Console.WriteLine("regression".PadRight(32) +
                          "token".PadRight(24) +
                          "avg p-value".PadRight(14) +
                          "min p-value".PadRight(14)
                          );
        var count = 0;
        foreach (var row in reportBuffer)
        {
            count++;
            if (reportBuffer.Count < count + limit)
                Console.WriteLine(row);
        }

        Console.WriteLine($"Limit: {limit}, Filters: {string.Join(',', filters.ToArray())}");
        Console.WriteLine($"Unique regression names:{uniqueName}");
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

            var elem = elements as Element[] ?? elements.ToArray();
            if (elem.Any())
            {
                foreach (var item in elem)
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

        miner.GenerateGoldRegressions(goldMiner, 1);

        var end = DateTime.Now;
        var interval = end - start;

        Console.WriteLine(
            $"{miner.DustCount} regressions in {interval.TotalMinutes:F3} min.  Regressions/ms: {miner.DustCount / interval.Milliseconds}");

        var myData = miner.Report("qangio");

        foreach (var speck in myData)
        {
            Console.WriteLine(speck);
        }

        Console.WriteLine(
            $"{miner.DustCount} regressions in {interval.TotalMinutes:F3} min.  Regressions/ms: {miner.DustCount / interval.Milliseconds}");
    }
    else if (IsMatch(command, @"^clear*", RegexOptions.IgnoreCase))
    {
        miner.Clear();
        goldMiner.Clear();

    }
    else if (IsMatch(command, @"^gamma", RegexOptions.IgnoreCase))
    {
        var myData = goldMiner.PrintOmegaElementsFor3DGammaStudy(SetName.Omega);
        foreach (var item in myData)
        {
            Console.WriteLine(item);
        }
    }
    else if (IsMatch(command, @"^matrix\s*(\w+)?\s*$", RegexOptions.IgnoreCase))
    {
        var tokens = Match(command, @"^matrix\s*(\w+)?\s*$", RegexOptions.IgnoreCase);
        if (!tokens.Groups[1].Success)
        {
            var rootDusts = goldMiner.RootStatisticMatrix(SetName.Omega);
            miner.AddRange(rootDusts);
            Console.WriteLine(string.Join('\n', GoldMiner.ToStringFormatRegressionsInDusts(rootDusts)));
            continue;
        }

        //else G1 is there :) !!!
        {
            var setName = tokens.Groups[1].Value;
            if (Enum.TryParse(setName, true, out SetName result))
            {
                var rootDusts = goldMiner.RootStatisticMatrix(result);
                Console.WriteLine(string.Join('\n', GoldMiner.ToStringFormatRegressionsInDusts(rootDusts)));
                miner.AddRange(rootDusts);
                continue;
            }

            // ... but G1 did not parse as  a valid set soo ... try to do something useful     

            var dusts = goldMiner.RootAllSetMatrix();
            miner.AddRange(dusts);
            Console.WriteLine(string.Join('\n', GoldMiner.ToStringFormatRegressionsInDusts(dusts)));
        }
    }
    else if (IsMatch(command, @"^keto.*", RegexOptions.IgnoreCase))
    {
        var myData = goldMiner.PrintKetoCta();
        foreach (var item in myData)
        {
            Console.WriteLine(item);
        }
    }
    else if (IsMatch(command, @"^dust", RegexOptions.IgnoreCase))
    {
        try
        {
            var tokens = Regex.Match(command, @"^dust\s*(?:(,?\w+\)?\,?)?\s*(?:(\d+(?:\.\d+)?)?\.?)?)$", RegexOptions.IgnoreCase);
            var filter = tokens.Groups[1].Value;
            var limit = double.TryParse(tokens.Groups[2].Value, out var max) ? max : 0.5;

            foreach (var line in miner.Report(filter, limit))
                Console.WriteLine(line);

            Console.WriteLine($"dust Command Parse: Success: {tokens.Success} : Select: {tokens.Groups[1].Value} : Limit: (i.e. 0.1) {tokens.Groups[2].Value}");
        }
        catch (Exception error)
        {
            Console.WriteLine($"Command parse error '{error.Message}'");
        }
    }
    else if (IsMatch(command, @"^hist*", RegexOptions.IgnoreCase))
    {
        var report = miner.RegressionHistogram();

        foreach (var line in report)
            Console.WriteLine(line);
    }

    else if (IsMatch(command, @"help", RegexOptions.IgnoreCase))
    {
        Console.WriteLine("Possible Commands: 'independent vs. regressor', dust Search  1 (prints thousands of lines, ,01 = 10, mine takes a minite, matrix, " +
                          "keto, clear, histogram, Element, q|exit|quit|end|help");
    }
    // Explore the regression for a single regression title across sub-phenotypes Zeta, Gamma, Theta, Eta
    else if (IsMatch(command, vsPattern, RegexOptions.IgnoreCase))
    {
        // Parse for subset name, if none default to Omega
        var tokens = Match(command, vsPattern, RegexOptions.IgnoreCase);
        string?[] title = tokens.Groups[4].Success ? command.Split('-') : [command];

        if (!tokens.Success)
        {
            Console.WriteLine($"Please submit regressions in the form of 'Dependent vs Regressor' for example 'Cac1 vs Cac0, (Omega|Beta)'");
            continue;
        }

        IEnumerable<Dust> dusts;
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
                DustsToCvs(dusts.Where(d => d.SetName.Equals(item))); // ** Majic

            DustsToRegressionList(dusts, true);
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

                DustsToRegressionList(newDusts);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Please us valid data names");
            }
        }
    }

}