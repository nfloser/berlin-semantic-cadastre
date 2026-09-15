using System.Diagnostics;
using BerlinCadastre.Application;
using BerlinCadastre.Domain;
using BerlinCadastre.Infrastructure.Mapping;
using BerlinCadastre.Infrastructure.Wfs;
using Microsoft.Extensions.Logging;

namespace BerlinCadastre.Infrastructure.Ingestion;

public sealed class BerlinCadastreIngestionService
{
    private readonly BerlinWfsClient _client;
    private readonly BerlinFeatureMapper _mapper;
    private readonly SpatialRelationshipDeriver _relationshipDeriver;
    private readonly ICadastreRepository _repository;
    private readonly BerlinIngestionOptions _options;
    private readonly ILogger<BerlinCadastreIngestionService> _logger;

    public BerlinCadastreIngestionService(BerlinWfsClient client, BerlinFeatureMapper mapper, SpatialRelationshipDeriver relationshipDeriver, ICadastreRepository repository, BerlinIngestionOptions options, ILogger<BerlinCadastreIngestionService> logger)
    {
        _client = client;
        _mapper = mapper;
        _relationshipDeriver = relationshipDeriver;
        _repository = repository;
        _options = options;
        _logger = logger;
    }

    public async Task<IngestionSummary> IngestAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset retrievedAt = DateTimeOffset.UtcNow;
        long managedMemoryBefore = GC.GetTotalMemory(false);
        Stopwatch totalStopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting Berlin ALKIS ingestion for bounding box {BoundingBox}", _options.InitialBoundingBox.ToWfsBbox());

        Stopwatch fetchWallStopwatch = Stopwatch.StartNew();
        Task<WfsFetchResult> districtTask = _client.GetFeaturesAsync(_options.DistrictEndpoint, _options.DistrictFeatureType, null, cancellationToken);
        Task<WfsFetchResult> parcelTask = _client.GetFeaturesAsync(_options.ParcelEndpoint, _options.ParcelFeatureType, _options.InitialBoundingBox, cancellationToken);
        Task<WfsFetchResult> buildingTask = _client.GetFeaturesAsync(_options.BuildingEndpoint, _options.BuildingFeatureType, _options.InitialBoundingBox, cancellationToken);
        await Task.WhenAll(districtTask, parcelTask, buildingTask).ConfigureAwait(false);
        fetchWallStopwatch.Stop();

        WfsFetchResult districtResult = await districtTask;
        WfsFetchResult parcelResult = await parcelTask;
        WfsFetchResult buildingResult = await buildingTask;

        Stopwatch mappingStopwatch = Stopwatch.StartNew();
        AdministrativeDistrict[] districts = [.. districtResult.Features.Select(feature => _mapper.MapDistrict(feature, retrievedAt))];
        CadastralParcel[] parcels = [.. parcelResult.Features.Select(feature => _mapper.MapParcel(feature, retrievedAt))];
        Building[] buildings = [.. buildingResult.Features.Select(feature => _mapper.MapBuilding(feature, retrievedAt))];
        mappingStopwatch.Stop();

        Stopwatch relationshipStopwatch = Stopwatch.StartNew();
        var enriched = _relationshipDeriver.Derive(parcels, buildings, districts);
        relationshipStopwatch.Stop();
        _repository.ReplaceAll(enriched.Parcels, enriched.Buildings, districts);
        totalStopwatch.Stop();

        int rejected = districtResult.Rejections.Count + parcelResult.Rejections.Count + buildingResult.Rejections.Count;
        int accepted = districtResult.Features.Count + parcelResult.Features.Count + buildingResult.Features.Count;
        TimeSpan parseDuration = districtResult.ParseDuration + parcelResult.ParseDuration + buildingResult.ParseDuration;
        TimeSpan requestDuration = districtResult.RequestDuration + parcelResult.RequestDuration + buildingResult.RequestDuration;
        long managedMemoryDelta = GC.GetTotalMemory(false) - managedMemoryBefore;

        if (rejected > 0) _logger.LogWarning("Berlin ALKIS ingestion rejected {RejectedFeatureCount} invalid source features", rejected);
        _logger.LogInformation("Berlin ALKIS ingestion completed: {DistrictCount} districts, {ParcelCount} parcels, {BuildingCount} buildings", districts.Length, enriched.Parcels.Count, enriched.Buildings.Count);
        _logger.LogInformation(
            "Berlin ALKIS ingestion metrics: {AcceptedFeatureCount} accepted, {RejectedFeatureCount} rejected; fetch wall {FetchWallMilliseconds} ms, cumulative WFS requests {RequestMilliseconds} ms, GeoJSON parse/validation {ParseMilliseconds} ms, domain mapping {MappingMilliseconds} ms, spatial relationship derivation {RelationshipMilliseconds} ms, total {TotalMilliseconds} ms, managed-memory delta {ManagedMemoryDeltaBytes} bytes",
            accepted,
            rejected,
            fetchWallStopwatch.Elapsed.TotalMilliseconds,
            requestDuration.TotalMilliseconds,
            parseDuration.TotalMilliseconds,
            mappingStopwatch.Elapsed.TotalMilliseconds,
            relationshipStopwatch.Elapsed.TotalMilliseconds,
            totalStopwatch.Elapsed.TotalMilliseconds,
            managedMemoryDelta);

        return new IngestionSummary(enriched.Parcels.Count, enriched.Buildings.Count, districts.Length, rejected, retrievedAt);
    }
}

public sealed record IngestionSummary(int Parcels, int Buildings, int Districts, int RejectedFeatures, DateTimeOffset RetrievedAt);
