using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;

namespace BerlinCadastre.Semantics;

public sealed class TurtleExportService
{
    public string SerializeValidated(IGraph graph, ShaclValidationService validator, string shapesPath)
    {
        validator.EnsureConforms(graph, shapesPath);
        CompressingTurtleWriter writer = new();
        StringBuilder builder = new();
        using System.IO.StringWriter textWriter = new(builder, System.Globalization.CultureInfo.InvariantCulture);
        writer.Save(graph, textWriter);
        return builder.ToString();
    }
}
