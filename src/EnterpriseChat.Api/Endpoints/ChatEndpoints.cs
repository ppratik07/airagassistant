using System.Text.Json;
using EnterpriseChat.Application.Chat;
using EnterpriseChat.Application.Models;

namespace EnterpriseChat.Api.Endpoints;

public static class ChatEndpoints
{
    //This creates JSON serialization settings.
    // public string UserName { get; set; }
    //
    // and web JSON generally becomes:
    //
    // {
    //     "userName": "Pratik"
    // }
    //
    // instead of:
    //
    // {
    //     "UserName": "Pratik"
    // }
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/chat/stream", StreamChat);
        return app;
    }

    private static async Task StreamChat( ChatRequest request, IChatService chatService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ConfigureSseResponse(httpContext.Response);

        try
        {
            //Call the chat service and asynchronously receive events one by one.
            await foreach (var streamEvent in chatService.StreamAsync(request, cancellationToken))
            {
                await WriteSseEventAsync(httpContext.Response, streamEvent, cancellationToken);
            }
        }
        //Handle the exception only if the request wasn't cancelled.
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            await WriteSseEventAsync(
                httpContext.Response,
                StreamEvent.Error(ex.Message),
                cancellationToken);
        }
    }

    private static void ConfigureSseResponse(HttpResponse response)
    {
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";
        response.ContentType = "text/event-stream";
    }

    private static async Task WriteSseEventAsync(
        HttpResponse response,
        StreamEvent streamEvent,
        CancellationToken cancellationToken)
    {
        var data = streamEvent.Data is null
            ? "null"
            : JsonSerializer.Serialize(streamEvent.Data, JsonOptions);

        await response.WriteAsync(
            $"event: {streamEvent.Type}\ndata: {data}\n\n",
            cancellationToken);

        await response.Body.FlushAsync(cancellationToken);
    }
}
