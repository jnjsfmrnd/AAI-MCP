using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgentApi.Models;

namespace AgentApi.Services
{
    public class PlannerService
    {
        private readonly LlmProvider _llmProvider;

        public PlannerService(LlmProvider llmProvider)
        {
            _llmProvider = llmProvider;
        }

        public Task<List<PlanStep>> CreatePlanAsync(string task)
        {
            // Currently a simple rule-based planner; future versions can use the LLM provider.
            var lower = task?.Trim().ToLowerInvariant() ?? string.Empty;
            var steps = new List<PlanStep>();

            // Always start by listing blobs in the datasets container
            steps.Add(new PlanStep
            {
                Step = 1,
                Action = "list_blobs",
                Args = JsonSerializer.SerializeToElement(new { container = "datasets" })
            });

            // If the task looks like it wants CSV processing, add a helper step
            if (lower.Contains("csv") || lower.Contains("clean") || lower.Contains("transform"))
            {
                steps.Add(new PlanStep
                {
                    Step = 2,
                    Action = "process_csvs",
                    Args = JsonSerializer.SerializeToElement(new
                    {
                        container = "datasets",
                        outputContainer = "outputs",
                        instructions = "remove empty rows"
                    })
                });
            }

            return Task.FromResult(steps);
        }
    }
}
