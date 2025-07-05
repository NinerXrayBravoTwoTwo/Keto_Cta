using DataMiner;
using Keto_Cta;
using LinearRegression;

class HistogramTool
{
    static void Main(string[] args)
    {
        var ctaDataPath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var MyMine = new GoldMiner(ctaDataPath);
        var Dust = new List<Dust>();

        // Load all charts (example using Element Delta vs. Element Delta)
        var elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");
        for (int x = 0; x < elementDelta.Length; x++)
            for (int y = 0; y < elementDelta.Length; y++)
                if (x != y)
                    Dust.AddRange(MyMine.GoldDust($"{elementDelta[x]} vs. {elementDelta[y]}"));

        // Histograms
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
            histograms[dust.SetName][Math.Min(bucket, 5)]++;
        }

        // Export histograms to CSV
        using var writer = new StreamWriter("histograms.csv");
        writer.WriteLine("Set,N regressions,Average p-value,0-0.2,0.2-0.4,0.4-0.6,0.6-0.8,0.8-1.0,NaN");
        foreach (var item in dataPoints)
        {
            var data = dataPoints[item.Key];
            var regression = new RegressionPvalue(data);
            var hist = histograms[item.Key];
            writer.WriteLine($"{item.Key},{regression.N},{regression.MeanX():F6},{hist[0]},{hist[1]},{hist[2]},{hist[3]},{hist[4]},{hist[5]}");
        }

        Console.WriteLine("Histograms exported to histograms.csv");
    }
}