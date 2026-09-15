using BerlinCadastre.Infrastructure.Mapping;
using BerlinCadastre.Infrastructure.Wfs;

namespace BerlinCadastre.LiveTests;

public sealed class BerlinWfsLiveTests
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };
    private readonly WfsGeoJsonParser _parser = new();

    [Theory]
    [Trait("Category", "Live")]
    [InlineData("https://gdi.berlin.de/services/wfs/alkis_flurstuecke", "alkis_flurstuecke:flurstuecke", "391000,5819000,393000,5821000,EPSG:25833")]
    [InlineData("https://gdi.berlin.de/services/wfs/alkis_gebaeude", "alkis_gebaeude:gebaeude", "391000,5819000,393000,5821000,EPSG:25833")]
    [InlineData("https://gdi.berlin.de/services/wfs/alkis_bezirke", "alkis_bezirke:bezirksgrenzen", null)]
    public async Task OfficialWfs_ReturnsGeoJsonCompatibleWithParser(string endpoint, string featureType, string? bbox)
    {
        string url = endpoint + "?service=WFS&version=2.0.0&request=GetFeature&typeNames=" + Uri.EscapeDataString(featureType) + "&srsName=EPSG%3A25833&outputFormat=json&count=1";
        if (bbox is not null) url += "&bbox=" + Uri.EscapeDataString(bbox);

        string json = await Http.GetStringAsync(url);
        WfsParseResult result = _parser.Parse(json, 25833);

        Assert.NotEmpty(result.Features);
        Assert.Empty(result.Rejections);
    }
}
