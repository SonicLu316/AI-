using System.Diagnostics;
using System.Text;
using System.Linq;
using AI錄音文字轉換.Models;
using Microsoft.Extensions.Options;

namespace AI錄音文字轉換.Services;

public class TranscriptionWorker : BackgroundService
{
    private readonly ILogger<TranscriptionWorker> _logger;
    private readonly ITranscriptionQueue _queue;
    private readonly TranscriptionStore _store;
    private readonly BuzzOptions _options;

    public TranscriptionWorker(
        ILogger<TranscriptionWorker> logger,
        ITranscriptionQueue queue,
        TranscriptionStore store,
        IOptions<BuzzOptions> options)
    {
        _logger = logger;
        _queue = queue;
        _store = store;
        _options = options.Value;
    }

    private string ResolvePath(string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
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
            var storedFilePath = ResolvePath(job.StoredFilePath);
            var processingPath = ResolvePath(Path.Combine(_options.BuzzProcessingPath, Path.GetFileName(job.StoredFilePath)));
            var outputDir = ResolvePath(_options.BuzzOutputPath);
            job.ProcessingFilePath = processingPath;

            // Move instead of copy from uploads to processing
            if (File.Exists(processingPath)) File.Delete(processingPath);
            File.Move(storedFilePath, processingPath);

            var profiles = _options.Profiles.Any() ? _options.Profiles : new List<BuzzProfile> { new BuzzProfile() };
            var outputPaths = new List<string>();
            var outputFilesMap = new Dictionary<string, string>();

            foreach (var profile in profiles)
            {
                _logger.LogInformation("Running profile {ProfileName} (ModelType={ModelType}) for job {JobId}", profile.Name, profile.ModelType, job.Id);
                var arguments = BuildArguments(processingPath, profile);
                var workingDir = Path.GetDirectoryName(processingPath) ?? outputDir;
                var startInfo = new ProcessStartInfo
                {
                    FileName = _options.BuzzExecutablePath,
                    Arguments = arguments,
                    WorkingDirectory = workingDir,
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
                }
                else
                {
                    var collected = CollectOutputFiles(workingDir, outputDir, profile.ModelType, job.Id, processingPath);
                    foreach (var kv in collected)
                    {
                        outputFilesMap[kv.Key] = kv.Value;
                        outputPaths.Add(kv.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        _logger.LogInformation("Buzz output ({ProfileName}): {Stdout}", profile.Name, stdout.Trim());
                    }
                }
            }

            // Remove processed audio file after transcription completes
            if (File.Exists(processingPath))
            {
                try { File.Delete(processingPath); } catch { /* ignore cleanup errors */ }
            }

            if (outputPaths.Count == 0)
            {
                _store.SetStatus(job.Id, TranscriptionJobStatus.Failed, "No output files generated from any profile.");
                return;
            }

            _store.SetStatus(job.Id, TranscriptionJobStatus.Completed, outputFiles: outputFilesMap, summaryPath: null);
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

        if (profile.WordTimestamps)
        {
            sb.Append("--word-timestamps ");
        }

        if (!string.IsNullOrWhiteSpace(profile.ExtraArgs))
        {
            sb.Append(profile.ExtraArgs.Trim());
            sb.Append(' ');
        }

        sb.Append('"').Append(inputPath).Append('"');
        return sb.ToString();
    }

    private Dictionary<string, string> CollectOutputFiles(string workingDir, string outputDir, string modelType, string jobId, string processingPath)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt", ".srt", ".vtt" };
        var processingBase = Path.GetFileNameWithoutExtension(processingPath) ?? string.Empty;

        if (!Directory.Exists(workingDir)) return result;

        foreach (var file in Directory.EnumerateFiles(workingDir))
        {
            var ext = Path.GetExtension(file);
            if (!allowedExt.Contains(ext)) continue;

            // avoid moving the input itself
            if (Path.GetFullPath(file).Equals(Path.GetFullPath(processingPath), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var baseName = Path.GetFileNameWithoutExtension(file) ?? string.Empty;
            if (!baseName.StartsWith(processingBase, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Rename to {jobId}_{modelType}.ext
            var destName = $"{jobId}_{modelType}{ext}";
            var destPath = Path.Combine(outputDir, destName);
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            if (File.Exists(destPath)) File.Delete(destPath);
            File.Move(file, destPath);

            result[$"{modelType}{ext}"] = destPath;
        }
        return result;
    }

    private void EnsureDirectories()
    {
        Directory.CreateDirectory(ResolvePath(_options.UploadPath));
        Directory.CreateDirectory(ResolvePath(_options.BuzzProcessingPath));
        Directory.CreateDirectory(ResolvePath(_options.BuzzOutputPath));
    }
}
