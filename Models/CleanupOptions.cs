namespace AI錄音文字轉換.Models;

/// <summary>
/// 清理排程設定選項
/// </summary>
public class CleanupOptions
{
    /// <summary>
    /// 每日執行時間（小時，0-23）
    /// </summary>
    public int ExecuteAtHour { get; set; } = 12;

    /// <summary>
    /// 每日執行時間（分鐘，0-59）
    /// </summary>
    public int ExecuteAtMinute { get; set; } = 0;

    /// <summary>
    /// 檔案保留天數
    /// </summary>
    public int RetentionDays { get; set; } = 14;

    /// <summary>
    /// 是否在啟動時立即執行一次
    /// </summary>
    public bool ExecuteOnStartup { get; set; } = false;
}
