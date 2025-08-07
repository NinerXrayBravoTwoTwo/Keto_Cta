using LinearRegression;

namespace DataMiner
{
    public class MineRegression : RegressionPvalue
    {
        public MineRegression(IEnumerable<(string id, double x, double y)> kxyList) : base(kxyList.Select(tuple => (tuple.x, tuple.y)).ToList())
        { }

        public string[] DataPointIds { get; init; }



        public override string ToString()
        {
            var x = MarginOfError();
            var y = MarginOfError(true);
            return $"X: {x.Mean:F3} moeX: {x.MarginOfError:F3} Y: {y.Mean:F3} moeY: {y.MarginOfError:F3} Slope: {Slope:F3} p-value: {base.PValue:F8}";
        }
    }
}
