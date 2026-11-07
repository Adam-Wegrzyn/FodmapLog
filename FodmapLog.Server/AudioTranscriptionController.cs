using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("transcribe")]
    public class AudioTranscriptionController : ControllerBase
    {
        /// <summary>
        /// ~8MB base64 ≈ 3 min of 16 kHz 16-bit mono WAV. That is the product cap.
        /// </summary>
        private const int MaxBase64Length = 8_000_000;

        private readonly IAudioTranscriptionService _audioService;
        private readonly ILogger<AudioTranscriptionController> _logger;

        public AudioTranscriptionController(
            IAudioTranscriptionService audioService,
            ILogger<AudioTranscriptionController> logger)
        {
            _audioService = audioService;
            _logger = logger;
        }

        [HttpPost("transcribe")]
        [RequestSizeLimit(12_000_000)]
        public async Task<IActionResult> Transcribe([FromBody] AudioRequestDto audioBase64)
        {
            if (string.IsNullOrEmpty(audioBase64?.value))
            {
                return BadRequest(new { error = "Audio data is required." });
            }

            if (audioBase64.value.Length > MaxBase64Length)
            {
                _logger.LogWarning("Transcription rejected: payload too large. PayloadChars={Length}",
                    audioBase64.value.Length);
                return BadRequest(new { error = "Audio payload is too large. Keep recordings under three minutes." });
            }

            var language = NormalizeSpeechLocale(audioBase64.language);
            try
            {
                var transcription = await _audioService.TranscribeAsync(audioBase64.value, language);
                return Ok(new { transcription });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transcription failed. Locale={Locale} PayloadChars={Length}",
                    language, audioBase64.value.Length);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "Transcription is temporarily unavailable." });
            }
        }

        private static string NormalizeSpeechLocale(string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return "en-US";
            }

            var normalized = language.Trim();
            if (normalized.StartsWith("pl", StringComparison.OrdinalIgnoreCase))
            {
                return "pl-PL";
            }

            return "en-US";
        }
    }

    public class AudioRequestDto
    {
        public string value { get; set; } = string.Empty;
        public string? language { get; set; }
    }
}
