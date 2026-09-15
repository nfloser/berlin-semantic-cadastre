using BerlinCadastre.Domain;
using NetTopologySuite.Geometries;

namespace BerlinCadastre.Application;

public sealed class SpatialQueryService
{
    private readonly ICadastreRepository _repository;

    public SpatialQueryService(ICadastreRepository repository) => _repository = repository;

    public IReadOnlyList<Building> FindBuildingsInsideParcel(ParcelId parcelId)
    {
        CadastralParcel parcel = _repository.FindParcel(parcelId) ?? throw new KeyNotFoundException($"Parcel '{parcelId}' was not found.");
        return [.. _repository.Buildings.Where(building =>
            parcel.Geometry.Geometry.EnvelopeInternal.Intersects(building.Geometry.Geometry.EnvelopeInternal) &&
            parcel.Geometry.Geometry.Covers(building.Geometry.Geometry))];
    }

    public IReadOnlyList<CadastralParcel> FindParcelsContainingPoint(Point point)
    {
        RequireInternalCrs(point);
        return [.. _repository.Parcels.Where(parcel =>
            parcel.Geometry.Geometry.EnvelopeInternal.Contains(point.Coordinate) && parcel.Geometry.Geometry.Covers(point))];
    }

    public IReadOnlyList<CadastralParcel> FindParcelsIntersecting(Geometry analysisGeometry)
    {
        RequireInternalCrs(analysisGeometry);
        return [.. _repository.Parcels.Where(parcel =>
            parcel.Geometry.Geometry.EnvelopeInternal.Intersects(analysisGeometry.EnvelopeInternal) &&
            parcel.Geometry.Geometry.Intersects(analysisGeometry))];
    }

    public IReadOnlyList<Building> FindBuildingsIntersecting(Geometry analysisGeometry)
    {
        RequireInternalCrs(analysisGeometry);
        return [.. _repository.Buildings.Where(building =>
            building.Geometry.Geometry.EnvelopeInternal.Intersects(analysisGeometry.EnvelopeInternal) &&
            building.Geometry.Geometry.Intersects(analysisGeometry))];
    }

    public IReadOnlyList<Building> FindBuildingsWithinDistance(Point point, double distanceMetres)
    {
        RequireInternalCrs(point);
        if (distanceMetres < 0) throw new ArgumentOutOfRangeException(nameof(distanceMetres));
        return [.. _repository.Buildings.Where(building => building.Geometry.Geometry.Distance(point) <= distanceMetres)];
    }

    public IReadOnlyList<Building> FindBuildingsInDistrict(DistrictId districtId)
    {
        AdministrativeDistrict district = _repository.FindDistrict(districtId) ?? throw new KeyNotFoundException($"District '{districtId}' was not found.");
        return [.. _repository.Buildings.Where(building =>
            building.DistrictId == districtId || district.Geometry.Geometry.Covers(building.Geometry.Geometry.PointOnSurface))];
    }

    public IReadOnlyList<CadastralParcel> FindParcelsWithMoreThanBuildings(int minimumBuildingCount)
    {
        if (minimumBuildingCount < 0) throw new ArgumentOutOfRangeException(nameof(minimumBuildingCount));
        return [.. _repository.Parcels.Where(parcel =>
            _repository.Buildings.Count(building => building.ParcelId == parcel.Id ||
                (parcel.Geometry.Geometry.EnvelopeInternal.Intersects(building.Geometry.Geometry.EnvelopeInternal) &&
                 parcel.Geometry.Geometry.Covers(building.Geometry.Geometry.PointOnSurface))) > minimumBuildingCount)];
    }

    private static void RequireInternalCrs(Geometry geometry)
    {
        if (geometry.SRID != CoordinateReferenceSystem.Etrs89Utm33N.Epsg)
            throw new ArgumentException("Spatial calculations require EPSG:25833 geometry.", nameof(geometry));
    }
}
