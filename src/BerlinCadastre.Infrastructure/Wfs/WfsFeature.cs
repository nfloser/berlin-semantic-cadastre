namespace BerlinCadastre.Infrastructure.Wfs;

public sealed record WfsFeature(
    string Id,
    NetTopologySuite.Geometries.Geometry Geometry,
    IReadOnlyDictionary<string, string?> Properties);

public sealed record WfsFeatureRejection(string? Id, string Reason);

public sealed record WfsParseResult(
    IReadOnlyList<WfsFeature> Features,
    IReadOnlyList<WfsFeatureRejection> Rejections,
    TimeSpan ParseDuration)
{
    public WfsParseResult(IReadOnlyList<WfsFeature> features, IReadOnlyList<WfsFeatureRejection> rejections)
        : this(features, rejections, TimeSpan.Zero)
    {
    }
}

public sealed record WfsFetchResult(
    IReadOnlyList<WfsFeature> Features,
    IReadOnlyList<WfsFeatureRejection> Rejections,
    TimeSpan ParseDuration,
    TimeSpan RequestDuration)
{
    public WfsFetchResult(IReadOnlyList<WfsFeature> features, IReadOnlyList<WfsFeatureRejection> rejections)
        : this(features, rejections, TimeSpan.Zero, TimeSpan.Zero)
    {
    }
}
