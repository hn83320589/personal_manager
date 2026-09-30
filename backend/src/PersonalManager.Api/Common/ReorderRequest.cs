using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Common;

/// <summary>一次送出完整的排序：<see cref="Ids"/> 必須剛好是使用者自己的全部項目。</summary>
public sealed record ReorderRequest([Required] IReadOnlyList<int> Ids);
