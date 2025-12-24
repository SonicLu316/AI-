using System.ComponentModel.DataAnnotations;

namespace AI¿ý­µ¤å¦rÂà´«.Models;

public class BuzzOptions
{
    [Required]
    public string UploadPath { get; set; } = Path.Combine("App_Data", "uploads");

    [Required]
    public string BuzzProcessingPath { get; set; } = Path.Combine("App_Data", "processing");

    [Required]
    public string BuzzOutputPath { get; set; } = Path.Combine("App_Data", "output");

    /// <summary>
    /// The buzz CLI executable path. Default assumes "buzz" is on PATH.
    /// </summary>
    [Required]
    public string BuzzExecutablePath { get; set; } = "buzz";

    public List<BuzzProfile> Profiles { get; set; } = new();
}

public class BuzzProfile
{
    public string Name { get; set; } = "Default";

    /// <summary>
    /// Buzz task to perform. Allowed: translate, transcribe. Default: transcribe.
    /// </summary>
    public string Task { get; set; } = "transcribe";

    /// <summary>
    /// Model type. Allowed: whisper, whispercpp, huggingface, fasterwhisper, openaiapi. Default: whisper.
    /// </summary>
    public string ModelType { get; set; } = "whisper";

    /// <summary>
    /// Model size when applicable. Allowed: tiny, base, small, medium, large. Default: tiny.
    /// </summary>
    public string ModelSize { get; set; } = "tiny";

    /// <summary>
    /// Language code. Leave empty to auto-detect.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Additional buzz CLI arguments appended as-is.
    /// </summary>
    public string? ExtraArgs { get; set; }

    public bool OutputTxt { get; set; } = true;

    public bool OutputSrt { get; set; }

    public bool OutputVtt { get; set; }
}
