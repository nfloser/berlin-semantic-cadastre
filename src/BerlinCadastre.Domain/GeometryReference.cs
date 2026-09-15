using NetTopologySuite.Geometries;

namespace BerlinCadastre.Domain;

public sealed record GeometryReference
{
    public GeometryReference(Geometry geometry, CoordinateReferenceSystem crs)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(crs);
        if (geometry.IsEmpty) throw new ArgumentException("Geometry must not be empty.", nameof(geometry));
        if (!geometry.IsValid) throw new ArgumentException("Geometry must be topologically valid.", nameof(geometry));
        if (geometry.SRID != crs.Epsg)
        {
            throw new ArgumentException($"Geometry SRID {geometry.SRID} does not match EPSG:{crs.Epsg}.", nameof(geometry));
        }

        Geometry = geometry;
        Crs = crs;
    }

    public Geometry Geometry { get; }
    public CoordinateReferenceSystem Crs { get; }
    public Envelope BoundingBox => Geometry.EnvelopeInternal;
    public Point Centroid => Geometry.Centroid;
}
