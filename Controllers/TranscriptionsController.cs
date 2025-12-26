using AI錄音文字轉換.Models;
using AI錄音文字轉換.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AI錄音文字轉換.Controllers;

/// <summary>
/// 音訊轉文字控制器
/// 提供音訊檔案上傳、轉換狀態查詢和結果下載的 API 端點
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TranscriptionsController : ControllerBase
{
    /// <summary>
    /// 轉換工作佇列服務
    /// </summary>
    private readonly ITranscriptionQueue _queue;
    
    /// <summary>
    /// 轉換工作儲存服務
    /// </summary>
    private readonly TranscriptionStore _store;
    
    /// <summary>
    /// Buzz 設定選項
    /// </summary>
    private readonly BuzzOptions _options;
    
    /// <summary>
    /// 日誌記錄器
    /// </summary>
    private readonly ILogger<TranscriptionsController> _logger;

    /// <summary>
    /// 建構子 - 初始化轉換控制器所需的相依服務
    /// </summary>
    /// <param name="queue">轉換工作佇列服務</param>
    /// <param name="store">轉換工作儲存服務</param>
    /// <param name="options">Buzz 設定選項</param>
    /// <param name="logger">日誌記錄器</param>
    public TranscriptionsController(
        ITranscriptionQueue queue,
        TranscriptionStore store,
        IOptions<BuzzOptions> options,
        ILogger<TranscriptionsController> logger)
    {
        _queue = queue;
        _store = store;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 新增音訊檔案並開始轉換排程
    /// </summary>
    /// <param name="file">上傳的音訊或影片檔案</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>回傳工作 ID、檔案名稱和狀態</returns>
    [HttpPost("audioAdd")]
    [RequestSizeLimit(200_000_000)] // ~200MB. Adjust in config if needed.
    public async Task<IActionResult> AudioAddAsync([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var uploadPath = Path.IsPathRooted(_options.UploadPath)
            ? _options.UploadPath
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _options.UploadPath));

        Directory.CreateDirectory(uploadPath);

        var extension = Path.GetExtension(file.FileName);
        var shortId = Guid.NewGuid().ToString("N").Substring(0, 10);
        var storedPath = Path.Combine(uploadPath, shortId + extension);

        await using (var stream = System.IO.File.Create(storedPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var job = new TranscriptionJob
        {
            Id = shortId,
            OriginalFileName = file.FileName,
            StoredFilePath = storedPath,
            Status = TranscriptionJobStatus.Pending
        };

        _store.Upsert(job);
        await _queue.EnqueueAsync(job, cancellationToken);

        return Ok(new
        {
            jobId = job.Id,
            fileName = job.OriginalFileName,
            status = job.Status.ToString()
        });
    }

    /// <summary>
    /// 查詢轉換工作的狀態
    /// </summary>
    /// <param name="request">包含工作 ID 的請求物件</param>
    /// <returns>回傳工作狀態、檔案資訊和輸出檔案清單</returns>
    [HttpPost("transcriptionQry")]
    public IActionResult TranscriptionQry([FromBody] TranscriptionQueryRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Id))
        {
            return BadRequest("Invalid job ID.");
        }

        if (!_store.TryGet(request.Id, out var job) || job is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            job.Id,
            job.OriginalFileName,
            job.Status,
            job.ErrorMessage,
            job.OutputFiles,
            job.SummaryPath,
            job.CreatedAt,
            job.CompletedAt
        });
    }

    /// <summary>
    /// 查詢並下載轉換完成的文字檔案或摘要
    /// </summary>
    /// <param name="request">包含工作 ID、檔案鍵值和是否為摘要的請求物件</param>
    /// <returns>回傳檔案串流供下載</returns>
    [HttpPost("transcriptionFileQry")]
    public IActionResult TranscriptionFileQry([FromBody] TranscriptionDownloadRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Id))
        {
            return BadRequest("Invalid job ID.");
        }

        if (!_store.TryGet(request.Id, out var job) || job is null)
        {
            return NotFound();
        }

        if (job.Status != TranscriptionJobStatus.Completed)
        {
            return BadRequest("Job has not completed yet.");
        }

        string? targetPath = null;

        if (request.Summary)
        {
            targetPath = job.SummaryPath;
        }
        else if (!string.IsNullOrEmpty(request.Key) && job.OutputFiles.TryGetValue(request.Key, out var path))
        {
            targetPath = path;
        }
        else if (job.OutputFiles.Any())
        {
            // Default to first available if no key specified
            targetPath = job.OutputFiles.Values.First();
        }

        if (string.IsNullOrWhiteSpace(targetPath) || !System.IO.File.Exists(targetPath))
        {
            return NotFound("File not found.");
        }

        var downloadName = Path.GetFileName(targetPath);
        return PhysicalFile(targetPath, "application/octet-stream", downloadName);
    }
}
