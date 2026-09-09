namespace EnterpriseChat.Infrastructure.AI;

public sealed class AzureOpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string Deployment { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = string.Empty;
}
