using DataMiner;
using Keto_Cta;
using MineReports;
using System.Text;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.Regex;

var ctaDataPath = "TestData/keto-cta-quant-and-semi-quant.csv";
var ctaQangioPath = "TestData/keto-cta-qangio.csv";
var goldMiner = new GoldMiner(ctaDataPath, ctaQangioPath);
var miner = new MineRegressionsWithGold(goldMiner);

#region Chart Specific Regression


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
//miner.AddRange(goldMiner.RootAllSetMatrix());

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
    else if (IsMatch(command, @"^title\s?", RegexOptions.IgnoreCase))
    {
        // parse filters and limit
        var result = new CommandParser("title").Parse(command);

        // Regression names
        var names =
            new Dictionary<string, (string title, int n, double sumPvalue, double minPvalue, Token depToken, Token regToken)>();

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
            if (result.SearchTerms.Length == 0 && result.DependentToken == Token.None && result.RegressionToken == Token.None
                || PassesFilters(item.title, result.SearchTerms)
                && FilterTokens(item.depToken, item.regToken, result.DependentToken, result.RegressionToken))
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

            bool PassesFilters(string thisTitle, string[]? findMe)
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
            if (reportBuffer.Count < count + result.Limit)
                Console.WriteLine(row);
        }

        Console.WriteLine(result.ToString());
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
        var tokens =
            Match(command, @"^Mine\s*([A-Z]+)?\s*(\d{1}k)?$", RegexOptions.IgnoreCase); //G2 = size, G1=Type of Mining
        if (!tokens.Success)
        {
            Console.WriteLine("Invalid 'mine' syntax. Try 'mine root','mine ratios', 'mine deltas' ...");
            continue;
        }

        var start = DateTime.Now;

        //miner.GenerateGoldRegressions(goldMiner, 1);
        
        var end = DateTime.Now;
        var interval = end - start;

        if (interval.Seconds > 3)
            Console.WriteLine(
                $"{miner.DustCount} regressions in {interval.TotalMinutes:F3} min.  Regressions/ms: {miner.DustCount / interval.Milliseconds}");


        foreach (var row in DustRegressionList.Build(miner.Dusts, Token.Visit, Token.Visit, 30, true))
            Console.WriteLine(row);


        if (interval.Seconds > 3)
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
    else if (IsMatch(command, @"^matrix\s*", RegexOptions.IgnoreCase))
    {
        var result = new CommandParser("matrix").Parse(command);
        IEnumerable<string> newFilterTerms = [];

        if (result.SearchTerms is { Length: 0 })
            Console.WriteLine($"No matrix mining operations were requested, 'visit', 'ratio', 'comratio', 'ratiovsdelta' ...");

        // var myDusts = new List<Dust>();
        foreach (var filter in result.SearchTerms)
        {
            // get regression matrix list
            // Push matrix into GoldMiner regressionNameQueue
            // turn matrix into AuDust
            // Create a histogram or regression list report


            switch (filter)
            {
                case "visit":
                    miner.AddRange(goldMiner.V1vsV0matrix());
                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("visit", StringComparison.OrdinalIgnoreCase));
                    break;

                case "ratio":
                    miner.AddRange(goldMiner.RootRatioMatrix());
                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("ratio", StringComparison.OrdinalIgnoreCase));
                    break;

                case "comratio":
                    miner.AddRange(goldMiner.RootComboRatio());
                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("comratio", StringComparison.OrdinalIgnoreCase));
                    break;

                case "ratiovsdelta":
                    miner.AddRange(goldMiner.RatioVsDelta());
                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("ratiovsdelta", StringComparison.OrdinalIgnoreCase));
                    break;

                case "cool":
                    miner.AddRange(goldMiner.CoolMatrix());
                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("cool", StringComparison.OrdinalIgnoreCase));
                    break;

                //case "mine":
                //    break;
                default:

                    break;
            }
        }
        if (miner.DustCount == 0)
            Console.WriteLine("No dust to report.");
        else
        {
            var report = DustRegressionList.Build(
                miner.Dusts,
                newFilterTerms.ToArray(),
                result.DependentToken,
                result.RegressionToken,
                result.Limit);

            if (report.Length < 2)
                Console.WriteLine(result);

            foreach (var row in report)
                Console.WriteLine(row);

            Console.WriteLine($"Total Regressions: {miner.DustCount}");
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
        var result = new CommandParser("dust").Parse(command);

        if (!result.IsSuccess)
        {
            Console.WriteLine("Command dust syntax error; Try 'dust' 50 or 'dust Cac 30'");
        }
        else
        {
            foreach (var line in DustRegressionList.Build(
                         miner.Dusts, result.SearchTerms,
                         result.DependentToken, result.RegressionToken,
                         result.Limit, true))
            {
                Console.WriteLine(line);
            }
            Console.WriteLine($"Totatoe ;) #nuggets: {miner.DustCount}");
        }
    }

    else if (IsMatch(command, @"^hist*", RegexOptions.IgnoreCase))
    {
        var report = MineReports.DustsPvalueHistogram.Build(miner.Dusts.ToArray());

        foreach (var line in report)
            Console.WriteLine(line);
    }
    else if (IsMatch(command, @"help", RegexOptions.IgnoreCase))
    {
        Console.WriteLine(
            "Possible Commands: 'independent vs. regressor', dust Search  1 (prints thousands of lines, ,01 = 10, mine takes a minite, matrix, " +
            "keto, clear, histogram, Element, q|exit|quit|end|help");
    }
    // Explore the regression for a single regression title across sub-phenotypes Zeta, Gamma, Theta, Eta
    else if (IsMatch(command, vsPattern, RegexOptions.IgnoreCase))
    {
        // Parse for subset name, if none default to Omega
        var tokens = Match(command, vsPattern, RegexOptions.IgnoreCase);
        string[] title = tokens.Groups[4].Success ? command.Split('-') : [command];

        if (!tokens.Success)
        {
            Console.WriteLine(
                $"Please submit regressions in the form of 'Dependent vs Regressor' for example 'Cac1 vs Cac0, (Omega|Beta)'");
            continue;
        }

        IEnumerable<Dust> dusts;
        try
        {
            dusts = goldMiner.GoldDust(title[0]);
            miner.AddRange(dusts);
        }
        catch (Exception error)
        {
            Console.WriteLine($"Error generating regression for '{title[0]}': {error.Message}");
            continue;
        }

        //localDusts.AddRange(dusts);

        List<SetName> useSets = [];

        // User wants specific LMHR sub phenotype set

        var inst = tokens.Groups[4].Success ? title[1]?.ToLower() : string.Empty;
        var isAllPhenotypes = inst != null && inst.Contains("all");

        // Scan decorations for set names
        foreach (var item in Enum.GetNames(typeof(SetName)))
        {
            if (inst != null && isAllPhenotypes | inst.Contains(item.ToLower()))
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

            foreach (var row in DustRegressionList.Build(dusts.ToArray()))
                Console.WriteLine(row);

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

                foreach (var row in DustRegressionList.Build(dusts.ToArray()))
                    Console.WriteLine(row);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Please us valid data names");
            }
        }
    }

    continue;
}

