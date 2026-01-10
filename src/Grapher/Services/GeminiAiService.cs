using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Grapher.Models;
using Microsoft.Extensions.Configuration;

namespace Grapher.Services
{
    public class GeminiAiService : IAiSummaryService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private const string ModelId = "gemini-2.5-flash"; 

        public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["AI_API_KEY"]; // Reading from User Secrets / Config
        }

        public async Task<string> GenerateProjectSummaryAsync(Project project, IEnumerable<TaskItem> tasks)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "AI API Key is missing. Please configure 'AI_API_KEY'.";
            }

            var prompt = BuildPrompt(project, tasks);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return $"Error calling AI service: {response.StatusCode} - {errorBody}";
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var text = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                return text ?? "No summary generated.";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        private string BuildPrompt(Project project, IEnumerable<TaskItem> tasks)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Please provide a concise summary for the project titled '{project.Title}'.");
            sb.AppendLine($"Project Description: {project.Description}");
            sb.AppendLine();
            sb.AppendLine("Here are the tasks associated with this project:");

            if (!tasks.Any())
            {
                sb.AppendLine("No tasks created yet.");
            }
            else
            {
                foreach (var task in tasks)
                {
                    var assignee = task.Assignments.Any() 
                        ? string.Join(", ", task.Assignments.Select(a => a.User?.UserName ?? "Unknown")) 
                        : "Unassigned";
                    
                    sb.AppendLine($"- Task: {task.Title} (Status: {task.Status}, Assigned to: {assignee})");
                    if (!string.IsNullOrWhiteSpace(task.Description))
                    {
                         sb.AppendLine($"  Description: {task.Description}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("Summarize the current progress, identify potential bottlenecks based on status, and suggest next steps. Keep it professional and brief.");
            return sb.ToString();
        }

        // Helper classes for JSON deserialization
        private class GeminiResponse
        {
            public Candidate[]? Candidates { get; set; }
        }

        private class Candidate
        {
            public Content? Content { get; set; }
        }

        private class Content
        {
            public Part[]? Parts { get; set; }
        }

        private class Part
        {
            public string? Text { get; set; }
        }
    }
}
