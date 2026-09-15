using System.Diagnostics;
using BerlinCadastre.Domain;

namespace BerlinCadastre.Infrastructure.Wfs;

public sealed class BerlinWfsClient
{
    private readonly HttpClient _httpClient;
    private readonly WfsGeoJsonParser _parser;
    private readonly WfsRequestOptions _options;

    public BerlinWfsClient(HttpClient httpClient, WfsGeoJsonParser parser, WfsRequestOptions options)
    {
        _httpClient = httpClient;
        _parser = parser;
        _options = options;
        if (_options.PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(options), "Page size must be positive.");
        if (_options.MaxPages <= 0) throw new ArgumentOutOfRangeException(nameof(options), "Max pages must be positive.");
    }

    public async Task<WfsFetchResult> GetFeaturesAsync(Uri baseUri, string featureType, BoundingBox? boundingBox, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        if (string.IsNullOrWhiteSpace(featureType)) throw new ArgumentException("Feature type must not be blank.", nameof(featureType));
        if (boundingBox is not null && boundingBox.Crs.Epsg != _options.SourceSrid)
            throw new ArgumentException($"WFS bounding boxes must be EPSG:{_options.SourceSrid}.", nameof(boundingBox));

        List<WfsFeature> all = [];
        List<WfsFeatureRejection> rejections = [];
        TimeSpan parseDuration = TimeSpan.Zero;
        TimeSpan requestDuration = TimeSpan.Zero;
        for (int page = 0; page < _options.MaxPages; page++)
        {
            int startIndex = page * _options.PageSize;
            Uri requestUri = BuildGetFeatureUri(baseUri, featureType, boundingBox, startIndex);
            Stopwatch requestStopwatch = Stopwatch.StartNew();
            string body = await GetWithRetryAsync(requestUri, cancellationToken).ConfigureAwait(false);
            requestStopwatch.Stop();
            requestDuration += requestStopwatch.Elapsed;

            WfsParseResult parsed = _parser.Parse(body, _options.SourceSrid);
            parseDuration += parsed.ParseDuration;
            all.AddRange(parsed.Features);
            rejections.AddRange(parsed.Rejections);

            int pageFeatureCount = parsed.Features.Count + parsed.Rejections.Count;
            if (pageFeatureCount < _options.PageSize)
            {
                return new WfsFetchResult(all, rejections, parseDuration, requestDuration);
            }
        }

        throw new InvalidOperationException($"WFS pagination reached the configured safety limit of {_options.MaxPages} pages.");
    }

    private Uri BuildGetFeatureUri(Uri baseUri, string featureType, BoundingBox? boundingBox, int startIndex)
    {
        Dictionary<string, string> parameters = new(StringComparer.Ordinal)
        {
            ["service"] = "WFS",
            ["version"] = "2.0.0",
            ["request"] = "GetFeature",
            ["typeNames"] = featureType,
            ["srsName"] = $"EPSG:{_options.SourceSrid}",
            ["outputFormat"] = _options.OutputFormat,
            ["count"] = _options.PageSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["startIndex"] = startIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        if (boundingBox is not null) parameters["bbox"] = boundingBox.ToWfsBbox();

        string query = string.Join('&', parameters.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        UriBuilder builder = new(baseUri) { Query = query };
        return builder.Uri;
    }

    private async Task<string> GetWithRetryAsync(Uri uri, CancellationToken cancellationToken)
    {
        Exception? last = null;
        for (int attempt = 1; attempt <= _options.RetryCount; attempt++)
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                last = exception;
                if (attempt < _options.RetryCount)
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken).ConfigureAwait(false);
            }
        }
        throw new HttpRequestException($"WFS request failed after {_options.RetryCount} attempts: {uri}", last);
    }
}
