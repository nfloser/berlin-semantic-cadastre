using BerlinCadastre.Domain;
using BerlinCadastre.Infrastructure.Mapping;

namespace BerlinCadastre.Infrastructure.Ingestion;

public sealed class BerlinIngestionOptions
{
    public Uri ParcelEndpoint { get; init; } = BerlinSourceCatalog.ParcelEndpoint;
    public Uri BuildingEndpoint { get; init; } = BerlinSourceCatalog.BuildingEndpoint;
    public Uri DistrictEndpoint { get; init; } = BerlinSourceCatalog.DistrictEndpoint;
    public string ParcelFeatureType { get; init; } = BerlinSourceCatalog.ParcelFeatureType;
    public string BuildingFeatureType { get; init; } = BerlinSourceCatalog.BuildingFeatureType;
    public string DistrictFeatureType { get; init; } = BerlinSourceCatalog.DistrictFeatureType;
    public BoundingBox InitialBoundingBox { get; init; } = new(391000, 5819000, 393000, 5821000, CoordinateReferenceSystem.Etrs89Utm33N);
}
