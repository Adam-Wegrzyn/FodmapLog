using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AudioTranscriptionController : ControllerBase
    {
        private readonly IAudioTranscriptionService _audioService;

        public AudioTranscriptionController(IAudioTranscriptionService audioService)
        {
            _audioService = audioService;
        }

        [HttpPost("transcribe")]
        public async Task<IActionResult> Transcribe([FromBody] AudioRequestDto audioBase64)
        {
            if (string.IsNullOrEmpty(audioBase64?.value))
                return BadRequest("Audio data is required.");

            var language = NormalizeSpeechLocale(audioBase64.language);
            var transcription = await _audioService.TranscribeAsync(audioBase64.value, language);
            return Ok(new { transcription });
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
