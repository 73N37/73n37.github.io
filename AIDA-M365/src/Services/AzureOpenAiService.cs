using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;

namespace AIDA.M365.Services;

/// <summary>
/// Azure OpenAI gpt-4o-mini service for smart event card summarization.
/// Falls back to rule-based summaries when the API is unavailable or not configured.
/// </summary>
public sealed class AzureOpenAiService : IAzureOpenAiService
{
    private readonly IntegrationStateContainer _state;
    private readonly ILogger<AzureOpenAiService> _logger;

    public AzureOpenAiService(
        IntegrationStateContainer state,
        ILogger<AzureOpenAiService> logger)
    {
        _state = state;
        _logger = logger;
    }

    public async Task<EventCardSummaryResult> SummarizeCardAsync(
        GodsEventCard card,
        CancellationToken cancellationToken = default)
    {
        if (card is null) throw new ArgumentNullException(nameof(card));

        // Fall back gracefully if OpenAI is not configured
        if (string.IsNullOrWhiteSpace(_state.AzureOpenAiEndpoint) ||
            string.IsNullOrWhiteSpace(_state.AzureOpenAiKey))
        {
            _logger.LogInformation("[OpenAI] Not configured — using rule-based fallback.");
            return BuildFallbackSummary(card);
        }

        try
        {
            var client = new OpenAIClient(
                new Uri(_state.AzureOpenAiEndpoint),
                new AzureKeyCredential(_state.AzureOpenAiKey));

            var deployment = string.IsNullOrWhiteSpace(_state.AzureOpenAiDeployment)
                ? "gpt-4o-mini"
                : _state.AzureOpenAiDeployment;

            var systemPrompt = """
                Du er en professionel events-koordinator-assistent for Engestofte Gods.
                Giv et kort, struktureret resumé af selskabsbegivenheden på dansk.
                Returnér præcist i dette format (ingen markdown-formattering):
                RESUMÉ: [én sætning om begivenheden]
                OPGAVE 1: [konkret handlingspunkt]
                OPGAVE 2: [konkret handlingspunkt]
                OPGAVE 3: [konkret handlingspunkt]
                """;

            var userPrompt = $"""
                Selskab: {card.Subject}
                Type: {card.EventSubtype ?? "Ukendt"}
                Lokation: {card.EstateArea ?? "Ikke angivet"}
                Gæster: {card.GuestCount}
                Forplejning: {card.CateringOption ?? "Ikke angivet"}
                Koordinator: {card.AssignedCoordinator ?? "Ikke tildelt"}
                Dato: {card.StartUtc.ToLocalTime():dd. MMM yyyy, HH:mm}
                """;

            var options = new ChatCompletionsOptions
            {
                DeploymentName = deployment,
                MaxTokens = 300
            };
            options.Messages.Add(new ChatRequestSystemMessage(systemPrompt));
            options.Messages.Add(new ChatRequestUserMessage(userPrompt));

            var response = await client.GetChatCompletionsAsync(options, cancellationToken);
            var raw = response.Value.Choices[0].Message.Content;
            return ParseAiResponse(raw);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[OpenAI] API call failed — using rule-based fallback.");
            return BuildFallbackSummary(card);
        }
    }

    private static EventCardSummaryResult ParseAiResponse(string raw)
    {
        var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string summary = "AI Resumé genereret.";
        var actions = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("RESUMÉ:", StringComparison.OrdinalIgnoreCase))
                summary = trimmed["RESUMÉ:".Length..].Trim();
            else if (trimmed.StartsWith("OPGAVE", StringComparison.OrdinalIgnoreCase) && trimmed.Contains(':'))
                actions.Add(trimmed[(trimmed.IndexOf(':') + 1)..].Trim());
        }

        return EventCardSummaryResult.Success(summary, [.. actions]);
    }

    private static EventCardSummaryResult BuildFallbackSummary(GodsEventCard card)
    {
        var summary = $"Selskab i {card.EstateArea ?? "Den Store Lade"} med {card.GuestCount} gæster. Koordinator: {card.AssignedCoordinator ?? "afventer tildeling"}.";
        string[] actions =
        [
            $"Bekræft reservation for {card.EstateArea ?? "lokalet"}",
            $"Opsæt borde til {card.GuestCount} gæster",
            $"Koordiner forplejning: {card.CateringOption ?? "ikke valgt"}"
        ];
        return EventCardSummaryResult.Success(summary, actions);
    }
}
