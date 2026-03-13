using System.Threading.Tasks;

namespace AgentApi.Services
{
    public class LlmProvider
    {
        // TODO: Implement real LLM provider (Azure/Groq/Ollama).
        // For now this is a stub used for future integration.

        public Task<string> CompleteAsync(string prompt)
        {
            // Placeholder: returns the prompt for debugging.
            return Task.FromResult(prompt);
        }
    }
}
