using BerlinCadastre.Infrastructure.Wfs;

namespace BerlinCadastre.Infrastructure.Tests;

public sealed class WfsGeoJsonParserTests
{
    [Fact]
    public void Parse_MapsGeoJsonFeatureWithoutLeakingSourceSchema()
    {
        const string json = """
        {"type":"FeatureCollection","features":[{"type":"Feature","id":"flurstuecke.42","properties":{"flstnr":"110001-00042","foo":7},"geometry":{"type":"Polygon","coordinates":[[[391000,5820000],[391010,5820000],[391010,5820010],[391000,5820010],[391000,5820000]]]}}]}
        """;

        WfsParseResult result = new WfsGeoJsonParser().Parse(json, 25833);

        WfsFeature feature = Assert.Single(result.Features);
        Assert.Empty(result.Rejections);
        Assert.Equal("flurstuecke.42", feature.Id);
        Assert.Equal(25833, feature.Geometry.SRID);
        Assert.Equal("110001-00042", feature.Properties["flstnr"]);
    }

    [Fact]
    public void Parse_RejectsMissingIdentifiersInsteadOfInventingOne()
    {
        const string json = """
        {"type":"FeatureCollection","features":[{"type":"Feature","properties":{},"geometry":{"type":"Point","coordinates":[391000,5820000]}}]}
        """;

        WfsParseResult result = new WfsGeoJsonParser().Parse(json, 25833);

        Assert.Empty(result.Features);
        Assert.Single(result.Rejections);
    }
}
