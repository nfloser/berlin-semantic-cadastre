using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VDS.RDF;
using VDS.RDF.Writing;

namespace BerlinCadastre.Semantics;

public sealed class TurtleExportService
{
    private readonly ILogger<TurtleExportService> _logger;

    public TurtleExportService(ILogger<TurtleExportService>? logger = null)
    {
        _logger = logger ?? NullLogger<TurtleExportService>.Instance;
    }

    public string SerializeValidated(IGraph graph, ShaclValidationService validator, string shapesPath)
    {
        long managedMemoryBefore = GC.GetTotalMemory(false);

        Stopwatch validationStopwatch = Stopwatch.StartNew();
        validator.EnsureConforms(graph, shapesPath);
        validationStopwatch.Stop();

        Stopwatch serializationStopwatch = Stopwatch.StartNew();
        CompressingTurtleWriter writer = new();
        StringBuilder builder = new();
        using System.IO.StringWriter textWriter = new(builder, System.Globalization.CultureInfo.InvariantCulture);
        writer.Save(graph, textWriter);
        string turtle = builder.ToString();
        serializationStopwatch.Stop();

        _logger.LogInformation(
            "Semantic export metrics: {TripleCount} triples, SHACL validation {ValidationMilliseconds} ms, Turtle serialization {SerializationMilliseconds} ms, {Utf8ByteCount} UTF-8 bytes, managed-memory delta {ManagedMemoryDeltaBytes} bytes",
            graph.Triples.Count,
            validationStopwatch.Elapsed.TotalMilliseconds,
            serializationStopwatch.Elapsed.TotalMilliseconds,
            Encoding.UTF8.GetByteCount(turtle),
            GC.GetTotalMemory(false) - managedMemoryBefore);

        return turtle;
    }
}
