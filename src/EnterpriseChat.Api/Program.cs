using EnterpriseChat.Api.Endpoints;
using EnterpriseChat.Application.Chat;
using EnterpriseChat.Application.Tools;
using EnterpriseChat.Infrastructure.AI;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAIClient(
    builder.Configuration);

builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<IChatOrchestrator, ChatOrchestrator>();

builder.Services.AddSingleton<AIFunction>(
    _ => AIFunctionFactory.Create(
        WeatherTool.GetWeather,
        "get_weather"));

builder.Services.AddSingleton<IToolRegistry, ToolRegistry>();

var app = builder.Build();

app.MapChatEndpoints();

app.Run();