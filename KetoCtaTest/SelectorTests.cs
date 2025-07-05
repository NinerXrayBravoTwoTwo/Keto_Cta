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
            var result = miner.Dust(SetName.Alpha, "Cac0");
            Assert.Null(result);
        }

        [Fact]
        public void SelectorXySimpleTest()
        {
            var result = new CreateSelector("DNcpv vs. DCac");
            Assert.Equal("DNcpv", result.RegressorDicer.Target);
            Assert.Equal("DCac", result.DependantDicer.Target);

            // Select data and test results
            var dataPoints = new List<(double x, double y)>();

            var goldMiner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
            dataPoints.AddRange(goldMiner.Omega.Select(result.Selector));

            var regression = new RegressionPvalue(dataPoints);
            testOutputHelper.WriteLine(regression.ToString());
        }

        [Fact]
        public void SelectorDeltaRegressorTest()
        {
            var result = new CreateSelector("DCac vs. LnPav1");
            Assert.False(result.IsRatio);
            Assert.Equal("DCac", result.RegressorDicer.Target);
            Assert.Equal("Visits[1].LnPav", result.DependantDicer.Target);
        }

        //[Fact]
        //public void SelectorXyComplexTest()
        //{
        //    var result = new CreateSelector("Ncpv0 vs. LnDCac");
        //    Assert.Equal("Visits[0].Ncpv", result.RegressorDicer.RootAttribute);
        //    Assert.Equal("LnDCac", result.DependantDicer.VariableName);
        //}

        [Fact]
        public void SelectorXyComplexTest()
        {
            var result = new CreateSelector("Ncpv0 vs. LnDCac");
            Assert.Equal("Visits[0].Ncpv", result.RegressorDicer.Target);
            Assert.Equal("LnDCac", result.DependantDicer.Target);
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
            string[] testCharts =
            [
                "Tps0 / Cac0 vs. DNcpv",
                "DTps / LnTps1 vs. DTcpv",
                "DCac / LnDPav vs. Tcpv1",
                "DCac / LnNcpv1 vs. LnTps0",
                "DTcpv / Cac0 vs. LnTcpv1",
                "DPav / LnDTps vs. Pav0",
                "DPav / LnNcpv0 vs. LnNcpv1",
                "LnDTps / LnDNcpv vs. DPav",
                "LnDTps / Ncpv1 vs. LnTcpv1",
                "LnDCac / LnDPav vs. LnNcpv1",
                "LnDCac / LnCac0 vs. LnCac1",
                "LnDNcpv / Tps1 vs. Tps0",
                "LnDNcpv / Cac1 vs. Tcpv1",
                "LnDNcpv / Tcpv1 vs. Ncpv1",
                "LnDNcpv / Pav1 vs. LnTcpv1",
                "LnDNcpv / LnTps0 vs. Tcpv1",
                "LnDNcpv / LnCac0 vs. LnNcpv0",
                "LnDPav / Tps0 vs. DCac",
                "LnDPav / LnNcpv1 vs. Ncpv1",
                "Tps0 / LnTcpv0 vs. LnDTps",
                "Tps1 / LnPav1 vs. LnTps1",
                "Cac0 / Cac1 vs. Tps1"

            ];


            foreach (var chart in testCharts)
            {

                //var xSelector = new Func<Element, (double numerator, double denominator)>(item => (item.LnDNcpv, item.LnDCac));
                //// Define ySelector to return y value (LnDTps)
                //var ySelector = new Func<Element, double>(item => item.LnDTps);

                var selector = new CreateSelector(chart);

                if (selector.IsRatio)
                {
                    var goldMiner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
                    var dust = goldMiner.Dust(SetName.Beta, chart);

                    var regression = dust.Regression;
                    testOutputHelper.WriteLine($"Regression for {chart}: {regression.PValue():F6}, {regression.Slope():F3}, {regression.N}");
                }
                else
                {
                    testOutputHelper.WriteLine($"The selector for chart '{chart}' is not a ratio selector.");
                }
            }
        }

        [Fact]
        public void RatioBuilderTest()
        {
            /* Notes:
               Build Histogram of all the Ratio regressions by p-value
               Scatter plot p-value vs. R^2 score for all ratio regressions there are 26000 x 8 of these
             */

            var elementAttributes = "DTps,DCac,DNcpv,DTcpv,DPav,LnDTps,LnDCac,LnDNcpv,LnDTcpv,LnDPav".Split(',');
            var visitAttributes = "Tps,Cac,Ncpv,Tcpv,Pav,LnTps,LnCac,LnNcpv,LnTcpv,LnPav".Split(',');

            var bothVisits = new List<string>();
            foreach (var visit in visitAttributes)
            {
                bothVisits.Add($"{visit}0");
                bothVisits.Add($"{visit}1");
            }
            var allAttributes = elementAttributes.Concat(bothVisits).ToList();

            Dictionary<string, string> chartMap = new Dictionary<string, string>();
            var inverseDetected = 0;
            var dependentInRatio = 0;
            var numEqualDenom = 0;

            foreach (var numerator in allAttributes)
            {
                foreach (var denominator in allAttributes)
                {
                    if (numerator != denominator)
                    {
                        foreach (var dependent in allAttributes)
                        {
                            // no ratios with dependent in regressor
                            if (dependent == numerator || dependent == denominator)
                            {
                                dependentInRatio++;
                                continue;
                            }

                            if (numerator.Equals(denominator))
                            {
                                numEqualDenom++;
                                continue; // skip self-ratio of identity
                            }

                            var chart = $"{numerator} / {denominator} vs. {dependent}";
                            string[] reg = [numerator, denominator];
                            var key = string.Join(',', reg.OrderBy(r => r)) + $",{dependent}";

                            if (!chartMap.TryAdd(key, chart))
                                inverseDetected++;
                        }
                    }

                }
            }
            testOutputHelper.WriteLine($"Charts with inverse ratio skipped: {inverseDetected}\nDependent in Ratio skipped: {dependentInRatio}\nNumerator equal denominator: {numEqualDenom}");
            testOutputHelper.WriteLine($"Total unique charts: {chartMap.Count}");

            foreach (var chart in chartMap.Values)
                if (RandomGen.Next() % 311 == 1)
                    testOutputHelper.WriteLine($"{chart}");


        }
    }
}
