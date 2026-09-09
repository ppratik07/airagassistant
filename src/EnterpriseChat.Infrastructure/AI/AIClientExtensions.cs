using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace EnterpriseChat.Infrastructure.AI;

public static class AIClientExtensions
{
    public static IServiceCollection AddAIClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiKey =
            configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenAI:ApiKey is not configured.");

        var model =
            configuration["OpenAI:Model"]
            ?? "gpt-4.1-mini";

        var client = new OpenAIClient(apiKey);

        IChatClient chatClient = client
            .GetChatClient(model)
            .AsIChatClient();

        services.AddSingleton(chatClient);

        return services;
    }
}
