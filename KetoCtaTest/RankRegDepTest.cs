using DataMiner;
using LinearRegression;
using Xunit.Abstractions;

namespace KetoCtaTest
{
    public class RankRegDepTest(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public void ConvertRankTest()
        {
            // Arrange
            const string path = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(path);

            const string title = "DNcpv vs. RankD";
            var selector = new CreateSelector(title);
            var cor = new MonoVarient(selector, goldMiner.Eta);

            // Act 1
            Assert.True(cor.IsRegRank);
            Assert.False(cor.IsDepRank);
            Assert.False(cor.IsAsc);


            foreach (var (id, x, y) in cor.RawDataPoints)
            {
                testOutputHelper.WriteLine($"sel- Element ID: {id}, X: {x}, Y: {y}");
            }

            testOutputHelper.WriteLine("");
            foreach (var (id, x, y) in cor.DataPoints)
            {
                testOutputHelper.WriteLine($"new- Element ID: {id}, X: {x}, Y: {y}");
            }

            Assert.NotEqual(0, cor.DataPoints[^1].y);
            Assert.Equal(6.3, cor.DataPoints[^1].y, 0.300);

            for (var x = 0; x < cor.DataPoints.Length - 1; x++) Assert.Equal(x + 1, cor.DataPoints[x].x);

            // Assert
            var regression = new RegressionPvalue(cor.DataPoints.ToList());

            // Act 2
            Assert.NotNull(selector);
            Assert.NotNull(regression);
            Assert.Equal(0.3, regression.PValue, 0.32);
            testOutputHelper.WriteLine($"Regression: {regression}");
        }

        [Fact]
        public void ConvertRankDepTest()
        {
            // Arrange
            const string path = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(path);

            const string title = "RankD vs. DNcpv";
            var selector = new CreateSelector(title);
            var cor = new MonoVarient(selector, goldMiner.Eta);

            // Act 1
            Assert.True(cor.IsDepRank);
            Assert.False(cor.IsRegRank);
            Assert.False(cor.IsAsc);


            foreach (var (id, x, y) in cor.RawDataPoints)
            {
                testOutputHelper.WriteLine($"sel- Element ID: {id}, X: {x}, Y: {y}");
            }

            testOutputHelper.WriteLine("");
            foreach (var (id, x, y) in cor.DataPoints)
            {
                testOutputHelper.WriteLine($"new- Element ID: {id}, X: {x}, Y: {y}");
            }

            Assert.NotEqual(0, cor.DataPoints[^1].x);
            Assert.Equal(6.3, cor.DataPoints[^1].x, 0.300);

            for (var x = 0; x < cor.DataPoints.Length - 1; x++) Assert.Equal(x + 1, cor.DataPoints[x].y);

            // Assert
            var regression = new RegressionPvalue(cor.DataPoints.ToList());

            // Act 2
            Assert.NotNull(selector);
            Assert.NotNull(regression);
            Assert.Equal(0.3, regression.PValue, 0.32);
            testOutputHelper.WriteLine($"Regression: {regression}");
        }

        [Fact]
        public void SimpleDataSetRegRankALookup()
        {
            // Arrange
            const string path = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(path);

            const string title = "DCac vs. RankA";
            var selector = new CreateSelector(title);
            var selResult = goldMiner.Zeta.Select(selector.Selector);

            // Act 1
            Assert.Equal(Token.RankA, selector.RegressorCompile.token);
            Assert.NotEqual(Token.RankA, selector.DependentCompile.token);
            Assert.NotEqual(Token.RankD, selector.DependentCompile.token);
            Assert.Equal(Token.Element, selector.DependentCompile.token);

            var enumerable = selResult as (string id, double x, double y)[] ?? selResult.ToArray();
            var valueTuples = selResult as (string id, double x, double y)[] ?? enumerable.ToArray();

            Assert.Equal(0, valueTuples[0].y);

            var newTuples = valueTuples.OrderBy(t => t.y).Select((t, index) => (t.id, x: (double)(index + 1), t.y))
                .ToArray();
            foreach (var (id, x, y) in valueTuples)
            {
                testOutputHelper.WriteLine($"sel- Element ID: {id}, X: {x}, Y: {y}");
            }

            testOutputHelper.WriteLine("");
            foreach (var (id, x, y) in newTuples)
            {
                testOutputHelper.WriteLine($"new- Element ID: {id}, X: {x}, Y: {y}");
            }

            Assert.NotEqual(0, newTuples[0].y);
            Assert.Equal(-19, newTuples[0].y);

            for (var x = 0; x < newTuples.Length - 1; x++)
                Assert.Equal(x + 1, newTuples[x].x);

            var xyList = newTuples
                .Select(tuple => (tuple.x, tuple.y))
                .ToList();

            // Assert
            // Assert
            var regression = new RegressionPvalue(xyList);

            // Act 2
            Assert.NotNull(selResult);
            Assert.NotEmpty(xyList);
            Assert.NotNull(regression);
            Assert.Equal(0.3275, regression.PValue, 0.32);
            testOutputHelper.WriteLine($"Regression: {regression}");
        }

        [Fact]
        public void SimpleDataSetRegRandDLookup()
        {
            // Arrange
            const string path = "TestData/keto-cta-quant-and-semi-quant.csv";
            var goldMiner = new GoldMiner(path);

            const string title = "DNcpv vs. RankD";
            var selector = new CreateSelector(title);
            var selResult = goldMiner.Eta.Select(selector.Selector);

            // Act 1
            Assert.Equal(Token.RankD, selector.RegressorCompile.token);
            Assert.NotEqual(Token.RankD, selector.DependentCompile.token);
            Assert.NotEqual(Token.RankA, selector.DependentCompile.token);
            Assert.Equal(Token.Element, selector.DependentCompile.token);

            var enumerable = selResult as (string id, double x, double y)[] ?? selResult.ToArray();
            var valueTuples = selResult as (string id, double x, double y)[] ?? enumerable.ToArray();

            Assert.NotEqual(0, valueTuples[0].y);

            var newTuples = valueTuples.OrderByDescending(t => t.y)
                .Select((t, index) => (t.id, x: (double)(index + 1), t.y))
                .ToArray();

            foreach (var (id, x, y) in valueTuples)
            {
                testOutputHelper.WriteLine($"sel- Element ID: {id}, X: {x}, Y: {y}");
            }

            testOutputHelper.WriteLine("");
            foreach (var (id, x, y) in newTuples)
            {
                testOutputHelper.WriteLine($"new- Element ID: {id}, X: {x}, Y: {y}");
            }

            Assert.NotEqual(0, newTuples[^1].y);
            Assert.Equal(6.3, newTuples[^1].y, 6.2);

            for (var x = 0; x < newTuples.Length - 1; x++) Assert.Equal(x + 1, newTuples[x].x);

            var xyList = newTuples.Select(tuple => (tuple.x, tuple.y)).ToList();

            // Assert
            var regression = new RegressionPvalue(xyList);

            // Act 2
            Assert.NotNull(selResult);
            Assert.NotEmpty(xyList);
            Assert.NotNull(regression);
            Assert.Equal(0.3, regression.PValue, 0.32);
            testOutputHelper.WriteLine($"Regression: {regression}");
        }

    }
}