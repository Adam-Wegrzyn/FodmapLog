using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services
{
    public class AudioTranscriptionService : IAudioTranscriptionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<AudioTranscriptionService> _logger;

        public AudioTranscriptionService(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<AudioTranscriptionService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<string> TranscribeAsync(string audioBase64)
        {
            if (bool.TryParse(_config["UseAiStubs"], out var useAiStubs) && useAiStubs)
            {
                _logger.LogInformation("UseAiStubs enabled — returning local stub transcription.");
                await Task.Delay(400);
                return "I had oatmeal with milk at 8, then felt bloated around 10.";
            }

            var azureFunctionUrl = _config["Azure:TranscriptionFunctionUrl"];
            var apiKey = _config["TranscribeFunctionKey"];

            var request = new HttpRequestMessage(HttpMethod.Post, azureFunctionUrl)
            {
                Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { audio = audioBase64 }),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Add("x-functions-key", apiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("transcription").GetString() ?? string.Empty;
        }
    }
}
