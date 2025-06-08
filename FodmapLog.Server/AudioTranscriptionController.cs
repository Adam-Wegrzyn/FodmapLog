using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            if (string.IsNullOrEmpty(audioBase64.value))
                return BadRequest("Audio data is required.");

            var transcription = await _audioService.TranscribeAsync(audioBase64.value);
            return Ok(new { transcription });
        }
    }

    public class AudioRequestDto
    {
        public string value { get; set; }
    }
}
