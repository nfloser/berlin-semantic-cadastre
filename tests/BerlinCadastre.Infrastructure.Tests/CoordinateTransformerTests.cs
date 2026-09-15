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

        NetTopologySuite.Geometries.Geometry transformed = new CoordinateTransformer().ToWgs84(point);

        Assert.Equal(4326, transformed.SRID);
        Assert.InRange(transformed.Coordinate.X, 13.2, 13.6);
        Assert.InRange(transformed.Coordinate.Y, 52.3, 52.7);
    }

    [Fact]
    public void Transformation_RoundTripsBerlinCoordinateWithinCentimetreTolerance()
    {
        GeometryFactory factory = new(new PrecisionModel(), 25833);
        Point source = factory.CreatePoint(new Coordinate(391800.123, 5820000.456));
        CoordinateTransformer transformer = new();

        Point roundTripped = (Point)transformer.ToInternal(transformer.ToWgs84(source));

        Assert.Equal(25833, roundTripped.SRID);
        Assert.InRange(Math.Abs(roundTripped.X - source.X), 0, 0.01);
        Assert.InRange(Math.Abs(roundTripped.Y - source.Y), 0, 0.01);
    }
}
