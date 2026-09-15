using BerlinCadastre.Domain;

namespace BerlinCadastre.Application;

public interface ICadastreRepository
{
    IReadOnlyList<CadastralParcel> Parcels { get; }
    IReadOnlyList<Building> Buildings { get; }
    IReadOnlyList<AdministrativeDistrict> Districts { get; }

    CadastralParcel? FindParcel(ParcelId id);
    Building? FindBuilding(BuildingId id);
    AdministrativeDistrict? FindDistrict(DistrictId id);
    void ReplaceAll(IEnumerable<CadastralParcel> parcels, IEnumerable<Building> buildings, IEnumerable<AdministrativeDistrict> districts);
}
