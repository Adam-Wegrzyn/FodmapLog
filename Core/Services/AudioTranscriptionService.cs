using Core.Interfaces;
using Microsoft.Extensions.Configuration;

public class AudioTranscriptionService : IAudioTranscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AudioTranscriptionService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> TranscribeAsync(string audioBase64)
    {
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
        return doc.RootElement.GetProperty("transcription").GetString();
    }
}
