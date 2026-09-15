using BerlinCadastre.Domain;
using BerlinCadastre.Infrastructure.Wfs;

namespace BerlinCadastre.Infrastructure.Mapping;

public sealed class BerlinFeatureMapper
{
    public CadastralParcel MapParcel(WfsFeature feature, DateTimeOffset retrievedAt) => new(
        new ParcelId(feature.Id), Reference(feature), null,
        DataSource.BerlinAlkis("berlin-alkis-parcels", "ALKIS Berlin Flurstücke", BerlinSourceCatalog.ParcelEndpoint, feature.Id, retrievedAt),
        feature.Properties);

    public Building MapBuilding(WfsFeature feature, DateTimeOffset retrievedAt) => new(
        new BuildingId(feature.Id), Reference(feature), null, null,
        DataSource.BerlinAlkis("berlin-alkis-buildings", "ALKIS Berlin Gebäude", BerlinSourceCatalog.BuildingEndpoint, feature.Id, retrievedAt),
        feature.Properties);

    public AdministrativeDistrict MapDistrict(WfsFeature feature, DateTimeOffset retrievedAt)
    {
        string name = FirstNonBlank(feature.Properties, "namgem", "name", "bezirk") ?? feature.Id;
        return new AdministrativeDistrict(
            new DistrictId(feature.Id), name, Reference(feature),
            DataSource.BerlinAlkis("berlin-alkis-districts", "ALKIS Berlin Bezirke", BerlinSourceCatalog.DistrictEndpoint, feature.Id, retrievedAt),
            feature.Properties);
    }

    private static GeometryReference Reference(WfsFeature feature) => new(feature.Geometry, CoordinateReferenceSystem.Etrs89Utm33N);

    private static string? FirstNonBlank(IReadOnlyDictionary<string, string?> values, params string[] keys)
    {
        foreach (string key in keys)
        {
            if (values.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)) return value;
        }
        return null;
    }
}
