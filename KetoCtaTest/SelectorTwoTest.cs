using DataMiner;
using Keto_Cta;
using LinearRegression;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace KetoCtaTest
{
    public class SelectorTwoTest(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public void CreateSelectorTwo_ValidInput_CreatesSelectors()
        {
            // Arrange
            var regressionString = "LnTps1 vs LnTps2";
            var createSelectorTwo = new CreateSelectorTwo(regressionString);
            // Act
            var (token, numerator, denominator) = CreateSelectorTwo.Compile("LnTps1");
            // Assert
            Assert.Equal(Token.VisitAttribute, token);
            Assert.Equal("Visit[1].Tps", numerator);
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
                    var reflectValue = (double) GetNestedPropertyValue(element, compiled.numerator);
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

            const string title = "Cac0 vs Cac1";
            var c2Selector = new CreateSelectorTwo(title);

            var result = goldMiner.Zeta.Select(c2Selector.Selector);
            Assert.NotNull(result);
        }


        /**                                **/
        // ** Helper Methods for Nested Property Retrieval **/
        private static readonly Regex PropertyRegex = new(@"([a-zA-Z_][a-zA-Z0-9_]*)(\[(\d+)\])?", RegexOptions.Compiled);
        private static readonly Dictionary<string, PropertyInfo?> PropertyCache = new();

        private static object? GetNestedPropertyValue(object? obj, string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath) || obj == null)
                return null;
            
            var properties = propertyPath.Split('.');

            var current = obj;

            foreach (var property in properties)
            {
                if (current == null) return null;

                var match = PropertyRegex.Match(property);
                if (!match.Success) return null;

                var propName = match.Groups[1].Value;
                var cacheKey = $"{current.GetType().FullName}.{propName}";
                if (!PropertyCache.TryGetValue(cacheKey, out var propInfo))
                {
                    propInfo = current.GetType().GetProperty(propName);
                    PropertyCache[cacheKey] = propInfo;
                }
                if (propInfo == null) return null;

                current = propInfo.GetValue(current);

                if (!match.Groups[2].Success || current is not System.Collections.IList list) continue;
                var index = int.Parse(match.Groups[3].Value);
                current = index >= 0 && index < list.Count ? list[index] : null;
            }
            return current;
        }
    }
}
