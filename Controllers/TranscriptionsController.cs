using AI錄音文字轉換.Models;
using AI錄音文字轉換.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AI錄音文字轉換.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranscriptionsController : ControllerBase
{
    private readonly ITranscriptionQueue _queue;
    private readonly TranscriptionStore _store;
    private readonly BuzzOptions _options;
    private readonly ILogger<TranscriptionsController> _logger;

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

    [HttpPost("upload")]
    [RequestSizeLimit(200_000_000)] // ~200MB. Adjust in config if needed.
    public async Task<IActionResult> UploadAsync([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        Directory.CreateDirectory(_options.UploadPath);

        var jobId = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var storedPath = Path.Combine(_options.UploadPath, jobId + extension);

        await using (var stream = System.IO.File.Create(storedPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var job = new TranscriptionJob
        {
            Id = jobId,
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

    [HttpGet("{id}")]
    public IActionResult GetStatus(Guid id)
    {
        if (!_store.TryGet(id, out var job) || job is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            job.Id,
            job.OriginalFileName,
            job.Status,
            job.ErrorMessage,
            job.OutputFiles, // Return dictionary
            job.SummaryPath,
            job.CreatedAt,
            job.CompletedAt
        });
    }

    [HttpGet("{id}/download")]
    public IActionResult Download(Guid id, [FromQuery] string? key = null, [FromQuery] bool summary = false)
    {
        if (!_store.TryGet(id, out var job) || job is null)
        {
            return NotFound();
        }

        if (job.Status != TranscriptionJobStatus.Completed)
        {
            return BadRequest("Job has not completed yet.");
        }

        string? targetPath = null;

        if (summary)
        {
            targetPath = job.SummaryPath;
        }
        else if (!string.IsNullOrEmpty(key) && job.OutputFiles.TryGetValue(key, out var path))
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
