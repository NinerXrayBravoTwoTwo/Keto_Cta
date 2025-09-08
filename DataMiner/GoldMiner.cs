using Keto_Cta;
using LinearRegression;
using System.Collections.Concurrent;

namespace DataMiner;
// Assuming GoldMiner class with DustsQueue
public partial class GoldMiner
{
    public readonly ConcurrentDictionary<Guid, Dust> DustDictionary = new();
    public readonly ConcurrentQueue<string> RegressionNameQueue = new();
    public readonly ConcurrentQueue<Dust> DustQueue = new();
}

public partial class GoldMiner
{
    public GoldMiner(string ketoCtaPath, string qAngioPath = "")
    {
        //var heartflow = ReadHeartflowCsvFile(heartFlowPath); // Placeholder for future Heartflow data integration
        var qangio = ReadQangioCsvFile(qAngioPath);

        var elements = ReadKetoCtaFile(ketoCtaPath, qangio) ??
                       throw new ArgumentException("CSV file returned null elements.", nameof(ketoCtaPath));

        Omega = elements.Where(e => e.MemberSet is LeafSetName.Zeta or LeafSetName.Gamma or LeafSetName.Theta or LeafSetName.Eta).ToArray();
        Alpha = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta or LeafSetName.Gamma).ToArray();
        Beta = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta).ToArray();
        Zeta = elements.Where(e => e.MemberSet == LeafSetName.Zeta).ToArray();
        Gamma = elements.Where(e => e.MemberSet == LeafSetName.Gamma).ToArray();
        Theta = elements.Where(e => e.MemberSet == LeafSetName.Theta).ToArray();
        Eta = elements.Where(e => e.MemberSet == LeafSetName.Eta).ToArray();
        BetaUZeta = elements.Where(e => e.MemberSet is LeafSetName.Theta or LeafSetName.Eta or LeafSetName.Zeta).ToArray();

        _setNameToData = new Dictionary<SetName, Element[]>
        {
            { SetName.Omega, Omega },
            { SetName.Alpha, Alpha },
            { SetName.Beta, Beta },
            { SetName.Zeta, Zeta },
            { SetName.Gamma, Gamma },
            { SetName.Eta, Eta },
            { SetName.Theta, Theta },
            { SetName.BetaUZeta, BetaUZeta },
        };
    }

    public Element[] Omega;
    public Element[] Alpha;
    public Element[] Beta;
    public Element[] Zeta;
    public Element[] Gamma;
    public Element[] Theta;
    public Element[] Eta;
    public Element[] BetaUZeta;

    private readonly Dictionary<SetName, Element[]> _setNameToData;
    private readonly Dictionary<string, CreateSelector> _selectorCache = new();

    #region load Data

    public IEnumerable<Element> Elements => Omega;

    /// <summary>
    /// Reads a Keto-CTA CSV file from the specified path and parses its contents into a list of <see cref="Element"/> objects.
    /// </summary>
    /// <remarks>The method expects the CSV file to have a specific structure where each row contains numeric
    /// values  separated by commas. The first row is assumed to be a header and is skipped during processing. If a row
    /// contains invalid numeric data, it is skipped, and a message is logged to the console.</remarks>
    /// <param name="ketoCtaPath">The file path of the CSV file to read. The file must exist and be accessible.</param>
    /// <param name="qAngioData"></param>
    /// <param name="heartFlowData"></param>
    /// <returns>A list of <see cref="Element"/> objects created from the parsed rows of the CSV file.  Each <see
    /// cref="Element"/> contains two <see cref="Visit"/> objects representing the data in the row.</returns>
    private static List<Element> ReadKetoCtaFile(string ketoCtaPath, List<QAngio>? qAngioData = null, List<HeartflowData>? heartFlowData = null)
    {
        var list = new List<Element>();
        using var reader = new StreamReader(ketoCtaPath);
        var index = 0;
        if (!reader.EndOfStream) reader.ReadLine();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
#pragma warning disable CS8602
            var values = line.Split(',');
#pragma warning restore CS8602

            index++;
            double qa1;
            double qa2;
            if (qAngioData != null)
            {
                var qa = qAngioData.FirstOrDefault(q => q.Id == index);
                qa1 = qa?.QAngio1 ?? double.NaN;
                qa2 = qa?.QAngio2 ?? double.NaN;
            }
            else
            {
                // If no QAngio data is provided, set to NaN
                qa1 = double.NaN;
                qa2 = double.NaN;
            }

            try
            {
                var visit1 = new Visit("V1", null, int.Parse(values[0]), int.Parse(values[2]), double.Parse(values[4]),
                    double.Parse(values[6]), double.Parse(values[8]), qa1);
                var visit2 = new Visit("V2", null, int.Parse(values[1]), int.Parse(values[3]), double.Parse(values[5]),
                    double.Parse(values[7]), double.Parse(values[9]), qa2);

                var element = new Element(index.ToString(), [visit1, visit2]);
                list.Add(element);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Skipping line {index + 1}: invalid number format ({ex.Message}).");
            }
        }

        return list;
    }

    private static List<QAngio> ReadQangioCsvFile(string qAngioPath)
    {
        if (string.IsNullOrEmpty(qAngioPath)) return [];

        var list = new List<QAngio>();
        using var reader = new StreamReader(qAngioPath);
        var lineNumber = 0;
        if (!reader.EndOfStream) reader.ReadLine();
        while (!reader.EndOfStream)
        {
            lineNumber++;

            var line = reader.ReadLine();
#pragma warning disable CS8602
            var values = line.Split(',');
#pragma warning restore CS8602

            try
            {
                var qAngioRow = new QAngio(int.Parse(values[0]), double.Parse(values[1]), double.Parse(values[2]));
                list.Add(qAngioRow);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"QAngio - Skipping Row {lineNumber + 1}: invalid number format ({ex.Message}).");
            }
        }

        return list;
    }

    private static List<QAngio> ReadHeartflowCsvFile(string heartFlowPath)
    {
        if (string.IsNullOrEmpty(heartFlowPath)) return [];

        var list = new List<QAngio>();
        using var reader = new StreamReader(heartFlowPath);
        var lineNumber = 0;
        if (!reader.EndOfStream) reader.ReadLine();
        while (!reader.EndOfStream)
        {
            lineNumber++;

            var line = reader.ReadLine();
#pragma warning disable CS8602
            var values = line.Split(',');
#pragma warning restore CS8602

            try
            {
                var qAngioRow = new QAngio(int.Parse(values[0]), double.Parse(values[1]), double.Parse(values[2]));
                list.Add(qAngioRow);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"QAngio - Skipping Row {lineNumber + 1}: invalid number format ({ex.Message}).");
            }
        }

        return list;
    }

    #endregion

    /// <summary>
    /// Generates an array of gold dust data based on the specified chart title.
    /// </summary>
    /// <remarks>This method creates gold dust data for a predefined set of names and filters out any null
    /// entries. The resulting array contains only valid <see cref="Dust"/> objects.</remarks>
    /// <param name="chartTitle">The title of the chart used to generate the gold dust data.</param>
    /// <returns>An array of <see cref="Dust"/> objects representing the gold dust data.  The array will exclude any null values.</returns>
    public Dust[] GoldDust(string chartTitle)
    {
        return new List<Dust?>
        {
            AuDust(SetName.Omega, chartTitle),
            AuDust(SetName.Alpha, chartTitle),
            AuDust(SetName.Zeta, chartTitle),
            AuDust(SetName.Beta, chartTitle),
            AuDust(SetName.Gamma, chartTitle),
            AuDust(SetName.Theta, chartTitle),
            AuDust(SetName.Eta, chartTitle),
            AuDust(SetName.BetaUZeta, chartTitle),
        }.Where(d => d != null).Cast<Dust>().ToArray();
    }

    /// <summary>
    ///  
    /// </summary>
    /// <param name="setName"></param>
    /// <param name="chartTitle"></param>
    /// <returns></returns>
    public Dust? AuDust(SetName setName, string chartTitle)
    {
        if (!_setNameToData.TryGetValue(setName, out var data) || data.Length == 0)
        {
            Console.WriteLine($"No data for set {setName} in chart {chartTitle}");
            return null;
        }

        if (!_selectorCache.TryGetValue(chartTitle.ToLower(), out var selector))
        {
            try
            {
                selector = new CreateSelector(chartTitle);

                if (!selector.IsDepRegByRank)
                    _selectorCache.Add(chartTitle.ToLower(), selector);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Invalid chart title {chartTitle}: {ex.Message} Note that 'vs' must be a separate token with white space separation from dependent vs regressor.");

                return null;
            }
        }


        IEnumerable<(string id, double x, double y)> selectedData;

        if (selector.IsDepRegByRank)
        {
            var mv = new RankSelector(selector, data);
            selectedData = mv.DataPoints.Select(t => (t.id, t.x, t.y));
        }
        else
        {
            selectedData = data.Select(selector.Selector);
        }

        var regression = new RegressionPvalue(selectedData.Where(t => !double.IsNaN(t.x) && !double.IsNaN(t.y)).ToList());

        return regression.DataPointsCount() < 3 ? null : new Dust(setName, chartTitle, regression, selector.DependentCompile.token, selector.RegressorCompile.token);
    }

    public string[] PrintBetaElements(SetName setName)
    {
        if (!_setNameToData.TryGetValue(setName, out var elements))
        {
            return [];
        }

        List<string> myData =
        [
            "index,Set,DCac,DNCpv,LnDCac,LnDNcpv," +
            "Ratio0,Ratio1," +
            "Cac0,Cac1,LnCac0,LnCac1," +
            "Ncpv0,Ncpv1,LnNcpv0,LnNcpv1," +
            "Cac0/Ncpv0,Cac0/Ncpv1," +
            "Ln/LnNcpv0,LnCac0/LnNcpv1"
        ];

        foreach (var element in elements)
        {
            var ratio0 = element.Visits[0].Ncpv == 0.0 ? 0 : MathUtils.Ln(element.Visits[0].Cac / element.Visits[0].Ncpv);
            var ratio1 = element.Visits[1].Ncpv == 0.0 ? 0 : MathUtils.Ln(element.Visits[0].Cac / element.Visits[1].Ncpv);

            myData.Add(
                $"{element.Id},{element.MemberSet},{element.DCac},{element.DNcpv},{element.LnDCac},{element.LnDNcpv},"
                + $"{element.Visits[0].Cac},{element.Visits[1].Cac},{ratio0},{ratio1},"
                + $"{element.Visits[0].LnCac},{element.Visits[1].LnCac},"
                + $"{element.Visits[0].Ncpv},{element.Visits[1].Ncpv},{element.Visits[0].LnNcpv},{element.Visits[1].LnNcpv},"
                + $"{element.Visits[0].Cac / element.Visits[0].Ncpv},{element.Visits[0].Cac / element.Visits[1].Ncpv},"
                + $"{element.Visits[0].LnCac / element.Visits[0].LnNcpv},{element.Visits[0].LnCac / element.Visits[1].LnNcpv}");
        }

        return myData.ToArray();
    }

    public string[] PrintKetoCtaTd(SetName[] setNames)
    {
        var elements =
            setNames.Length == 0
           ? _setNameToData[SetName.Omega]
           : _setNameToData[setNames[0]];

        const string headerFormat = "{0,-4}{1,8}{2,7:F0}{3,7:F0}{4,9:F4}{5,7:F4}{6,10:F4}{7,12:F4}{8,14:F3}{9,17:F3}{10,15:F3}";
        const string rowFormat = headerFormat;

        var reportBuffer = new List<string>
        {
            string.Format(
                headerFormat,
                "Id",
                "Set",
                "Cac0",
                "Cac1",
                "CacPred",
                "Ncpv0",
                "Ncpv1",
                "NcpvPred",
                "Td-Cac-years",
                "Td-Ncpv-years",
                "Td-QAngio-yrs")
        };

        foreach (var element in elements.OrderByDescending(e => e.TdCac))
        {
            reportBuffer.Add(string.Format(
                rowFormat,
                element.Id, // 0
                element.MemberSet,
                FormatNumber(element.Visits[0].Cac, 0),
                FormatNumber(element.Visits[1].Cac, 0),
                FormatNumber(element.CacPredict, 2), // 4
                FormatNumber(element.Visits[0].Ncpv, 1),
                FormatNumber(element.Visits[1].Ncpv, 1),
                FormatNumber(element.NcpvPredict, 2), // 7
                FormatNumber(element.TdCac, 5),
                FormatNumber(element.TdNcpv, 5),
                FormatNumber(element.TdQangio, 5) //10
            ));
        }

        return reportBuffer.ToArray();
    }

    private static string FormatNumber(double value, int precision)
    {
        if (double.IsNaN(value))
        {
            return "NaN".PadLeft(precision + 2); // +2 for decimal point and sign
        }

        if (double.IsInfinity(value))
        {
            return (value > 0 ? "Inf" : "-Inf").PadLeft(precision + 2);
        }

        return value.ToString($"F{precision}").PadLeft(precision + 2);
    }

    public string[] HalfLife(SetName[] resultSetNames)
    {
        // convert SetName into LeafSetName
        LeafSetName[] leafSetNames = resultSetNames.Select(setName => setName switch
            {
                SetName.Zeta => LeafSetName.Zeta,
                SetName.Gamma => LeafSetName.Gamma,
                SetName.Eta => LeafSetName.Eta,
                SetName.Theta => LeafSetName.Theta,
                _ => throw new ArgumentOutOfRangeException(nameof(resultSetNames), $"Unknown SetName: {setName}")
            })
            .ToArray();

        List<string> myData =
        [
            "Id,subSet, Cac 0,Cac 1,Ncpv 0,Ncpv 1"
        ];

        myData.AddRange(from element in Elements
                        where IsSetNameMatch(element.MemberSet, leafSetNames)
                        select $"{element.Id},{element.MemberSet},{element.Visits[0].Cac},{element.Visits[1].Cac},{element.Visits[0].Ncpv},{element.Visits[1].Ncpv}");

        ////return myData.ToArray();

        var Id = (from element in Elements where IsSetNameMatch(element.MemberSet, leafSetNames) select element.Id)
            .ToList();

        var Cac0 = (from element in Elements
                    where IsSetNameMatch(element.MemberSet, leafSetNames)
                    select element.Visits[0].Cac).ToList();
        var Cac1 = (from element in Elements
                    where IsSetNameMatch(element.MemberSet, leafSetNames)
                    select element.Visits[1].Cac).ToList();
        var Ncpv0 = (from element in Elements
                     where IsSetNameMatch(element.MemberSet, leafSetNames)
                     select element.Visits[0].Ncpv).ToList();
        var Ncpv1 = (from element in Elements
                     where IsSetNameMatch(element.MemberSet, leafSetNames)
                     select element.Visits[1].Ncpv).ToList();

        myData.AddRange(
        [
            "\n\n# Input dataset\ndata = {",
            "\t\"Id\": [" + string.Join(',', Id) + "],",
            "\t\"Cac0\": [" + string.Join(',', Cac0) + "],",
            "\t\"Cac1\": [" + string.Join(',', Cac1) + "],",
            "\t\"Ncpv0\": [" + string.Join(',', Ncpv0) + "],",
            "\t\"Ncpv1\": [" + string.Join(',', Ncpv1) + "]\n}"

        ]);

        return myData.ToArray();
        /*
             * # Input dataset
               data = {
                   "Id": [62,76,78,79,80,82,83,84,85,86,88,90,93,96,98,99,100],
                   "Cac0": [27,69,81,53,66,191,217,17,222,211,88,388,199,556,265,194,221],
                   "Cac1": [41,105,97,103,97,218,245,35,253,254,100,400,230,768,322,272,256],
                   "Ncpv0": [45.3,53.4,82.4,130.5,78,233.8,169,58.9,244.9,365.6,238.5,147.2,290.8,255.8,200.3,71.8,450.6],
                   "Ncpv1": [51.6,112.2,168.4,179,89.6,345.8,182.2,76.4,357.9,428.5,307.3,194.8,378.9,389.6,275.1,103.6,606.5]
             */
    }

    private static bool IsSetNameMatch(LeafSetName dustSetName, LeafSetName[] setNames)
    {
        return setNames.Length == 0 || setNames.Contains(dustSetName);
    }

    public string[] PrintKetoCtaExtended(bool header = true)
    {
        if (!_setNameToData.TryGetValue(SetName.Omega, out var elements))
        {
            return [];
        }

        //V1_Total_Plaque_Score,V2_Total_Plaque_Score,V1_CAC,V2_CAC,V1_Non_Calcified_Plaque_Volume,V2_Non_Calcified_Plaque_Volume,V1_Total_Calcified_Plaque_Volume,V2_Total_Calcified_Plaque_Volume,V1_Percent_Atheroma_Volume,V2_Percent_Atheroma_Volume
        // 
        List<string> myData = header
            ?
            [
                "Index,Set," +
                "Tps0,Tps1,Cac0,Cac1,Ncpv0,Ncpv1,Tcpv0,Tcpv1,Pav0,Pav1,Qangio0,Qangio1," +
                "LnTps0,LnTps1,LnCac0,LnCac1,LnNcpv0,LnNcpv1,LnTcpv0,LnTcpv1,LnPav0,LnPav1,LnQangio0,LnQangio1," +
                "DTps,DCac,DNcpv,DTcpv,DPav,DQangio,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav,LnDQangio"
            ]
            : [];

        myData.AddRange(elements.Select(elem =>
            $"{elem.Id},{elem.MemberSet}," +
            $"{elem.Visits[0].Tps},{elem.Visits[1].Tps},{elem.Visits[0].Cac},{elem.Visits[1].Cac},{elem.Visits[0].Ncpv},{elem.Visits[1].Ncpv},{elem.Visits[0].Tcpv},{elem.Visits[1].Tcpv},{elem.Visits[0].Pav},{elem.Visits[1].Pav},{elem.Visits[0].Qangio},{elem.Visits[1].Qangio}," +
            $"{elem.Visits[0].LnTps},{elem.Visits[1].LnTps},{elem.Visits[0].LnCac},{elem.Visits[1].LnCac},{elem.Visits[0].LnNcpv},{elem.Visits[1].LnNcpv},{elem.Visits[0].Pav},{elem.Visits[0].LnTcpv},{elem.Visits[1].LnTcpv},{elem.Visits[0].LnPav},{elem.Visits[1].LnPav},{elem.Visits[0].LnQangio},{elem.Visits[1].LnQangio}," +
            $"{elem.DTps},{elem.DCac},{elem.DNcpv},{elem.DTcpv},{elem.DPav},{elem.DQangio},{elem.LnDTps},{elem.LnDCac},{elem.LnDNcpv},{elem.LnDTcpv},{elem.LnDPav},{elem.LnDQangio}"));

        return myData.ToArray();
    }

    public void Clear()
    {
        RegressionNameQueue.Clear();
        DustQueue.Clear();
        DustDictionary.Clear();

        _selectorCache.Clear();
    }
}
