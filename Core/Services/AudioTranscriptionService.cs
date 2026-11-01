using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services
{
    /// <summary>
    /// Sends the recorded WAV to Azure Speech REST (short-audio) in-process.
    /// No Azure Function hop — keys live on the web app.
    /// </summary>
    public class AudioTranscriptionService : IAudioTranscriptionService
    {
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(2);

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

        public async Task<string> TranscribeAsync(string audioBase64, string speechLocale = "en-US")
        {
            if (bool.TryParse(_config["UseAiStubs"], out var useAiStubs) && useAiStubs)
            {
                _logger.LogInformation("UseAiStubs enabled — returning local stub transcription. Locale={Locale}", speechLocale);
                await Task.Delay(400);
                return speechLocale.StartsWith("pl", StringComparison.OrdinalIgnoreCase)
                    ? "Zjadłem owsiankę z mlekiem o ósmej, potem około dziesiątej miałem wzdęcia."
                    : "I had oatmeal with milk at 8, then felt bloated around 10.";
            }

            var apiKey = _config["AzureSpeechApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "AzureSpeechApiKey is not configured. Set it in App Settings or user secrets.");
            }

            var region = _config["AzureSpeechRegion"];
            if (string.IsNullOrWhiteSpace(region))
            {
                region = "eastus";
            }

            var locale = NormalizeLocale(speechLocale);
            byte[] wav;
            try
            {
                wav = Convert.FromBase64String(audioBase64 ?? string.Empty);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Audio payload is not valid base64.", ex);
            }

            var url =
                $"https://{region}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1" +
                $"?language={Uri.EscapeDataString(locale)}&format=simple";

            var sw = Stopwatch.StartNew();
            _logger.LogInformation(
                "Calling Azure Speech. Region={Region} Locale={Locale} Bytes={Bytes}",
                region,
                locale,
                wav.Length);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new ByteArrayContent(wav);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

            using var cts = new CancellationTokenSource(RequestTimeout);
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cts.Token);
            }
            catch (OperationCanceledException) when (!cts.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Azure Speech timed out. Region={Region}", region);
                throw new TimeoutException("Azure Speech timed out.", ex);
            }

            var body = await response.Content.ReadAsStringAsync(cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Azure Speech failed. Status={Status} Body={Body}",
                    (int)response.StatusCode,
                    Truncate(body, 400));
                throw new HttpRequestException($"Azure Speech returned {(int)response.StatusCode}.");
            }

            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            var root = doc.RootElement;
            var text = root.TryGetProperty("DisplayText", out var display)
                ? display.GetString() ?? string.Empty
                : string.Empty;
            text = text.Trim();

            _logger.LogInformation(
                "Azure Speech finished. ElapsedMs={Elapsed} Chars={Chars} Status={Status}",
                sw.ElapsedMilliseconds,
                text.Length,
                root.TryGetProperty("RecognitionStatus", out var status) ? status.GetString() : "unknown");

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("No speech could be recognized.");
            }

            return text;
        }

        private static string NormalizeLocale(string? speechLocale)
        {
            if (string.IsNullOrWhiteSpace(speechLocale))
            {
                return "en-US";
            }

            return speechLocale.Trim().StartsWith("pl", StringComparison.OrdinalIgnoreCase)
                ? "pl-PL"
                : "en-US";
        }

        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max] + "…";
    }
}
