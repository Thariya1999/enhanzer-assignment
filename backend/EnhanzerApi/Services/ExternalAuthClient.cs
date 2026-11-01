using System.Text;
using System.Text.Json;
using EnhanzerApi.Models;
using Microsoft.Extensions.Options;

namespace EnhanzerApi.Services;

public interface IExternalAuthClient
{
    Task<ExternalLoginResult> LoginAsync(string email, string password, CancellationToken ct = default);
}

/// <summary>Calls the Enhanzer staging POS API with the GetLoginData action.</summary>
public class ExternalAuthClient(HttpClient http, IOptions<ExternalApiOptions> options, ILogger<ExternalAuthClient> logger)
    : IExternalAuthClient
{
    private readonly ExternalApiOptions _opt = options.Value;

    public async Task<ExternalLoginResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        // Exact property names required by the external API (do not use camelCase here).
        var payload = new
        {
            API_Action = "GetLoginData",
            Device_Id = _opt.DeviceId,
            Sync_Time = "",
            Company_Code = email,
            API_Body = new { Username = email, Pw = password }
        };

        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await http.PostAsync(_opt.Url, content, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("External login API returned {Status}", (int)response.StatusCode);
            return new ExternalLoginResult(false, "Invalid email or password.", Array.Empty<LocationDto>());
        }

        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch (JsonException)
        {
            logger.LogWarning("External login API returned a non-JSON response");
            return new ExternalLoginResult(false, "Unexpected response from the authentication service.", Array.Empty<LocationDto>());
        }

        using (doc)
        {
            var locationsEl = FindProperty(doc.RootElement, "User_Locations");
            if (locationsEl is { ValueKind: JsonValueKind.Array } arr)
            {
                var list = new List<LocationDto>();
                foreach (var item in arr.EnumerateArray())
                {
                    var code = ReadString(item, "Location_Code");
                    var name = ReadString(item, "Location_Name");
                    if (!string.IsNullOrWhiteSpace(code))
                        list.Add(new LocationDto(code!, name ?? code!));
                }
                return new ExternalLoginResult(true, null, list);
            }

            var message = FindProperty(doc.RootElement, "Message") ?? FindProperty(doc.RootElement, "Error");
            var text = message is { ValueKind: JsonValueKind.String } m ? m.GetString() : null;
            return new ExternalLoginResult(false, string.IsNullOrWhiteSpace(text) ? "Invalid email or password." : text, Array.Empty<LocationDto>());
        }
    }

    private static string? ReadString(JsonElement obj, string name)
    {
        if (obj.ValueKind != JsonValueKind.Object) return null;
        foreach (var p in obj.EnumerateObject())
        {
            if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                return p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() : p.Value.ToString();
        }
        return null;
    }

    /// <summary>Case-insensitive recursive search; also looks inside JSON that is embedded as a string.</summary>
    private static JsonElement? FindProperty(JsonElement el, string name, int depth = 0)
    {
        if (depth > 6) return null;

        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var p in el.EnumerateObject())
                    if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                        return p.Value;
                foreach (var p in el.EnumerateObject())
                {
                    var found = FindProperty(p.Value, name, depth + 1);
                    if (found.HasValue) return found;
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in el.EnumerateArray())
                {
                    var found = FindProperty(item, name, depth + 1);
                    if (found.HasValue) return found;
                }
                break;

            case JsonValueKind.String:
                var s = el.GetString()?.Trim();
                if (s is { Length: > 1 } && (s[0] == '{' || s[0] == '['))
                {
                    try
                    {
                        using var inner = JsonDocument.Parse(s);
                        var found = FindProperty(inner.RootElement, name, depth + 1);
                        if (found.HasValue) return found.Value.Clone();
                    }
                    catch (JsonException) { /* not JSON, ignore */ }
                }
                break;
        }
        return null;
    }
}
