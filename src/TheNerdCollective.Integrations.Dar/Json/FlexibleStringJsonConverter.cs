using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Json
{
    internal sealed class FlexibleStringJsonConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var integer))
                    {
                        return integer.ToString(CultureInfo.InvariantCulture);
                    }

                    return reader.GetDecimal().ToString(CultureInfo.InvariantCulture);
                case JsonTokenType.True:
                    return "true";
                case JsonTokenType.False:
                    return "false";
                default:
                    throw new JsonException($"Kan ikke konvertere {reader.TokenType} til string.");
            }
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value);
        }
    }

    internal sealed class FlexibleStringJsonConverterNonNullable : JsonConverter<string>
    {
        private readonly FlexibleStringJsonConverter _inner = new FlexibleStringJsonConverter();

        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return _inner.Read(ref reader, typeToConvert, options)
                ?? throw new JsonException("Forventede en string-værdi.");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
            _inner.Write(writer, value, options);
    }
}
