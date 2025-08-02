using LinearRegression;
using System;

namespace DataMiner
{
    public class MineRegression : RegressionPvalue
    {
        public MineRegression(IEnumerable<(string id, double x, double y)> kxyList) : base(kxyList.Select(tuple => (tuple.x, tuple.y)).ToList())
        {
            DataPointIds = kxyList.Select(tuple => tuple.id).ToArray();
        
            //List <(double x, double y)> xyList = kxyList.Select(tuple => (tuple.x, tuple.y)).ToList();
            
            //base.DataPoints = xyList;

        }

        public string[] DataPointIds { get; init; }

        public new double Slope => (base.Slope());
        public new double YIntercept => (base.YIntercept());
        public new double RSquared => (base.RSquared());
        public new double PValue => (base.PValue());
        public new double MeanX => (base.MeanX());
        public new double MeanY => (base.MeanY());
        public new double StdDevX => (base.StdDevX());
        public new double StdDevY => (base.StdDevY());
        public new double Correlation => (base.Correlation());

        public override string ToString()
        {
            var x = MarginOfError();
            var y = MarginOfError(true);
            return $"X: {x.Mean:F3} moeX: {x.MarginOfError:F3} Y: {y.Mean:F3} moeY: {y.MarginOfError:F3} Slope: {Slope():F3} p-value: {base.PValue():F8}";
        }
    }
}
