using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AgentApi.Services
{
    public class LlmProvider
    {
        private readonly HttpClient _http;
        private readonly string _provider;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _model;

        public LlmProvider(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _provider = configuration["LLM_PROVIDER"]?.ToLowerInvariant() ?? "mock";
            _endpoint = configuration["LLM_ENDPOINT"] ?? string.Empty;
            _apiKey = configuration["LLM_API_KEY"] ?? string.Empty;
            _model = configuration["LLM_MODEL"] ?? string.Empty;
        }

        public async Task<string> CompleteAsync(string prompt)
        {
            if (_provider == "mock" || string.IsNullOrWhiteSpace(_endpoint) || string.IsNullOrWhiteSpace(_apiKey))
            {
                // Mock behavior (default) for local development / tests.
                return MockCompletion(prompt);
            }

            return _provider switch
            {
                "azure" => await AzureCompletionAsync(prompt),
                "ollama" => await OllamaCompletionAsync(prompt),
                "gemini" => await GeminiCompletionAsync(prompt), // Added Gemini provider
                _ => MockCompletion(prompt)
            };
        }

        private string MockCompletion(string prompt)
        {
            // A simple plan generator used during development (no external API calls).
            var lower = prompt.Trim().ToLowerInvariant();
            if (lower.Contains("csv") || lower.Contains("clean") || lower.Contains("transform"))
            {
                return "[ { \"step\": 1, \"action\": \"list_blobs\", \"args\": { \"container\": \"datasets\" } }, { \"step\": 2, \"action\": \"process_csvs\", \"args\": { \"container\": \"datasets\", \"outputContainer\": \"outputs\", \"instructions\": \"remove empty rows\" } } ]";
            }

            return "[ { \"step\": 1, \"action\": \"list_blobs\", \"args\": { \"container\": \"datasets\" } } ]";
        }

        private async Task<string> AzureCompletionAsync(string prompt)
        {
            // Azure OpenAI (REST): https://learn.microsoft.com/azure/cognitive-services/openai/quickstart
            // Expecting LLM_ENDPOINT like https://<resource-name>.openai.azure.com/
            // and LLM_MODEL as deployment name.

            var requestUri = new Uri(new Uri(_endpoint.TrimEnd('/')), $"openai/deployments/{_model}/completions?api-version=2023-05-15");

            using var message = new HttpRequestMessage(HttpMethod.Post, requestUri);
            message.Headers.Add("api-key", _apiKey);
            message.Content = new StringContent(JsonSerializer.Serialize(new
            {
                prompt,
                max_tokens = 1024,
                temperature = 0.2
            }), Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(message);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                return choices[0].GetProperty("text").GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private async Task<string> OllamaCompletionAsync(string prompt)
        {
            // Ollama local inference (default endpoint: http://localhost:11434)
            // Expecting LLM_ENDPOINT to include the base URL (e.g., http://localhost:11434).

            var baseUri = new Uri(_endpoint.TrimEnd('/'));
            var requestUri = new Uri(baseUri, "/api/chat");

            using var message = new HttpRequestMessage(HttpMethod.Post, requestUri);
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            message.Content = new StringContent(JsonSerializer.Serialize(new
            {
                model = _model,
                messages = new[] { new { role = "user", content = prompt } }
            }), Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(message);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    return choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
                }
            }
            catch
            {
                // Fall back to raw text
            }

            return content;
        }

        private async Task<string> GeminiCompletionAsync(string prompt)
        {
            // Gemini API integration
            var requestUri = new Uri(new Uri(_endpoint.TrimEnd('/')), "/v1/completions");

            using var message = new HttpRequestMessage(HttpMethod.Post, requestUri);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            message.Content = new StringContent(JsonSerializer.Serialize(new
            {
                prompt,
                max_tokens = 1024,
                temperature = 0.7
            }), Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(message);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                return choices[0].GetProperty("text").GetString() ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
