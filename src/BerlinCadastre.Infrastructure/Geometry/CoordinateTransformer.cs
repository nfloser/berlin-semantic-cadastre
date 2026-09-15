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
        CoordinateTransformationFactory transformationFactory = new();
        ProjectedCoordinateSystem etrs89Utm33N = CreateEtrs89Utm33N();
        _toWgs84 = transformationFactory.CreateFromCoordinateSystems(etrs89Utm33N, GeographicCoordinateSystem.WGS84).MathTransform;
        _toInternal = transformationFactory.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, etrs89Utm33N).MathTransform;
    }

    public NetTopologySuite.Geometries.Geometry ToWgs84(NetTopologySuite.Geometries.Geometry source) => Transform(source, 25833, 4326, _toWgs84);

    public NetTopologySuite.Geometries.Geometry ToInternal(NetTopologySuite.Geometries.Geometry source) => Transform(source, 4326, 25833, _toInternal);

    private static ProjectedCoordinateSystem CreateEtrs89Utm33N()
    {
        CoordinateSystemFactory coordinateSystemFactory = new();
        GeographicCoordinateSystem etrs89 = coordinateSystemFactory.CreateGeographicCoordinateSystem(
            "ETRS89",
            AngularUnit.Degrees,
            HorizontalDatum.ETRF89,
            PrimeMeridian.Greenwich,
            new AxisInfo("Longitude", AxisOrientationEnum.East),
            new AxisInfo("Latitude", AxisOrientationEnum.North));

        List<ProjectionParameter> parameters =
        [
            new("latitude_of_origin", 0),
            new("central_meridian", 15),
            new("scale_factor", 0.9996),
            new("false_easting", 500000),
            new("false_northing", 0)
        ];

        IProjection projection = coordinateSystemFactory.CreateProjection("UTM zone 33N", "Transverse_Mercator", parameters);
        return coordinateSystemFactory.CreateProjectedCoordinateSystem(
            "ETRS89 / UTM zone 33N",
            etrs89,
            projection,
            LinearUnit.Metre,
            new AxisInfo("Easting", AxisOrientationEnum.East),
            new AxisInfo("Northing", AxisOrientationEnum.North));
    }

    private static NetTopologySuite.Geometries.Geometry Transform(NetTopologySuite.Geometries.Geometry source, int expectedSrid, int targetSrid, MathTransform transform)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.SRID != expectedSrid)
        {
            throw new ArgumentException($"Expected EPSG:{expectedSrid} geometry, got SRID {source.SRID}.", nameof(source));
        }

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
