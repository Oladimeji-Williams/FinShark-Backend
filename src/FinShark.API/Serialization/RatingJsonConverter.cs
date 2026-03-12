using System.Globalization;
using Newtonsoft.Json;
using FinShark.Domain.ValueObjects;

namespace FinShark.API.Serialization;

/// <summary>
/// JSON converter for Rating value object (Newtonsoft.Json).
/// Accepts numeric values or numeric strings.
/// </summary>
public sealed class RatingJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(Rating) || objectType == typeof(Rating?);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            if (IsNullableType(objectType))
            {
                return null;
            }

            throw new JsonSerializationException("Rating is required.");
        }

        if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
        {
            if (reader.Value is not null)
            {
                var value = Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture);
                if (Rating.TryFrom(value, out var rating))
                {
                    return rating;
                }
            }

            throw new JsonSerializationException("Invalid rating value.");
        }

        if (reader.TokenType == JsonToken.String)
        {
            var raw = reader.Value?.ToString();
            if (Rating.TryFrom(raw, out var rating))
            {
                return rating;
            }

            throw new JsonSerializationException($"Invalid rating value '{raw}'.");
        }

        throw new JsonSerializationException("Invalid token for rating.");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var rating = (Rating)value;
        writer.WriteValue(rating.Value);
    }

    private static bool IsNullableType(Type objectType) =>
        Nullable.GetUnderlyingType(objectType) != null;
}
