using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Json
{
    internal static class DarJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            options.Converters.Add(new FlexibleStringJsonConverter());
            options.Converters.Add(new FlexibleStringJsonConverterNonNullable());
            options.Converters.Add(new FlexibleNullableInt32JsonConverter());
            options.Converters.Add(new FlexibleNullableInt64JsonConverter());
            options.Converters.Add(new FlexibleNullableDecimalJsonConverter());

            return options;
        }

        public static T DeserializeRequired<T>(JsonElement element) where T : class =>
            element.Deserialize<T>(Options)
            ?? throw new InvalidOperationException($"Kunne ikke deserialisere {typeof(T).Name}.");

        public static T? DeserializeOptional<T>(JsonElement element) where T : class
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            return element.Deserialize<T>(Options);
        }

        public static IReadOnlyList<T> DeserializeList<T>(JsonElement element) where T : class
        {
            if (element.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<T>();
            }

            var items = new List<T>();
            foreach (var item in element.EnumerateArray())
            {
                var dto = item.Deserialize<T>(Options);
                if (dto != null)
                {
                    items.Add(dto);
                }
            }

            return items;
        }
    }
}
