using BerlinCadastre.Domain;
using NetTopologySuite.IO;
using VDS.RDF;

namespace BerlinCadastre.Semantics;

public sealed class RdfGraphBuilder
{
    private readonly WKTWriter _wktWriter = new();

    public IGraph Build(IEnumerable<CadastralParcel> parcels, IEnumerable<Building> buildings, IEnumerable<AdministrativeDistrict> districts)
    {
        Graph graph = new();
        graph.NamespaceMap.AddNamespace("cad", new Uri(SemanticUris.CadNamespace));
        graph.NamespaceMap.AddNamespace("geo", new Uri(SemanticUris.GeoNamespace));
        graph.NamespaceMap.AddNamespace("prov", new Uri(SemanticUris.ProvNamespace));

        foreach (AdministrativeDistrict district in districts) AddDistrict(graph, district);
        foreach (CadastralParcel parcel in parcels) AddParcel(graph, parcel);
        foreach (Building building in buildings) AddBuilding(graph, building);
        return graph;
    }

    private void AddParcel(IGraph graph, CadastralParcel parcel)
    {
        IUriNode feature = AddFeature(graph, "parcel", parcel.Id.Value, SemanticUris.CadParcel, parcel.Geometry, parcel.Source);
        if (parcel.DistrictId is DistrictId districtId)
            graph.Assert(feature, graph.CreateUriNode(SemanticUris.CadLocatedIn), graph.CreateUriNode(SemanticUris.Feature("district", districtId.Value)));
    }

    private void AddBuilding(IGraph graph, Building building)
    {
        IUriNode feature = AddFeature(graph, "building", building.Id.Value, SemanticUris.CadBuilding, building.Geometry, building.Source);
        if (building.ParcelId is ParcelId parcelId)
            graph.Assert(feature, graph.CreateUriNode(SemanticUris.CadLocatedOnParcel), graph.CreateUriNode(SemanticUris.Feature("parcel", parcelId.Value)));
        if (building.DistrictId is DistrictId districtId)
            graph.Assert(feature, graph.CreateUriNode(SemanticUris.CadLocatedIn), graph.CreateUriNode(SemanticUris.Feature("district", districtId.Value)));
    }

    private void AddDistrict(IGraph graph, AdministrativeDistrict district) =>
        AddFeature(graph, "district", district.Id.Value, SemanticUris.CadDistrict, district.Geometry, district.Source);

    private IUriNode AddFeature(IGraph graph, string kind, string id, Uri classUri, GeometryReference geometry, DataSource source)
    {
        IUriNode feature = graph.CreateUriNode(SemanticUris.Feature(kind, id));
        IUriNode geometryNode = graph.CreateUriNode(SemanticUris.Geometry(kind, id));
        IUriNode sourceNode = AddSource(graph, source);
        IUriNode rdfType = graph.CreateUriNode(SemanticUris.RdfType);

        graph.Assert(feature, rdfType, graph.CreateUriNode(classUri));
        graph.Assert(feature, rdfType, graph.CreateUriNode(SemanticUris.GeoFeature));
        graph.Assert(feature, graph.CreateUriNode(SemanticUris.CadIdentifier), graph.CreateLiteralNode(id));
        graph.Assert(feature, graph.CreateUriNode(SemanticUris.GeoHasGeometry), geometryNode);
        graph.Assert(feature, graph.CreateUriNode(SemanticUris.ProvWasDerivedFrom), sourceNode);

        graph.Assert(geometryNode, rdfType, graph.CreateUriNode(SemanticUris.GeoGeometry));
        string lexicalWkt = $"<{geometry.Crs.Urn}> {_wktWriter.Write(geometry.Geometry)}";
        graph.Assert(geometryNode, graph.CreateUriNode(SemanticUris.GeoAsWkt), graph.CreateLiteralNode(lexicalWkt, SemanticUris.GeoWktLiteral));
        return feature;
    }

    private static IUriNode AddSource(IGraph graph, DataSource source)
    {
        IUriNode node = graph.CreateUriNode(SemanticUris.Source(source.Id));
        graph.Assert(node, graph.CreateUriNode(SemanticUris.RdfType), graph.CreateUriNode(SemanticUris.ProvEntity));
        graph.Assert(node, graph.CreateUriNode(SemanticUris.CadSourceEndpoint), graph.CreateLiteralNode(source.Endpoint.AbsoluteUri));
        graph.Assert(node, graph.CreateUriNode(SemanticUris.CadDatasetTitle), graph.CreateLiteralNode(source.DatasetTitle));
        graph.Assert(node, graph.CreateUriNode(SemanticUris.CadRetrievedAt), graph.CreateLiteralNode(source.RetrievedAt.ToString("O"), new Uri(SemanticUris.XsdNamespace + "dateTime")));
        return node;
    }
}
