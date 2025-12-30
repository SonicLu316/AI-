using AI錄音文字轉換.Models;
using Microsoft.Extensions.Options;

namespace AI錄音文字轉換.Services;

/// <summary>
/// 背景清理服務 - 每天固定時間清理過期的音訊檔案和輸出檔案
/// </summary>
public class CleanupWorker : BackgroundService
{
    private readonly ILogger<CleanupWorker> _logger;
    private readonly CleanupOptions _cleanupOptions;
    private readonly BuzzOptions _buzzOptions;

    public CleanupWorker(
        ILogger<CleanupWorker> logger,
        IOptions<CleanupOptions> cleanupOptions,
        IOptions<BuzzOptions> buzzOptions)
    {
        _logger = logger;
        _cleanupOptions = cleanupOptions.Value;
        _buzzOptions = buzzOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("清理服務已啟動，每天 {Hour:D2}:{Minute:D2} 執行，保留天數: {Days} 天",
            _cleanupOptions.ExecuteAtHour, _cleanupOptions.ExecuteAtMinute, _cleanupOptions.RetentionDays);

        // 是否在啟動時立即執行
        if (_cleanupOptions.ExecuteOnStartup)
        {
            await PerformCleanupAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = CalculateDelayUntilNextRun();
                _logger.LogDebug("下次執行時間: {NextRun}, 等待 {Delay}", 
                    DateTime.Now.Add(delay), delay);

                await Task.Delay(delay, stoppingToken);
                await PerformCleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // 正常關閉
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理作業發生錯誤");
                // 發生錯誤時等待 1 小時後重試
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("清理服務已停止");
    }

    /// <summary>
    /// 計算距離下次執行的延遲時間
    /// </summary>
    private TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTime.Now;
        var scheduledTime = new DateTime(
            now.Year, 
            now.Month, 
            now.Day, 
            _cleanupOptions.ExecuteAtHour, 
            _cleanupOptions.ExecuteAtMinute, 
            0);

        // 如果今天的執行時間已過，則排程到明天
        if (scheduledTime <= now)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }

        var delay = scheduledTime - now;
        return delay;
    }

    /// <summary>
    /// 執行清理作業
    /// </summary>
    private async Task PerformCleanupAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("開始執行清理作業...");

        var cutoffDate = DateTime.Now.AddDays(-_cleanupOptions.RetentionDays);
        var totalDeleted = 0;
        var totalSize = 0L;

        // 清理目錄列表
        var directories = new[]
        {
            _buzzOptions.UploadPath,
            _buzzOptions.BuzzProcessingPath,
            _buzzOptions.BuzzOutputPath
        };

        foreach (var relativePath in directories)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var fullPath = Path.IsPathRooted(relativePath)
                ? relativePath
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));

            if (!Directory.Exists(fullPath))
            {
                _logger.LogDebug("目錄不存在，跳過: {Path}", fullPath);
                continue;
            }

            var (deleted, size) = await CleanupDirectoryAsync(fullPath, cutoffDate, stoppingToken);
            totalDeleted += deleted;
            totalSize += size;
        }

        _logger.LogInformation("清理作業完成，共刪除 {Count} 個檔案，釋放 {Size:F2} MB",
            totalDeleted, totalSize / 1024.0 / 1024.0);
    }

    /// <summary>
    /// 清理指定目錄中的過期檔案
    /// </summary>
    private Task<(int deleted, long size)> CleanupDirectoryAsync(
        string directoryPath, 
        DateTime cutoffDate, 
        CancellationToken stoppingToken)
    {
        var deleted = 0;
        var size = 0L;

        try
        {
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    var fileInfo = new FileInfo(file);
                    
                    // 檢查檔案最後修改時間
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        size += fileInfo.Length;
                        fileInfo.Delete();
                        deleted++;
                        
                        _logger.LogDebug("已刪除過期檔案: {File}", file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "刪除檔案失敗: {File}", file);
                }
            }

            // 清理空的子目錄
            CleanupEmptyDirectories(directoryPath);

            _logger.LogInformation("目錄 {Path} 清理完成，刪除 {Count} 個檔案", 
                directoryPath, deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理目錄失敗: {Path}", directoryPath);
        }

        return Task.FromResult((deleted, size));
    }

    /// <summary>
    /// 清理空的子目錄
    /// </summary>
    private void CleanupEmptyDirectories(string directoryPath)
    {
        try
        {
            foreach (var subDir in Directory.GetDirectories(directoryPath))
            {
                CleanupEmptyDirectories(subDir);

                if (!Directory.EnumerateFileSystemEntries(subDir).Any())
                {
                    Directory.Delete(subDir);
                    _logger.LogDebug("已刪除空目錄: {Dir}", subDir);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "清理空目錄失敗: {Path}", directoryPath);
        }
    }
}
