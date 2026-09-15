using BerlinCadastre.Domain;

namespace BerlinCadastre.Application;

public sealed class SpatialRelationshipDeriver
{
    public (IReadOnlyList<CadastralParcel> Parcels, IReadOnlyList<Building> Buildings) Derive(
        IEnumerable<CadastralParcel> parcelSource,
        IEnumerable<Building> buildingSource,
        IReadOnlyList<AdministrativeDistrict> districts)
    {
        CadastralParcel[] parcels = [.. parcelSource];
        Building[] buildings = [.. buildingSource];

        CadastralParcel[] enrichedParcels = [.. parcels.Select(parcel => parcel with
        {
            DistrictId = FindDistrict(parcel.Geometry.Geometry.PointOnSurface, districts)?.Id
        })];

        Building[] enrichedBuildings = [.. buildings.Select(building =>
        {
            CadastralParcel? parcel = enrichedParcels.FirstOrDefault(candidate =>
                candidate.Geometry.Geometry.EnvelopeInternal.Intersects(building.Geometry.Geometry.EnvelopeInternal) &&
                candidate.Geometry.Geometry.Covers(building.Geometry.Geometry.PointOnSurface));
            AdministrativeDistrict? district = parcel?.DistrictId is DistrictId districtId
                ? districts.FirstOrDefault(candidate => candidate.Id == districtId)
                : FindDistrict(building.Geometry.Geometry.PointOnSurface, districts);

            return building with { ParcelId = parcel?.Id, DistrictId = district?.Id };
        })];

        return (enrichedParcels, enrichedBuildings);
    }

    private static AdministrativeDistrict? FindDistrict(NetTopologySuite.Geometries.Point point, IReadOnlyList<AdministrativeDistrict> districts) =>
        districts.FirstOrDefault(district =>
            district.Geometry.Geometry.EnvelopeInternal.Contains(point.Coordinate) && district.Geometry.Geometry.Covers(point));
}
