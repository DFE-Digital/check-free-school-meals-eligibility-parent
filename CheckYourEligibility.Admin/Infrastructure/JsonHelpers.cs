namespace CheckYourEligibility.Admin.Infrastructure;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Internal helper methods for serializing and deserializing JSON encoded data
/// from DfE Sign-in API's.
/// </summary>
/// <remarks>
/// <para>This is achieved by providing consistent JSON serialization options.</para>
/// </remarks>
internal static class JsonHelpers
{
    private static JsonSerializerOptions s_jsonSerializerOptions = null!;

    public static JsonSerializerOptions JsonSerializerOptions
    {
        get
        {
            if (s_jsonSerializerOptions == null)
            {
                s_jsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                s_jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }
            return s_jsonSerializerOptions;
        }
    }

    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonSerializerOptions);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
    }
}
