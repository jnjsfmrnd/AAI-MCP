using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AgentApi.Models;
using AgentApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<LlmProvider>();
builder.Services.AddSingleton<McpClient>();
builder.Services.AddSingleton<PlannerService>();
builder.Services.AddSingleton<ExecutorService>();

var app = builder.Build();

app.MapGet("/", () => "Agent API is running");

app.MapPost("/agent/run", async (AgentRequest request, PlannerService planner, ExecutorService executor) =>
{
    if (string.IsNullOrWhiteSpace(request.Task))
    {
        return Results.BadRequest(new { error = "Task is required" });
    }

    // TODO: Use planner + executor to build plan and execute tool calls
    var response = new AgentResponse
    {
        Plan = new object[0],
        ToolCalls = new object[0],
        Result = "Not implemented yet"
    };

    return Results.Ok(response);
});

app.Run();
