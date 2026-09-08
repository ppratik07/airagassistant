using EnterpriseChat.Application.Models;

namespace EnterpriseChat.Application.Chat;

public interface IChatService
{
      IAsyncEnumerable<StreamEvent> StreamAsync(ChatRequest request,CancellationToken cancellationToken);
}

