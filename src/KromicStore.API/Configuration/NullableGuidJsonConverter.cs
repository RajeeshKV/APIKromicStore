using System.Text.Json;
using System.Text.Json.Serialization;

namespace KromicStore.API.Configuration;

/// <summary>
/// Handles the common frontend pattern of sending "" for an empty optional GUID field.
/// Without this, System.Text.Json throws a deserialization error for Guid? when value is "".
/// With this, "" and null both deserialize to null; a valid GUID string deserializes normally.
/// </summary>
public sealed class NullableGuidJsonConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();

            // Empty string → treat as null
            if (string.IsNullOrWhiteSpace(str))
                return null;

            if (Guid.TryParse(str, out var guid))
                return guid;

            throw new JsonException($"Cannot convert \"{str}\" to Guid.");
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing Guid?.");
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value);
        else
            writer.WriteNullValue();
    }
}
