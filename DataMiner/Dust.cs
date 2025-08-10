using Keto_Cta;
using LinearRegression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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

        var hashMe = $"{RegressionName}{SetName}{regression.RSquared:F5}".ToLower();
        UniqueKey = GenerateGuidMd5(Regex.Replace(hashMe, @"\s*", string.Empty));
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
    public Guid UniqueKey = Guid.Empty;

    public static Guid GenerateGuidMd5(string hashMeHarder)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(hashMeHarder);

        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        // MD5 is already 16 bytes, perfect for Guid
        return new Guid(hashBytes);
    }

    public override string ToString()
    {
        return $"{RegressionName},{SetName} {Regression.N},Slope {Regression.Slope:F5}," +
               $"p-value: {Regression.PValue:F3}";
    }
}