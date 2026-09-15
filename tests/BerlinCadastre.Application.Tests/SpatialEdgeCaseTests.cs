using BerlinCadastre.Application;
using BerlinCadastre.Domain;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Application.Tests;

public sealed class SpatialEdgeCaseTests
{
    private static readonly GeometryFactory Factory = new(new PrecisionModel(), 25833);

    [Fact]
    public void Intersections_IncludeGeometryThatOnlyTouchesParcelBoundary()
    {
        CadastralParcel parcel = Fixtures.Parcel("p1", Square(0, 0, 10));
        Polygon touching = Square(10, 2, 2);
        SpatialQueryService service = new(new InMemoryCadastreRepository([parcel], [], []));

        IReadOnlyList<CadastralParcel> result = service.FindParcelsIntersecting(touching);

        Assert.Single(result);
    }

    [Fact]
    public void MetricQuery_RejectsLongitudeLatitudeGeometry()
    {
        SpatialQueryService service = new(new InMemoryCadastreRepository([], [], []));
        GeometryFactory wgs84 = new(new PrecisionModel(), 4326);
        Point berlin = wgs84.CreatePoint(new Coordinate(13.405, 52.52));

        Assert.Throws<ArgumentException>(() => service.FindBuildingsWithinDistance(berlin, 100));
    }

    [Fact]
    public void DistanceQuery_UsesProjectedMetres()
    {
        Building building = Fixtures.Building("b1", Square(20, 0, 2));
        SpatialQueryService service = new(new InMemoryCadastreRepository([], [building], []));
        Point origin = Factory.CreatePoint(new Coordinate(0, 0));

        Assert.Empty(service.FindBuildingsWithinDistance(origin, 19.9));
        Assert.Single(service.FindBuildingsWithinDistance(origin, 20));
    }

    private static Polygon Square(double x, double y, double size) => Factory.CreatePolygon([
        new Coordinate(x, y),
        new Coordinate(x + size, y),
        new Coordinate(x + size, y + size),
        new Coordinate(x, y + size),
        new Coordinate(x, y)
    ]);
}
