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
        Assert.NotEmpty(saltMiner.Omega); // Fixed: Accessing static member with type name
        Assert.NotEmpty(saltMiner.Alpha); // Fixed: Accessing static member with type name
        Assert.NotEmpty(saltMiner.Beta); // Fixed: Accessing static member with type name
        Assert.NotEmpty(saltMiner.Zeta); // Fixed: Accessing static member with type name
        Assert.NotEmpty(saltMiner.Gamma); // Fixed: Accessing static member with type name
        Assert.NotEmpty(saltMiner.Theta); // Fixed: Accessing static member with type name
        Assert.NotEmpty(saltMiner.Eta); // Fixed: Accessing static member with type name

        Assert.Equal(100, saltMiner.Omega.Length); // Fixed: Accessing static member with type name
        Assert.Equal(100, saltMiner.Alpha.Length + saltMiner.Zeta.Length); // Fixed: Accessing static member with type name

        testOutputHelper.WriteLine($"Omega Count: {saltMiner.Omega.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Alpha Count: {saltMiner.Alpha.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Zeta Count: {saltMiner.Zeta.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Beta Count: {saltMiner.Beta.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Gamma Count: {saltMiner.Gamma.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Theta Count: {saltMiner.Theta.Length}"); // Fixed: Accessing static member with type name
        testOutputHelper.WriteLine($"Eta Count: {saltMiner.Eta.Length}"); // Fixed: Accessing static member with type name
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

        testOutputHelper.WriteLine($"Omega Regression: Label=Omega, Slope={result[0].Slope()}, Intercept={result[0].YIntercept()}, P-value={result[0].PValue()}");

        testOutputHelper.WriteLine("Salt mining completed successfully.");
    }
}

