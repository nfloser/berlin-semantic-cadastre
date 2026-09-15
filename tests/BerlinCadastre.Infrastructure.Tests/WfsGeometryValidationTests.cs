using BerlinCadastre.Infrastructure.Wfs;

namespace BerlinCadastre.Infrastructure.Tests;

public sealed class WfsGeometryValidationTests
{
    [Fact]
    public void Parser_RejectsTopologicallyInvalidSourcePolygon()
    {
        const string json = """
        {"type":"FeatureCollection","features":[{"type":"Feature","id":"invalid.1","properties":{},"geometry":{"type":"Polygon","coordinates":[[[0,0],[10,10],[10,0],[0,10],[0,0]]]}}]}
        """;

        WfsParseResult result = new WfsGeoJsonParser().Parse(json, 25833);

        Assert.Empty(result.Features);
        WfsFeatureRejection rejection = Assert.Single(result.Rejections);
        Assert.Equal("invalid.1", rejection.Id);
        Assert.Contains("invalid", rejection.Reason, StringComparison.OrdinalIgnoreCase);
    }
}
