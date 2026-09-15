using System.Net;
using System.Net.Http.Json;
using BerlinCadastre.Application;
using BerlinCadastre.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Api.Tests;

public sealed class CadastreApiTests : IClassFixture<CadastreApiFactory>
{
    private readonly HttpClient _client;

    public CadastreApiTests(CadastreApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsOk() => Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health")).StatusCode);

    [Fact]
    public async Task BuildingsInParcel_ReturnsSeededBuilding()
    {
        var result = await _client.GetFromJsonAsync<List<FeatureDto>>("/spatial/buildings/in-parcel/p1");
        FeatureDto building = Assert.Single(result!);
        Assert.Equal("b1", building.Id);
        Assert.Equal("p1", building.ParcelId);
    }

    [Fact]
    public async Task GeoJsonExport_IsWgs84FeatureCollection()
    {
        string json = await _client.GetStringAsync("/export/geojson?type=buildings");
        Assert.Contains("\"FeatureCollection\"", json);
        Assert.Contains("\"EPSG:4326\"", json);
        Assert.Contains("\"b1\"", json);
    }

    public sealed record FeatureDto(string Id, string? ParcelId);
}

public sealed class CadastreApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICadastreRepository>();
            services.AddSingleton<ICadastreRepository>(_ => SeedRepository());
        });
    }

    private static ICadastreRepository SeedRepository()
    {
        GeometryFactory factory = new(new PrecisionModel(), 25833);
        Polygon parcelGeometry = factory.CreatePolygon([new Coordinate(391000, 5820000), new Coordinate(391100, 5820000), new Coordinate(391100, 5820100), new Coordinate(391000, 5820100), new Coordinate(391000, 5820000)]);
        Polygon buildingGeometry = factory.CreatePolygon([new Coordinate(391010, 5820010), new Coordinate(391020, 5820010), new Coordinate(391020, 5820020), new Coordinate(391010, 5820020), new Coordinate(391010, 5820010)]);
        DataSource source = DataSource.BerlinAlkis("test", "Fixture", new Uri("https://example.test"), "fixture", DateTimeOffset.UnixEpoch);
        CadastralParcel parcel = new(new ParcelId("p1"), new GeometryReference(parcelGeometry, CoordinateReferenceSystem.Etrs89Utm33N), null, source, new Dictionary<string, string?>());
        Building building = new(new BuildingId("b1"), new GeometryReference(buildingGeometry, CoordinateReferenceSystem.Etrs89Utm33N), new ParcelId("p1"), null, source, new Dictionary<string, string?>());
        return new InMemoryCadastreRepository([parcel], [building], []);
    }
}
