using BerlinCadastre.Domain;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Domain.Tests;

public sealed class DomainContractTests
{
    [Fact]
    public void ParcelId_RejectsBlankValues()
    {
        Assert.Throws<ArgumentException>(() => new ParcelId(" "));
    }

    [Fact]
    public void GeometryReference_RequiresMatchingSrid()
    {
        Geometry geometry = new GeometryFactory(new PrecisionModel(), 4326).CreatePoint(new Coordinate(13.4, 52.5));

        Assert.Throws<ArgumentException>(() => new GeometryReference(geometry, CoordinateReferenceSystem.Etrs89Utm33N));
    }

    [Fact]
    public void GeometryReference_AcceptsValidMetricGeometry()
    {
        Geometry geometry = new GeometryFactory(new PrecisionModel(), 25833).CreatePoint(new Coordinate(391000, 5820000));

        GeometryReference reference = new(geometry, CoordinateReferenceSystem.Etrs89Utm33N);

        Assert.Equal(25833, reference.Crs.Epsg);
    }
}
