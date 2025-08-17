using Keto_Cta;
using MineReports;
using Xunit.Abstractions;

namespace KetoCtaTest
{
    public class CommandParseTest(ITestOutputHelper testOutputHelper)
    {

        [Fact]
        public void ParseCommandWithLimit()
        {
            // Arrange
            var cmdParser = new CommandParser("todo");
            string cmdRequest = "todo 100";
            // Act
            var result = cmdParser.Parse(cmdRequest);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(100, result.Limit);
            Assert.Empty(result.SearchTerms);
            Assert.Equal(Token.None, result.DependentToken);
            Assert.Equal(Token.None, result.RegressionToken);
            Assert.Empty(result.SetNames);
        }

        [Fact]
        public void ParseCommandWithoutLimit()
        {
            // Arrange
            var cmdParser = new CommandParser("todo");
            string cmdRequest = "todo";
            // Act
            var result = cmdParser.Parse(cmdRequest);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(30, result.Limit); // Default limit
            Assert.Empty(result.SearchTerms);
            Assert.Equal(Token.None, result.DependentToken);
            Assert.Equal(Token.None, result.RegressionToken);
            Assert.Empty(result.SetNames);
        }

        [Fact]
        public void ParseCommandWithNoLimit()
        {
            // Arrange
            var cmdParser = new CommandParser("todo");
            string cmdRequest = "todo 0";
            // Act
            var result = cmdParser.Parse(cmdRequest);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(30, result.Limit); // Default limit when 0 is provided
            Assert.Empty(result.SearchTerms);
            Assert.Equal(Token.None, result.DependentToken);
            Assert.Equal(Token.None, result.RegressionToken);
            Assert.Empty(result.SetNames);
        }

        [Fact]
        public void ParseCommandWithSearchTerms()
        {
            // Arrange
            var cmdParser = new CommandParser("todo");
            string cmdRequest = "todo 100 searchTerm1 searchTerm2";
            // Act
            var result = cmdParser.Parse(cmdRequest);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(100, result.Limit);
            Assert.Equal(["searchTerm1", "searchTerm2"], result.SearchTerms);
            Assert.Equal(Token.None, result.DependentToken);
            Assert.Equal(Token.None, result.RegressionToken);
            Assert.Empty(result.SetNames);
        }

        [Fact]
        public void ParseCommandWithQueryWithCommas()
        {
            // Arrange
            var cmdParser = new CommandParser("todo");
            string cmdRequest = "todo 100 depVisit regLnRatio Omega Alpha";
            // Act
            var result = cmdParser.Parse(cmdRequest);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(100, result.Limit);
            Assert.Empty(result.SearchTerms);
            Assert.Equal(Token.Visit, result.DependentToken);
            Assert.Equal(Token.LnRatio, result.RegressionToken);
            Assert.Equal([SetName.Omega, SetName.Alpha], result.SetNames);

        }

        [Fact]
        public void ParseCommandWithQueryNoCommas()
        {
            // Arrange
            var cmdParser = new CommandParser("todo");
            string cmdRequest = "todo 100 depVisit regLnRatio Omega Alpha";
            // Act
            var result = cmdParser.Parse(cmdRequest);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(100, result.Limit);
            Assert.Empty(result.SearchTerms);
            Assert.Equal(Token.Visit, result.DependentToken);
            Assert.Equal(Token.LnRatio, result.RegressionToken);
            Assert.Equal([SetName.Omega, SetName.Alpha], result.SetNames);

        }

        [Fact]
        public void ParseCommandWithInvalidInput()
        {
            // Arrange
            var cmdParser = new CommandParser("matrix");
            string cmdRequest = "matrix invalid input 100, meMeMeAndMeAgain,";
            // Act
            var result = cmdParser.Parse(cmdRequest);
            // Assert
            Assert.NotEqual(100, result.Limit);
            Assert.False(result.IsSuccess);
            Assert.Equal(4, result.SearchTerms.Length);
            Assert.True(result.ToString().Contains("Error: Invalid comma", StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal(Token.None, result.DependentToken);
            Assert.Equal(Token.None, result.RegressionToken);
            Assert.Empty(result.SetNames);
            testOutputHelper.WriteLine(result.ToString());
        }

        [Fact]
        public void ParseCommandWithEmptyInput()
        {
            // Arrange
            var cmdParser = new CommandParser("todo");
            string cmdRequest = "todo";
            // Act
            var result = cmdParser.Parse(cmdRequest);
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(30, result.Limit);
            Assert.Empty(result.SearchTerms);
            Assert.Equal(Token.None, result.DependentToken);
            Assert.Equal(Token.None, result.RegressionToken);
            Assert.Empty(result.SetNames);
        }

    }

}