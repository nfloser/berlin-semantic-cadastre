namespace BerlinCadastre.Infrastructure.Wfs;

public sealed class WfsRequestOptions
{
    public int PageSize { get; init; } = 1000;
    public int MaxPages { get; init; } = 100;
    public int RetryCount { get; init; } = 3;
    public string OutputFormat { get; init; } = "json";
    public int SourceSrid { get; init; } = 25833;
}
