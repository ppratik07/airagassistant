using Microsoft.Extensions.AI;

namespace EnterpriseChat.Application.Tools;

public sealed class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, AIFunction> _tools;

    public ToolRegistry(IEnumerable<AIFunction> tools)
    {
        _tools = tools.ToDictionary(
            tool => tool.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<AITool> GetTools()
    {
        return _tools.Values
            .Cast<AITool>()
            .ToList();
    }

    public async Task<object?> ExecuteAsync(FunctionCallContent functionCall,CancellationToken cancellationToken) =>
    {
        if (!_tools.TryGetValue(
                functionCall.Name,
                out var tool))
        {
            throw new InvalidOperationException(
                $"Tool '{functionCall.Name}' is not registered.");
        }

        return await tool.InvokeAsync(
            functionCall.Arguments,
            cancellationToken);
    }
}