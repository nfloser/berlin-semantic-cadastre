namespace BerlinCadastre.Domain;

public sealed record CadastralParcel(
    ParcelId Id,
    GeometryReference Geometry,
    DistrictId? DistrictId,
    DataSource Source,
    IReadOnlyDictionary<string, string?> Attributes);
