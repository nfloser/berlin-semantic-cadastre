namespace BerlinCadastre.Infrastructure.Mapping;

public static class BerlinSourceCatalog
{
    public static readonly Uri ParcelEndpoint = new("https://gdi.berlin.de/services/wfs/alkis_flurstuecke");
    public static readonly Uri BuildingEndpoint = new("https://gdi.berlin.de/services/wfs/alkis_gebaeude");
    public static readonly Uri DistrictEndpoint = new("https://gdi.berlin.de/services/wfs/alkis_bezirke");

    public const string ParcelFeatureType = "alkis_flurstuecke:flurstuecke";
    public const string BuildingFeatureType = "alkis_gebaeude:gebaeude";
    public const string DistrictFeatureType = "alkis_bezirke:bezirksgrenzen";
}
