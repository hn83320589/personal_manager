using System.ComponentModel.DataAnnotations;

namespace PersonalManager.Api.Models;

public enum FileKind { Image, Pdf, Word, PowerPoint, Excel, Archive }

/// <summary>使用者上傳的檔案。類型與 MIME 由伺服器依檔案內容判定，不採用用戶端送來的值。</summary>
public class FileUpload : IOwnedByUser
{
    public int Id { get; set; }
    public int UserId { get; set; }

    /// <summary>原始檔名（只用於顯示與下載時的名稱）。</summary>
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>實際儲存的名稱：GUID + 副檔名，與使用者輸入無關，不會有路徑穿越。</summary>
    [StringLength(100)]
    public string StoredName { get; set; } = string.Empty;

    [StringLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    public FileKind Kind { get; set; }

    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    /// <summary>圖片的寬高，前台用來在載入前保留正確比例的版面；非圖片或讀不到時為 null。</summary>
    public int? Width { get; set; }
    public int? Height { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
