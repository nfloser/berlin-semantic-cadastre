using BerlinCadastre.Infrastructure.Mapping;
using BerlinCadastre.Infrastructure.Wfs;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Infrastructure.Tests;

public sealed class BerlinFeatureMapperTests
{
    [Fact]
    public void MapDistrict_UsesVerifiedNameFieldWhenPresentAndPreservesAttributes()
    {
        GeometryFactory factory = new(new PrecisionModel(), 25833);
        WfsFeature feature = new("bezirksgrenzen.1", factory.CreatePoint(new Coordinate(391000, 5820000)), new Dictionary<string, string?> { ["namgem"] = "Mitte", ["gem"] = "001" });
        BerlinFeatureMapper mapper = new();

        var district = mapper.MapDistrict(feature, DateTimeOffset.UnixEpoch);

        Assert.Equal("Mitte", district.Name);
        Assert.Equal("001", district.Attributes["gem"]);
        Assert.Equal("bezirksgrenzen.1", district.Source.SourceFeatureId);
    }
}
