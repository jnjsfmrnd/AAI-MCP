using System.Text.Json;

namespace AgentApi.Models
{
    public class PlanStep
    {
        public int Step { get; set; }
        public string Action { get; set; } = string.Empty;
        public JsonElement? Args { get; set; }
    }
}
