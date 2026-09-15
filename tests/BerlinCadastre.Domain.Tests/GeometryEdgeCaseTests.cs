using BerlinCadastre.Domain;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Domain.Tests;

public sealed class GeometryEdgeCaseTests
{
    private static readonly GeometryFactory Factory = new(new PrecisionModel(), 25833);

    [Fact]
    public void GeometryReference_RejectsEmptyGeometry()
    {
        Point empty = Factory.CreatePoint();

        Assert.Throws<ArgumentException>(() => new GeometryReference(empty, CoordinateReferenceSystem.Etrs89Utm33N));
    }

    [Fact]
    public void GeometryReference_RejectsSelfIntersectingPolygon()
    {
        Polygon bowTie = Factory.CreatePolygon([
            new Coordinate(0, 0),
            new Coordinate(10, 10),
            new Coordinate(10, 0),
            new Coordinate(0, 10),
            new Coordinate(0, 0)
        ]);

        Assert.False(bowTie.IsValid);
        Assert.Throws<ArgumentException>(() => new GeometryReference(bowTie, CoordinateReferenceSystem.Etrs89Utm33N));
    }

    [Fact]
    public void GeometryReference_PreservesMultiPolygonWithoutFlattening()
    {
        Polygon first = Square(0, 0, 10);
        Polygon second = Square(20, 20, 5);
        MultiPolygon multiPolygon = Factory.CreateMultiPolygon([first, second]);

        GeometryReference reference = new(multiPolygon, CoordinateReferenceSystem.Etrs89Utm33N);

        Assert.IsType<MultiPolygon>(reference.Geometry);
        Assert.Equal(2, reference.Geometry.NumGeometries);
    }

    private static Polygon Square(double x, double y, double size) => Factory.CreatePolygon([
        new Coordinate(x, y),
        new Coordinate(x + size, y),
        new Coordinate(x + size, y + size),
        new Coordinate(x, y + size),
        new Coordinate(x, y)
    ]);
}
