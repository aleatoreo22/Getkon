using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Getkon;

internal static class Util
{
    /// <summary>
    /// Converts a JsonElement to its corresponding C# type representation.
    /// </summary>
    /// <param name="element">The JsonElement to convert.</param>
    /// <returns>
    /// - For strings: returns a string
    /// - For numbers: returns long or double
    /// - For booleans: returns bool
    /// - For null/undefined: returns null
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the number type is unknown or when encountering an unsupported JsonValueKind.
    /// </exception>
    internal static object ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt64(out long l))
                    return l;
                else if (element.TryGetDouble(out double d))
                    return d;
                else
                    throw new InvalidOperationException("Unknow number.");
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                throw new InvalidOperationException($"Unknow type: {element.ValueKind}");
        }
    }

    public static bool FrontendIsRunning()
    {
        try
        {
            var response = new HttpClient().GetAsync("http://localhost:5173/").Result;
            return response.StatusCode != System.Net.HttpStatusCode.NotFound;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
