namespace AgentApi.Models
{
    public class AgentResponse
    {
        public object? Plan { get; set; }
        public object? ToolCalls { get; set; }
        public string? Result { get; set; }
    }
}
