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
            var selector = new CreateSelector("Tps0 vs. DCac");
            Assert.NotNull(selector.Selector);
            Assert.False(selector.IsLogMismatch);
        }
        [Fact]
        public void SelectorXySimpleTest()
        {
            var result = new CreateSelector("DCac vs. DNcpv");
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
        public void CreateSelector_ValidRatio_SetsProperties()
        {
            var selector = new CreateSelector("LnCac1 vs. Cac0/Ncpv0");
            Assert.True(selector.IsRatio);
            Assert.Equal("Cac0", selector.Numerator?.VariableName);
            Assert.Equal("Ncpv0", selector.Denominator?.VariableName);
            Assert.Equal("LnCac1", selector.DependantDicer.VariableName);
            Assert.True(selector.IsRatio);
        }

        [Fact]
        public void CreateSelector_LogMismatch_Detected()
        {
            var selector = new CreateSelector("Cac1 vs. LnCac0/Ncpv0");
            Assert.False(selector.IsRatio);
        }

        [Fact]
        public void CreateSelector_SameVariables_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CreateSelector("Cac0 vs. Cac0"));
        }

        [Fact]
        public void SimpleVariableDicer_ValidInput_SetsProperties()
        {
            var dicer = new SimpleVariableDicer("LnDCac1");
            Assert.Equal("Visits[1].LnDCac", dicer.RootAttribute);
            Assert.Equal("Visits[1].LnDCac", dicer.Target);
            Assert.True(dicer.IsLogarithmic);
            Assert.True(dicer.IsDelta);
            Assert.True(dicer.IsVisit);
        }

        [Fact]
        public void RatioVariableDicer_ValidInput_SetsProperties()
        {
            var dicer = new RatioVariableDicer("LnDCac1 / LnNcpv1");
            Assert.Equal("Visits[1].LnDCac/Visits[1].LnNcpv", dicer.Target);
            Assert.Equal("Visits[1].LnDCac/Visits[1].LnNcpv", dicer.RootAttribute);
            Assert.True(dicer.IsLogarithmic);
            Assert.True(dicer.IsDelta);
            Assert.True(dicer.IsVisit);
        }

        [Fact]
        public void SimpleVariableDicer_InvalidInput_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new SimpleVariableDicer("InvalidVariable"));
        }

        [Fact]
        public void Dust_InvalidChartTitle_ThrowsArgumentException()
        {
            var miner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
            var result = miner.AuDust(SetName.Alpha, "Cac0");
            Assert.Null(result);
        }

        [Fact]
        public void SelectorDeltaRegressorTest()
        {
            var result = new CreateSelector("LnPav1 vs. DCac ");
            Assert.False(result.IsRatio);
            Assert.Equal("DCac", result.RegressorDicer.Target);
            Assert.Equal("Visits[1].LnPav", result.DependantDicer.Target);
        }

        [Fact]
        public void SelectorXyComplexTestA()
        {
            var result = new CreateSelector("Cac0 vs. Ncpv1");
            Assert.Equal("Visits[1].Ncpv", result.RegressorDicer.RootAttribute);
            Assert.Equal("Visits[0].Cac", result.DependantDicer.RootAttribute);
        }

        [Fact]
        public void SelectorXyComplexTestB()
        {
            var result = new CreateSelector("LnDCac vs. Ncpv0");
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

                            var result = goldMiner.AuDust(SetName.Omega, chart);
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

                                        var result = goldMiner.AuDust(SetName.Omega, chart);
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
                "DNcpv vs. Tps0 / Cac0",
                "DTcpv vs. DTps / LnTps1",
                "Tcpv1 vs. DCac / LnDPav",
                "LnTps0 vs. DCac / LnNcpv1",
                "LnTcpv1 vs. DTcpv / Cac0",
                "Pav0 vs. DPav / LnDTps",
                "LnNcpv1 vs. DPav / LnNcpv0",
                "DPav vs. LnDTps / LnDNcpv",
                "LnTcpv1 vs. LnDTps / Ncpv1",
                "LnNcpv1 vs. LnDCac / LnDPav",
                "LnCac1 vs. LnDCac / LnCac0",
                "Tps0 vs. LnDNcpv / Tps1",
                "Tcpv1 vs. LnDNcpv / Cac1",
                "Ncpv1 vs. LnDNcpv / Tcpv1",
                "LnTcpv1 vs. LnDNcpv / Pav1",
                "Tcpv1 vs. LnDNcpv / LnTps0",
                "LnNcpv0 vs. LnDNcpv / LnCac0",
                "DCac vs. LnDPav / Tps0",
                "Ncpv1 vs. LnDPav / LnNcpv1",
                "LnDTps vs. Tps0 / LnTcpv0",
                "LnTps1 vs. Tps1 / LnPav1",
                "Tps1 vs. Cac0 / Cac1"

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
                    var dust = goldMiner.AuDust(SetName.Beta, chart);

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
        public void RatioLnOfRatioSelectorTest()
        {
            string[] testCharts =
            [
                "DNcpv vs. Ln(Tps0 / Cac0)",
                "LnTcpv1 vs. Ln(DTcpv / Cac0)",
                "LnTcpv1 vs. Ln(DTps / Ncpv1)",
                "LnNcpv1 vs. Ln(DPav / Ncpv0)",
                "Tps1 vs. Ln(Cac0 / Cac1)",
                "LnTps1 vs. Ln(Tps1 / Pav1)",
                "Tcpv1 vs. Ln(DCac / DPav)",
                "DTcpv vs. Ln(DTps / Tps1)",
                "LnTps0 vs. Ln(DCac / Ncpv1)",
                "Pav0 vs. Ln(DPav / LnDTps)",
                "DPav vs. Ln(LnDTps / LnDNcpv)",
                "LnNcpv1 vs. Ln(LnDCac / LnDPav)",
                "LnCac1 vs. Ln(LnDCac / LnCac0)",
                "Tps0 vs. Ln(LnDNcpv / Tps1)",
                "Tcpv1 vs. Ln(LnDNcpv / Cac1)",
                "Ncpv1 vs. Ln(LnDNcpv / Tcpv1)",
                "LnTcpv1 vs. Ln(LnDNcpv / Pav1)",
                "Tcpv1 vs. Ln(LnDNcpv / LnTps0)",
                "LnNcpv0 vs. Ln(LnDNcpv / LnCac0)",
                "DCac vs. Ln(LnDPav / Tps0)",
                "Ncpv1 vs. Ln(nDPav / LnNcpv1)",
                "LnDTps vs. Ln(Tps0 / LnTcpv0)",
                "Tcpv1 vs. Ln(DCac / LnDPav)",

            ];


            foreach (var chart in testCharts)
            {

                //var xSelector = new Func<Element, (double numerator, double denominator)>(item => (item.LnDNcpv, item.LnDCac));
                //// Define ySelector to return y value (LnDTps)
                //var ySelector = new Func<Element, double>(item => item.LnDTps);

                var selector = new CreateSelector(chart);

                if (selector.IsRatioLnWrapper)
                {
                    var goldMiner = new GoldMiner("TestData/keto-cta-quant-and-semi-quant.csv");
                    var dust = goldMiner.AuDust(SetName.Beta, chart);

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
            var numEqualDenominator = 0;

            foreach (var numerator in allAttributes)
            {
                foreach (var denominator in allAttributes)
                {
                    if (numerator == denominator) continue; //Todo: Evaluate if bothneeded  

                    foreach (var dependent in allAttributes)
                    {
                        // no ratios with dependent in regressor
                        if (dependent == numerator || dependent == denominator)
                        {
                            dependentInRatio++;
                            continue;
                        }

                        if (numerator.Equals(denominator)) //Todo
                        {
                            numEqualDenominator++;
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
            testOutputHelper.WriteLine($"Charts with inverse ratio skipped: {inverseDetected}\nDependent in Ratio skipped: {dependentInRatio}\nNumerator equal denominator: {numEqualDenominator}");
            testOutputHelper.WriteLine($"Total unique charts: {chartMap.Count}");

            foreach (var chart in chartMap.Values)
                if (RandomGen.Next() % 311 == 1)
                    testOutputHelper.WriteLine($"{chart}");


        }
    }
}
