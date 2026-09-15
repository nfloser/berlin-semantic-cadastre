using System.Text.Json;
using NetTopologySuite.IO;

namespace BerlinCadastre.Infrastructure.Wfs;

public sealed class WfsGeoJsonParser
{
    private readonly GeoJsonReader _reader = new();

    public WfsParseResult Parse(string json, int sourceSrid)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("WFS response must not be blank.", nameof(json));
        }

        using JsonDocument document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("features", out JsonElement features) || features.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("Expected a GeoJSON FeatureCollection with a features array.");
        }

        List<WfsFeature> accepted = [];
        List<WfsFeatureRejection> rejected = [];
        foreach (JsonElement element in features.EnumerateArray())
        {
            string? id = ReadId(element);
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new InvalidDataException("Feature has no stable identifier.");
                }

                if (!element.TryGetProperty("geometry", out JsonElement geometryElement) || geometryElement.ValueKind == JsonValueKind.Null)
                {
                    throw new InvalidDataException("Feature has no geometry.");
                }

                NetTopologySuite.Geometries.Geometry geometry = _reader.Read<NetTopologySuite.Geometries.Geometry>(geometryElement.GetRawText());
                geometry.SRID = sourceSrid;
                if (geometry.IsEmpty)
                {
                    throw new InvalidDataException("Feature geometry is empty.");
                }

                if (!geometry.IsValid)
                {
                    throw new InvalidDataException("Feature geometry is topologically invalid.");
                }

                accepted.Add(new WfsFeature(id, geometry, ReadProperties(element)));
            }
            catch (Exception exception) when (exception is InvalidDataException or JsonException or ArgumentException)
            {
                rejected.Add(new WfsFeatureRejection(id, exception.Message));
            }
        }

        return new WfsParseResult(accepted, rejected);
    }

    private static string? ReadId(JsonElement feature)
    {
        if (feature.TryGetProperty("id", out JsonElement id) && id.ValueKind is JsonValueKind.String or JsonValueKind.Number)
        {
            return id.ToString();
        }

        if (feature.TryGetProperty("properties", out JsonElement properties))
        {
            foreach (string candidate in new[] { "gml_id", "gml:id", "id", "objectid" })
            {
                if (properties.TryGetProperty(candidate, out JsonElement property) && property.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
                {
                    return property.ToString();
                }
            }
        }

        return null;
    }

    private static IReadOnlyDictionary<string, string?> ReadProperties(JsonElement feature)
    {
        Dictionary<string, string?> result = new(StringComparer.OrdinalIgnoreCase);
        if (!feature.TryGetProperty("properties", out JsonElement properties) || properties.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (JsonProperty property in properties.EnumerateObject())
        {
            result[property.Name] = property.Value.ValueKind == JsonValueKind.Null ? null : property.Value.ToString();
        }

        return result;
    }
}
