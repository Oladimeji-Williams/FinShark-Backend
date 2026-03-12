namespace FinShark.Domain.ValueObjects;

/// <summary>
/// Industry classification for stocks.
/// Implemented as a value object to enforce invariants and avoid invalid values.
/// </summary>
public readonly struct IndustryType : IEquatable<IndustryType>
{
    private readonly string _value;

    private IndustryType(string value)
    {
        _value = value;
    }

    /// <summary>Technology and software companies</summary>
    public static IndustryType Technology { get; } = new("Technology");

    /// <summary>Financial services and banking</summary>
    public static IndustryType Finance { get; } = new("Finance");

    /// <summary>Healthcare and pharmaceutical companies</summary>
    public static IndustryType Healthcare { get; } = new("Healthcare");

    /// <summary>Energy, oil, and utilities</summary>
    public static IndustryType Energy { get; } = new("Energy");

    /// <summary>Retail and consumer goods</summary>
    public static IndustryType Retail { get; } = new("Retail");

    /// <summary>Manufacturing and industrial</summary>
    public static IndustryType Manufacturing { get; } = new("Manufacturing");

    /// <summary>Real estate and property</summary>
    public static IndustryType RealEstate { get; } = new("RealEstate");

    /// <summary>Telecommunications</summary>
    public static IndustryType Telecommunications { get; } = new("Telecommunications");

    /// <summary>Transportation and logistics</summary>
    public static IndustryType Transportation { get; } = new("Transportation");

    /// <summary>Consumer discretionary and entertainment</summary>
    public static IndustryType Entertainment { get; } = new("Entertainment");

    /// <summary>Other or unclassified industry</summary>
    public static IndustryType Other { get; } = new("Other");

    private static readonly IReadOnlyDictionary<string, IndustryType> ByName =
        new Dictionary<string, IndustryType>(StringComparer.OrdinalIgnoreCase)
        {
            ["Technology"] = Technology,
            ["Finance"] = Finance,
            ["Healthcare"] = Healthcare,
            ["Energy"] = Energy,
            ["Retail"] = Retail,
            ["Manufacturing"] = Manufacturing,
            ["RealEstate"] = RealEstate,
            ["Telecommunications"] = Telecommunications,
            ["Transportation"] = Transportation,
            ["Entertainment"] = Entertainment,
            ["Other"] = Other
        };

    private static readonly IReadOnlyDictionary<int, IndustryType> ByCode =
        new Dictionary<int, IndustryType>
        {
            [1] = Technology,
            [2] = Finance,
            [3] = Healthcare,
            [4] = Energy,
            [5] = Retail,
            [6] = Manufacturing,
            [7] = RealEstate,
            [8] = Telecommunications,
            [9] = Transportation,
            [10] = Entertainment,
            [11] = Other
        };

    /// <summary>
    /// Canonical string value (e.g., "Technology").
    /// Defaults to "Other" for the default struct value.
    /// </summary>
    public string Value => _value ?? Other._value;

    /// <summary>
    /// Returns all supported industry types in code order.
    /// </summary>
    public static IReadOnlyList<IndustryType> All { get; } = ByCode
        .OrderBy(kvp => kvp.Key)
        .Select(kvp => kvp.Value)
        .ToArray();

    /// <summary>
    /// Try to parse from a string value (case-insensitive).
    /// </summary>
    public static bool TryFrom(string? value, out IndustryType industry)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            industry = default;
            return false;
        }

        return ByName.TryGetValue(value.Trim(), out industry);
    }

    /// <summary>
    /// Parse from a string value (case-insensitive).
    /// </summary>
    public static IndustryType From(string value)
    {
        if (!TryFrom(value, out var industry))
        {
            throw new ArgumentException($"Unknown industry type '{value}'.", nameof(value));
        }

        return industry;
    }

    /// <summary>
    /// Try to parse from the legacy numeric code.
    /// </summary>
    public static bool TryFrom(int code, out IndustryType industry)
    {
        return ByCode.TryGetValue(code, out industry);
    }

    /// <summary>
    /// Parse from the legacy numeric code.
    /// </summary>
    public static IndustryType From(int code)
    {
        if (!TryFrom(code, out var industry))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown industry type code.");
        }

        return industry;
    }

    public override string ToString() => Value;

    public bool Equals(IndustryType other) =>
        StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);

    public override bool Equals(object? obj) =>
        obj is IndustryType other && Equals(other);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public static bool operator ==(IndustryType left, IndustryType right) => left.Equals(right);

    public static bool operator !=(IndustryType left, IndustryType right) => !left.Equals(right);
}
