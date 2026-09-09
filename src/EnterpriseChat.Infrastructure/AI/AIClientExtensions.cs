using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ClientModel;

namespace EnterpriseChat.Infrastructure.AI;

public static class AIClientExtensions
{
    public static IServiceCollection AddAIClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = BindAzureOpenAIOptions(configuration);

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                "Azure OpenAI API key is not configured. Set AZURE_OPENAI_API_KEY.");
        }

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new InvalidOperationException(
                "Azure OpenAI endpoint is not configured. Set AZURE_OPENAI_ENDPOINT.");
        }

        if (string.IsNullOrWhiteSpace(options.Deployment))
        {
            throw new InvalidOperationException(
                "Azure OpenAI deployment is not configured. Set AZURE_OPENAI_DEPLOYMENT.");
        }

        var clientOptions = CreateClientOptions(options.ApiVersion);

        var azureClient = clientOptions is null
            ? new AzureOpenAIClient(
                new Uri(options.Endpoint),
                new ApiKeyCredential(options.ApiKey))
            : new AzureOpenAIClient(
                new Uri(options.Endpoint),
                new ApiKeyCredential(options.ApiKey),
                clientOptions);

        IChatClient chatClient = azureClient
            .GetChatClient(options.Deployment)
            .AsIChatClient();

        services.AddSingleton(chatClient);

        return services;
    }

    private static AzureOpenAIOptions BindAzureOpenAIOptions(IConfiguration configuration)
    {
        return new AzureOpenAIOptions
        {
            ApiKey = GetValue(
                configuration,
                "AZURE_OPENAI_API_KEY",
                "AzureOpenAI:ApiKey"),
            Endpoint = GetValue(
                configuration,
                "AZURE_OPENAI_ENDPOINT",
                "AzureOpenAI:Endpoint"),
            Deployment = GetValue(
                configuration,
                "AZURE_OPENAI_DEPLOYMENT",
                "AzureOpenAI:Deployment"),
            ApiVersion = GetValue(
                configuration,
                "AZURE_OPENAI_API_VERSION",
                "AzureOpenAI:ApiVersion")
        };
    }

    private static string GetValue(
        IConfiguration configuration,
        string environmentVariable,
        string configurationKey) =>
        configuration[environmentVariable]
        ?? configuration[configurationKey]
        ?? string.Empty;

    private static AzureOpenAIClientOptions? CreateClientOptions(string? apiVersion)
    {
        if (string.IsNullOrWhiteSpace(apiVersion))
        {
            return null;
        }

        var enumName = "V" + apiVersion
            .Replace("-preview", "_Preview", StringComparison.OrdinalIgnoreCase)
            .Replace('-', '_');

        if (Enum.TryParse(
                enumName,
                ignoreCase: true,
                out AzureOpenAIClientOptions.ServiceVersion version))
        {
            return new AzureOpenAIClientOptions(version);
        }

        return null;
    }
}
