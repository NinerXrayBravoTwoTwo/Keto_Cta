using Keto_Cta;
using LinearRegression;

namespace DataMiner;

public class Dust(SetName set, string title, RegressionPvalue regression)
{
    public readonly SetName SetName = set;
    public readonly string Title = title;
    public readonly RegressionPvalue Regression = regression ?? throw new ArgumentNullException(nameof(regression));

    public override string ToString()
    {
        return
            $"{set}, {Title}: Slope {Regression.Slope():F5}, N={Regression.N}, p-value: {Regression.PValue():F3}";
    }
}