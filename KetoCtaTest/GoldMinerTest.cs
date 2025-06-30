using DataMiner;
using Keto_Cta;
using LinearRegression;
using Xunit.Abstractions;

namespace KetoCtaTest
{
    public class SelectorTest(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public void CreateSelector_ValidChartTitle_SetsNonNullSelector()
        {
            var selector = new CreateSelector("Tps0 vs. DTps");
            Assert.NotNull(selector.Selector);
            Assert.False(selector.IsLogMismatch);
        }

        [Fact]
        public void Dust_ValidSetNameAndChartTitle_ReturnsDust()
        {
            var miner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv"); // Assume test.csv populates datasets
            var dust = miner.Dust(SetName.Omega, "Tps0 vs. DTps");
            Assert.NotNull(dust);
            Assert.Equal(SetName.Omega, dust.SetName);
            Assert.Equal("Tps0 vs. DTps", dust.Title);
            Assert.True(dust.Regression.DataPointsCount() >= 3);
        }

       [Fact]
        public void Dust_InvalidChartTitle_ThrowsArgumentException()
        {
            var miner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
            Assert.Throws<ArgumentException>(() => miner.Dust(SetName.Omega, "Tps0"));
        }

        [Fact]
        public void BaselinePredictDelta_GeneratesRegressions()
        {
            var miner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
            var dusts = miner.BaselinePredictDelta();
            Assert.NotEmpty(dusts);
            Assert.True(dusts.Length <= 90, "Should skip some combinations");
            Assert.All(dusts, d => Assert.True(d.Regression.DataPointsCount() >= 3));
        }
        [Fact]
        public void SelectorXySimpleTest()
        {
            var result = new CreateSelector("DNcpv vs. DCac");
            Assert.Equal("DNcpv", result.Regressor.Target);
            Assert.Equal("DCac", result.Dependant.Target);

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
            var result = new CreateSelector("Ncpv0 vs. LnDCac");
            Assert.Equal("Visits[0].Ncpv", result.Regressor.Target);
            Assert.Equal("LnDCac", result.Dependant.Target);
        }

        [Fact]
        public void GenerateElementCharts()
        {
            string[] attributes = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

            const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(filePath);
            int logMismatch = 0;

            var index = 0;
            for (var x = 0; x < attributes.Length; x++)
                for (var y = 0; y < attributes.Length; y++)
                    if (x != y)
                    {
                        var chart = $"{attributes[x]} vs. {attributes[y]}";
                        // testOutputHelper.WriteLine($"{index++}: Generating chart for {chart}");
                        var selector = new CreateSelector(chart);
                        if (selector.IsLogMismatch)
                        {
                            logMismatch++;
                            continue;
                        }

                        var result = goldMiner.Dust(SetName.Omega, chart);
                        testOutputHelper.WriteLine($"{index++}; {result}");
                    }

            testOutputHelper.WriteLine(
                $"Total Log Mismatch: {logMismatch} out of {index + logMismatch} charts generated.");

        }

        [Fact]
        public void GenerateVisitCharts()
        {
            var attributes = "Tps,Cac,Ncpv,Tcpv,Pav,LnTps,LnCac,LnNcpv,LnTcpv,LnPav".Split(",");

            const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(filePath);
            var logMismatch = 0;

            var index = 0;
            for (var dVisit = 0; dVisit < 2; dVisit++)
                for (var x = 0; x < attributes.Length; x++)
                    for (var y = 0; y < attributes.Length; y++)
                        if (x != y)
                            for (var iVisit = 0; iVisit < 2; iVisit++)
                                if (true)
                                {
                                    index++;
                                    var chart = $"{attributes[x]}{dVisit} vs. {attributes[y]}{iVisit}";
                                    var selector = new CreateSelector(chart);
                                    if (selector.IsLogMismatch)
                                    {
                                        logMismatch++;
                                        continue;
                                    }

                                    var result = goldMiner.Dust(SetName.Omega, chart);
                                    index++;
                                    if (result.Regression.PValue() < 0.5)
                                        testOutputHelper.WriteLine($"{index}; {result}");
                                }

            testOutputHelper.WriteLine(
                $"Total Log Mismatch: {logMismatch} out of {index + logMismatch} charts generated.");
        }


    }

    public class GoldMinerTest(ITestOutputHelper testOutputHelper)
    {
        //private readonly SelectorTest _selectorTest = new SelectorTest(testOutputHelper);

        [Fact]
        public void Dust_ValidSetNameAndChartTitle_ReturnsDust()
        {
            var miner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv"); // Assume test.csv populates datasets
            var dust = miner.Dust(SetName.Omega, "Tps0 vs. DTps");
            Assert.NotNull(dust);
            Assert.Equal(SetName.Omega, dust.SetName);
            Assert.Equal("Tps0 vs. DTps", dust.Title);
        }

        [Fact]
        public void Dust_UnsupportedSetName_ReturnsNull()
        {
            var miner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
            var dust = miner.Dust((SetName)999, "Tps0 vs. DTps");
            Assert.Null(dust);
        }

        [Fact]
        public void Dust_InvalidChartTitle_ThrowsArgumentException()
        {
            var miner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
            Assert.Throws<ArgumentException>(() => miner.Dust(SetName.Omega, "Tps0")); // Missing 'vs.'
        }

