namespace BerlinCadastre.Domain;

public sealed record AdministrativeDistrict(
    DistrictId Id,
    string Name,
    GeometryReference Geometry,
    DataSource Source,
    IReadOnlyDictionary<string, string?> Attributes);
