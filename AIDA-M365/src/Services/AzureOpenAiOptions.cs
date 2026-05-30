namespace AIDA.M365.Services;

public sealed class AzureOpenAiOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string DeploymentName { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = "2024-02-15-preview";

    public string? ApiKey { get; set; }
}
