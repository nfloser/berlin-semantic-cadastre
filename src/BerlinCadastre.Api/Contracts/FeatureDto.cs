using BerlinCadastre.Domain;
using NetTopologySuite.IO;

namespace BerlinCadastre.Api.Contracts;

public sealed record FeatureDto(
    string Id,
    string FeatureType,
    string? ParcelId,
    string? DistrictId,
    int Epsg,
    string Wkt,
    IReadOnlyDictionary<string, string?> Attributes,
    ProvenanceDto Provenance)
{
    private static readonly WKTWriter Writer = new();

    public static FeatureDto From(CadastralParcel parcel) => new(parcel.Id.Value, "parcel", null, parcel.DistrictId?.Value, parcel.Geometry.Crs.Epsg, Writer.Write(parcel.Geometry.Geometry), parcel.Attributes, ProvenanceDto.From(parcel.Source));
    public static FeatureDto From(Building building) => new(building.Id.Value, "building", building.ParcelId?.Value, building.DistrictId?.Value, building.Geometry.Crs.Epsg, Writer.Write(building.Geometry.Geometry), building.Attributes, ProvenanceDto.From(building.Source));
    public static FeatureDto From(AdministrativeDistrict district) => new(district.Id.Value, "district", null, district.Id.Value, district.Geometry.Crs.Epsg, Writer.Write(district.Geometry.Geometry), district.Attributes, ProvenanceDto.From(district.Source));
}

public sealed record ProvenanceDto(string Dataset, string Endpoint, string Licence, string SourceFeatureId, DateTimeOffset RetrievedAt)
{
    public static ProvenanceDto From(DataSource source) => new(source.DatasetTitle, source.Endpoint.AbsoluteUri, source.Licence, source.SourceFeatureId, source.RetrievedAt);
}

public sealed record SpatialIntersectionRequest(string Wkt, int Srid = 25833, string FeatureType = "parcels");
