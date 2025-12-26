using System.Collections.Concurrent;
using AI錄音文字轉換.Models;

namespace AI錄音文字轉換.Services;

public class TranscriptionStore
{
    private readonly ConcurrentDictionary<string, TranscriptionJob> _jobs = new(StringComparer.OrdinalIgnoreCase);

    public TranscriptionJob Upsert(TranscriptionJob job)
    {
        _jobs.AddOrUpdate(job.Id, job, (_, _) => job);
        return job;
    }

    public bool TryGet(string id, out TranscriptionJob? job)
    {
        var found = _jobs.TryGetValue(id, out var existing);
        job = existing;
        return found;
    }

    public void SetStatus(string id, TranscriptionJobStatus status, string? error = null, Dictionary<string, string>? outputFiles = null, string? summaryPath = null)
    {
        if (_jobs.TryGetValue(id, out var job))
        {
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
        }
    }
}
