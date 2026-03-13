# PLAN.md — Build Plan for Agentic AI System
# Goal: Build a cloud-deployed agentic AI system using:
# - C# (.NET 8) agent (planner + executor)
# - Python MCP server (tools)
# - Azure Functions (free tier)
# - React UI added last

# ============================================================
# 0. Current Status (2026-03-12)
# ============================================================

- ✅ Repository scaffold created (agent-api + mcp-server)
- ✅ Initial git commit made and pushed to GitHub
- ✅ MCP tool `blob.list` implemented and callable
- ✅ MCP tools scaffolded: `blob.read`, `blob.write`, `csv.transform`, `http.fetch`, `file.summarize`
- ✅ Basic .NET API scaffold created (placeholder `/agent/run` endpoint)

## Next steps

1. Wire up `PlannerService` + `ExecutorService` + `McpClient` to execute tool plans
2. Add local dev docs (Azure Functions Core Tools, env vars, storage emulator)
3. Add end-to-end test for "list blobs" and "read + transform + write"


# ============================================================
# 1. Repository Structure
# ============================================================

root/
  agent-api/                # C# .NET 8 Minimal API (planner + executor)
  mcp-server/               # Python MCP server (Azure Functions)
  shared/                   # Shared models, prompts, contracts (optional)
  infra/                    # Azure infra scripts (optional)
  README.md
  PLAN.md

# ============================================================
# 2. MCP Server (Python, Azure Functions)
# ============================================================

# Create folder: mcp-server/
# Initialize Azure Functions (Python 3.10+)
# Each MCP tool is an HTTP-triggered function.

mcp-server/
  __init__.py
  host.json
  local.settings.json
  requirements.txt

  tools/
    blob_list/__init__.py
    blob_read/__init__.py
    blob_write/__init__.py
    csv_transform/__init__.py
    http_fetch/__init__.py
    file_summarize/__init__.py

# ------------------------------------------------------------
# 2.1 Implement MCP Tool: blob.list
# ------------------------------------------------------------
# Input:
#   { "container": "datasets" }
# Output:
#   { "tool": "blob.list", "success": true, "data": ["file1.csv", ...] }

# ------------------------------------------------------------
# 2.2 Implement MCP Tool: blob.read
# ------------------------------------------------------------
# Input:
#   { "container": "datasets", "blob": "file.csv" }
# Output:
#   { "tool": "blob.read", "success": true, "data": "<file contents>" }

# ------------------------------------------------------------
# 2.3 Implement MCP Tool: blob.write
# ------------------------------------------------------------
# Input:
#   { "container": "outputs", "blob": "clean.csv", "content": "..." }
# Output:
#   { "tool": "blob.write", "success": true }

# ------------------------------------------------------------
# 2.4 Implement MCP Tool: csv.transform
# ------------------------------------------------------------
# Input:
#   { "csv": "<raw csv>", "instructions": "remove empty rows" }
# Output:
#   { "tool": "csv.transform", "success": true, "data": "<clean csv>" }

# ------------------------------------------------------------
# 2.5 Implement MCP Tool: http.fetch
# ------------------------------------------------------------
# Input:
#   { "url": "https://api.example.com" }
# Output:
#   { "tool": "http.fetch", "success": true, "data": { ... } }

# ------------------------------------------------------------
# 2.6 Implement MCP Tool: file.summarize
# ------------------------------------------------------------
# Input:
#   { "content": "<text>" }
# Output:
#   { "tool": "file.summarize", "success": true, "data": "<summary>" }

# All tools must:
# - Accept JSON input
# - Return JSON with { tool, success, data }
# - Use Azure Blob Storage SDK
# - Use aiohttp for HTTP fetches
# - Use pandas for CSV transforms (optional)

# ============================================================
# 3. Agent API (C#, .NET 8 Minimal API)
# ============================================================

agent-api/
  Program.cs
  appsettings.json
  Models/
    AgentRequest.cs
    AgentResponse.cs
    PlanStep.cs
    ToolCall.cs
  Services/
    PlannerService.cs
    ExecutorService.cs
    LlmProvider.cs
    McpClient.cs
  Prompts/
    planner_prompt.txt
    executor_prompt.txt

# ------------------------------------------------------------
# 3.1 Endpoint: POST /agent/run
# ------------------------------------------------------------
# Input:
#   { "task": "Summarize all CSVs in blob storage" }
# Output:
#   {
#     "plan": [...],
#     "toolCalls": [...],
#     "result": "..."
#   }

# ------------------------------------------------------------
# 3.2 PlannerService
# ------------------------------------------------------------
# Responsibilities:
# - Generate a JSON plan using LLM
# - Plan format:
#   [
#     { "step": 1, "action": "list_blobs", "args": { ... } },
#     { "step": 2, "action": "read_blob", "args": { ... } },
#     ...
#   ]

# ------------------------------------------------------------
# 3.3 ExecutorService
# ------------------------------------------------------------
# Responsibilities:
# - Loop through plan
# - For each step:
#     - If action requires tool → call MCP server
#     - Feed tool result back into LLM
# - Produce final output

# ------------------------------------------------------------
# 3.4 McpClient
# ------------------------------------------------------------
# Responsibilities:
# - POST to MCP server endpoints
# - Handle retries
# - Deserialize JSON responses

# ------------------------------------------------------------
# 3.5 LlmProvider
# ------------------------------------------------------------
# Providers:
# - Azure AI (Phi-3.5, Llama 3.1)
# - Groq (free)
# - Ollama (local)

# Interface:
#   Task<string> CompleteAsync(string prompt)

# ============================================================
# 4. LLM Integration
# ============================================================

# Environment variables:
#   LLM_PROVIDER=azure|groq|ollama
#   LLM_ENDPOINT=<url>
#   LLM_API_KEY=<key>
#   LLM_MODEL=<model-name>

# Planner prompt:
#   "Break the user task into a sequence of tool calls. Return JSON only."

# Executor prompt:
#   "Given the tool result, decide the next step. Return JSON only."

# ============================================================
# 5. Azure Deployment
# ============================================================

# 5.1 Deploy MCP server
# - Azure Functions Python
# - Consumption plan (free)
# - Enable CORS

# 5.2 Deploy Agent API
# - Azure Functions .NET 8 isolated
# - Configure environment variables

# 5.3 Create Blob Storage containers
# - datasets/
# - outputs/

# ============================================================
# 6. End-to-End Tests (Before UI)
# ============================================================

# Test 1:
# Input: "List all files in blob storage"
# Expect:
# - Planner generates steps
# - Executor calls blob.list
# - Returns list of files

# Test 2:
# Input: "Read all CSVs, clean them, and upload summaries"
# Expect:
# - Multi-step plan
# - CSV transforms
# - Writes to outputs/

# ============================================================
# 7. React UI (Add Last)
# ============================================================

# UI will:
# - Send POST /agent/run
# - Display plan
# - Display tool calls
# - Display final output

# ============================================================
# End of PLAN.md
# ============================================================