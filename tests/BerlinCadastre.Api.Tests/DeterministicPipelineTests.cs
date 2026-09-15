using System.Net.Http.Json;
using BerlinCadastre.Api.Contracts;
using BerlinCadastre.Application;
using BerlinCadastre.Infrastructure.Export;
using BerlinCadastre.Infrastructure.Geometry;
using BerlinCadastre.Infrastructure.Mapping;
using BerlinCadastre.Infrastructure.Wfs;
using BerlinCadastre.Semantics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VDS.RDF;

namespace BerlinCadastre.Api.Tests;

public sealed class DeterministicPipelineTests
{
    [Fact]
    public async Task FixturePipeline_MapsDerivesPublishesQueriesAndExports()
    {
        WfsGeoJsonParser parser = new();
        BerlinFeatureMapper mapper = new();
        DateTimeOffset retrievedAt = DateTimeOffset.Parse("2026-09-15T08:00:00Z");

        WfsFeature districtFeature = Assert.Single(parser.Parse(DistrictJson, 25833).Features);
        WfsFeature parcelFeature = Assert.Single(parser.Parse(ParcelJson, 25833).Features);
        WfsFeature buildingFeature = Assert.Single(parser.Parse(BuildingJson, 25833).Features);

        var district = mapper.MapDistrict(districtFeature, retrievedAt);
        var parcel = mapper.MapParcel(parcelFeature, retrievedAt);
        var building = mapper.MapBuilding(buildingFeature, retrievedAt);
        var enriched = new SpatialRelationshipDeriver().Derive([parcel], [building], [district]);
        InMemoryCadastreRepository repository = new(enriched.Parcels, enriched.Buildings, [district]);

        Assert.Equal(parcel.Id, Assert.Single(repository.Buildings).ParcelId);
        Assert.Equal(district.Id, Assert.Single(repository.Parcels).DistrictId);

        IGraph graph = new RdfGraphBuilder().Build(repository.Parcels, repository.Buildings, repository.Districts);
        Assert.Contains(graph.Triples, triple => triple.Predicate.Equals(graph.CreateUriNode(SemanticUris.CadLocatedOnParcel)));
        Assert.Contains(graph.Triples, triple => triple.Predicate.Equals(graph.CreateUriNode(SemanticUris.ProvWasDerivedFrom)));

        string geoJson = new GeoJsonExportService(new CoordinateTransformer()).ExportBuildings(repository.Buildings);
        Assert.Contains("\"FeatureCollection\"", geoJson);
        Assert.Contains("\"EPSG:4326\"", geoJson);
        Assert.Contains("building.1", geoJson);

        await using PipelineFactory factory = new(repository);
        using HttpClient client = factory.CreateClient();
        List<FeatureDto>? apiResult = await client.GetFromJsonAsync<List<FeatureDto>>("/spatial/buildings/in-parcel/parcel.1");

        FeatureDto apiBuilding = Assert.Single(apiResult!);
        Assert.Equal("building.1", apiBuilding.Id);
        Assert.Equal("parcel.1", apiBuilding.ParcelId);
        Assert.Equal("district.1", apiBuilding.DistrictId);
    }

    private sealed class PipelineFactory(ICadastreRepository repository) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICadastreRepository>();
                services.AddSingleton(repository);
            });
        }
    }

    private const string DistrictJson = """
    {"type":"FeatureCollection","features":[{"type":"Feature","id":"district.1","properties":{"namgem":"Test District"},"geometry":{"type":"Polygon","coordinates":[[[391000,5820000],[391200,5820000],[391200,5820200],[391000,5820200],[391000,5820000]]]}}]}
    """;

    private const string ParcelJson = """
    {"type":"FeatureCollection","features":[{"type":"Feature","id":"parcel.1","properties":{"flstnr":"fixture"},"geometry":{"type":"Polygon","coordinates":[[[391020,5820020],[391120,5820020],[391120,5820120],[391020,5820120],[391020,5820020]]]}}]}
    """;

    private const string BuildingJson = """
    {"type":"FeatureCollection","features":[{"type":"Feature","id":"building.1","properties":{"source":"fixture"},"geometry":{"type":"Polygon","coordinates":[[[391040,5820040],[391060,5820040],[391060,5820060],[391040,5820060],[391040,5820040]]]}}]}
    """;
}
