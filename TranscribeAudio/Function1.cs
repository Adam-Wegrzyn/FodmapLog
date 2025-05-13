using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TranscribeAudio
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly IConfiguration _configuration;

        public Function1(ILogger<Function1> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function("Function1")]
        public async Task<IActionResult> Run([Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing audio file transcription");

            try
            {
                // Read and log the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Request body successfully read.");

                dynamic data = JsonConvert.DeserializeObject(requestBody);

                if (data?.audio == null)
                {
                    _logger.LogWarning("Audio file is missing in the request.");
                    return new BadRequestObjectResult("Audio file is required.");
                }

                // Decode the base64 audio and log details
                _logger.LogInformation("Decoding base64 audio data...");
                byte[] audioBytes;
                try
                {
                    audioBytes = Convert.FromBase64String((string)data.audio);
                    _logger.LogInformation($"Audio data successfully decoded. Size: {audioBytes.Length} bytes.");
                }
                catch (FormatException ex)
                {
                    _logger.LogError($"Error decoding base64 audio data: {ex.Message}");
                    return new BadRequestObjectResult("Invalid base64 audio data.");
                }

                // Write audio to a temporary file and log the file path
                string tempFilePath = Path.GetTempFileName();
                _logger.LogInformation($"Temporary file created at: {tempFilePath}");
                await File.WriteAllBytesAsync(tempFilePath, audioBytes);
                _logger.LogInformation("Audio data successfully written to temporary file.");

                // Configure Azure Speech SDK
                var apiKey = Environment.GetEnvironmentVariable("AzureSpeechApiKey");
                var config = SpeechConfig.FromSubscription(apiKey, "eastus");
                config.SpeechRecognitionLanguage = "en-US";
                _logger.LogInformation("Azure Speech SDK configured successfully.");

                // Perform speech recognition
                using var audioInput = AudioConfig.FromWavFileInput(tempFilePath);
                using var recognizer = new SpeechRecognizer(config, audioInput);

                _logger.LogInformation("Starting speech recognition...");
                var result = await recognizer.RecognizeOnceAsync();

                // Handle recognition results
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    _logger.LogInformation($"Transcription successful: {result.Text}");
                    return new OkObjectResult(new { transcription = result.Text });
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    _logger.LogWarning("No speech could be recognized.");
                    return new BadRequestObjectResult("No speech could be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    _logger.LogError($"Speech recognition canceled: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        _logger.LogError($"ErrorCode={cancellation.ErrorCode}");
                        _logger.LogError($"ErrorDetails={cancellation.ErrorDetails}");
                    }

                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
                else
                {
                    _logger.LogError($"Speech recognition failed: {result.Reason}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error processing audio file: {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}