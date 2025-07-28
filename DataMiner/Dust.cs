using Keto_Cta;
using LinearRegression;

namespace DataMiner;

public class Dust
{
    public Dust(SetName set, string title, RegressionPvalue regression)
    {
        SetName = set;
        ChartTitle = title;
        try
        {
            Regression = regression ?? throw new ArgumentNullException(nameof(regression));
        }
        catch (ArgumentException error)
        {
            System.Diagnostics.Debug.WriteLine($"Dust; {error.Message} {title}, {set}");
            throw;
        }
    }

    public Dust(SetName set, string title)
    {
        SetName = set;
        ChartTitle = title;
        Regression = new RegressionPvalue(); // Initialize with a default value
    }

    public readonly SetName SetName;
    public readonly string ChartTitle;
    public RegressionPvalue Regression;
    public bool IsInteresting => Regression.N >= 2 && Regression.PValue() > 0.0 && Regression.PValue() <= 0.601;

    public override string ToString()
    {
        return $"{SetName}, {ChartTitle}, Slope {Regression.Slope():F5}, "
               + $"N={Regression.N}, p-value: {Regression.PValue():F3}";
    }
}