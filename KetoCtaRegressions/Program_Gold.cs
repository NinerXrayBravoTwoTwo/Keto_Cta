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

RegressionNamesProcessor threadA = new RegressionNamesProcessor(goldMiner, goldMiner.RegressionNameQueue);
GoldDustProcessor threadB = new GoldDustProcessor(goldMiner, goldMiner.DustQueue);

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
var regressionList = new DustRegressionList(RegressionReport.PValue);

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
        foreach (var dust in goldMiner.DustDictionary.Values)
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
            if (result.SearchTerms.Length == 0 && result is { DependentToken: Token.None, RegressionToken: Token.None }
                || PassesFilters(item.title, result.SearchTerms)
                && FilterTokens(item.depToken, item.regToken, result.DependentToken, result.RegressionToken))
            {
                var sb = new StringBuilder();
                sb.Append($"{item.title}".PadRight(47));
                sb.Append($"{item.depToken} vs {item.regToken}".PadRight(24));
                sb.Append($"{item.sumPvalue / item.n:F4}".PadRight(14));
                sb.Append($"{item.minPvalue:F6}".PadRight(14));
                reportBuffer.Add(sb.ToString());

                uniqueName++;
            }

            continue;

            bool PassesFilters(string thisTitle, string[]? findMe)
            {
                var isPass = true;

                if (findMe != null)
                    foreach (var token in findMe)
                        if (!thisTitle.Contains(token, StringComparison.OrdinalIgnoreCase))
                            isPass = false;

                return isPass;
            }
            bool FilterTokens(Token itemDepToken, Token itemRegToken, Token depToken, Token regToken)
            {
                return (depToken == Token.None || itemDepToken == depToken)
                       && (regToken == Token.None || itemRegToken == regToken);
            }
        }

        Console.WriteLine("regression".PadRight(47) +
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
        var result = new CommandParser("mine").Parse(command);

        if (!result.IsSuccess)
        {
            Console.WriteLine("Command dust syntax error; Try 'dust' 50 or 'dust Cac 30'");
        }
        else
        {
            if (miner.Success)
                Console.WriteLine("Woooa!  Cool yer jets there Haus, these regressions already are running.\n" +
                                  " If you want to start over type 'Clear' which will erase all computed data and clear the queues");


            miner.MineOperation();
            foreach (var line in regressionList.Build(
                         goldMiner.DustDictionary.Values, result.SearchTerms,
                         result.DependentToken, result.RegressionToken, result.SetNames,
                         result.Limit, true))
            {
                Console.WriteLine(line);
            }
            Console.WriteLine($"Total Regressions: {goldMiner.DustDictionary.Count} Queued Names: {goldMiner.RegressionNameQueue.Count} Queued Dusts: {goldMiner.DustQueue.Count}");
        }
    }
    else if (IsMatch(command, @"^clear*", RegexOptions.IgnoreCase))
    {
        goldMiner.Clear();
        miner.Clear();
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
            Console.WriteLine($"No matrix mining operations were requested, 'visit', 'ratio', Delta' 'cool' 'LnStudy' 'mono', 'mine'");

        // var myDusts = new List<Dust>();
        foreach (var filter in result.SearchTerms)
        {
            // get regression matrix list
            // Push matrix into GoldMiner regressionNameQueue
            // turn matrix into AuDust
            // Create a histogram or regression list report

            switch (filter.ToLower())
            {
                case "visit":
                    miner.V1vsV0matrix()
                        .ToList()
                        .ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("visit", StringComparison.OrdinalIgnoreCase));
                    break;

                case "ratio":
                    miner.RootRatioMatrix()
                        .ToList()
                        .ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));

                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("ratio", StringComparison.OrdinalIgnoreCase));
                    break;

                case "delta":
                    miner.RatioVsDelta()
                        .ToList()
                        .ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));

                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("ratiovsdelta", StringComparison.OrdinalIgnoreCase));
                    break;

                case "cool":
                    miner.CoolMatrix()
                        .ToList()
                        .ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("cool", StringComparison.OrdinalIgnoreCase));
                    break;

                case "lnstudy":
                    miner.LnStudy()
                        .ToList()
                        .ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("lnstudy", StringComparison.OrdinalIgnoreCase));
                    break;

                case "mono":
                    miner.ElemMono()
                        .ToList()
                        .ForEach(item => goldMiner.RegressionNameQueue.Enqueue(item));
                    newFilterTerms = result.SearchTerms.Where(s => !s.Contains("mono", StringComparison.OrdinalIgnoreCase));
                    break;

                default:
                    newFilterTerms = result.SearchTerms;
                    break;
            }
        }

        if (goldMiner.DustDictionary.Count == 0)

            Console.WriteLine("No dust to report.");
        else
        {
            var report = regressionList.Build(
                goldMiner.DustDictionary.Values, newFilterTerms.ToArray(),
                result.DependentToken, result.RegressionToken,
                result.SetNames, result.Limit, true);

            if (report.Length < 2)
                Console.WriteLine(result);

            foreach (var row in report)
                Console.WriteLine(row);

            Console.WriteLine($"Total Regressions: {goldMiner.DustDictionary.Count} Queued Names: {goldMiner.RegressionNameQueue.Count} Queued Dusts: {goldMiner.DustQueue.Count}");
        }
    }
    else if (IsMatch(command, @"^keto.*", RegexOptions.IgnoreCase))
    {
        var result = new CommandParser("keto").Parse(command);

        if (result.SearchTerms is { Length: 0 })
            Console.WriteLine("Keto-CTA data report options: Extend, growth, HalfLife ");

        foreach (var filter in result.SearchTerms)
        {
            string[] reportRows = [];
            switch (filter.ToLower())
            {
                case "extended":
                    reportRows = goldMiner.PrintKetoCtaExtended();
                    break;

                case "halflife":
                    reportRows = goldMiner.HalfLife(result.SetNames);
                    break;

                case "growth":
                    reportRows = goldMiner.PrintKetoCtaTd(result.SetNames);
                    break;
            }

            foreach (var row in reportRows)
                Console.WriteLine(row);
        }
    }
    else if (IsMatch(command, @"^dust", RegexOptions.IgnoreCase))
    {
        var result = new CommandParser("dust").Parse(command);

        foreach (var item in result.SearchTerms)
        {
            if (item.Contains("pvalue", StringComparison.InvariantCulture))
                regressionList = new DustRegressionList(RegressionReport.PValue);
            else if (item.Contains("conf", StringComparison.InvariantCulture))
                regressionList = new DustRegressionList(RegressionReport.ConfInterval);
        }

        if (!result.IsSuccess)
        {
            Console.WriteLine("Command dust syntax error; Dust Report options 'ConfInterval or pvalue' Try 'dust' 50 or 'dust Cac 30'");
        }
        else
        {
            foreach (var line in regressionList.Build(
                         goldMiner.DustDictionary.Values, result.SearchTerms,
                         result.DependentToken, result.RegressionToken, result.SetNames,
                         result.Limit, true))
            {
                Console.WriteLine(line);
            }
            Console.WriteLine($"Total Regressions: {goldMiner.DustDictionary.Count} Queued Names: {goldMiner.RegressionNameQueue.Count} Queued Dusts: {goldMiner.DustQueue.Count}");
        }
    }
    else if (IsMatch(command, @"^hist*", RegexOptions.IgnoreCase))
    {
        var report = DustsPvalueHistogram.Build(goldMiner.DustDictionary.Values.ToArray());

        foreach (var line in report)
            Console.WriteLine(line);

        Console.WriteLine($"Total Regressions: {goldMiner.DustDictionary.Count} Queued Names: {goldMiner.RegressionNameQueue.Count} Queued Dusts: {goldMiner.DustQueue.Count}");

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
        // Parse for subset name, if none default to Queued Dusts: {goldMiner.DustQueue.Count}
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
        }
        catch (Exception error)
        {
            Console.WriteLine($"Error generating regression for '{title[0]}': {error.Message}");
            continue;
        }

        dusts.ToList().ForEach(item => goldMiner.DustQueue.Enqueue(item));

        List<SetName> useSets = [];

        // User wants specific LMHR sub phenotype set

        var inst = tokens.Groups[4].Success ? title[1].ToLower() : string.Empty;
        var isAllPhenotypes = inst.Contains("all");

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
                DustsToCvs(dusts.Where(d => d.SetName.Equals(item))); // ** Majic **

            foreach (var row in regressionList.Build(dusts.ToArray()))
                Console.WriteLine(row);

            Console.WriteLine("Enter another Chart Title or 'exit' to quit:");
        }
        else // create new dusts
        {
            Console.WriteLine("Can not get here. Thread lock error on Regression-Dictionary?");
        }
    }
}