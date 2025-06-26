using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataMiner;
using Xunit.Abstractions;

namespace KetoCtaTest
{
    public class GoldMinerTest(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public void GoldMiner_RegressionTest()
        {
            const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new Gold(filePath);
            Assert.NotNull(goldMiner);
            Assert.NotEmpty(goldMiner.Omega);
            Assert.NotEmpty(goldMiner.Alpha);
            Assert.NotEmpty(goldMiner.Beta);
            Assert.NotEmpty(goldMiner.Zeta);
            Assert.NotEmpty(goldMiner.Gamma);
            Assert.NotEmpty(goldMiner.Theta);
            Assert.NotEmpty(goldMiner.Eta);
            Assert.Equal(100, goldMiner.Omega.Length);
            Assert.Equal(100, goldMiner.Alpha.Length + goldMiner.Zeta.Length);
            testOutputHelper.WriteLine($"Omega Count: {goldMiner.Omega.Length}");
            testOutputHelper.WriteLine($"Alpha Count: {goldMiner.Alpha.Length}");
            testOutputHelper.WriteLine($"Zeta Count: {goldMiner.Zeta.Length}");
            testOutputHelper.WriteLine($"Beta Count: {goldMiner.Beta.Length}");
            testOutputHelper.WriteLine($"Gamma Count: {goldMiner.Gamma.Length}");
            testOutputHelper.WriteLine($"Theta Count: {goldMiner.Theta.Length}");
            testOutputHelper.WriteLine($"Eta Count: {goldMiner.Eta.Length}");
        }

        [Fact]
        public void GoldMiner_NullDustSet()
        {
            const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new Gold(filePath);

            Assert.NotNull(goldMiner);
            var result = goldMiner.GoldDust(string.Empty);
            Assert.NotNull(result);
            Assert.Empty(result);
            testOutputHelper.WriteLine("Gold mining completed successfully.");
        }

        [Fact]
        public void SelectorXYSimpleTest()
        {
            var result = new CreateSelector("DNcpv vs. DCac");
            Assert.Equal("DNcpv", result.Dependant);
            Assert.Equal("DCac", result.Independent);
        }
        [Fact]
        public void SelectorXYComplexTest()
        {
            var result = new CreateSelector("Ncps0 vs. LnDcac");
            Assert.Equal("Ncps0", result.Dependant);
            Assert.Equal("LnDcac", result.Independent);
        }
    }
}
