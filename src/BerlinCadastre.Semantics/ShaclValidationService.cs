using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;

namespace BerlinCadastre.Semantics;

public sealed class ShaclValidationService
{
    public bool Conforms(IGraph dataGraph, IGraph shapesGraph)
    {
        ArgumentNullException.ThrowIfNull(dataGraph);
        ArgumentNullException.ThrowIfNull(shapesGraph);
        ShapesGraph shapes = new(shapesGraph);
        return shapes.Conforms(dataGraph);
    }

    public bool Conforms(IGraph dataGraph, string shapesPath)
    {
        if (string.IsNullOrWhiteSpace(shapesPath)) throw new ArgumentException("Shapes path must not be blank.", nameof(shapesPath));
        Graph shapesGraph = new();
        FileLoader.Load(shapesGraph, shapesPath);
        return Conforms(dataGraph, shapesGraph);
    }

    public void EnsureConforms(IGraph dataGraph, string shapesPath)
    {
        if (!Conforms(dataGraph, shapesPath))
            throw new InvalidDataException("Semantic graph failed SHACL validation and will not be published.");
    }
}
