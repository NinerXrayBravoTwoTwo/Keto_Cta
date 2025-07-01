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
                        try
                        {
                            var selector = new CreateSelector(chart);
                            if (selector.IsLogMismatch)
                            {
                                logMismatch++;
                                continue;
                            }

                            var result = goldMiner.Dust(SetName.Omega, chart);
                            testOutputHelper.WriteLine($"{index++}; {result}");
                        }
                        catch (ArgumentException er)
                        {
                            testOutputHelper.WriteLine($"{er.GetType()}: {er.Message} for chart: {chart}");
                        }

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
                                    try
                                    {
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
                                    catch (ArgumentException er)
                                    {
                                        testOutputHelper.WriteLine($"{er.GetType()}: {er.Message} for chart: {chart}");
                                    }
                                }

            testOutputHelper.WriteLine(
                $"Total Log Mismatch: {logMismatch} out of {index + logMismatch} charts generated.");
        }

        [Fact]
        public void RatioSelectorTest()
        {
            /*
             * chartLabel = "DNcpv/DCac vs. DTps";
               chartLabel = "ln-ln DNcpv/DCac vs. DTps";
               chartLabel = "DNcpv/DPav vs. DTps";
               chartLabel = "ln-ln DNcpv/DPav vs. DTps";
               chartLabel = "DNcpv/DTcpv vs. DTps";
             */
            string[] numerator = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(",");
            string[] denominator = "Tps0,Cac0,Ncpv0,Tcpv0,Pav0,LnTps0,LnCac0,LnNcpv0,LnTcpv0,LnPav0".Split(",");

            var xSelector = new Func<Element, (double numerator, double denominator)>(item => (item.LnDNcpv, item.LnDCac));
            // Define ySelector to return y value (LnDTps)
            var ySelector = new Func<Element, double>(item => item.LnDTps);
        }


    }
}
