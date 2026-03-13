using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgentApi.Models;

namespace AgentApi.Services
{
    public class ExecutorService
    {
        private readonly PlannerService _planner;
        private readonly McpClient _mcpClient;

        public ExecutorService(PlannerService planner, McpClient mcpClient)
        {
            _planner = planner;
            _mcpClient = mcpClient;
        }

        public async Task<AgentResponse> ExecuteAsync(string task)
        {
            var plan = await _planner.CreatePlanAsync(task);
            var toolCalls = new List<ToolCall>();

            foreach (var step in plan)
            {
                ToolCall? call = step.Action switch
                {
                    "list_blobs" => await ExecuteListBlobs(step),
                    "process_csvs" => await ExecuteProcessCsvs(step, toolCalls),
                    _ => new ToolCall
                    {
                        Tool = step.Action,
                        Success = false,
                        Data = JsonDocument.Parse(JsonSerializer.Serialize(new { error = "Unknown action" })).RootElement
                    }
                };

                toolCalls.Add(call);
            }

            var last = toolCalls.LastOrDefault();
            string result = last?.Success == true ? "Completed" : "Failed";
            if (last?.Data != null)
            {
                result = last.Data.ToString() ?? result;
            }

            return new AgentResponse
            {
                Plan = plan,
                ToolCalls = toolCalls,
                Result = result
            };
        }

        private async Task<ToolCall> ExecuteListBlobs(PlanStep step)
        {
            var container = GetStringArg(step.Args, "container") ?? "datasets";
            return await _mcpClient.ListBlobsAsync(container);
        }

        private async Task<ToolCall> ExecuteProcessCsvs(PlanStep step, List<ToolCall> toolCalls)
        {
            var container = GetStringArg(step.Args, "container") ?? "datasets";
            var outputContainer = GetStringArg(step.Args, "outputContainer") ?? "outputs";
            var instructions = GetStringArg(step.Args, "instructions") ?? "";

            var listCall = await _mcpClient.ListBlobsAsync(container);
            toolCalls.Add(listCall);

            if (!listCall.Success || listCall.Data is null)
            {
                return new ToolCall
                {
                    Tool = "process_csvs",
                    Success = false,
                    Data = JsonDocument.Parse(JsonSerializer.Serialize(new { error = "Could not list blobs" })).RootElement
                };
            }

            var fileNames = ExtractStringList(listCall.Data);
            var csvFiles = fileNames.Where(f => f.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var csv in csvFiles)
            {
                var read = await _mcpClient.ReadBlobAsync(container, csv);
                toolCalls.Add(read);

                if (!read.Success || read.Data is null)
                    continue;

                var rawCsv = read.Data.Value.GetString() ?? string.Empty;
                var transform = await _mcpClient.TransformCsvAsync(rawCsv, instructions);
                toolCalls.Add(transform);

                var summaryInput = transform.Success && transform.Data.HasValue
                    ? transform.Data.Value.GetString() ?? rawCsv
                    : rawCsv;

                var summarize = await _mcpClient.SummarizeAsync(summaryInput);
                toolCalls.Add(summarize);

                var outputBlob = $"summary-{csv}.txt";
                var write = await _mcpClient.WriteBlobAsync(outputContainer, outputBlob, summarize.Data?.GetString() ?? string.Empty);
                toolCalls.Add(write);
            }

            return new ToolCall
            {
                Tool = "process_csvs",
                Success = true,
                Data = JsonDocument.Parse(JsonSerializer.Serialize(new { processed = csvFiles.Count })).RootElement
            };
        }

        private static string? GetStringArg(JsonElement? args, string key)
        {
            if (args is not JsonElement element || element.ValueKind != JsonValueKind.Object)
                return null;

            if (!element.TryGetProperty(key, out var value) || value.ValueKind != JsonValueKind.String)
                return null;

            return value.GetString();
        }

        private static List<string> ExtractStringList(JsonElement? element)
        {
            var result = new List<string>();
            if (!element.HasValue)
                return result;

            var jsonElement = element.Value;
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                        result.Add(item.GetString() ?? string.Empty);
                }
            }

            return result;
        }
    }
}
