using DataMiner;
using Keto_Cta;
using LinearRegression;

namespace MineReports;

public class DustsPvalueHistogram
{
    public static string[] Build(Dust[] dusts, string[]? args = null)
    {
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
            { SetName.BetaUZeta, new int[6] },
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
            { SetName.BetaUZeta, [] },
        };

        foreach (var dust in dusts)
        {
            dataPoints[dust.SetName].Add((dust.Regression.PValue, dust.Regression.StdDevX));
            var bucket = (int)(double.IsNaN(dust.Regression.PValue) ? 6 : dust.Regression.PValue * 5);
            histograms[dust.SetName][Math.Min(bucket, 5)]++; // Clamp to 5 for NaN bin
        }

        List<string> report =
        [
            "\nCalculated Subset Regressions:\n"+
            "Set".PadRight(12) +
            "0-0.2".PadLeft(10) +
            "0.2-0.4".PadLeft(10) +
            "0.4-0.6".PadLeft(10) +
            "0.6-0.8".PadLeft(10) +
            "0.8-1.0".PadLeft(10) +
            "NaN".PadLeft(10)
        ];

        //var subsetRegressions = new Dictionary<SetName, RegressionPvalue>();

        foreach (var item in dataPoints)
        {
            var data = dataPoints[item.Key];
            if (data.Count != 0)
            {
                var regression = new RegressionPvalue(data);
                //      subsetRegressions[item.Key] = regression;
                var hist = histograms[item.Key];
                report.Add(
                        $"{item.Key}".PadRight(12) +
                        $"{hist[0]}".PadLeft(10) +
                        $"{hist[1]}".PadLeft(10) +
                        $"{hist[2]}".PadLeft(10) +
                        $"{hist[3]}".PadLeft(10) +
                        $"{hist[4]}".PadLeft(10) +
                        $"{hist[5]}".PadLeft(10));
            }
        }
        return report.ToArray();
    }
}