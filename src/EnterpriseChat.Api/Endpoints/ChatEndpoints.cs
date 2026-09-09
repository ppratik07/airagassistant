using System.Text.Json;
using EnterpriseChat.Application.Chat;
using EnterpriseChat.Application.Models;

namespace EnterpriseChat.Api.Endpoints;

public static class ChatEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/chat/stream", async (
            ChatRequest request,
            IChatService chatService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            httpContext.Response.Headers.CacheControl = "no-cache";
            httpContext.Response.Headers.Connection = "keep-alive";
            httpContext.Response.ContentType = "text/event-stream";

            try
            {
                await foreach (var streamEvent in chatService.StreamAsync(request, cancellationToken))
                {
                    var data = streamEvent.Data is null
                        ? "null"
                        : JsonSerializer.Serialize(streamEvent.Data, JsonOptions);

                    await httpContext.Response.WriteAsync(
                        $"event: {streamEvent.Type}\ndata: {data}\n\n",
                        cancellationToken);

                    await httpContext.Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                var error = StreamEvent.Error(ex.Message);
                var data = JsonSerializer.Serialize(error.Data, JsonOptions);

                await httpContext.Response.WriteAsync(
                    $"event: {error.Type}\ndata: {data}\n\n",
                    cancellationToken);

                await httpContext.Response.Body.FlushAsync(cancellationToken);
            }
        });

        return app;
    }
}
