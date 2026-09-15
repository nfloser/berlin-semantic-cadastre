using BerlinCadastre.Domain;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace BerlinCadastre.Infrastructure.Geometry;

public sealed class CoordinateTransformer
{
    private readonly MathTransform _toWgs84;
    private readonly MathTransform _toInternal;

    public CoordinateTransformer()
    {
        CoordinateTransformationFactory factory = new();
        ProjectedCoordinateSystem utm33 = ProjectedCoordinateSystem.WGS84_UTM(33, true);
        _toWgs84 = factory.CreateFromCoordinateSystems(utm33, GeographicCoordinateSystem.WGS84).MathTransform;
        _toInternal = factory.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, utm33).MathTransform;
    }

    public NetTopologySuite.Geometries.Geometry ToWgs84(NetTopologySuite.Geometries.Geometry source) => Transform(source, 25833, 4326, _toWgs84);
    public NetTopologySuite.Geometries.Geometry ToInternal(NetTopologySuite.Geometries.Geometry source) => Transform(source, 4326, 25833, _toInternal);

    private static NetTopologySuite.Geometries.Geometry Transform(NetTopologySuite.Geometries.Geometry source, int expectedSrid, int targetSrid, MathTransform transform)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.SRID != expectedSrid) throw new ArgumentException($"Expected EPSG:{expectedSrid} geometry, got SRID {source.SRID}.", nameof(source));
        NetTopologySuite.Geometries.Geometry copy = (NetTopologySuite.Geometries.Geometry)source.Copy();
        copy.Apply(new TransformSequenceFilter(transform));
        copy.SRID = targetSrid;
        copy.GeometryChanged();
        return copy;
    }

    private sealed class TransformSequenceFilter(MathTransform transform) : ICoordinateSequenceFilter
    {
        public bool Done => false;
        public bool GeometryChanged => true;

        public void Filter(CoordinateSequence sequence, int i)
        {
            double[] transformed = transform.Transform([sequence.GetX(i), sequence.GetY(i)]);
            sequence.SetX(i, transformed[0]);
            sequence.SetY(i, transformed[1]);
        }
    }
}
