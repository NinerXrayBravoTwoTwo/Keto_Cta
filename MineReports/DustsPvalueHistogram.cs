using DataMiner;
using Keto_Cta;
using LinearRegression;

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
            { SetName.Qangio, new int[6] }
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
            { SetName.Qangio ,[] }
        };

        foreach (var dust in dusts)
        {
            dataPoints[dust.SetName].Add((dust.Regression.PValue, dust.Regression.StdDevX));
            var bucket = (int)(double.IsNaN(dust.Regression.PValue) ? 6 : dust.Regression.PValue * 5);
            histograms[dust.SetName][Math.Min(bucket, 5)]++; // Clamp to 5 for NaN bin
        }

        List<string> report =
        [
            "\nCalculated Subset Regressions:\nSet, 0-0.2, 0.2-0.4, 0.4-0.6, 0.6-0.8, 0.8-1.0, NaN"
        ];

        var subsetRegressions = new Dictionary<SetName, RegressionPvalue>();

        foreach (var item in dataPoints)
        {
            var data = dataPoints[item.Key];
            if (data.Count != 0)
            {
                var regression = new RegressionPvalue(data);
                subsetRegressions[item.Key] = regression;
                var hist = histograms[item.Key];
                report.Add(
                    $"{item.Key}, {hist[0]}, {hist[1]}, {hist[2]}, {hist[3]}, {hist[4]}, {hist[5]}");
            }
        }
        return report.ToArray();
    }
}