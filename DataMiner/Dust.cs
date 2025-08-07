using Keto_Cta;

namespace DataMiner;

public class Dust
{
    public Dust(SetName set, string title, MineRegression regression)
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
        Regression = new MineRegression([]); // Initialize with a default value
    }

    public readonly SetName SetName;
    public readonly string RegressionName;
    public MineRegression Regression;
    public bool IsInteresting => Regression.N >= 2 && Regression.PValue() > 0.0 && Regression.PValue() <= 0.601;

    public override string ToString()
    {
        return $"{RegressionName},{SetName} {Regression.N},Slope {Regression.Slope():F5}," +
               $"p-value: {Regression.PValue():F3}";
    }
}