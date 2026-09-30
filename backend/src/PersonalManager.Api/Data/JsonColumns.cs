using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PersonalManager.Api.Data;

/// <summary>
/// 以 JSON 文字儲存值物件清單。使用一般字串欄位而非資料庫專屬的 JSON 型別，
/// SQLite 與 MySQL 都能直接支援。
/// </summary>
public static class JsonColumns
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        AllowOutOfOrderMetadataProperties = true
    };

    public static PropertyBuilder<List<T>> HasJsonConversion<T>(this PropertyBuilder<List<T>> property) =>
        property.HasConversion(
            value => JsonSerializer.Serialize(value, Options),
            json => JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>(),
            // 以序列化結果比較內容，EF 才能偵測到清單內的變更
            new ValueComparer<List<T>>(
                (a, b) => JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options),
                value => JsonSerializer.Serialize(value, Options).GetHashCode(),
                value => JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(value, Options), Options)!));
}
