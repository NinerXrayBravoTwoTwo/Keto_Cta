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
        Assert.NotEmpty(saltMiner.Omega);
        Assert.NotEmpty(saltMiner.Alpha);

        Assert.NotEmpty(saltMiner.Beta);
        Assert.NotEmpty(saltMiner.Zeta);
        Assert.NotEmpty(saltMiner.Gamma);
        Assert.NotEmpty(saltMiner.Theta);
        Assert.NotEmpty(saltMiner.Eta);

        Assert.Equal(100, saltMiner.Omega.Length);
        Assert.Equal(100, saltMiner.Alpha.Length + saltMiner.Zeta.Length);

        testOutputHelper.WriteLine($"Omega Count: {saltMiner.Omega.Length}");
        testOutputHelper.WriteLine($"Alpha Count: {saltMiner.Alpha.Length}");
        testOutputHelper.WriteLine($"Zeta Count: {saltMiner.Zeta.Length}");
        testOutputHelper.WriteLine($"Beta Count: {saltMiner.Beta.Length}");
        testOutputHelper.WriteLine($"Gamma Count: {saltMiner.Gamma.Length}");
        testOutputHelper.WriteLine($"Theta Count: {saltMiner.Theta.Length}");
        testOutputHelper.WriteLine($"Eta Count: {saltMiner.Eta.Length}");
    }
    [Fact]
    public void LnNcpLnDcac()
    {
        const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        var saltMiner = new SaltMiner(filePath);
        Assert.NotNull(saltMiner);
        var result =saltMiner.MineLnNcpLnDcac();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(7, result.Length); // Expecting 7 sets: Omega, Alpha, Zeta, Beta, Gamma, Theta, Eta
        Assert.All(result, Assert.NotNull); // Ensure all results are not null
        Assert.Equal(100, result[0].NumberSamples);

        testOutputHelper.WriteLine("Salt mining completed successfully.");
    }

}

