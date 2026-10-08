using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace plantilla_aspnet_core_api.Infrastructure;

public sealed class SupabaseRestClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<JsonDocument> GetAsync(string resource, string query, CancellationToken ct)
    {
        using var response = await httpClient.GetAsync($"{resource}?{query}", ct);
        await EnsureSuccessAsync(response, ct);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
    }

    public async Task PostAsync(string resource, object payload, CancellationToken ct, string? prefer = null)
    {
        using var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
        if (prefer is not null)
            content.Headers.Add("Prefer", prefer);
        using var response = await httpClient.PostAsync(resource, content, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task PatchAsync(string resource, string query, object payload, CancellationToken ct)
    {
        using var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Patch, $"{resource}?{query}") { Content = content };
        request.Headers.Add("Prefer", "return=minimal");
        using var response = await httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException($"Supabase REST devolvió {(int)response.StatusCode}: {body}");
    }
}
