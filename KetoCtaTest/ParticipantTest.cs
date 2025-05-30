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
        var participant = new Participant("d4e5f", "a1b2c", visits);
        // Assert
        Assert.NotNull(participant);
        Assert.Equal("d4e5f", participant.ParticipantId);
        Assert.Equal("a1b2c", participant.Hash);
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
        var exception = Assert.Throws<ArgumentException>(() => new Participant("d4e5f", "a1b2c", visits));
        Assert.Contains("Visits list must contain at least two visits.", exception.Message);
    }

    [Fact]
    public void Participant_Constructor_NullVisits_ShouldThrowArgumentNullException()
    {
        // Arrange
        List<Visit>? visits = null;
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Participant("d4e5f", "a1b2c", visits!));
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
        
        var participant = new Participant("d4e5f", "a1b2c", visits);
        _testOutputHelper.WriteLine(participant.ToString());


        // Act
        var result = participant.ToString();
        // Assert
        Assert.Contains("ParticipantId: d4e5f", result);
        Assert.Contains("Hash: a1b2c", result);
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
        var exception = Assert.Throws<ArgumentNullException>(() => new Participant(null!, "a1b2c", visits));
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
        var participant = new Participant("d4e5f", "a1b2c", visits);
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
        var participant = new Participant("d4e5f", "a1b2c", visits);
        // Act
        var isZeta = participant.IsZeta;
        // Assert
        Assert.False(isZeta);
    }

    [Fact]
    public void IsAlpha_InvalidData_ShouldReturnFalse()  
    {
        // Arrange so is not a Zeta participant
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 0, 0, 0), // delta Cac is 0
            new("V2", null, 0, 0, 0, 0, 0)
        };

        // Act & Assert all zeros both visits should be in Alpha not Zeta
        var participant = new Participant("d4e5f", "a1b2c", visits);

        _testOutputHelper.WriteLine(participant.ToString());
        Assert.False(participant.IsZeta, "Is not supposed to be Zeta");
        Assert.True(participant.IsAlpha, "Is supposed tp be Alpha"); // Should not be an Alpha participant since it is a Zeta participant
    }



}