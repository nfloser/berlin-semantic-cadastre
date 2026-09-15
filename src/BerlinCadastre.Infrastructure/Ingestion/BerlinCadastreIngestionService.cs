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
        _logger.LogInformation("Starting Berlin ALKIS ingestion for bounding box {BoundingBox}", _options.InitialBoundingBox.ToWfsBbox());

        Task<WfsFetchResult> districtTask = _client.GetFeaturesAsync(_options.DistrictEndpoint, _options.DistrictFeatureType, null, cancellationToken);
        Task<WfsFetchResult> parcelTask = _client.GetFeaturesAsync(_options.ParcelEndpoint, _options.ParcelFeatureType, _options.InitialBoundingBox, cancellationToken);
        Task<WfsFetchResult> buildingTask = _client.GetFeaturesAsync(_options.BuildingEndpoint, _options.BuildingFeatureType, _options.InitialBoundingBox, cancellationToken);
        await Task.WhenAll(districtTask, parcelTask, buildingTask).ConfigureAwait(false);

        WfsFetchResult districtResult = await districtTask;
        WfsFetchResult parcelResult = await parcelTask;
        WfsFetchResult buildingResult = await buildingTask;

        AdministrativeDistrict[] districts = [.. districtResult.Features.Select(feature => _mapper.MapDistrict(feature, retrievedAt))];
        CadastralParcel[] parcels = [.. parcelResult.Features.Select(feature => _mapper.MapParcel(feature, retrievedAt))];
        Building[] buildings = [.. buildingResult.Features.Select(feature => _mapper.MapBuilding(feature, retrievedAt))];
        var enriched = _relationshipDeriver.Derive(parcels, buildings, districts);
        _repository.ReplaceAll(enriched.Parcels, enriched.Buildings, districts);

        int rejected = districtResult.Rejections.Count + parcelResult.Rejections.Count + buildingResult.Rejections.Count;
        if (rejected > 0) _logger.LogWarning("Berlin ALKIS ingestion rejected {RejectedFeatureCount} invalid source features", rejected);
        _logger.LogInformation("Berlin ALKIS ingestion completed: {DistrictCount} districts, {ParcelCount} parcels, {BuildingCount} buildings", districts.Length, enriched.Parcels.Count, enriched.Buildings.Count);
        return new IngestionSummary(enriched.Parcels.Count, enriched.Buildings.Count, districts.Length, rejected, retrievedAt);
    }
}

public sealed record IngestionSummary(int Parcels, int Buildings, int Districts, int RejectedFeatures, DateTimeOffset RetrievedAt);
