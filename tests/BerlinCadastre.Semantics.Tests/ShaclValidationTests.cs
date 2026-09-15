using BerlinCadastre.Semantics;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace BerlinCadastre.Semantics.Tests;

public sealed class ShaclValidationTests
{
    [Fact]
    public void Validate_RejectsParcelWithoutProvenance()
    {
        IGraph data = new RdfGraphBuilder().Build([SemanticFixtures.Parcel], [], [SemanticFixtures.District]);
        data.Retract(data.Triples.Where(t => t.Predicate.Equals(data.CreateUriNode(SemanticUris.ProvWasDerivedFrom))).ToArray());
        IGraph shapes = new Graph();
        StringParser.Parse(shapes, File.ReadAllText(FindShapesFile()));

        bool conforms = new ShaclValidationService().Conforms(data, shapes);

        Assert.False(conforms);
    }

    private static string FindShapesFile()
    {
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../ontology/shapes.ttl"));
        Assert.True(File.Exists(path), $"Shapes file not found at {path}");
        return path;
    }
}
