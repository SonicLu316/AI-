namespace AI錄音文字轉換.Models;

/// <summary>
/// 會議資料模型
/// </summary>
public class Meeting
{
    /// <summary>
    /// 會議ID (主鍵)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 會議標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 會議時間
    /// </summary>
    public DateTimeOffset MeetingTime { get; set; }

    /// <summary>
    /// 會議地點
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// 會議成員 (以逗號分隔)
    /// </summary>
    public string Members { get; set; } = string.Empty;

    /// <summary>
    /// 音訊檔案ID (關聯到 TranscriptionJob)
    /// </summary>
    public Guid? AudioFileId { get; set; }

    /// <summary>
    /// 音訊檔案名稱
    /// </summary>
    public string? AudioFileName { get; set; }

    /// <summary>
    /// 創建時間
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// 是否已刪除 (軟刪除)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// 會議查詢請求
/// </summary>
public class MeetingQueryRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchKeyword { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

/// <summary>
/// 會議新增/修改請求
/// </summary>
public class MeetingRequest
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset MeetingTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Members { get; set; } = string.Empty;
    public Guid? AudioFileId { get; set; }
    public string? AudioFileName { get; set; }
}
