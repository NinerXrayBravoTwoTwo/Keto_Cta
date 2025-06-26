using DataMiner;
using LinearRegression;
using Xunit.Abstractions;

namespace KetoCtaTest
{
    public class SelectorTest(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public void SelectorXySimpleTest()
        {
            var result = new CreateSelector("DNcpv vs. DCac");
            Assert.Equal("DNcpv", result.Dependant);
            Assert.Equal("DCac", result.Independent);

            // Select data and test results
            var dataPoints = new List<(double x, double y)>();

            var goldMiner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
            dataPoints.AddRange(goldMiner.Omega.Select(result.Selector));

            var regression = new RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine(regression.ToString());
        }

        [Fact]
        public void SelectorXyComplexTest()
        {
            var result = new CreateSelector("Ncps0 vs. LnDcac");
            Assert.Equal("Ncps0", result.Dependant);
            Assert.Equal("LnDcac", result.Independent);
        }

        [Fact]
        public void GenerateElementCharts()
        {
            string[] attributes = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

            var index = 0;
            for (var x = 0; x < attributes.Length; x++)
                for (var y = 0; y < attributes.Length; y++)
                    if (x != y)
                        testOutputHelper.WriteLine($"{index++}: Generating chart for {attributes[x]} vs. {attributes[y]}");

        }

        [Fact]
        public void GenerateVisitCharts()
        {
            string[] attributes = "Tps,Cac,Ncpv,Tcpv,DPav,LnTps,LnCac,LnNcpv,LnTcpv,LnPav".Split(",");

            var index = 0;
            for (var dvisit = 0; dvisit < 2; dvisit++)
                for (var x = 0; x < attributes.Length; x++)
                    for (var y = 0; y < attributes.Length; y++)
                        if (x != y)
                            for (var ivisit = 0; ivisit < 2; ivisit++)
                                if (index++ % 3 == 0)
                                    testOutputHelper.WriteLine($"{index}: Generating chart for {attributes[x]}{dvisit} vs. {attributes[y]}{ivisit}");

        }
    }

    public class GoldMinerTest(ITestOutputHelper testOutputHelper)
    {
        private readonly SelectorTest _selectorTest = new SelectorTest(testOutputHelper);

        [Fact]
        public void GoldMiner_RegressionTest()
        {
            const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(filePath);
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
            var goldMiner = new GoldMiner(filePath);

            Assert.NotNull(goldMiner);
            var result = goldMiner.GoldDust(string.Empty);
            Assert.NotNull(result);
            Assert.Empty(result);
            testOutputHelper.WriteLine("Gold mining completed successfully.");
        }
    }
}
