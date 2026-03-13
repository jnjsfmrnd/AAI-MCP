using System.Collections.Generic;

namespace AgentApi.Models
{
    public class AgentResponse
    {
        public List<PlanStep> Plan { get; set; } = new();
        public List<ToolCall> ToolCalls { get; set; } = new();
        public string? Result { get; set; }
    }
}
