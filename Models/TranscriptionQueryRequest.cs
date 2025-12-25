namespace AI錄音文字轉換.Models;

/// <summary>
/// 查詢轉換工作狀態的請求模型
/// </summary>
public class TranscriptionQueryRequest
{
    /// <summary>
    /// 工作 ID
    /// </summary>
    public Guid Id { get; set; }
}
