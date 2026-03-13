using System.Text.Json;

namespace AgentApi.Models
{
    public class ToolCall
    {
        public string Tool { get; set; } = string.Empty;
        public bool Success { get; set; }
        public JsonElement? Data { get; set; }
    }
}
