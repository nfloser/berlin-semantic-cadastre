using BerlinCadastre.Api;
using BerlinCadastre.Api.Contracts;
using BerlinCadastre.Application;
using BerlinCadastre.Domain;
using BerlinCadastre.Infrastructure.Export;
using BerlinCadastre.Infrastructure.Geometry;
using BerlinCadastre.Infrastructure.Ingestion;
using BerlinCadastre.Infrastructure.Mapping;
using BerlinCadastre.Infrastructure.Wfs;
using BerlinCadastre.Semantics;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using VDS.RDF;
using VDS.RDF.Writing;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICadastreRepository, InMemoryCadastreRepository>();
builder.Services.AddSingleton<SpatialQueryService>();
builder.Services.AddSingleton<SpatialRelationshipDeriver>();
builder.Services.AddSingleton<WfsGeoJsonParser>();
builder.Services.AddSingleton(new WfsRequestOptions
{
    PageSize = builder.Configuration.GetValue("Cadastre:PageSize", 1000),
    MaxPages = builder.Configuration.GetValue("Cadastre:MaxPages", 100),
    RetryCount = builder.Configuration.GetValue("Cadastre:RetryCount", 3),
    OutputFormat = builder.Configuration.GetValue("Cadastre:OutputFormat", "json") ?? "json",
    SourceSrid = 25833
});
builder.Services.AddSingleton(CreateIngestionOptions(builder.Configuration));
builder.Services.AddHttpClient<BerlinWfsClient>(client => client.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddSingleton<BerlinFeatureMapper>();
builder.Services.AddSingleton<BerlinCadastreIngestionService>();
builder.Services.AddSingleton<CoordinateTransformer>();
builder.Services.AddSingleton<GeoJsonExportService>();
builder.Services.AddSingleton<RdfGraphBuilder>();
builder.Services.AddSingleton<ShaclValidationService>();
builder.Services.AddSingleton<TurtleExportService>();
builder.Services.AddHostedService<CadastreStartupService>();

WebApplication app = builder.Build();
string applicationVersion = typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "unknown";

app.MapGet("/health", () => Results.Ok(new { status = "ok", version = applicationVersion }));
app.MapGet("/ready", (ICadastreRepository repository) =>
{
    var payload = new { ready = repository.Parcels.Count > 0 && repository.Buildings.Count > 0 && repository.Districts.Count > 0, parcels = repository.Parcels.Count, buildings = repository.Buildings.Count, districts = repository.Districts.Count };
    return payload.ready ? Results.Ok(payload) : Results.Json(payload, statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/parcels", (ICadastreRepository repository, int? limit) => Results.Ok(repository.Parcels.Take(ClampLimit(limit)).Select(FeatureDto.From)));
app.MapGet("/parcels/{id}", (string id, ICadastreRepository repository) => repository.FindParcel(new ParcelId(id)) is { } parcel ? Results.Ok(FeatureDto.From(parcel)) : Results.NotFound());
app.MapGet("/buildings", (ICadastreRepository repository, int? limit) => Results.Ok(repository.Buildings.Take(ClampLimit(limit)).Select(FeatureDto.From)));
app.MapGet("/buildings/{id}", (string id, ICadastreRepository repository) => repository.FindBuilding(new BuildingId(id)) is { } building ? Results.Ok(FeatureDto.From(building)) : Results.NotFound());
app.MapGet("/districts", (ICadastreRepository repository) => Results.Ok(repository.Districts.Select(FeatureDto.From)));
app.MapGet("/districts/{id}", (string id, ICadastreRepository repository) => repository.FindDistrict(new DistrictId(id)) is { } district ? Results.Ok(FeatureDto.From(district)) : Results.NotFound());

app.MapGet("/spatial/buildings/in-parcel/{parcelId}", (string parcelId, SpatialQueryService service) =>
{
    try { return Results.Ok(service.FindBuildingsInsideParcel(new ParcelId(parcelId)).Select(FeatureDto.From)); }
    catch (KeyNotFoundException) { return Results.NotFound(); }
});
app.MapGet("/spatial/buildings/in-district/{districtId}", (string districtId, SpatialQueryService service) =>
{
    try { return Results.Ok(service.FindBuildingsInDistrict(new DistrictId(districtId)).Select(FeatureDto.From)); }
    catch (KeyNotFoundException) { return Results.NotFound(); }
});
app.MapGet("/spatial/parcels/containing-point", (double x, double y, int? srid, SpatialQueryService service, CoordinateTransformer transformer) =>
{
    try
    {
        Point point = CreateInternalPoint(x, y, srid ?? 25833, transformer);
        return Results.Ok(service.FindParcelsContainingPoint(point).Select(FeatureDto.From));
    }
    catch (ArgumentException exception) { return Results.BadRequest(new { error = exception.Message }); }
});
app.MapGet("/spatial/nearby", (double x, double y, double distanceMetres, int? srid, SpatialQueryService service, CoordinateTransformer transformer) =>
{
    try
    {
        Point point = CreateInternalPoint(x, y, srid ?? 25833, transformer);
        return Results.Ok(service.FindBuildingsWithinDistance(point, distanceMetres).Select(FeatureDto.From));
    }
    catch (ArgumentException exception) { return Results.BadRequest(new { error = exception.Message }); }
});
app.MapGet("/spatial/parcels/multi-building", (int? minBuildings, SpatialQueryService service) =>
    Results.Ok(service.FindParcelsWithMoreThanBuildings(minBuildings ?? 1).Select(FeatureDto.From)));
app.MapPost("/spatial/intersections", (SpatialIntersectionRequest request, SpatialQueryService service, CoordinateTransformer transformer) =>
{
    try
    {
        Geometry geometry = new WKTReader().Read(request.Wkt);
        geometry.SRID = request.Srid;
        Geometry internalGeometry = request.Srid switch
        {
            25833 => geometry,
            4326 => transformer.ToInternal(geometry),
            _ => throw new ArgumentException("Only EPSG:25833 and EPSG:4326 are accepted by the API.")
        };
        return request.FeatureType.ToLowerInvariant() switch
        {
            "parcels" => Results.Ok(service.FindParcelsIntersecting(internalGeometry).Select(FeatureDto.From)),
            "buildings" => Results.Ok(service.FindBuildingsIntersecting(internalGeometry).Select(FeatureDto.From)),
            _ => Results.BadRequest(new { error = "featureType must be 'parcels' or 'buildings'." })
        };
    }
    catch (Exception exception) when (exception is ParseException or ArgumentException)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
});

app.MapGet("/export/geojson", (string? type, ICadastreRepository repository, GeoJsonExportService exporter) =>
{
    string json = (type ?? "parcels").ToLowerInvariant() switch
    {
        "parcels" => exporter.ExportParcels(repository.Parcels),
        "buildings" => exporter.ExportBuildings(repository.Buildings),
        "districts" => exporter.ExportDistricts(repository.Districts),
        _ => string.Empty
    };
    return json.Length == 0 ? Results.BadRequest(new { error = "type must be parcels, buildings, or districts." }) : Results.Text(json, "application/geo+json");
});

app.MapGet("/semantic/graph", (ICadastreRepository repository, RdfGraphBuilder graphBuilder, TurtleExportService exporter, ShaclValidationService validator, IConfiguration configuration) =>
{
    IGraph graph = graphBuilder.Build(repository.Parcels, repository.Buildings, repository.Districts);
    string turtle = exporter.SerializeValidated(graph, validator, ResolveShapesPath(configuration));
    return Results.Text(turtle, "text/turtle");
});
app.MapGet("/export/turtle", (ICadastreRepository repository, RdfGraphBuilder graphBuilder, TurtleExportService exporter, ShaclValidationService validator, IConfiguration configuration) =>
{
    IGraph graph = graphBuilder.Build(repository.Parcels, repository.Buildings, repository.Districts);
    string turtle = exporter.SerializeValidated(graph, validator, ResolveShapesPath(configuration));
    return Results.Text(turtle, "text/turtle");
});
app.MapGet("/semantic/entity/{kind}/{id}", (string kind, string id, ICadastreRepository repository, RdfGraphBuilder graphBuilder) =>
{
    IGraph full = graphBuilder.Build(repository.Parcels, repository.Buildings, repository.Districts);
    Uri entityUri;
    try { entityUri = SemanticUris.Feature(NormalizeKind(kind), id); }
    catch (ArgumentException exception) { return Results.BadRequest(new { error = exception.Message }); }
    IUriNode entity = full.CreateUriNode(entityUri);
    if (!full.Triples.WithSubject(entity).Any()) return Results.NotFound();
    Graph subgraph = new();
    foreach (Triple triple in full.Triples.WithSubject(entity)) subgraph.Assert(CopyTripleTo(triple, subgraph));
    foreach (Triple link in full.Triples.WithSubject(entity).Where(t => t.Object.NodeType == NodeType.Uri))
    {
        IUriNode objectNode = (IUriNode)link.Object;
        if (objectNode.Uri.AbsoluteUri.EndsWith("/geometry", StringComparison.Ordinal) || objectNode.Uri.AbsoluteUri.Contains("/source/", StringComparison.Ordinal))
            foreach (Triple triple in full.Triples.WithSubject(objectNode)) subgraph.Assert(CopyTripleTo(triple, subgraph));
    }
    System.IO.StringWriter text = new();
    new CompressingTurtleWriter().Save(subgraph, text);
    return Results.Text(text.ToString(), "text/turtle");
});
app.MapGet("/semantic/provenance/{kind}/{id}", (string kind, string id, ICadastreRepository repository) =>
{
    DataSource? source = kind.ToLowerInvariant() switch
    {
        "parcel" or "parcels" => repository.FindParcel(new ParcelId(id))?.Source,
        "building" or "buildings" => repository.FindBuilding(new BuildingId(id))?.Source,
        "district" or "districts" => repository.FindDistrict(new DistrictId(id))?.Source,
        _ => null
    };
    return source is null ? Results.NotFound() : Results.Ok(ProvenanceDto.From(source));
});

app.Run();

static int ClampLimit(int? limit) => Math.Clamp(limit ?? 100, 1, 1000);

static Point CreateInternalPoint(double x, double y, int srid, CoordinateTransformer transformer)
{
    GeometryFactory factory = new(new PrecisionModel(), srid);
    Point point = factory.CreatePoint(new Coordinate(x, y));
    return srid switch
    {
        25833 => point,
        4326 => (Point)transformer.ToInternal(point),
        _ => throw new ArgumentException("Only EPSG:25833 and EPSG:4326 are accepted by the API.")
    };
}

static Triple CopyTripleTo(Triple triple, IGraph target) => new(
    CopyNodeTo(triple.Subject, target),
    CopyNodeTo(triple.Predicate, target),
    CopyNodeTo(triple.Object, target));

static INode CopyNodeTo(INode node, IGraph target) => node switch
{
    IUriNode uriNode => target.CreateUriNode(uriNode.Uri),
    ILiteralNode literalNode when literalNode.DataType is not null => target.CreateLiteralNode(literalNode.Value, literalNode.DataType),
    ILiteralNode literalNode when !string.IsNullOrWhiteSpace(literalNode.Language) => target.CreateLiteralNode(literalNode.Value, literalNode.Language),
    ILiteralNode literalNode => target.CreateLiteralNode(literalNode.Value),
    IBlankNode blankNode => target.CreateBlankNode(blankNode.InternalID),
    _ => throw new NotSupportedException($"Cannot copy RDF node type {node.NodeType} into entity subgraph.")
};

static string NormalizeKind(string kind) => kind.ToLowerInvariant() switch
{
    "parcel" or "parcels" => "parcel",
    "building" or "buildings" => "building",
    "district" or "districts" => "district",
    _ => throw new ArgumentException("kind must be parcel, building, or district.")
};

static BerlinIngestionOptions CreateIngestionOptions(IConfiguration configuration)
{
    string[] bboxValues = (configuration["Cadastre:InitialBoundingBox"] ?? "391000,5819000,393000,5821000").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    if (bboxValues.Length != 4 || !bboxValues.All(value => double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _)))
        throw new InvalidOperationException("Cadastre:InitialBoundingBox must contain four comma-separated EPSG:25833 numbers.");
    double[] bbox = bboxValues.Select(value => double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
    return new BerlinIngestionOptions
    {
        ParcelEndpoint = new Uri(configuration["Cadastre:ParcelEndpoint"] ?? BerlinSourceCatalog.ParcelEndpoint.AbsoluteUri),
        BuildingEndpoint = new Uri(configuration["Cadastre:BuildingEndpoint"] ?? BerlinSourceCatalog.BuildingEndpoint.AbsoluteUri),
        DistrictEndpoint = new Uri(configuration["Cadastre:DistrictEndpoint"] ?? BerlinSourceCatalog.DistrictEndpoint.AbsoluteUri),
        ParcelFeatureType = configuration["Cadastre:ParcelFeatureType"] ?? BerlinSourceCatalog.ParcelFeatureType,
        BuildingFeatureType = configuration["Cadastre:BuildingFeatureType"] ?? BerlinSourceCatalog.BuildingFeatureType,
        DistrictFeatureType = configuration["Cadastre:DistrictFeatureType"] ?? BerlinSourceCatalog.DistrictFeatureType,
        InitialBoundingBox = new BoundingBox(bbox[0], bbox[1], bbox[2], bbox[3], CoordinateReferenceSystem.Etrs89Utm33N)
    };
}

static string ResolveShapesPath(IConfiguration configuration)
{
    string configured = configuration["Semantic:ShapesPath"] ?? "ontology/shapes.ttl";
    if (Path.IsPathRooted(configured)) return configured;
    string outputPath = Path.Combine(AppContext.BaseDirectory, configured);
    return File.Exists(outputPath) ? outputPath : Path.GetFullPath(configured);
}

public partial class Program;
