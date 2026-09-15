namespace BerlinCadastre.Domain;

public sealed record CoordinateReferenceSystem
{
    public static readonly CoordinateReferenceSystem Etrs89Utm33N = new(25833, "ETRS89 / UTM zone 33N", true);
    public static readonly CoordinateReferenceSystem Wgs84 = new(4326, "WGS 84", false);

    public CoordinateReferenceSystem(int epsg, string name, bool metric)
    {
        if (epsg <= 0) throw new ArgumentOutOfRangeException(nameof(epsg));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("CRS name must not be blank.", nameof(name));
        Epsg = epsg;
        Name = name;
        IsMetric = metric;
    }

    public int Epsg { get; }
    public string Name { get; }
    public bool IsMetric { get; }
    public string Urn => $"http://www.opengis.net/def/crs/EPSG/0/{Epsg}";
}
