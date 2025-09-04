using DataMiner;

namespace MineReports
{
    public class ListRegressionBasic : IRegressionReport
    {
        public static IRegressionReport CreateInstance()
        {
            return new ListRegressionBasic();
        }

        private const string HeaderFormat = "{0,-41}{1,-10}{2,10:F3}{3,8:F3}{4,10:F3}{5,8:F3}{6,10:F4}{7,10:F3}{8,13:F8}";
        private const string RowFormat = HeaderFormat;

        public List<string> ReportBuffer(IEnumerable<Dust> orderedDusts)
        {
            var reportBuffer = new List<string>
        {
            string.Format(
                HeaderFormat,
                "Regression",
                "Set",
                "Mean X",
                "moe X",
                "Mean Y",
                "moe Y",
                "Slope",
                "xSD",
                "p-value")
        };

            reportBuffer.AddRange(from dust in orderedDusts select FormatRow(dust));

            return reportBuffer;
        }

        private static string FormatRow(Dust dust)
        {
            string regressionName = dust.RegressionName;
            string setName = dust.SetName.ToString();

            // Truncate strings to avoid overflow, compatible with older C#
            regressionName = regressionName.Length > 41 ? regressionName.Substring(0, 41) : regressionName;
            setName = setName.Length > 10 ? setName.Substring(0, 10) : setName;

            var moeX = dust.Regression.MarginOfError();
            var moeY = dust.Regression.MarginOfError(true);
            return string.Format(
                RowFormat,
                regressionName,
                setName + "-" + dust.Regression.N,
                FormatNumber(moeX.Mean, 3),
                FormatNumber(moeX.MarginOfError, 3),
                FormatNumber(moeY.Mean, 3),
                FormatNumber(moeY.MarginOfError, 3),
                FormatNumber(dust.Regression.Slope, 4),
                FormatNumber(dust.Regression.StdDevX, 3),
                FormatNumber(dust.Regression.PValue, 8));
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
    }
}
