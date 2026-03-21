using Newtonsoft.Json;
using FinShark.Domain.ValueObjects;

namespace FinShark.API.Serialization;

/// <summary>
/// JSON converter for SectorType value object (Newtonsoft.Json).
/// Accepts string names (e.g., "Technology") and legacy numeric codes.
/// </summary>
public sealed class SectorTypeJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(SectorType) || objectType == typeof(SectorType?);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            if (IsNullableType(objectType))
            {
                return null;
            }

            throw new JsonSerializationException("Sector type is required.");
        }

        if (reader.TokenType == JsonToken.String)
        {
            var raw = reader.Value?.ToString();
            if (SectorType.TryFrom(raw, out var industry))
            {
                return industry;
            }

            throw new JsonSerializationException($"Invalid sector type '{raw}'.");
        }

        if (reader.TokenType == JsonToken.Integer)
        {
            if (reader.Value is long longValue)
            {
                var code = Convert.ToInt32(longValue);
                if (SectorType.TryFrom(code, out var industry))
                {
                    return industry;
                }
            }

            throw new JsonSerializationException("Invalid sector type code.");
        }

        throw new JsonSerializationException("Invalid token for sector type.");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var industry = (SectorType)value;
        writer.WriteValue(industry.Value);
    }

    private static bool IsNullableType(Type objectType) =>
        Nullable.GetUnderlyingType(objectType) != null;
}
