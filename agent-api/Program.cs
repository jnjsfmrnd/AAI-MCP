using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// TODO: Add services (LlmProvider, McpClient, PlannerService, ExecutorService)

var app = builder.Build();

app.MapGet("/", () => "Agent API is running");

app.MapPost("/agent/run", () => Results.BadRequest(new { error = "Not implemented" }));

app.Run();
