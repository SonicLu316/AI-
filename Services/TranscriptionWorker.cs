using System.Diagnostics;
using System.Text;
using AI錄音文字轉換.Models;
using Microsoft.Extensions.Options;

namespace AI錄音文字轉換.Services;

public class TranscriptionWorker : BackgroundService
{
    private readonly ILogger<TranscriptionWorker> _logger;
    private readonly ITranscriptionQueue _queue;
    private readonly TranscriptionStore _store;
    private readonly BuzzOptions _options;
    private readonly ITextSummarizer _summarizer;

    public TranscriptionWorker(
        ILogger<TranscriptionWorker> logger,
        ITranscriptionQueue queue,
        TranscriptionStore store,
        IOptions<BuzzOptions> options,
        ITextSummarizer summarizer)
    {
        _logger = logger;
        _queue = queue;
        _store = store;
        _options = options.Value;
        _summarizer = summarizer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureDirectories();
        _logger.LogInformation("Buzz transcription worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _queue.DequeueAsync(stoppingToken);
                await ProcessJobAsync(job, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing transcription queue");
            }
        }
    }

    private async Task ProcessJobAsync(TranscriptionJob job, CancellationToken cancellationToken)
    {
        _store.SetStatus(job.Id, TranscriptionJobStatus.Processing);
        _logger.LogInformation("Processing job {JobId} for file {File}", job.Id, job.OriginalFileName);

        try
        {
            var processingPath = Path.Combine(_options.BuzzProcessingPath, Path.GetFileName(job.StoredFilePath));
            job.ProcessingFilePath = processingPath;

            File.Copy(job.StoredFilePath, processingPath, overwrite: true);

            var outputDir = _options.BuzzOutputPath;
            var profiles = _options.Profiles.Any() ? _options.Profiles : new List<BuzzProfile> { new BuzzProfile() };
            var outputPaths = new List<string>();
            var outputFilesMap = new Dictionary<string, string>();

            foreach (var profile in profiles)
            {
                _logger.LogInformation("Running profile {ProfileName} for job {JobId}", profile.Name, job.Id);
                var arguments = BuildArguments(processingPath, profile);
                var startInfo = new ProcessStartInfo
                {
                    FileName = _options.BuzzExecutablePath,
                    Arguments = arguments,
                    WorkingDirectory = outputDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(startInfo);
                if (process is null)
                {
                    throw new InvalidOperationException($"Failed to start buzz process for profile {profile.Name}");
                }

                var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken);
                var stdout = await stdoutTask;
                var stderr = await stderrTask;

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Buzz profile {ProfileName} exited with code {ExitCode}. stderr: {Error}", profile.Name, process.ExitCode, stderr);
                    // Continue to next profile or fail? Let's log and continue, but mark job as failed if all fail?
                    // For now, if any fails, we log error but try to proceed.
                }
                else
                {
                    // Find any output file (txt, srt, vtt)
                    var outputs = FindOutputFiles(processingPath, outputDir);
                    foreach (var outPath in outputs)
                    {
                        var ext = Path.GetExtension(outPath);
                        var newPath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(outPath)}_{profile.Name}{ext}");
                        if (File.Exists(newPath)) File.Delete(newPath);
                        File.Move(outPath, newPath);
                        
                        // Key: ProfileName_Extension (e.g. Transcribe_srt)
                        outputFilesMap[$"{profile.Name}{ext}"] = newPath;
                        outputPaths.Add(newPath);
                    }
                }
            }

            if (outputPaths.Count == 0)
            {
                _store.SetStatus(job.Id, TranscriptionJobStatus.Failed, "No output files generated from any profile.");
                return;
            }

            // Summarize logic (pick first txt or srt)
            var summarySource = outputPaths.FirstOrDefault(p => p.EndsWith(".txt")) ?? outputPaths.FirstOrDefault(p => p.EndsWith(".srt"));
            string summary = "";
            if (summarySource != null)
            {
                var content = await File.ReadAllTextAsync(summarySource, cancellationToken);
                summary = await _summarizer.SummarizeAsync(content, cancellationToken);
            }
            
            var summaryPath = "";
            if (!string.IsNullOrEmpty(summary))
            {
                summaryPath = Path.Combine(outputDir, $"{job.Id}_summary.txt");
                await File.WriteAllTextAsync(summaryPath, summary, cancellationToken);
            }

            _store.SetStatus(job.Id, TranscriptionJobStatus.Completed, outputFiles: outputFilesMap, summaryPath: summaryPath);
            _logger.LogInformation("Job {JobId} completed. Outputs: {Outputs}", job.Id, string.Join(", ", outputFilesMap.Keys));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed", job.Id);
            _store.SetStatus(job.Id, TranscriptionJobStatus.Failed, ex.Message);
        }
    }

    private string BuildArguments(string inputPath, BuzzProfile profile)
    {
        var sb = new StringBuilder();
        sb.Append("add ");

        if (!string.IsNullOrWhiteSpace(profile.Task))
        {
            sb.Append($"--task {profile.Task} ");
        }

        if (!string.IsNullOrWhiteSpace(profile.ModelType))
        {
            sb.Append($"--model-type {profile.ModelType} ");
        }

        if (!string.IsNullOrWhiteSpace(profile.ModelSize))
        {
            sb.Append($"--model-size {profile.ModelSize} ");
        }

        if (!string.IsNullOrWhiteSpace(profile.Language))
        {
            sb.Append($"--language {profile.Language} ");
        }

        if (profile.OutputTxt)
        {
            sb.Append("--txt ");
        }

        if (profile.OutputSrt)
        {
            sb.Append("--srt ");
        }

        if (profile.OutputVtt)
        {
            sb.Append("--vtt ");
        }

        if (!string.IsNullOrWhiteSpace(profile.ExtraArgs))
        {
            sb.Append(profile.ExtraArgs.Trim());
            sb.Append(' ');
        }

        sb.Append('"').Append(inputPath).Append('"');
        return sb.ToString();
    }

    private List<string> FindOutputFiles(string processingPath, string outputDir)
    {
        var baseName = Path.GetFileNameWithoutExtension(processingPath);
        var extensions = new[] { ".txt", ".srt", ".vtt" };
        var found = new List<string>();

        foreach (var ext in extensions)
        {
            var candidates = new List<string>
            {
                Path.Combine(outputDir, baseName + ext),
                Path.Combine(Path.GetDirectoryName(processingPath) ?? outputDir, baseName + ext)
            };
            
            foreach (var c in candidates)
            {
                if (File.Exists(c)) found.Add(c);
            }
        }
        return found;
    }

    private void EnsureDirectories()
    {
        Directory.CreateDirectory(_options.UploadPath);
        Directory.CreateDirectory(_options.BuzzProcessingPath);
        Directory.CreateDirectory(_options.BuzzOutputPath);
    }
}
