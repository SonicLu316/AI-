namespace AI¿ý­µ¤å¦rÂà´«.Models;

public enum TranscriptionJobStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

public class TranscriptionJob
{
    /// <summary>
    /// 10-char short ID (from GUID)
    /// </summary>
    public string Id { get; init; } = string.Empty;

    public string OriginalFileName { get; init; } = string.Empty;

    public string StoredFilePath { get; init; } = string.Empty;

    public string? ProcessingFilePath { get; set; }

    public Dictionary<string, string> OutputFiles { get; set; } = new();

    public string? SummaryPath { get; set; }

    public TranscriptionJobStatus Status { get; set; } = TranscriptionJobStatus.Pending;

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }
}
