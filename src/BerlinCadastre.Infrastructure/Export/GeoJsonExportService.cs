using System.Text;
using System.Text.Json;
using BerlinCadastre.Domain;
using BerlinCadastre.Infrastructure.Geometry;
using NetTopologySuite.IO;

namespace BerlinCadastre.Infrastructure.Export;

public sealed class GeoJsonExportService
{
    private readonly CoordinateTransformer _transformer;
    private readonly GeoJsonWriter _geometryWriter = new();

    public GeoJsonExportService(CoordinateTransformer transformer) => _transformer = transformer;

    public string ExportParcels(IEnumerable<CadastralParcel> parcels) => Export(parcels.Select(parcel =>
        new ExportFeature(parcel.Id.Value, "parcel", parcel.Geometry, parcel.DistrictId?.Value, null, parcel.Source, parcel.Attributes)));

    public string ExportBuildings(IEnumerable<Building> buildings) => Export(buildings.Select(building =>
        new ExportFeature(building.Id.Value, "building", building.Geometry, building.DistrictId?.Value, building.ParcelId?.Value, building.Source, building.Attributes)));

    public string ExportDistricts(IEnumerable<AdministrativeDistrict> districts) => Export(districts.Select(district =>
        new ExportFeature(district.Id.Value, "district", district.Geometry, district.Id.Value, null, district.Source, district.Attributes, district.Name)));

    private string Export(IEnumerable<ExportFeature> source)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "FeatureCollection");
            writer.WriteStartObject("metadata");
            writer.WriteString("crs", "EPSG:4326");
            writer.WriteString("sourceCrs", "EPSG:25833");
            writer.WriteString("geometryPolicy", "Coordinates are transformed to WGS84 for RFC 7946 interoperability.");
            writer.WriteEndObject();
            writer.WriteStartArray("features");
            foreach (ExportFeature feature in source)
            {
                writer.WriteStartObject();
                writer.WriteString("type", "Feature");
                writer.WriteString("id", feature.Id);
                writer.WritePropertyName("geometry");
                NetTopologySuite.Geometries.Geometry wgs84 = _transformer.ToWgs84(feature.Geometry.Geometry);
                using JsonDocument geometryJson = JsonDocument.Parse(_geometryWriter.Write(wgs84));
                geometryJson.RootElement.WriteTo(writer);
                writer.WriteStartObject("properties");
                writer.WriteString("id", feature.Id);
                writer.WriteString("featureType", feature.Type);
                if (feature.Name is not null) writer.WriteString("name", feature.Name);
                if (feature.ParcelId is not null) writer.WriteString("parcelId", feature.ParcelId);
                if (feature.DistrictId is not null) writer.WriteString("districtId", feature.DistrictId);
                writer.WriteString("sourceDataset", feature.Source.DatasetTitle);
                writer.WriteString("sourceFeatureId", feature.Source.SourceFeatureId);
                writer.WriteString("sourceEndpoint", feature.Source.Endpoint.AbsoluteUri);
                writer.WriteString("retrievedAt", feature.Source.RetrievedAt);
                foreach (KeyValuePair<string, string?> attribute in feature.Attributes)
                {
                    if (attribute.Value is null) writer.WriteNull(attribute.Key); else writer.WriteString(attribute.Key, attribute.Value);
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private sealed record ExportFeature(string Id, string Type, GeometryReference Geometry, string? DistrictId, string? ParcelId, DataSource Source, IReadOnlyDictionary<string, string?> Attributes, string? Name = null);
}
