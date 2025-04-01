using Flow.Launcher.Plugin;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;

namespace Flow.Launcher.Plugin.AzureOpenAI
{
    public class Main : IPlugin, ISettingProvider, IContextMenu
    {
        private PluginInitContext _context;
        private static readonly HttpClient client = new HttpClient();
        private Settings _settings;

        public void Init(PluginInitContext context)
        {
            _context = context;
            _context.API.LogInfo("AzureOpenAI", "Initializing AzureOpenAI plugin");
            _settings = context.API.LoadSettingJsonStorage<Settings>();
            _context.API.LogInfo("AzureOpenAI", "Settings loaded");
            _context.API.LogInfo("AzureOpenAI", $"Endpoint: {_settings.Endpoint}");
            _context.API.LogInfo("AzureOpenAI", $"ApiKey: {_settings.ApiKey}");
            _context.API.LogInfo("AzureOpenAI", $"ModelName: {_settings.ModelName}");
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();

            // Check if the ActionKeyword is used to trigger the query submission
            if (!string.IsNullOrEmpty(query.Search))
            {
                results.Add(new Result
                {
                    Title = "Press Enter to submit the query",
                    SubTitle = query.Search,
                    IcoPath = "Images/app.png",
                    Action = _ =>
                    {
                        // Run the GetAIResponse method asynchronously
                        Task.Run(async () =>
                        {
                            _context.API.LogInfo("AzureOpenAI:", "trying to send the request");
                            var response = await GetAIResponse(query.Search.Trim());

                            // Update the Flow Launcher UI with the response
                            _context.API.ChangeQuery(query.RawQuery, true);
                            _context.API.ChangeQuery(response, true);
                        });
                        return false; // Keep the Flow Launcher open
                    }
                });
            }

            return results;
        }

        private async Task<string> GetAIResponse(string input)
        {
            var requestBody = new
            {
                model = _settings.ModelName,
                temperature = 0.7,
                top_p = 1,
                stream = true,
                messages = new[]
                {
                    new { role = "user", content = input }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Add the api-key header only if it is not already present
            if (!client.DefaultRequestHeaders.Contains("api-key"))
            {
                client.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);
            }

            _context.API.LogInfo("AzureOpenAI:", "Before sending the request");
            var response = await client.PostAsync($"{_settings.Endpoint}/openai/deployments/{_settings.ModelName}/chat/completions?api-version=2024-05-01-preview", content);
            _context.API.LogInfo("AzureOpenAI:", "Got the response from the API");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseBody);
            string line;
            var resultBuilder = new StringBuilder();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                _context.API.LogInfo("AzureOpenAI:", $"{line}");
                if (line.StartsWith("data: "))
                {
                    line = line.Substring("data: ".Length);
                    if (line.Trim() == "[DONE]") break;

                    var responseJson = JsonSerializer.Deserialize<Dictionary<string, object>>(line);
                    if (responseJson != null && responseJson.ContainsKey("choices"))
                    {
                        var choices = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responseJson["choices"].ToString());
                        if (choices != null && choices.Count > 0)
                        {
                            var delta = choices[0]["delta"] as JsonElement?;
                            if (delta != null && delta.Value.TryGetProperty("content", out var contentElement))
                            {
                                resultBuilder.Append(contentElement.GetString());
                                // Update the UI in a thread-safe manner
                                _context.API.ChangeQuery(resultBuilder.ToString(), true);
                            }
                        }
                    }
                }
            }

            return resultBuilder.ToString();
        }

        public Control CreateSettingPanel()
        {
            return new SettingsControl(_settings);
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            return new List<Result>();
        }
    }
} 