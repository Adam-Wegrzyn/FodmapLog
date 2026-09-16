using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
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
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing audio file transcription");
            string? tempFilePath = null;
            var sw = Stopwatch.StartNew();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic? data = JsonConvert.DeserializeObject(requestBody);

                if (data?.audio == null)
                {
                    _logger.LogWarning("Audio file is missing in the request.");
                    return new BadRequestObjectResult("Audio file is required.");
                }

                byte[] audioBytes;
                try
                {
                    audioBytes = Convert.FromBase64String((string)data.audio);
                    _logger.LogInformation("Audio decoded. Bytes={Length}", audioBytes.Length);
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Error decoding base64 audio data");
                    return new BadRequestObjectResult("Invalid base64 audio data.");
                }

                tempFilePath = Path.Combine(Path.GetTempPath(), $"fodmap-speech-{Guid.NewGuid():N}.wav");
                await File.WriteAllBytesAsync(tempFilePath, audioBytes);

                var apiKey = Environment.GetEnvironmentVariable("AzureSpeechApiKey");
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError("AzureSpeechApiKey is not configured.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                var region = Environment.GetEnvironmentVariable("AzureSpeechRegion");
                if (string.IsNullOrWhiteSpace(region))
                {
                    region = "eastus";
                }

                var config = SpeechConfig.FromSubscription(apiKey, region);
                config.SpeechRecognitionLanguage =
                    Environment.GetEnvironmentVariable("AzureSpeechLanguage") ?? "en-US";
                // Allow longer pauses between phrases without ending the whole session early.
                config.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "1500");
                config.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "10000");

                using var audioInput = AudioConfig.FromWavFileInput(tempFilePath);
                using var recognizer = new SpeechRecognizer(config, audioInput);

                // RecognizeOnceAsync stops at the first silence (~1s). Continuous recognition
                // walks the whole WAV so later speech after a pause is kept.
                var transcript = new StringBuilder();
                var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var canceledWithError = false;

                recognizer.Recognized += (_, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech
                        && !string.IsNullOrWhiteSpace(e.Result.Text))
                    {
                        if (transcript.Length > 0)
                        {
                            transcript.Append(' ');
                        }

                        transcript.Append(e.Result.Text.Trim());
                    }
                };

                recognizer.Canceled += (_, e) =>
                {
                    if (e.Reason == CancellationReason.Error)
                    {
                        canceledWithError = true;
                        _logger.LogError(
                            "Speech recognition canceled: ErrorCode={Code} Details={Details}",
                            e.ErrorCode,
                            e.ErrorDetails);
                    }

                    done.TrySetResult(false);
                };

                recognizer.SessionStopped += (_, _) => done.TrySetResult(true);

                _logger.LogInformation("Starting continuous speech recognition...");
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                // Bound wait so a hung session cannot block the function forever.
                var finished = await Task.WhenAny(done.Task, Task.Delay(TimeSpan.FromMinutes(2)))
                    .ConfigureAwait(false);
                if (finished != done.Task)
                {
                    _logger.LogWarning("Speech recognition timed out after 2 minutes.");
                }

                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

                var text = transcript.ToString().Trim();
                _logger.LogInformation(
                    "Speech recognition finished. ElapsedMs={Elapsed} Chars={Chars} CanceledError={Canceled}",
                    sw.ElapsedMilliseconds,
                    text.Length,
                    canceledWithError);

                if (string.IsNullOrWhiteSpace(text))
                {
                    return new BadRequestObjectResult("No speech could be recognized.");
                }

                if (canceledWithError && text.Length == 0)
                {
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                return new OkObjectResult(new { transcription = text });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing audio file");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch
                    {
                        // best-effort cleanup
                    }
                }
            }
        }
    }
}
