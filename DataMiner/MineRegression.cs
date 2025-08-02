using LinearRegression;
using System;

namespace DataMiner
{
    public class MineRegression : RegressionPvalue
    {
        public MineRegression(IEnumerable<(string id, double x, double y)> kxyList)
        {
            base.DataPoints = kxyList.Select(triple => (triple.x, triple.y)).ToList();
            DataPointIds = kxyList.Select(triple => triple.id).ToArray();
        }

        public string[] DataPointIds { get; set; }

        public new double Slope => (base.Slope());
        public new double Intercept => (base.YIntercept());
        public new double R2 => (base.RSquared());
        public new double PValue => (base.PValue());
        public new double MeanX => (base.MeanX());
        public new double MeanY => (base.MeanY());
        public new (double Mean, double MarginOfError) MarginOfErrorX => (base.MarginOfError().Mean, base.MarginOfError().MarginOfError);
        public new (double Mean, double MarginOfError) MarginOfErrorY => (base.MarginOfError(true).Mean, base.MarginOfError(true).MarginOfError);
        public new double StdDevX => (base.StdDevX());
        public new double StdDevY => (base.StdDevY());
        public new double Correlation => (base.Correlation());

        public override string ToString()
        {
            var x = MarginOfErrorX;
            var y = MarginOfErrorY;
            return $"X: {x.Mean:F3} moeX: {x.MarginOfError:F3} Y: {y.Mean:F3} moeY: {y.MarginOfError:F3} Slope: {Slope():F3} p-value: {base.PValue():F8}";
        }
    }
}
