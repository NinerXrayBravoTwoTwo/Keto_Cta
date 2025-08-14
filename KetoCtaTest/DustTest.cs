using DataMiner;
using Keto_Cta;
using LinearRegression;
using Xunit.Abstractions;
using static Xunit.Assert;

namespace KetoCtaTest
{
    public class DustTest(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public void CreateDustTestItGuid()
        {
            //Arrange
            var dataPoints = new List<(string id, double x, double y)> { ("a", 1, 2), ("b", 2, 4), ("c", 3, 6), ("d", 4, 8) };
            //Act

            var regression = new RegressionPvalue(dataPoints);

            var dust = new Dust(SetName.Alpha, "Ncpv1 vs Ncpv0", regression, Token.Element, Token.Visit);

            //Act

            //Assert
            NotNull(dust);
            Equal(SetName.Alpha, dust.SetName);
            Equal("Ncpv1 vs Ncpv0", dust.RegressionName);
            False(dust.IsInteresting);
            NotEqual(Guid.Empty, dust.UniqueKey);
        }
    }
}
