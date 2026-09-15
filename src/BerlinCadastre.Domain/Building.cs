namespace BerlinCadastre.Domain;

public sealed record Building(
    BuildingId Id,
    GeometryReference Geometry,
    ParcelId? ParcelId,
    DistrictId? DistrictId,
    DataSource Source,
    IReadOnlyDictionary<string, string?> Attributes);
