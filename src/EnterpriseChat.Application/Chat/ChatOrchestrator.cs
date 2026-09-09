using System.Runtime.CompilerServices;
using EnterpriseChat.Application.Models;
using EnterpriseChat.Application.Tools;
using Microsoft.Extensions.AI;

namespace EnterpriseChat.Application.Chat;

public sealed class ChatOrchestrator : IChatOrchestrator
{
    private readonly IChatClient _chatClient;
    private readonly IToolRegistry _toolRegistry;

    public ChatOrchestrator(
        IChatClient chatClient,
        IToolRegistry toolRegistry)
    {
        _chatClient = chatClient;
        _toolRegistry = toolRegistry;
    }

    public async IAsyncEnumerable<StreamEvent> ExecuteAsync(
        ChatRequest request,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new(
                ChatRole.System,
                """
                You are a helpful assistant.

                When the user asks about weather,
                always use the get_weather tool.

                Do not invent weather information.
                """),

            new(
                ChatRole.User,
                request.Message)
        };

        var options = new ChatOptions
        {
            Tools = _toolRegistry.GetTools().ToList()
        };

        while (true)
        {
            var assistantMessage =
                new ChatMessage(ChatRole.Assistant, []);

            await foreach (
                var update in _chatClient
                    .GetStreamingResponseAsync(
                        messages,
                        options,
                        cancellationToken))
            {
                foreach (var content in update.Contents)
                {
                    switch (content)
                    {
                        case TextContent text
                            when !string.IsNullOrEmpty(text.Text):

                            assistantMessage.Contents.Add(
                                new TextContent(text.Text));

                            yield return
                                StreamEvent.Token(text.Text);

                            break;

                        case FunctionCallContent functionCall:

                            assistantMessage.Contents.Add(
                                functionCall);

                            yield return
                                StreamEvent.ToolCall(
                                    functionCall.Name,
                                    functionCall.Arguments);

                            break;
                    }
                }
            }

            messages.Add(assistantMessage);

            var toolCalls = assistantMessage
                .Contents
                .OfType<FunctionCallContent>()
                .ToList();

            if (toolCalls.Count == 0)
            {
                break;
            }

            foreach (var toolCall in toolCalls)
            {
                var result =
                    await _toolRegistry.ExecuteAsync(
                        toolCall,
                        cancellationToken);

                messages.Add(
                    new ChatMessage(
                        ChatRole.Tool,
                        [
                            new FunctionResultContent(
                                toolCall.CallId,
                                result)
                        ]));

                yield return StreamEvent.ToolResult(
                    toolCall.Name,
                    result);
            }
        }

        yield return StreamEvent.Done();
    }
}