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
            _logger.LogInformation("Processing audio file transcryption");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                if (data?.audio == null)
                {
                    return new BadRequestObjectResult("Audio file is required.");
                }

                byte[] audioBytes = Convert.FromBase64String((string)data.audio);
                string tempFilePath = Path.GetTempFileName();
                await File.WriteAllBytesAsync(tempFilePath, audioBytes);

                var config = SpeechConfig.FromSubscription(_configuration["AzureSpeechApiKey"], "eastus");
                config.SpeechRecognitionLanguage = "en-US";

                using var audioInput = AudioConfig.FromWavFileInput(tempFilePath);
                using var recognizer = new SpeechRecognizer(config, audioInput);

                var result = await recognizer.RecognizeOnceAsync();

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
                _logger.LogError($"Error processing audio file: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        }
    }
