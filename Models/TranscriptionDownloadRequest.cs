namespace AI錄音文字轉換.Models;

/// <summary>
/// 下載轉換檔案的請求模型
/// </summary>
public class TranscriptionDownloadRequest
{
    /// <summary>
    /// 工作 ID（10 碼短 ID）
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 檔案鍵值（用於選擇特定輸出檔案）
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 是否下載摘要檔案
    /// </summary>
    public bool Summary { get; set; }
}
