# AAI-MCP

Agentic AI system using:
- C# (.NET 8) agent (planner + executor)
- Python MCP server (Azure Functions)

## Structure

- `agent-api/` - .NET Minimal API for planning and execution
- `mcp-server/` - Python Azure Functions MCP tools
- `shared/` - shared models or prompts (optional)
- `infra/` - infrastructure scripts (optional)

## Next steps

1. Scaffold Azure Function app under `mcp-server/`.
2. Implement MCP tool endpoints (blob.list/read/write, csv.transform, http.fetch, file.summarize).
3. Scaffold .NET Web API in `agent-api/` and wire up planner/executor services.
