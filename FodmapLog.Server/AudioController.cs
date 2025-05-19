using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly IAudioTranscriptionService _audioService;

        public AudioController(IAudioTranscriptionService audioService)
        {
            _audioService = audioService;
        }

        [HttpPost("transcribe")]
        public async Task<IActionResult> Transcribe([FromBody] AudioRequestDto request)
        {
            if (string.IsNullOrEmpty(request.AudioBase64))
                return BadRequest("Audio data is required.");

            var transcription = await _audioService.TranscribeAsync(request.AudioBase64);
            return Ok(new { transcription });
        }
    }

    public class AudioRequestDto
    {
        public string AudioBase64 { get; set; }
    }
}
