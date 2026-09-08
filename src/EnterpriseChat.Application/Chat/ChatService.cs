using EnterpriseChat.Application.Models;

namespace EnterpriseChat.Application.Chat;

public sealed class ChatService : IChatService
{
    private readonly IChatOrchestrator _chatOrchestrator;

    public ChatService(IChatOrchestrator chatOrchestrator)
    {
        _chatOrchestrator = chatOrchestrator;
    }

    public IAsyncEnumerable<StreamEvent> StreamAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        return _chatOrchestrator.ExecuteAsync(request, cancellationToken);
    }
}