using AI錄音文字轉換.Models;

namespace AI錄音文字轉換.Services;

public interface ITextSummarizer
{
    Task<string> SummarizeAsync(string content, CancellationToken cancellationToken);
}

/// <summary>
/// Placeholder summarizer. Replace with Gemini/ChatGPT integration when credentials are available.
/// </summary>
public class DefaultTextSummarizer : ITextSummarizer
{
    public Task<string> SummarizeAsync(string content, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Task.FromResult(string.Empty);
        }

        // Very naive summary: keep first 1,000 characters.
        var normalized = content.Replace("\r\n", "\n");
        var summary = normalized.Length <= 1000 ? normalized : normalized[..1000] + "...";
        return Task.FromResult(summary);
    }
}
