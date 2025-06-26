namespace KetoCtaTest;
using Keto_Cta;
using Xunit.Abstractions;


public class SerializationJsonTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
    #region Visit Serialization Tests
    [Fact]
    public void SerializeVisit()
    {
        var visit = new Visit("123", DateTime.Now, 10, 20, 30.5, 40.5, 50.5);
        var json = System.Text.Json.JsonSerializer.Serialize(visit);
        _testOutputHelper.WriteLine(json);
        Assert.NotNull(json);
        Assert.Contains("123", json);

    }
    //[Fact]
    //public void DeserializeVisit()
    //{
    //    var json = "{\"Id\":\"123\",\"VisitDate\":\"2023-10-01T00:00:00Z\",\"Tps\":10.0,\"Cac\":20.0,\"Ncpv\":30.5,\"Tcpv\":40.5,\"Pav\":50.5}";
    //    var visit = System.Text.Json.JsonSerializer.Deserialize<Visit>(json);
    //    Assert.NotNull(visit);
    //    Assert.Equal("123", visit.Id);
    //    Assert.Equal(10, visit.Tps);
    //}

    //[Fact]
    //public void SerializeVisitWithLnValues()
    //{
    //    var visit = new Visit("123", DateTime.Now, 10, 20, 30.5, 40.5, 50.5);
    //    var json = System.Text.Json.JsonSerializer.Serialize(visit);
    //    _testOutputHelper.WriteLine(json);
    //    Assert.NotNull(json);
    //    Assert.Contains("LnTps", json);
    //    Assert.Contains("LnCac", json);
    //    Assert.Contains("LnNcpv", json);
    //    Assert.Contains("LnTcpv", json);
    //    Assert.Contains("LnPav", json);
    //}

    //[Fact]
    //public void DeserializeVisitWithLnValues()
    //{
    //    var json = "{\"Id\":\"123\",\"VisitDate\":\"2023-10-01T00:00:00Z\",\"Tps\":10.0,\"Cac\":20.0,\"Ncpv\":30.5,\"Tcpv\":40.5,\"Pav\":50.5,\"LnTps\":2.302585092994046,\"LnCac\":2.995732173547877,\"LnNcpv\":3.4011973816621555,\"LnTcpv\":3.6888794541139363,\"LnPav\":3.912023005428146}";
    //    var visit = System.Text.Json.JsonSerializer.Deserialize<Visit>(json);
    //    Assert.NotNull(visit);
    //    Assert.Equal("123", visit.Id);
    //    Assert.Equal(10, visit.Tps);
    //    Assert.Equal(2.302585092994046, visit.LnTps, 6);
    //}
    #endregion

    #region Event Serialization Tests
    [Fact]
    public void SerializeElement()
    {
        var visits = new List<Visit>
        {
            new("V1", null, 0, 0, 0, 0, 0),
            new("V2", null, 0, 0, 0, 0, 0)
        };
        var element = new Element("Element1", visits);

        var json = System.Text.Json.JsonSerializer.Serialize(element);
        _testOutputHelper.WriteLine(json);
        Assert.NotNull(json);
        Assert.Contains("Element1", json);
    }
    //[Fact]
    //public void DeserializeElement()
    //{
    //    var json = "{\"Id\":\"Element1\",\"Visits\":[{\"Id\":\"V1\",\"VisitDate\":null,\"Tps\":0.0,\"Cac\":0.0,\"Ncpv\":0.0,\"Tcpv\":0.0,\"Pav\":0.0},{\"Id\":\"V2\",\"VisitDate\":null,\"Tps\":0,\"Cac\":0,\"Ncpv\":0.0,\"Tcpv\":0.0,\"Pav\":0.0}],\"MemberSet\":1}";
    //    var element = System.Text.Json.JsonSerializer.Deserialize<Element>(json);
    //    Assert.NotNull(element);
    //    Assert.Equal("Element1", element.Id);
    //    Assert.Equal(2, element.Visits.Count);
    //}
    #endregion

}