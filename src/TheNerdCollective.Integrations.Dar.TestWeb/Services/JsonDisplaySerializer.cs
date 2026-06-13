using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.TestWeb.Services;

internal static class JsonDisplaySerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    internal static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options);
}
