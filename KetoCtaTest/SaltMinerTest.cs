using DataMiner;
using Xunit.Abstractions;

namespace KetoCtaTest;

public class SaltMinerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void SaltMiner_RegressionTest()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";

        var saltMiner = new SaltMiner(filePath);
        Assert.NotNull(saltMiner);
        Assert.NotEmpty(SaltMiner.Omega); // Fixed: Accessing static member with type name
        Assert.NotEmpty(SaltMiner.Alpha); // Fixed: Accessing static member with type name
        Assert.NotEmpty(SaltMiner.Beta); // Fixed: Accessing static member with type name
        Assert.NotEmpty(SaltMiner.Zeta); // Fixed: Accessing static member with type name
        Assert.NotEmpty(SaltMiner.Gamma); // Fixed: Accessing static member with type name
        Assert.NotEmpty(SaltMiner.Theta); // Fixed: Accessing static member with type name
        Assert.NotEmpty(SaltMiner.Eta); // Fixed: Accessing static member with type name

        Assert.Equal(100, SaltMiner.Omega.Length); // Fixed: Accessing static member with type name
        Assert.Equal(100, SaltMiner.Alpha.Length + SaltMiner.Zeta.Length); // Fixed: Accessing static member with type name

        testOutputHelper.WriteLine($"Omega Count: {SaltMiner.Omega.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Alpha Count: {SaltMiner.Alpha.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Zeta Count: {SaltMiner.Zeta.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Beta Count: {SaltMiner.Beta.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Gamma Count: {SaltMiner.Gamma.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Theta Count: {SaltMiner.Theta.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Eta Count: {SaltMiner.Eta.Length}"); // Fixed: Accessing static member with type name
    }

    [Fact]
    public void LnNcpLnDcac()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var saltMiner = new SaltMiner(filePath);
        Assert.NotNull(saltMiner);
        var result = saltMiner.MineLnDNcpLnDCac();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(7, result.Length); // Expecting 7 sets: Omega, Alpha, Zeta, Beta, Gamma, Theta, Eta
        Assert.All(result, Assert.NotNull); // Ensure all results are not null
        Assert.Equal(100, result[0].N);

        testOutputHelper.WriteLine($"Omega Regression: Label=Omega, Slope={result[0].Slope}, Intercept={result[0].YIntercept}, P-value={result[0].PValue}");

        testOutputHelper.WriteLine("Salt mining completed successfully.");
    }
}

