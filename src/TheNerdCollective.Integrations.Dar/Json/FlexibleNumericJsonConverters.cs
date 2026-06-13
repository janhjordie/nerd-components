using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Json
{
    internal sealed class FlexibleNullableInt32JsonConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.Number when reader.TryGetInt32(out var value):
                    return value;
                case JsonTokenType.Number:
                    return Convert.ToInt32(reader.GetDecimal());
                case JsonTokenType.String when string.IsNullOrWhiteSpace(reader.GetString()):
                    return null;
                case JsonTokenType.String:
                    return ParseInt32(reader.GetString());
                default:
                    throw new JsonException($"Kan ikke konvertere {reader.TokenType} til int?.");
            }
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteNumberValue(value.Value);
        }

        private static int? ParseInt32(string? value)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
            {
                return Convert.ToInt32(decimalValue);
            }

            return null;
        }
    }

    internal sealed class FlexibleNullableInt64JsonConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.Number when reader.TryGetInt64(out var value):
                    return value;
                case JsonTokenType.Number:
                    return Convert.ToInt64(reader.GetDecimal());
                case JsonTokenType.String when string.IsNullOrWhiteSpace(reader.GetString()):
                    return null;
                case JsonTokenType.String when long.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed):
                    return parsed;
                case JsonTokenType.String when decimal.TryParse(reader.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue):
                    return Convert.ToInt64(decimalValue);
                default:
                    throw new JsonException($"Kan ikke konvertere {reader.TokenType} til long?.");
            }
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteNumberValue(value.Value);
        }
    }

    internal sealed class FlexibleNullableDecimalJsonConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.Number:
                    return reader.GetDecimal();
                case JsonTokenType.String when string.IsNullOrWhiteSpace(reader.GetString()):
                    return null;
                case JsonTokenType.String when decimal.TryParse(reader.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed):
                    return parsed;
                default:
                    throw new JsonException($"Kan ikke konvertere {reader.TokenType} til decimal?.");
            }
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteNumberValue(value.Value);
        }
    }
}
