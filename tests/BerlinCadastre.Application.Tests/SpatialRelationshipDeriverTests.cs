using BerlinCadastre.Application;
using BerlinCadastre.Domain;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Application.Tests;

public sealed class SpatialRelationshipDeriverTests
{
    private static readonly GeometryFactory Factory = new(new PrecisionModel(), 25833);

    [Fact]
    public void Derive_LinksBuildingToParcelAndDistrictBySpace()
    {
        AdministrativeDistrict district = new(new DistrictId("d1"), "District", Ref(Square(0, 0, 100)), Source("d1"), new Dictionary<string, string?>());
        CadastralParcel parcel = new(new ParcelId("p1"), Ref(Square(10, 10, 20)), null, Source("p1"), new Dictionary<string, string?>());
        Building building = new(new BuildingId("b1"), Ref(Square(12, 12, 3)), null, null, Source("b1"), new Dictionary<string, string?>());

        var result = new SpatialRelationshipDeriver().Derive([parcel], [building], [district]);

        Assert.Equal(new DistrictId("d1"), result.Parcels[0].DistrictId);
        Assert.Equal(new ParcelId("p1"), result.Buildings[0].ParcelId);
        Assert.Equal(new DistrictId("d1"), result.Buildings[0].DistrictId);
    }

    private static GeometryReference Ref(Geometry geometry) => new(geometry, CoordinateReferenceSystem.Etrs89Utm33N);
    private static DataSource Source(string id) => DataSource.BerlinAlkis("test", "Test", new Uri("https://example.test"), id, DateTimeOffset.UnixEpoch);
    private static Polygon Square(double x, double y, double size) => Factory.CreatePolygon([
        new Coordinate(x, y), new Coordinate(x + size, y), new Coordinate(x + size, y + size),
        new Coordinate(x, y + size), new Coordinate(x, y)
    ]);
}
