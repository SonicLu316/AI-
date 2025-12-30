using System.Collections.Concurrent;
using System.Text.Json;
using AI錄音文字轉換.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AI錄音文字轉換.Services;

public class TranscriptionStore
{
    private readonly ConcurrentDictionary<string, TranscriptionJob> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly RedisOptions _options;
    private readonly ILogger<TranscriptionStore> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public TranscriptionStore(
        IConnectionMultiplexer redis,
        IOptions<RedisOptions> options,
        ILogger<TranscriptionStore> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 取得 Redis Key
    /// </summary>
    private string GetKey(string id) => $"{_options.KeyPrefix}{id}";

    /// <summary>
    /// 新增或更新工作
    /// </summary>
    public TranscriptionJob Upsert(TranscriptionJob job)
    {
        try
        {
            var key = GetKey(job.Id);
            var json = JsonSerializer.Serialize(job, _jsonOptions);

            // 儲存到 Redis，使用設定檔中的過期天數
            _db.StringSet(key, json, TimeSpan.FromDays(_options.ExpirationDays));

            // 同時更新本地快取
            _cache.AddOrUpdate(job.Id, job, (_, _) => job);

            _logger.LogDebug("已儲存工作 {JobId} 至 Redis，過期時間 {Days} 天", job.Id, _options.ExpirationDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存工作 {JobId} 至 Redis 失敗", job.Id);
            // 即使 Redis 失敗，仍保留在本地快取
            _cache.AddOrUpdate(job.Id, job, (_, _) => job);
        }

        return job;
    }

    /// <summary>
    /// 嘗試取得工作
    /// </summary>
    public bool TryGet(string id, out TranscriptionJob? job)
    {
        // 先從本地快取查詢
        if (_cache.TryGetValue(id, out job))
        {
            return true;
        }

        // 快取沒有則從 Redis 查詢
        try
        {
            var key = GetKey(id);
            var json = _db.StringGet(key);
            
            if (json.HasValue)
            {
                job = JsonSerializer.Deserialize<TranscriptionJob>(json.ToString(), _jsonOptions);
                
                if (job != null)
                {
                    // 加入本地快取
                    _cache.TryAdd(id, job);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "從 Redis 取得工作 {JobId} 失敗", id);
        }

        job = null;
        return false;
    }

    /// <summary>
    /// 設定工作狀態
    /// </summary>
    public void SetStatus(string id, TranscriptionJobStatus status, string? error = null,
        Dictionary<string, string>? outputFiles = null, string? summaryPath = null)
    {
        if (!TryGet(id, out var job) || job is null)
        {
            _logger.LogWarning("找不到工作 {JobId}，無法更新狀態", id);
            return;
        }

        job.Status = status;
        job.ErrorMessage = error;

        if (outputFiles != null)
        {
            foreach (var kvp in outputFiles)
            {
                job.OutputFiles[kvp.Key] = kvp.Value;
            }
        }

        if (!string.IsNullOrWhiteSpace(summaryPath))
        {
            job.SummaryPath = summaryPath;
        }

        if (status is TranscriptionJobStatus.Completed or TranscriptionJobStatus.Failed)
        {
            job.CompletedAt = DateTimeOffset.UtcNow;
        }

        // 更新至 Redis
        Upsert(job);
    }
}
