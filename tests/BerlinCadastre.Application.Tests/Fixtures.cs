using BerlinCadastre.Domain;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Application.Tests;

internal static class Fixtures
{
    private static readonly Uri Endpoint = new("https://example.test/wfs");

    public static CadastralParcel Parcel(string id, Geometry geometry) => new(
        new ParcelId(id), new GeometryReference(geometry, CoordinateReferenceSystem.Etrs89Utm33N), null,
        DataSource.BerlinAlkis("test", "Test", Endpoint, id, DateTimeOffset.UnixEpoch), new Dictionary<string, string?>());

    public static Building Building(string id, Geometry geometry) => new(
        new BuildingId(id), new GeometryReference(geometry, CoordinateReferenceSystem.Etrs89Utm33N), null, null,
        DataSource.BerlinAlkis("test", "Test", Endpoint, id, DateTimeOffset.UnixEpoch), new Dictionary<string, string?>());
}
