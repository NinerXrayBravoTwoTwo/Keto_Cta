using Keto_Cta;
using Xunit.Abstractions;

namespace KetoCtaTest;

public class ElementTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void Participant_Constructor_ValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 9.3, 0, 0.004),
            new("V2", null, 0, 0, 18.8, 0, 0.007)
        };
        // Act
        var element = new Element("d4e5f", visits);
        // Assert
        Assert.NotNull(element);
        Assert.Equal("d4e5f", element.Id);

        Assert.Equal(2, element.Visits.Count);

        _testOutputHelper.WriteLine(element.ToString());
    }

    [Fact]
    public void Participant_Constructor_VisitsLessThanTwo_ShouldThrowArgumentException()
    {
        // Arrange
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 9.3, 0, 0.004)
        };
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Element("d4e5f", visits));
        Assert.Contains("Visits list must contain at least two visits.", exception.Message);
    }

    [Fact]
    public void Participant_Constructor_NullVisits_ShouldThrowArgumentNullException()
    {
        // Arrange
        List<Visit>? visits = null;
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Element("d4e5f", visits!));
        Assert.Equal("Value cannot be null. (Parameter 'visits')", exception.Message);
    }

    [Fact]
    public void ToString_ValidData_ShouldReturnExpectedString()
    {
        // Arrange
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 9.3, 0, 0.004),
            new("V2", null, 0, 0, 18.8, 0, 0.007)
        };

        var element = new Element("d4e5f", visits);
        _testOutputHelper.WriteLine(element.ToString());


        // Act
        var result = element.ToString();
        // Assert
        var id = "d4e5f";
        Assert.Contains($"ParticipantId: {id}", result);
        Assert.Equal(2, element.Visits.Count);
    }

    [Fact]
    public void Participant_Constructor_NullParticipantId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 9.3, 0, 0.004),
            new("V2", null, 0, 0, 18.8, 0, 0.007)
        };
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Element(null!, visits));
        Assert.Equal("Value cannot be null. (Parameter 'id')", exception.Message);
    }

    [Fact]
    public void IsZeta_ValidData_ShouldReturnTrue()
    {
        // Arrange
        var visits = new List<Visit>
        {
            new("V1", null, 1, 1, 9.3, 0, 0.004),
            new("V2", null, 0, 0, 18.8, 0, 0.007)
        };
        var element = new Element("d4e5f", visits);
        // Act
        var isZeta = element.IsZeta;
        // Assert
        Assert.True(isZeta);
    }

    [Fact]
    public void IsZeta_InvalidData_ShouldReturnFalse()
    {
        // Arrange
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 18.8, 0, 0.002),
            new("V2", null, 1, 1, 19.3, 1, 0.007)
        };
        var element = new Element("d4e5f", visits);
        // Act
        var isZeta = element.IsZeta;
        // Assert
        Assert.False(isZeta);
    }

    [Fact]
    public void ElementIsBetaSetIfAllCleeryValuesAreZero()
    {
        // Arrange
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 0, 0, 0),
            new("V2", null, 0, 0, 0, 0, 0)
        };

        var element = new Element("d4e5f", visits);

        // Act
        var isBeta = element.IsGamma;

        Assert.True(isBeta, $"Element should be in Gamma set when all values are zero. {element.MemberSet}");
    }


    [Fact]
    public void IsAlpha_ZetaFalse_AlphaTrue()
    {
        // Arrange so is not a Zeta element
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 0, 0, 0), // delta Cp is 0
            new("V2", null, 0, 0, 0, 0, 0)
        };

        // Act & Assert all zeros both visits should be in Alpha not Zeta
        var element = new Element("d4e5f", visits);

        _testOutputHelper.WriteLine(element.ToString());
        Assert.False(element.IsZeta, "Is not supposed to be Zeta");
    }

    [Fact]
    public void IsGammaLimit_ReturnTrue()
    {
        // Arrange so is a Beta Participant delta-CAC >= 0 and TPS >= 0, should fail IsGamma
        var visits = new List<Visit>
        {
            new("V1", null, 0, 21, 0, 0, 0), //cac is positive but the delta cac is 0
            new("V2", null, 0, 21, 0, 0, 0)
        };
        var element = new Element("d4e5f", visits);

        _testOutputHelper.WriteLine(element.ToString());

        Assert.False(element.IsGamma, "Is not supposed to be Gamma");
        Assert.Equal(0, element.DCac);


        // Arrange so is a Gamma Participant CAC = 0 both visits and delta TPS not negative
        var lowTpc = RandomGen.Next(0, 10);
        visits = new List<Visit>
        {
            new("V1", null, lowTpc, 0, 0, 0, 0), // delta Cp is 0
            new("V2", null, RandomGen.Next(lowTpc, 10), 0, 0, 0, 0)
        };
        element = new Element("d4e5f", visits);

        _testOutputHelper.WriteLine(element.ToString());

        Assert.True(element.IsGamma, " Expected both visits to have CAC = 0");
    }

    [Fact]
    public void IsBetaLimit_ReturnTrue()
    {
        // Arrange so is a Beta Participant delta-CAC >= 0 and TPS >= 0, should fail IsGamma
        var visits = new List<Visit>
        {
            new("V1", null, 0, 21, 0, 0, 0), //cp is positive but the delta cac is 0
            new("V2", null, 0, 21, 0, 0, 0)
        };
        var element = new Element("d4e5f", visits);

        _testOutputHelper.WriteLine(element.ToString());

        Assert.False(element.IsGamma, "Is not supposed to be Gamma");
        Assert.Equal(0, element.DCac);
    }

    [Fact]
    public void IsEtaLimit_ReturnTrue()
    {
        // Arrange so is a Beta Participant delta-CAC >= 0 and TPS >= 0, should fail IsGamma
        var visits = new List<Visit>
        {
            new("V1", null, 0, 21, 0, 0, 0), //cac is positive but the delta cac is 0
            new("V2", null, 0, 32, 0, 0, 0)
        };
        var element = new Element("d4e5f", visits);

        _testOutputHelper.WriteLine(element.ToString());

        Assert.True(element.IsEta, "Expected result to belong to Eta");
        Assert.Equal(11, element.DCac);
    }

    [Fact]
    public void IsThetaLimit_ReturnTrue()
    {
        // Arrange so is a Beta Participant delta-CAC >= 0 and TPS >= 0, should fail IsGamma
        var visits = new List<Visit>
        {
            new("V1", null, 0, 21, 0, 0, 0), //cac is positive but the delta cac is 0
            new("V2", null, 0, 31, 0, 0, 0)
        };
        var element = new Element("d4e5f", visits);
        _testOutputHelper.WriteLine(element.ToString());
        Assert.True(element.MemberSet == SetName.Theta, "Expected result to belong to Theta");
        Assert.True(element.IsTheta, "Expected result to belong to Theta");
        Assert.Equal(10, element.DCac);
    }

    [Fact]
    public void IsThetaLimit_ReturnFalse()
    {
        // Arrange so is a Beta Participant delta-CAC >= 0 and TPS >= 0, should fail IsGamma
        var visits = new List<Visit>
        {
            new("V1", null, 0, 21, 0, 0, 0), //cac is positive but the delta cac is 0
            new("V2", null, 0, 32, 0, 0, 0)
        };
        var element = new Element("d4e5f", visits);
        _testOutputHelper.WriteLine(element.ToString());
        Assert.False(element.MemberSet == SetName.Theta, "element.MemberSet == SetName.Theta");
        Assert.False(element.IsTheta, "element is supposed to be Beta");
        Assert.Equal(11, element.DCac);
    }
}