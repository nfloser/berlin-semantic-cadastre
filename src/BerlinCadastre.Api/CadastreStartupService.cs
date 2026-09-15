using BerlinCadastre.Infrastructure.Ingestion;

namespace BerlinCadastre.Api;

public sealed class CadastreStartupService : BackgroundService
{
    private readonly BerlinCadastreIngestionService _ingestion;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CadastreStartupService> _logger;

    public CadastreStartupService(BerlinCadastreIngestionService ingestion, IConfiguration configuration, ILogger<CadastreStartupService> logger)
    {
        _ingestion = ingestion;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue("Cadastre:LoadOnStartup", false)) return;
        try
        {
            await _ingestion.IngestAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Live Berlin cadastral ingestion failed. The API remains available but readiness stays degraded.");
        }
    }
}