        [Fact]
        public void CreateSelector_ValidChartTitle_SetsNonNullSelector()
        {
            var selector = new CreateSelector("Tps0 vs. DTps");
            Assert.NotNull(selector.Selector);
            Assert.False(selector.IsLogMismatch);
        }

        [Fact]
        public void CreateSelector_NullElement_ThrowsArgumentNullException()
        {
            var selector = new CreateSelector("Tps0 vs. DTps");
            Assert.Throws<ArgumentNullException>(() => selector.Selector(null));
        }
        [Fact]
        public void Dust_NullChartTitle_ThrowsArgumentException()
        {
            var miner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
            Assert.Throws<ArgumentNullException>(() => miner.Dust(SetName.Omega, null));
        }

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

        //[Fact]
        //public void GoldMiner_NullDustSet()
        //{
        //    const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
        //    var goldMiner = new GoldMiner(filePath);

        //    Assert.NotNull(goldMiner);
        //    var result = goldMiner.Dust(string.Empty);
        //    Assert.NotNull(result);
        //    Assert.Empty(result);
        //    testOutputHelper.WriteLine("Gold mining completed successfully.");
        //}

        [Fact]
        public void GenerateVisitChartsWithRegression()
        {
            const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(filePath);

            var index = 0;
            foreach (var item in goldMiner.BaselinePredictDelta())
            {
                testOutputHelper.WriteLine($"Index {index++}: {item}");
            }
        }

        [Fact]
        public void CreateSingleGoldDust()
        {
            const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(filePath);

            var result = goldMiner.Dust(SetName.Omega, "DNcpv vs. DCac");
            testOutputHelper.WriteLine(result.Regression.ToString());
        }

        [Fact]
        public void BaselinePredictElementDelta()
        {
            string[] visitBaseline = "Tps0,Cac0,Ncpv0,Tcpv0,Pav0,LnTps0,LnCac0,LnNcpv0,LnTcpv0,LnPav0".Split(",");
            string[] elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");

            const string filePath = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(filePath);
            Assert.NotNull(goldMiner);
            // Ensure the goldMiner is not null and has data
            testOutputHelper.WriteLine("Index, Title, Set, Slope, p-value, Correlation");
            var index = 0;
            var logMismatch = 0;
            for (var x = 0; x < visitBaseline.Length; x++)
            {
                for (var y = 0; y < elementDelta.Length; y++)
                {
                    if (x != y)
                    {
                        var chart = $"{visitBaseline[x]} vs. {elementDelta[y]}";
                        var selector = new CreateSelector(chart);
                        if (selector.IsLogMismatch)
                        {
                            logMismatch++;
                            continue;
                        }

                        var result = goldMiner.Dust(SetName.Omega, chart);
                        var reg = result.Regression;
                        testOutputHelper.WriteLine(
                            $"{index++}, {result.Title}, {result.SetName}, {reg.Slope():F4}, {reg.PValue():F4}, {reg.Correlation():F4}");
                    }
                }
            }

            testOutputHelper.WriteLine(
                $"Total Log Mismatch: {logMismatch} out of {index + logMismatch} charts generated.");
        }

        [Fact]
        public void RatioRegressorTest()
        {
            string[] numerator = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");
            string[] denominator = "Tps0,Cac0,Ncpv0,Tcpv0,Pav0,LnTps0,LnCac0,LnNcpv0,LnTcpv0,LnPav0".Split(",");

            var xSelector = new Func<Element, (double numerator, double denominator)>(item => (item.LnDNcpv, item.LnDCac));
            // Define ySelector to return y value (LnDTps)
            var ySelector = new Func<Element, double>(item => item.LnDTps);
        }

        [Fact]
        public void GoldDustTest()
        {
            var mine = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
            var result = new List<Dust>();
            result.AddRange(mine.GoldDust("DNcpv vs. DCac"));
            result.AddRange(mine.GoldDust("LnDNcpv vs. LnDCac"));
            Assert.NotNull(result);
            Assert.Equal(16, result.Count());

            foreach (var dust in result)
            {
                testOutputHelper.WriteLine(dust.ToString());
            }

        }
        [Fact]
        public void GoldDustRegressionTest()
        {
            var mine = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");

            string[] elementDelta = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");
            var index = 0;
            var logMismatch = 0;

            var result = new List<Dust>();

            for (var x = 0; x < elementDelta.Length; x++)
            {
                for (var y = 0; y < elementDelta.Length; y++)
                {
                    if (x != y)
                    {
                        var chart = $"{elementDelta[x]} vs. {elementDelta[y]}";
                        var selector = new CreateSelector(chart);
                        if (selector.IsLogMismatch)
                        {
                            logMismatch++;
                            continue;
                        }
                        result.AddRange(mine.GoldDust(chart));
                    }
                }
            }

            foreach (var dust in result)
            {
                if (index++ % 3 == 1)
                    testOutputHelper.WriteLine($"{index}, " + dust.ToString());
            }
            testOutputHelper.WriteLine(
                $"Total Log Mismatch: {logMismatch} out of {index + logMismatch} charts generated.");
        }
    }
}
