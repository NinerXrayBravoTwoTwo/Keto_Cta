using System.Reflection;
using Keto_Cta;
using LinearRegression;

namespace DataMiner;

public class Dust
{
    public Dust(SetName set, string title, RegressionPvalue regression)
    {
        SetName = set;
        Title = title;
        RegressionPvalue = regression ?? throw new ArgumentNullException(nameof(regression));
    }

    public Dust(SetName set, string title, CreateSelector selector)
    {
        SetName = set;
        Title = title;
        RegressionPvalue = new RegressionPvalue(); // Initialize with a default value
    }

    public readonly SetName SetName;
    public readonly string Title;
    public RegressionPvalue RegressionPvalue;

    public override string ToString()
    {
        return $"{SetName}, {Title}, Slope {RegressionPvalue.Slope():F5}, "
               + "N={RegressionPvalue.N}, p-value: {RegressionPvalue.PValue():F3}";
    }
}