using System.Diagnostics;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services
{
    /// <summary>
    /// Forwards WAV audio (base64) to the TranscribeAudio function.
    /// In Development this should point at the local Functions host
    /// (see Azure:TranscriptionFunctionUrl in appsettings.Development.json).
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

            var functionUrl = _config["Azure:TranscriptionFunctionUrl"];
            var apiKey = _config["TranscribeFunctionKey"];
            if (string.IsNullOrWhiteSpace(functionUrl))
            {
                throw new InvalidOperationException(
                    "Azure:TranscriptionFunctionUrl is not configured. " +
                    "For local dev set it to http://localhost:7004/api/Function1 in appsettings.Development.json.");
            }

            var locale = string.IsNullOrWhiteSpace(speechLocale) ? "en-US" : speechLocale.Trim();
            var sw = Stopwatch.StartNew();
            _logger.LogInformation(
                "Calling transcription function at {Url}. Locale={Locale} PayloadChars={Length}",
                functionUrl,
                locale,
                audioBase64?.Length ?? 0);

            using var request = new HttpRequestMessage(HttpMethod.Post, functionUrl)
            {
                Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { audio = audioBase64, language = locale }),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            // Local Functions host usually does not need a key; Azure-hosted does.
            if (!string.IsNullOrWhiteSpace(apiKey) && !IsLocalUrl(functionUrl))
            {
                request.Headers.TryAddWithoutValidation("x-functions-key", apiKey);
            }

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
                _logger.LogError(ex, "Transcription function timed out at {Url}", functionUrl);
                throw new TimeoutException($"Transcription function timed out at {functionUrl}.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling transcription function at {Url}", functionUrl);
                throw;
            }

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Transcription function failed. Url={Url} Status={Status} Body={Body}",
                    functionUrl,
                    (int)response.StatusCode,
                    Truncate(body, 500));
                throw new HttpRequestException(
                    $"Transcription function returned {(int)response.StatusCode} from {functionUrl}.");
            }

            using var doc = System.Text.Json.JsonDocument.Parse(body);
            var text = doc.RootElement.TryGetProperty("transcription", out var prop)
                ? prop.GetString() ?? string.Empty
                : string.Empty;

            _logger.LogInformation(
                "Transcription function returned. Url={Url} ElapsedMs={Elapsed} Chars={Chars}",
                functionUrl,
                sw.ElapsedMilliseconds,
                text.Length);
            return text;
        }

        private static bool IsLocalUrl(string url) =>
            url.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            || url.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);

        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max] + "…";
    }
}
