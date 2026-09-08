namespace EnterpriseChat.Application.Models;

public sealed record StreamEvent(string Type, object? Data)
{
    public static StreamEvent Token(string text) => new("token", text);
    public static StreamEvent ToolCall(string name, object? args) => new("tool_call", new { name, args });
    public static StreamEvent ToolResult(string name, object? result) => new("tool_result", new { name, result });
    public static StreamEvent Done() => new("done", null);
    public static StreamEvent Error(string message) => new("error", message);
}