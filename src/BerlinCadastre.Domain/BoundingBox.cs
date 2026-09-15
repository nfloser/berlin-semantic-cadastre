using NetTopologySuite.Geometries;

namespace BerlinCadastre.Domain;

public sealed record BoundingBox
{
    public BoundingBox(double minX, double minY, double maxX, double maxY, CoordinateReferenceSystem crs)
    {
        if (minX >= maxX) throw new ArgumentException("minX must be lower than maxX.");
        if (minY >= maxY) throw new ArgumentException("minY must be lower than maxY.");
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
        Crs = crs ?? throw new ArgumentNullException(nameof(crs));
    }

    public double MinX { get; }
    public double MinY { get; }
    public double MaxX { get; }
    public double MaxY { get; }
    public CoordinateReferenceSystem Crs { get; }
    public Envelope Envelope => new(MinX, MaxX, MinY, MaxY);
    public string ToWfsBbox() => FormattableString.Invariant($"{MinX},{MinY},{MaxX},{MaxY},EPSG:{Crs.Epsg}");
}
