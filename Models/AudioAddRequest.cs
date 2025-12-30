namespace AI錄音文字轉換.Models;

/// <summary>
/// 新增音訊檔案的請求模型
/// </summary>
public class AudioAddRequest
{
    /// <summary>
    /// 工作 ID（由外部傳入，若未提供則自動產生）
    /// </summary>
    public string? JobId { get; set; }
}
