using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AgentApi.Models;

namespace AgentApi.Services
{
    public class PlannerService
    {
        private readonly LlmProvider _llmProvider;

        // Prompt used to instruct the LLM to return a JSON-only plan.
        private const string PromptTemplate =
            "You are a planner that breaks a user task into a sequence of steps. " +
            "Each step must be a tool call. Respond with JSON only. " +
            "Return an array of objects where each object has 'step', 'action', and 'args'.\n\n" +
            "User task:";

        public PlannerService(LlmProvider llmProvider)
        {
            _llmProvider = llmProvider;
        }

        public async Task<List<PlanStep>> CreatePlanAsync(string task)
        {
            var prompt = $"{PromptTemplate} {task}";

            try
            {
                var response = await _llmProvider.CompleteAsync(prompt);
                var plan = JsonSerializer.Deserialize<List<PlanStep>>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (plan is { Count: > 0 })
                {
                    return plan;
                }
            }
            catch
            {
                // Fall back to a simple plan if parsing fails.
            }

            return CreateFallbackPlan(task);
        }

        private static List<PlanStep> CreateFallbackPlan(string task)
        {
            var lower = task?.Trim().ToLowerInvariant() ?? string.Empty;
            var steps = new List<PlanStep>();

            steps.Add(new PlanStep
            {
                Step = 1,
                Action = "list_blobs",
                Args = JsonSerializer.SerializeToElement(new { container = "datasets" })
            });

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

            return steps;
        }
    }
}
