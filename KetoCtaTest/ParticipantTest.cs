using Keto_Cta;
using StatTest;
using Xunit.Abstractions;

namespace KetoCtaTest;

public class ParticipantTest(ITestOutputHelper testOutputHelper)
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
        var participant = new Participant("d4e5f", visits);
        // Assert
        Assert.NotNull(participant);
        Assert.Equal("d4e5f", participant.ParticipantId);

        Assert.Equal(2, participant.Visits.Count);

        _testOutputHelper.WriteLine(participant.ToString());
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
        var exception = Assert.Throws<ArgumentException>(() => new Participant("d4e5f", visits));
        Assert.Contains("Visits list must contain at least two visits.", exception.Message);
    }

    [Fact]
    public void Participant_Constructor_NullVisits_ShouldThrowArgumentNullException()
    {
        // Arrange
        List<Visit>? visits = null;
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Participant("d4e5f", visits!));
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

        var participant = new Participant("d4e5f", visits);
        _testOutputHelper.WriteLine(participant.ToString());


        // Act
        var result = participant.ToString();
        // Assert
        string id = "d4e5f";
        Assert.Contains($"ParticipantId: {id}", result);
        Assert.Equal(2, participant.Visits.Count);
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
        var exception = Assert.Throws<ArgumentNullException>(() => new Participant(null!, visits));
        Assert.Equal("Value cannot be null. (Parameter 'participantId')", exception.Message);
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
        var participant = new Participant("d4e5f", visits);
        // Act
        var isZeta = participant.IsZeta;
        // Assert
        Assert.True(isZeta);
    }

    [Fact]
    public void IsZeta_InvalidData_ShouldReturnFalse()
    {
        // Arrange
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 18.8, 0, 0.007),
            new("V2", null, 1, 1, 9.3, 0, 0.004)
        };
        var participant = new Participant("d4e5f", visits);
        // Act
        var isZeta = participant.IsZeta;
        // Assert
        Assert.False(isZeta);
    }

    [Fact]
    public void IsAlpha_ZetaFalse_AlphaTrue()
    {
        // Arrange so is not a Zeta participant
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 0, 0, 0), // delta Cac is 0
            new("V2", null, 0, 0, 0, 0, 0)
        };

        // Act & Assert all zeros both visits should be in Alpha not Zeta
        var participant = new Participant("d4e5f", visits);

        _testOutputHelper.WriteLine(participant.ToString());
        Assert.False(participant.IsZeta, "Is not supposed to be Zeta");
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
        var participant = new Participant("d4e5f", visits);

        _testOutputHelper.WriteLine(participant.ToString());

        Assert.False(participant.IsGamma, "Is not supposed to be Gamma");
        Assert.Equal(0, participant.DeltaCac);



        // Arrange so is a Gamma Participant CAC = 0 both visits and delta TPS not negative
        var lowTpc = RandomGen.Next(0, 10);
        visits = new List<Visit>
        {
                new("V1", null, lowTpc, 0, 0, 0, 0), // delta Cac is 0
                new("V2", null, RandomGen.Next(lowTpc,10), 0, 0, 0, 0)
        };
        participant = new Participant("d4e5f", visits);

        _testOutputHelper.WriteLine(participant.ToString());

        Assert.True(participant.IsGamma, " Expected both visits to have CAC = 0");
    }

    [Fact]
    public void IsBetaLimit_ReturnTrue()
    {
        // Arrange so is a Beta Participant delta-CAC >= 0 and TPS >= 0, should fail IsGamma
        var visits = new List<Visit>
        {
            new("V1", null, 0, 21, 0, 0, 0), //cac is positive but the delta cac is 0
            new("V2", null, 0, 21, 0, 0, 0)
        };
        var participant = new Participant("d4e5f", visits);

        _testOutputHelper.WriteLine(participant.ToString());

        Assert.False(participant.IsGamma, "Is not supposed to be Gamma");
        Assert.Equal(0, participant.DeltaCac);

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
        var participant = new Participant("d4e5f", visits);

        _testOutputHelper.WriteLine(participant.ToString());

        Assert.True(participant.IsEta, "Expected result to belong to Eta");
        Assert.Equal(11, participant.DeltaCac);
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
        var participant = new Participant("d4e5f", visits);
        _testOutputHelper.WriteLine(participant.ToString());
        Assert.True(participant.IsTheta, "Expected result to belong to Theta");
        Assert.Equal(10, participant.DeltaCac);
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
        var participant = new Participant("d4e5f", visits);
        _testOutputHelper.WriteLine(participant.ToString());
        Assert.False(participant.IsTheta, "Expected result to not belong to Theta");
        Assert.Equal(11, participant.DeltaCac);
    }


}