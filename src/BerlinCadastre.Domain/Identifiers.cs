namespace BerlinCadastre.Domain;

public readonly record struct ParcelId
{
    public ParcelId(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Parcel identifier must not be blank.", nameof(value));
        Value = value.Trim();
    }

    public string Value { get; }
    public override string ToString() => Value;
}

public readonly record struct BuildingId
{
    public BuildingId(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Building identifier must not be blank.", nameof(value));
        Value = value.Trim();
    }

    public string Value { get; }
    public override string ToString() => Value;
}

public readonly record struct DistrictId
{
    public DistrictId(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("District identifier must not be blank.", nameof(value));
        Value = value.Trim();
    }

    public string Value { get; }
    public override string ToString() => Value;
}
