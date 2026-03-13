using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AgentApi.Models;
using AgentApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<McpClient>();
builder.Services.AddHttpClient<LlmProvider>();
builder.Services.AddSingleton<PlannerService>();
builder.Services.AddSingleton<ExecutorService>();

var app = builder.Build();

app.MapGet("/", () => "Agent API is running");

app.MapPost("/agent/run", async (AgentRequest request, ExecutorService executor) =>
{
    if (string.IsNullOrWhiteSpace(request.Task))
    {
        return Results.BadRequest(new { error = "Task is required" });
    }

    var response = await executor.ExecuteAsync(request.Task);
    return Results.Ok(response);
});

app.Run();
