using BerlinCadastre.Application;
using BerlinCadastre.Domain;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Application.Tests;

public sealed class SpatialQueryServiceTests
{
    private static readonly GeometryFactory Factory = new(new PrecisionModel(), 25833);

    [Fact]
    public void FindBuildingsInsideParcel_ReturnsOnlyCoveredBuildings()
    {
        CadastralParcel parcel = Fixtures.Parcel("p1", Square(0, 0, 10));
        Building inside = Fixtures.Building("b1", Square(2, 2, 2));
        Building outside = Fixtures.Building("b2", Square(20, 20, 2));
        InMemoryCadastreRepository repository = new([parcel], [inside, outside], []);
        SpatialQueryService service = new(repository);

        IReadOnlyList<Building> result = service.FindBuildingsInsideParcel(parcel.Id);

        Assert.Single(result);
        Assert.Equal("b1", result[0].Id.Value);
    }

    [Fact]
    public void FindParcelsContainingPoint_UsesCoversSoBoundaryPointsAreIncluded()
    {
        CadastralParcel parcel = Fixtures.Parcel("p1", Square(0, 0, 10));
        InMemoryCadastreRepository repository = new([parcel], [], []);
        SpatialQueryService service = new(repository);

        IReadOnlyList<CadastralParcel> result = service.FindParcelsContainingPoint(Factory.CreatePoint(new Coordinate(0, 5)));

        Assert.Single(result);
    }

    private static Polygon Square(double x, double y, double size) => Factory.CreatePolygon([
        new Coordinate(x, y), new Coordinate(x + size, y), new Coordinate(x + size, y + size),
        new Coordinate(x, y + size), new Coordinate(x, y)
    ]);
}
