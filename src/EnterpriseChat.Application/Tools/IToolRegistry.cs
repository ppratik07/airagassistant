using Microsoft.Extensions.AI;

namespace EnterpriseChat.Application.Tools;

public interface IToolRegistry
{
   IReadOnlyList<AITool> GetTools(); 
   Task<object?> ExecuteAsync(FunctionCallContent functionCall,CancellationToken cancellationToken);
}