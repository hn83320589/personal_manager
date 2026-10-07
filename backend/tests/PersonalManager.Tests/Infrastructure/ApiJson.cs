using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersonalManager.Tests.Infrastructure;

/// <summary>讀寫 API 的 JSON（camelCase、enum 以字串表示），並解開 ApiResponse 外層。</summary>
public static class ApiJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<T> ReadDataAsync<T>(this HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<Envelope<T>>(Options)
                   ?? throw new InvalidOperationException("回應內容是空的");
        return body.Data!;
    }

    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PostAsJsonAsync(url, body, Options);

    public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PutAsJsonAsync(url, body, Options);

    /// <summary>JSON 物件的 id 欄位。</summary>
    public static int Id(this JsonElement item) => item.GetProperty("id").GetInt32();

    private sealed record Envelope<T>(bool Success, string Message, T? Data, List<string> Errors);
}

/// <summary>分頁回應（對應 API 的 PagedResult）。</summary>
public sealed record Paged<T>(List<T> Items, int TotalCount, int Page, int PageSize);
