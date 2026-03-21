namespace FinShark.Domain.ValueObjects;

/// <summary>
/// Sector classification for stocks.
/// Implemented as a value object to enforce invariants and avoid invalid values.
/// </summary>
public readonly struct SectorType : IEquatable<SectorType>
{
    private readonly string _value;

    private SectorType(string value)
    {
        _value = value;
    }

    // Alphabetical order

    /// <summary>Basic materials</summary>
    public static SectorType BasicMaterials { get; } = new("Basic Materials");

    /// <summary>Communication services</summary>
    public static SectorType CommunicationServices { get; } = new("Communication Services");

    /// <summary>Consumer cyclical</summary>
    public static SectorType ConsumerCyclical { get; } = new("Consumer Cyclical");

    /// <summary>Consumer defensive</summary>
    public static SectorType ConsumerDefensive { get; } = new("Consumer Defensive");

    /// <summary>Energy, oil, and utilities</summary>
    public static SectorType Energy { get; } = new("Energy");

    /// <summary>Consumer discretionary and entertainment</summary>
    public static SectorType Entertainment { get; } = new("Entertainment");

    /// <summary>Financial services and banking</summary>
    public static SectorType Finance { get; } = new("Finance");

    /// <summary>Financial services (FMP naming)</summary>
    public static SectorType FinancialServices { get; } = new("Financial Services");

    /// <summary>Healthcare and pharmaceutical companies</summary>
    public static SectorType Healthcare { get; } = new("Healthcare");

    /// <summary>Industrial companies</summary>
    public static SectorType Industrials { get; } = new("Industrials");

    /// <summary>Manufacturing and industrial</summary>
    public static SectorType Manufacturing { get; } = new("Manufacturing");

    /// <summary>Other or unclassified industry</summary>
    public static SectorType Other { get; } = new("Other");

    /// <summary>Real estate and property</summary>
    public static SectorType RealEstate { get; } = new("Real Estate");

    /// <summary>Retail and consumer goods</summary>
    public static SectorType Retail { get; } = new("Retail");

    /// <summary>Technology and software companies</summary>
    public static SectorType Technology { get; } = new("Technology");

    /// <summary>Telecommunications</summary>
    public static SectorType Telecommunications { get; } = new("Telecommunications");

    /// <summary>Transportation and logistics</summary>
    public static SectorType Transportation { get; } = new("Transportation");

    private static readonly IReadOnlyDictionary<string, SectorType> ByName =
        new Dictionary<string, SectorType>(StringComparer.OrdinalIgnoreCase)
        {
            ["Basic Materials"] = BasicMaterials,
            ["Communication Services"] = CommunicationServices,
            ["Consumer Cyclical"] = ConsumerCyclical,
            ["Consumer Defensive"] = ConsumerDefensive,
            ["Energy"] = Energy,
            ["Entertainment"] = Entertainment,
            ["Finance"] = Finance,
            ["Financial Services"] = FinancialServices,
            ["Healthcare"] = Healthcare,
            ["Industrials"] = Industrials,
            ["Manufacturing"] = Manufacturing,
            ["Other"] = Other,
            ["Real Estate"] = RealEstate,
            ["Retail"] = Retail,
            ["Technology"] = Technology,
            ["Telecommunications"] = Telecommunications,
            ["Transportation"] = Transportation
        };

    private static readonly IReadOnlyDictionary<int, SectorType> ByCode =
        new Dictionary<int, SectorType>
        {
            [1] = BasicMaterials,
            [2] = CommunicationServices,
            [3] = ConsumerCyclical,
            [4] = ConsumerDefensive,
            [5] = Energy,
            [6] = Entertainment,
            [7] = Finance,
            [8] = FinancialServices,
            [9] = Healthcare,
            [10] = Industrials,
            [11] = Manufacturing,
            [12] = RealEstate,
            [13] = Retail,
            [14] = Technology,
            [15] = Telecommunications,
            [16] = Transportation,
            [17] = Other
        };

    /// <summary>
    /// Canonical string value (e.g., "Technology").
    /// Defaults to "Other" for the default struct value.
    /// </summary>
    public string Value => _value ?? Other._value;

    /// <summary>
    /// Returns all supported Sector types in alphabetical order.
    /// </summary>
    public static IReadOnlyList<SectorType> All { get; } = ByName
        .Values
        .Distinct()
        .OrderBy(x => x.Value)
        .ToArray();

    /// <summary>
    /// Try to parse from a string value (case-insensitive).
    /// </summary>
    public static bool TryFrom(string? value, out SectorType sector)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            sector = default;
            return false;
        }

        var normalized = value.Trim();

        if (ByName.TryGetValue(normalized, out sector))
            return true;

        // Remove spaces fallback
        normalized = normalized.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);

        var match = ByName.Keys.FirstOrDefault(k =>
            k.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase)
             .Equals(normalized, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            sector = ByName[match];
            return true;
        }

        // Special handling
        if (normalized.Contains("realestate", StringComparison.OrdinalIgnoreCase))
        {
            sector = RealEstate;
            return true;
        }

        sector = default;
        return false;
    }

    public static SectorType From(string value)
    {
        if (!TryFrom(value, out var sector))
            throw new ArgumentException($"Unknown sector type '{value}'.", nameof(value));

        return sector;
    }

    public static bool TryFrom(int code, out SectorType sector) =>
        ByCode.TryGetValue(code, out sector);

    public static SectorType From(int code)
    {
        if (!TryFrom(code, out var sector))
            throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown sector type code.");

        return sector;
    }

    public override string ToString() => Value;

    public bool Equals(SectorType other) =>
        StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);

    public override bool Equals(object? obj) =>
        obj is SectorType other && Equals(other);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public static bool operator ==(SectorType left, SectorType right) => left.Equals(right);

    public static bool operator !=(SectorType left, SectorType right) => !left.Equals(right);
}
