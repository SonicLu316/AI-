namespace AI錄音文字轉換.Models;

/// <summary>
/// Redis 設定選項
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// Redis 連線字串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Key 前綴
    /// </summary>
    public string KeyPrefix { get; set; } = "transcription:";

    /// <summary>
    /// 資料過期天數
    /// </summary>
    public int ExpirationDays { get; set; } = 14;
}
