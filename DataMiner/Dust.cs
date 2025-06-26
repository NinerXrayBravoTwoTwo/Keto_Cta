using Keto_Cta;
using LinearRegression;

namespace DataMiner;

public class Dust(SetName set, string title, RegressionPvalue regression)
{
    public readonly SetName SetName = set;
    public readonly string Title = title;
    public readonly Regression Regression = regression ?? throw new ArgumentNullException(nameof(regression));

}