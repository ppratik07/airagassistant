using System.Runtime.CompilerServices;
using EnterpriseChat.Application.Models;
using EnterpriseChat.Application.Tools;
using Microsoft.Extensions.AI;

namespace EnterpriseChat.Application.Chat;

public sealed class ChatOrchestrator : IChatOrchestrator   //sealed means nobody can inherit from ChatOrchestrator.
{
    private readonly IChatClient _chatClient;
    private readonly IToolRegistry _toolRegistry;

    public ChatOrchestrator(
        IChatClient chatClient,
        IToolRegistry toolRegistry)
    {
        _chatClient = chatClient;  //This talks to your AI model.
        _toolRegistry = toolRegistry; //This manages your tools.
    }

    //IAsyncEnumerable = Send this event now, but keep the method alive because I'll send more later.
    //[EnumeratorCancellation]- The cancellation token lets the async stream stop when the client disconnects/cancels the request.
    public async IAsyncEnumerable<StreamEvent> ExecuteAsync(
        ChatRequest request,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    //create the conversation messages
    {
        var messages = new List<ChatMessage>
        {
            //System prompt
            new(
                ChatRole.System,
                """
                You are a helpful assistant.

                When the user asks about weather,
                always use the get_weather tool.

                Do not invent weather information.
                """),
            
            //This is the user prompt 
            new(
                ChatRole.User,
                request.Message)
        };
        
        //You're telling the AI: "Here are the tools you're allowed to use.
        var options = new ChatOptions
        {
            Tools = _toolRegistry.GetTools().ToList()
        };

        //SINCE infinite loop : Because AI tool calling can require multiple rounds.
        while (true)
        {
            //creating a place to collect everything the AI produces during this round.
            var assistantMessage =
                new ChatMessage(ChatRole.Assistant, []);

            //Send the conversation and available tools to the AI model, and give me its response piece by piece as it arrives.
            await foreach (
                var update in _chatClient  //talks to azure openai
                    .GetStreamingResponseAsync(
                        messages,
                        options, //tools
                        cancellationToken))
            {
                //An AI response can contain different types of content-TextContent,FunctionCallContent,FunctionResultContent
                foreach (var content in update.Contents)
                {
                    switch (content)
                    {
                        case TextContent text
                            when !string.IsNullOrEmpty(text.Text):

                            assistantMessage.Contents.Add(              //Add the text to assistant message
                                new TextContent(text.Text));

                            yield return
                                StreamEvent.Token(text.Text);           //Stream the token to the frontend

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

            messages.Add(assistantMessage);         //adds the assistant's response to the conversation.

            //Did the assistant message contain any function calls?
            var toolCalls = assistantMessage 
                .Contents
                .OfType<FunctionCallContent>()
                .ToList();

            if (toolCalls.Count == 0)
            {
                break;
            }

            //eg for below - SYSTEM
            // You are a helpful assistant...
            // 
            // USER
            // What's the weather in Bangalore?
            // 
            // ASSISTANT
            // Call get_weather(Bangalore)
            // 
            // TOOL
            // 28°C and sunny
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
            //Send to FE
                yield return StreamEvent.ToolResult(
                    toolCall.Name,
                    result);
            }
        }

        yield return StreamEvent.Done();        //The entire AI response is finished.
    }
}