using Keto_Cta;
using LinearRegression;

namespace DataMiner;

public class Dust
{
    public Dust(SetName set, string title, RegressionPvalue regression)
    {
        SetName = set;
        RegressionName = title;
        try
        {
            Regression = regression ?? throw new ArgumentNullException(nameof(regression));
        }
        catch (Exception error)
        {
            System.Diagnostics.Debug.WriteLine($"Dust; {error.Message} {title} {set}");
            throw;
        }
    }

    public Dust(SetName set, string title)
    {
        SetName = set;
        RegressionName = title;
        Regression = new RegressionPvalue(); // Initialize with a default value
    }

    public readonly SetName SetName;
    public readonly string RegressionName;
    public RegressionPvalue Regression;
    public bool IsInteresting => Regression is { N: >= 2, PValue: > 0.0 and <= 0.601 };

    public override string ToString()
    {
        return $"{RegressionName},{SetName} {Regression.N},Slope {Regression.Slope:F5}," +
               $"p-value: {Regression.PValue:F3}";
    }
}