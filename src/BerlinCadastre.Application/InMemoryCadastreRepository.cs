using BerlinCadastre.Domain;

namespace BerlinCadastre.Application;

public sealed class InMemoryCadastreRepository : ICadastreRepository
{
    private readonly object _gate = new();
    private IReadOnlyList<CadastralParcel> _parcels;
    private IReadOnlyList<Building> _buildings;
    private IReadOnlyList<AdministrativeDistrict> _districts;

    public InMemoryCadastreRepository(
        IEnumerable<CadastralParcel>? parcels = null,
        IEnumerable<Building>? buildings = null,
        IEnumerable<AdministrativeDistrict>? districts = null)
    {
        _parcels = [.. parcels ?? []];
        _buildings = [.. buildings ?? []];
        _districts = [.. districts ?? []];
    }

    public IReadOnlyList<CadastralParcel> Parcels { get { lock (_gate) return _parcels; } }
    public IReadOnlyList<Building> Buildings { get { lock (_gate) return _buildings; } }
    public IReadOnlyList<AdministrativeDistrict> Districts { get { lock (_gate) return _districts; } }

    public CadastralParcel? FindParcel(ParcelId id) => Parcels.FirstOrDefault(x => x.Id == id);
    public Building? FindBuilding(BuildingId id) => Buildings.FirstOrDefault(x => x.Id == id);
    public AdministrativeDistrict? FindDistrict(DistrictId id) => Districts.FirstOrDefault(x => x.Id == id);

    public void ReplaceAll(IEnumerable<CadastralParcel> parcels, IEnumerable<Building> buildings, IEnumerable<AdministrativeDistrict> districts)
    {
        ArgumentNullException.ThrowIfNull(parcels);
        ArgumentNullException.ThrowIfNull(buildings);
        ArgumentNullException.ThrowIfNull(districts);
        lock (_gate)
        {
            _parcels = [.. parcels];
            _buildings = [.. buildings];
            _districts = [.. districts];
        }
    }
}
