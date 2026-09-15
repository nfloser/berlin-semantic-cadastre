using BerlinCadastre.Infrastructure.Geometry;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Infrastructure.Tests;

public sealed class CoordinateTransformerTests
{
    [Fact]
    public void ToWgs84_TransformsBerlinUtmCoordinates()
    {
        GeometryFactory factory = new(new PrecisionModel(), 25833);
        Point point = factory.CreatePoint(new Coordinate(391800, 5820000));

        Geometry transformed = new CoordinateTransformer().ToWgs84(point);

        Assert.Equal(4326, transformed.SRID);
        Assert.InRange(transformed.Coordinate.X, 13.2, 13.6);
        Assert.InRange(transformed.Coordinate.Y, 52.3, 52.7);
    }
}
