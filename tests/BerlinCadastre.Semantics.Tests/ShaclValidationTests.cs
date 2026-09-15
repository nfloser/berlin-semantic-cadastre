using BerlinCadastre.Semantics;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace BerlinCadastre.Semantics.Tests;

public sealed class ShaclValidationTests
{
    [Fact]
    public void Validate_AcceptsCompleteGraph()
    {
        IGraph data = new RdfGraphBuilder().Build([SemanticFixtures.Parcel], [SemanticFixtures.Building], [SemanticFixtures.District]);
        IGraph shapes = LoadShapes();

        bool conforms = new ShaclValidationService().Conforms(data, shapes);

        Assert.True(conforms);
    }

    [Fact]
    public void Validate_RejectsParcelWithoutProvenance()
    {
        IGraph data = new RdfGraphBuilder().Build([SemanticFixtures.Parcel], [], [SemanticFixtures.District]);
        data.Retract(data.Triples.Where(t => t.Predicate.Equals(data.CreateUriNode(SemanticUris.ProvWasDerivedFrom))).ToArray());
        IGraph shapes = LoadShapes();

        bool conforms = new ShaclValidationService().Conforms(data, shapes);

        Assert.False(conforms);
    }

    private static IGraph LoadShapes()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "ontology", "shapes.ttl");
        Assert.True(File.Exists(path), $"Shapes file not found at {path}");
        IGraph shapes = new Graph();
        StringParser.Parse(shapes, File.ReadAllText(path));
        return shapes;
    }
}
