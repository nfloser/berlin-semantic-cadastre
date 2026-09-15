namespace BerlinCadastre.Semantics;

public static class SemanticUris
{
    public const string CadNamespace = "https://nfloser.github.io/berlin-semantic-cadastre/ontology#";
    public const string ResourceNamespace = "https://nfloser.github.io/berlin-semantic-cadastre/resource/";
    public const string GeoNamespace = "http://www.opengis.net/ont/geosparql#";
    public const string ProvNamespace = "http://www.w3.org/ns/prov#";
    public const string RdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    public const string XsdNamespace = "http://www.w3.org/2001/XMLSchema#";

    public static readonly Uri RdfType = new(RdfNamespace + "type");
    public static readonly Uri GeoFeature = new(GeoNamespace + "Feature");
    public static readonly Uri GeoGeometry = new(GeoNamespace + "Geometry");
    public static readonly Uri GeoHasGeometry = new(GeoNamespace + "hasGeometry");
    public static readonly Uri GeoAsWkt = new(GeoNamespace + "asWKT");
    public static readonly Uri GeoWktLiteral = new(GeoNamespace + "wktLiteral");
    public static readonly Uri ProvEntity = new(ProvNamespace + "Entity");
    public static readonly Uri ProvWasDerivedFrom = new(ProvNamespace + "wasDerivedFrom");
    public static readonly Uri CadParcel = new(CadNamespace + "CadastralParcel");
    public static readonly Uri CadBuilding = new(CadNamespace + "Building");
    public static readonly Uri CadDistrict = new(CadNamespace + "AdministrativeDistrict");
    public static readonly Uri CadIdentifier = new(CadNamespace + "identifier");
    public static readonly Uri CadLocatedOnParcel = new(CadNamespace + "locatedOnParcel");
    public static readonly Uri CadLocatedIn = new(CadNamespace + "locatedIn");
    public static readonly Uri CadSourceEndpoint = new(CadNamespace + "sourceEndpoint");
    public static readonly Uri CadDatasetTitle = new(CadNamespace + "datasetTitle");
    public static readonly Uri CadRetrievedAt = new(CadNamespace + "retrievedAt");

    public static Uri Feature(string kind, string id) => new(ResourceNamespace + kind + "/" + Uri.EscapeDataString(id));
    public static Uri Geometry(string kind, string id) => new(ResourceNamespace + kind + "/" + Uri.EscapeDataString(id) + "/geometry");
    public static Uri Source(string sourceId) => new(ResourceNamespace + "source/" + Uri.EscapeDataString(sourceId));
}
