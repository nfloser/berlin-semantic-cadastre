namespace BerlinCadastre.Domain;

public sealed record DataSource(
    string Id,
    string Provider,
    string DatasetTitle,
    Uri Endpoint,
    string ServiceType,
    string Licence,
    CoordinateReferenceSystem SourceCrs,
    DateTimeOffset RetrievedAt,
    string SourceFeatureId)
{
    public static DataSource BerlinAlkis(string id, string title, Uri endpoint, string sourceFeatureId, DateTimeOffset retrievedAt) =>
        new(id, "Senatsverwaltung für Stadtentwicklung, Bauen und Wohnen Berlin", title, endpoint, "OGC WFS 2.0.0", "Datenlizenz Deutschland – Zero – Version 2.0 (dl-de-zero-2.0)", CoordinateReferenceSystem.Etrs89Utm33N, retrievedAt, sourceFeatureId);
}
