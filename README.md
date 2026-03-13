# AAI-MCP

Agentic AI system using:
- C# (.NET 8) agent (planner + executor)
- Python MCP server (Azure Functions)

## Structure

- `agent-api/` - .NET Minimal API for planning and execution
- `mcp-server/` - Python Azure Functions MCP tools
- `shared/` - shared models or prompts (optional)
- `infra/` - infrastructure scripts (optional)

## Local development

### Run MCP server (Azure Functions)

1. Install [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local).
2. From `mcp-server/`, run:
   ```bash
   func start
   ```
3. Ensure `AZURE_STORAGE_CONNECTION_STRING` is set in `local.settings.json` (or your environment).

### Run Agent API (C#)

From `agent-api/`:

```bash
dotnet run
```

The API will be available on `http://localhost:5000` (or as printed by dotnet).

### Example request

```bash
curl -X POST http://localhost:5000/agent/run -H "Content-Type: application/json" -d '{"task":"List all files in blob storage"}'
```

## Next steps

1. Add local dev docs (Azure Functions Core Tools, env vars, storage emulator)
2. Add end-to-end test for "list blobs" and "read + transform + write"
3. Improve PlannerService logic (LLM integration) and make execution more dynamic
