namespace PersonalManager.Api.Models;

// 公開頁面的呈現設定（ADR-012），由使用者在後台選擇。以字串儲存，新增選項不影響既有資料。

/// <summary>作品集模式：決定建議的作品欄位、技能清單與預設卡片版型。</summary>
public enum PortfolioMode { Designer, Frontend, Backend }

/// <summary>作品卡片版型：圖像型、資訊型、技術型。</summary>
public enum CardStyle { Visual, Info, Tech }

/// <summary>圖像型卡片的圖片比例。</summary>
public enum CardRatio { Portrait, Square, Landscape }

/// <summary>技能在公開頁面的顯示方式。</summary>
public enum SkillDisplay { NameOnly, Level, Years }
