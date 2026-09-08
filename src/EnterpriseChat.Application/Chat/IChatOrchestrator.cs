using EnterpriseChat.Application.Models;

namespace EnterpriseChat.Application.Chat;

public interface IChatOrchestrator
{
    IAsyncEnumerable<StreamEvent> ExecuteAsync(ChatRequest request, CancellationToken cancellationToken);
}