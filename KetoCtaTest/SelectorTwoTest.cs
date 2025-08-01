using DataMiner;
using Keto_Cta;
using System.Reflection;
using System.Text.RegularExpressions;
using LinearRegression;
using Xunit.Abstractions;

namespace KetoCtaTest
{
    public class SelectorTwoTest(ITestOutputHelper testOutputHelper)
    {

        [Fact]
        public void CreateSelectorTwo_ValidInput_CreatesSelectors()
        {
            // Arrange
            var regressionString = "LnTps1 vs LnTps0";
            var createSelectorTwo = new CreateSelectorTwo(regressionString);
            // Act
            var (token, numerator, denominator) = CreateSelectorTwo.Compile("LnTps1");
            // Assert
            Assert.Equal(Token.VisitAttribute, token);
            Assert.Equal("Visits[1].LnTps", numerator);
            Assert.Equal(string.Empty, denominator);
        }

        [Fact]
        public void StaticCompileAttributeNormalizeTest()
        {
            List<Visit> visits = [
                new Visit("1", DateTime.Now.AddDays(-365), 100, 50, 20.5, 30.0, 15.0),
                new Visit("2", DateTime.Now, 110, 55, 22.0, 33.0, 16.5)
            ];

            var element = new Element("1", visits);

            // Arrange
            var allAttributes = "tps0|cac0|ncpv0|tCPV0|PAv1|Qangio1|dtps|Dcac|DNCpv|DTcpV|DPav|DQangio".Split('|');
            // Act
            var x = 1;
            while (x++ < 3)
            {
                foreach (var attribute in allAttributes)
                {
                    var attLn = x % 2 == 0 ? "ln" + attribute : attribute;
                    var normalized = CreateSelectorTwo.AttributeCaseNormalize(attLn);
                    testOutputHelper.WriteLine($"Normalized attribute: {attLn} -> {normalized}");
                    var compiled = CreateSelectorTwo.Compile(normalized);
                    // look up the expected normalized attLn
                    var reflectValue = (double)CreateSelectorTwo.GetNestedPropertyValue(element, compiled.numerator);
                    testOutputHelper.WriteLine($"\tCompiled   attribute: {attLn} -> {compiled} -> {reflectValue:F2}");

                }
            }
        }

        [Fact]
        public void SimpleDataSetXyLookup()
        {
            // Arrange
            const string path = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(path);

            const string title = "Cac1 vs. Cac0";
            var c2Selector = new CreateSelectorTwo(title);

            var result = goldMiner.Zeta.Select(c2Selector.Selector);
            // Act
            foreach (var (id, x, y) in result)
            {
                testOutputHelper.WriteLine($"Element ID: {id}, X: {x}, Y: {y}");
            }
            List<(double x, double y)> xyList = result
                .Select(tuple => (tuple.x, tuple.y))
                .ToList();

            // Assert
            var regression = new RegressionPvalue(xyList);
            Assert.NotNull(result);
            Assert.NotEmpty(xyList);
            Assert.NotNull(regression);
            Assert.Equal(0.00003316389, regression.PValue(), 0.000033);
            testOutputHelper.WriteLine($"Regression: {regression.ToString()}");
        }

        [Fact]
        public void RatioRegressor()
        {
            // Arrange
            const string ratio = "LnCac0 /  LnNcpv1";

            // Act
            var compile = CreateSelectorTwo.Compile(ratio);
            Assert.Equal("Visits[0].LnCac", compile.numerator);
            Assert.Equal("Visits[1].LnNcpv", compile.denominator);
        }

        [Fact]
        public void RatioRegressionCreateSelector()
        {
            // Arrange
            const string path = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(path);

            const string ratio = "Cac0 / Ncpv1 vs Cac0";
            // Act
            var c2 = new CreateSelectorTwo(ratio);

            // Assert
            var sel = goldMiner.Zeta.Select(c2.Selector);
            Assert.NotNull(sel);
            testOutputHelper.WriteLine($"Ratio Regression: {ratio}");
            testOutputHelper.WriteLine(c2.ToString());

            foreach (var (id, x, y) in sel)
            {
                testOutputHelper.WriteLine($"Element ID: {id}, X: {x}, Y: {y}");
            }

            var xyList = sel
                .Select(tuple => (tuple.x, tuple.y))
                .ToList();

            foreach (var list in xyList)
            {
                testOutputHelper.WriteLine($"X: {list.x}, Y: {list.y}");
            }
        }

        [Fact]
        public void GetRatioDataTest()
        {
            // Arrange
            const string path = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(path);

            string[] titles = ["Cac1 vs Cac0", "Ncpv1 vs Ncpv0", "Cac0 / Ncpv0 vs Cac0"];
            foreach (var title in titles)
            {
                var selector = new CreateSelectorTwo(title);
                testOutputHelper.WriteLine($"{selector.Title} : {selector.ToString()}");
            }

            // Act
        }
    }
}
