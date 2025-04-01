using Flow.Launcher.Plugin;

namespace Flow.Launcher.Plugin.AzureOpenAI
{
    public class Settings
    {
        public string Endpoint { get; set; } = "https://your-endpoint.openai.azure.com/";
        public string ApiKey { get; set; } = "your-api-key";
        public string ModelName { get; set; } = "your-model-name";
    }
} 