using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AgentApi.Models;
using Microsoft.Extensions.Configuration;

namespace AgentApi.Services
{
    public class McpClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public McpClient(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _baseUrl = configuration["MCP_SERVER_URL"] ?? "http://localhost:7071/api";
        }

        private static string GetFunctionName(string toolName)
        {
            // Convert tool names like "blob.list" into function paths like "blob_list".
            return toolName.Replace('.', '_');
        }

        private async Task<ToolCall> CallToolAsync(string toolName, object? args)
        {
            var functionName = GetFunctionName(toolName);
            var url = new Uri(new Uri(_baseUrl.TrimEnd('/')), functionName);

            var payload = JsonSerializer.Serialize(args ?? new { });
            var response = await _http.PostAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ToolCall
                {
                    Tool = toolName,
                    Success = false,
                    Data = JsonDocument.Parse(JsonSerializer.Serialize(new { error = content })).RootElement
                };
            }

            try
            {
                var toolCall = JsonSerializer.Deserialize<ToolCall>(content, _jsonOptions);
                if (toolCall is null)
                {
                    return new ToolCall { Tool = toolName, Success = false, Data = JsonDocument.Parse(JsonSerializer.Serialize(new { error = "Empty response" })).RootElement };
                }

                toolCall.Tool = toolName;
                return toolCall;
            }
            catch (Exception ex)
            {
                return new ToolCall
                {
                    Tool = toolName,
                    Success = false,
                    Data = JsonDocument.Parse(JsonSerializer.Serialize(new { error = ex.Message, raw = content })).RootElement
                };
            }
        }

        public Task<ToolCall> ListBlobsAsync(string container) => CallToolAsync("blob.list", new { container });

        public Task<ToolCall> ReadBlobAsync(string container, string blob) => CallToolAsync("blob.read", new { container, blob });

        public Task<ToolCall> WriteBlobAsync(string container, string blob, string content) => CallToolAsync("blob.write", new { container, blob, content });

        public Task<ToolCall> TransformCsvAsync(string csv, string instructions) => CallToolAsync("csv.transform", new { csv, instructions });

        public Task<ToolCall> HttpFetchAsync(string url) => CallToolAsync("http.fetch", new { url });

        public Task<ToolCall> SummarizeAsync(string content) => CallToolAsync("file.summarize", new { content });
    }
}
