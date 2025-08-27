using DataMiner;

namespace MineReports
{
    public class ListRegressionCi : IRegressionReport
    {
        public static IRegressionReport CreateInstance()
        {
            return new ListRegressionCi();
        }

        private const string HeaderFormat = "{0,-41}{1,-10}{2,10:F4}{3,8:F4}{4,10:F4}{5,8:F4}{6,10:F3}{7,10:F3}{8,13:F8}";
        private const string RowFormat = HeaderFormat;

        public List<string> ReportBuffer(bool notNaN, IEnumerable<Dust> orderedDusts)
        {
            var reportBuffer = new List<string>
        {
            string.Format(
                HeaderFormat,
                "Regression",
                "Set",
                "Lower",
                "Upper",
                "Slope",
                "Std Err",
                "xSD",
                "ySD",
                "p-value")
        };

            foreach (var dust in orderedDusts)
            {
                if (notNaN && double.IsNaN(dust.Regression.PValue))
                {
                    continue;
                }

                reportBuffer.Add(FormatRow(dust));
            }

            return reportBuffer;
        }

        private static string FormatRow(Dust dust)
        {
            string regressionName = dust.RegressionName;
            string setName = dust.SetName.ToString();

            // Truncate strings to avoid overflow, compatible with older C#
            regressionName = regressionName.Length > 41 ? regressionName.Substring(0, 41) : regressionName;
            setName = setName.Length > 10 ? setName.Substring(0, 10) : setName;

            var confInt = dust.Regression.ConfidenceIntervalPlus();

            return string.Format(
                RowFormat,
                regressionName,
                setName,
                FormatNumber(confInt.Lower, 4),
                FormatNumber(confInt.Upper, 4),
                FormatNumber(confInt.Slope, 4),
                FormatNumber(confInt.StandardError, 4),
                FormatNumber(dust.Regression.StdDevX, 3),
                FormatNumber(dust.Regression.StdDevY, 3),
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
