namespace EnterpriseChat.Application.Models;

public sealed record StreamEvent(string Type, object? Data)
{
    public static StreamEvent Token(string text) => new("token", new { text });
    public static StreamEvent ToolCall(string name, object? arguments) => new("tool_call", new { name, arguments });
    public static StreamEvent ToolResult(string name, object? result) => new("tool_result", new { name, result });
    public static StreamEvent Done() => new("done", null);
    public static StreamEvent Error(string message) => new("error", message);
}