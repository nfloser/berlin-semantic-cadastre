using BerlinCadastre.Semantics;
using VDS.RDF;

namespace BerlinCadastre.Semantics.Tests;

public sealed class RdfGraphBuilderTests
{
    [Fact]
    public void Build_AddsGeoSparqlGeometryProvenanceAndRelationships()
    {
        IGraph graph = new RdfGraphBuilder().Build([SemanticFixtures.Parcel], [SemanticFixtures.Building], [SemanticFixtures.District]);
        Uri building = SemanticUris.Feature("building", "building.1");
        Uri parcel = SemanticUris.Feature("parcel", "parcel.1");
        Uri district = SemanticUris.Feature("district", "district.1");

        Assert.True(graph.ContainsTriple(new Triple(graph.CreateUriNode(building), graph.CreateUriNode(SemanticUris.CadLocatedOnParcel), graph.CreateUriNode(parcel))));
        Assert.True(graph.ContainsTriple(new Triple(graph.CreateUriNode(building), graph.CreateUriNode(SemanticUris.CadLocatedIn), graph.CreateUriNode(district))));
        Assert.Contains(graph.Triples, triple => triple.Predicate.Equals(graph.CreateUriNode(SemanticUris.GeoAsWkt)) && triple.Object.NodeType == NodeType.Literal);
        Assert.Contains(graph.Triples, triple => triple.Predicate.Equals(graph.CreateUriNode(SemanticUris.ProvWasDerivedFrom)));
    }
}
