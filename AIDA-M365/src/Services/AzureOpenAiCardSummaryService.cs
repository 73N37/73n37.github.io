using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AIDA.M365.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIDA.M365.Services;

public sealed class AzureOpenAiCardSummaryService : IAiCardSummaryService
{
    private readonly HttpClient _httpClient;
    private readonly AzureOpenAiOptions _options;
    private readonly ILogger<AzureOpenAiCardSummaryService> _logger;

    public AzureOpenAiCardSummaryService(
        HttpClient httpClient,
        IOptions<AzureOpenAiOptions> options,
        ILogger<AzureOpenAiCardSummaryService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CardSummaryResult> SummarizeAsync(
        KanbanEventCard card,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(card.BodyContent))
        {
            return CardSummaryResult.Failure("This event has no body content to summarize.");
        }

        if (string.IsNullOrWhiteSpace(_options.Endpoint) || string.IsNullOrWhiteSpace(_options.DeploymentName))
        {
            return CardSummaryResult.Failure("Azure OpenAI is not configured.");
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                BuildChatCompletionsUri());

            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                request.Headers.Add("api-key", _options.ApiKey);
            }

            request.Content = JsonContent.Create(
                new ChatCompletionRequest(
                    [
                        new ChatMessage(
                            "system",
                            "Extract a concise executive summary and action items from an Outlook event body. Return plain text with a 'Summary:' line and an 'Action Items:' section."),
                        new ChatMessage(
                            "user",
                            $"Subject: {card.Subject}\n\nBody:\n{card.BodyContent}")
                    ],
                    Temperature: 0.2,
                    MaxTokens: 600));

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
                cancellationToken: cancellationToken);

            var content = payload?.Choices.FirstOrDefault()?.Message.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                return CardSummaryResult.Failure("Azure OpenAI returned an empty summary.");
            }

            return CardSummaryResult.Success(
                ExtractSummary(content),
                ExtractActionItems(content));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to summarize event {EventId}", card.GraphEventId);
            return CardSummaryResult.Failure("Smart Summarize failed.");
        }
    }

    private Uri BuildChatCompletionsUri()
    {
        var endpoint = _options.Endpoint.TrimEnd('/');
        var deployment = Uri.EscapeDataString(_options.DeploymentName);
        return new Uri($"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={_options.ApiVersion}");
    }

    private static string ExtractSummary(string content)
    {
        var summaryLine = content
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(line => line.StartsWith("Summary:", StringComparison.OrdinalIgnoreCase));

        return summaryLine is null
            ? content.Trim()
            : summaryLine["Summary:".Length..].Trim();
    }

    private static IReadOnlyList<string> ExtractActionItems(string content) =>
        content
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("-", StringComparison.Ordinal) || line.StartsWith("*", StringComparison.Ordinal))
            .Select(line => line.TrimStart('-', '*', ' '))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

    private sealed record ChatCompletionRequest(
        IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("max_tokens")] int MaxTokens);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record ChatCompletionResponse(
        [property: JsonPropertyName("choices")] IReadOnlyList<ChatChoice> Choices);

    private sealed record ChatChoice(
        [property: JsonPropertyName("message")] ChatMessage Message);
}
