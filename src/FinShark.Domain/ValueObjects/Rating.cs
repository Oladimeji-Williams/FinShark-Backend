using System.Globalization;

namespace FinShark.Domain.ValueObjects;

/// <summary>
/// Rating value object (1-5).
/// </summary>
public readonly struct Rating : IEquatable<Rating>
{
    private readonly byte _value;
    private readonly bool _isSet;

    private Rating(byte value)
    {
        _value = value;
        _isSet = true;
    }

    public bool IsSet => _isSet;

    public bool IsValid => _isSet && _value is >= 1 and <= 5;

    public int Value => _isSet
        ? _value
        : throw new InvalidOperationException("Rating is not set.");

    public static Rating From(int value)
    {
        if (value < 1 || value > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Rating must be between 1 and 5.");
        }

        return new Rating((byte)value);
    }

    public static bool TryFrom(int value, out Rating rating)
    {
        if (value is >= 1 and <= 5)
        {
            rating = new Rating((byte)value);
            return true;
        }

        rating = default;
        return false;
    }

    public static bool TryFrom(string? value, out Rating rating)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            rating = default;
            return false;
        }

        if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return TryFrom(parsed, out rating);
        }

        rating = default;
        return false;
    }

    public override string ToString() => _isSet ? _value.ToString(CultureInfo.InvariantCulture) : "Unspecified";

    public bool Equals(Rating other) => _isSet == other._isSet && _value == other._value;

    public override bool Equals(object? obj) => obj is Rating other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_isSet, _value);

    public static bool operator ==(Rating left, Rating right) => left.Equals(right);

    public static bool operator !=(Rating left, Rating right) => !left.Equals(right);
}
