using BerlinCadastre.Domain;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Semantics.Tests;

internal static class SemanticFixtures
{
    private static readonly GeometryFactory Factory = new(new PrecisionModel(), 25833);
    private static DataSource Source(string id) => DataSource.BerlinAlkis("alkis", "ALKIS Berlin", new Uri("https://gdi.berlin.de/services/wfs/alkis"), id, DateTimeOffset.Parse("2026-09-15T08:00:00Z"));
    private static GeometryReference Ref(double x, double y) => new(Factory.CreatePoint(new Coordinate(x, y)), CoordinateReferenceSystem.Etrs89Utm33N);

    public static AdministrativeDistrict District => new(new DistrictId("district.1"), "Mitte", Ref(391000, 5820000), Source("district.1"), new Dictionary<string, string?>());
    public static CadastralParcel Parcel => new(new ParcelId("parcel.1"), Ref(391001, 5820001), new DistrictId("district.1"), Source("parcel.1"), new Dictionary<string, string?>());
    public static Building Building => new(new BuildingId("building.1"), Ref(391002, 5820002), new ParcelId("parcel.1"), new DistrictId("district.1"), Source("building.1"), new Dictionary<string, string?>());
}
