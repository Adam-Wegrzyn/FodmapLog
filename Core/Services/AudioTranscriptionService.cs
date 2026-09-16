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

            // Choose local function URL when enabled
            var useLocal = bool.TryParse(_config["UseLocalTranscriptionFunction"], out var parsedUseLocal) && parsedUseLocal;
            var functionUrl = useLocal
                ? _config["Azure:TranscriptionFunctionLocalUrl"] ?? "http://localhost:7071/api/Function1"
                : _config["Azure:TranscriptionFunctionUrl"];

            if (string.IsNullOrWhiteSpace(functionUrl))
            {
                _logger.LogError("Transcription function URL is not configured.");
                throw new InvalidOperationException("Transcription function URL not configured.");
            }

            // Function key - optional for local or remote (either header or ?code in url can be used)
            var apiKey = _config["TranscribeFunctionKey"] ?? _config["AzureFunctionsKey"];

            var payload = System.Text.Json.JsonSerializer.Serialize(new { audio = audioBase64 });
            using var request = new HttpRequestMessage(HttpMethod.Post, functionUrl)
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                // prefer header for Azure Functions; for local testing either header or ?code works
                request.Headers.Add("x-functions-key", apiKey);
            }

            _logger.LogDebug("Sending audio to transcription function. Url={Url} UseLocal={UseLocal}", functionUrl, useLocal);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling transcription function at {Url}", functionUrl);
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Transcription function returned {Status}. Body: {Body}", (int)response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("transcription", out var t))
                {
                    return t.GetString() ?? string.Empty;
                }

                _logger.LogWarning("Transcription response JSON doesn't contain 'transcription' property. Raw: {Raw}", json);
                return string.Empty;
            }
            catch (System.Text.Json.JsonException)
            {
                _logger.LogWarning("Failed to parse transcription response as JSON. Raw: {Raw}", json);
                throw;
            }
        }
    }
}
