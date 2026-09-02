using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace FodmapLog.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OpenAIController : ControllerBase
    {
        private readonly string? _apiKey;
        private readonly ILogger<OpenAIController> _logger;
        private const int MaxTranscriptLength = 8000;

        public OpenAIController(IConfiguration configuration, ILogger<OpenAIController> logger)
        {
            _apiKey = configuration["openAIApiKey"];
            _logger = logger;
        }

        [HttpPost]
        [Route("GeneratemealLogFromAI")]
        public async Task<IActionResult> GeneratemealLogFromAI([FromBody] TranscribedInput input, CancellationToken cancellationToken)
        {
            if (input?.Transcript is null || string.IsNullOrWhiteSpace(input.Transcript))
            {
                return BadRequest(new { error = "Transcript is required." });
            }

            if (input.Transcript.Length > MaxTranscriptLength)
            {
                return BadRequest(new { error = $"Transcript exceeds {MaxTranscriptLength} characters." });
            }

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "OpenAI is not configured." });
            }

            var jsonExample = @"[
   {
        ""date"": ""2025-05-10T06:11:08.945"",
        ""mealLog"": {
          ""date"": ""2025-05-10T06:11:08.945"",
          ""productQuantity"": [
            {
              ""product"": {
                ""name"": ""płatki ryżowe""
              },
              ""quantity"": 100,
              ""unit"":{
                    ""name"" : ""Kilogram""
                }
            },
            {
              ""product"": {
                ""name"": ""mleko""
              },
              ""quantity"": 1,
              ""unit"": {
                    ""name"" : ""Liter""
                }
            }
          ]
        },
        ""symptomsLog"": null
      },
      {
        ""date"": ""2025-05-10T08:31:00"",
        ""mealLog"": null,
        ""symptomsLog"": {
          ""date"": ""2025-05-10T08:31:00"",
          ""symptoms"": [
            {
              ""symptomType"": {
                    ""name"" : ""Nausea""
                },
              ""symptomScale"": 1
            },
            {
              ""symptomType"": {
                    ""name"" : ""Burping""
                },
              ""symptomScale"": 2
            }
          ]
        }
      }
    ]";

            ChatClient client = new(model: "gpt-4o", apiKey: _apiKey);

            var prompt =
                $@"Convert the following user input (Meal and symptom dairy) into a JSON format with no additional text,
                without any formatting, code blocks, or extra characters.. Return only the JSON.
                User Input: '{input.Transcript}'
                Symptom scale please convert to int -> 0 (excellent) - 10 (the worst)
                JSON Format Example:
                {jsonExample}
                Output only the JSON in this format based on the provided input.";

            _logger.LogInformation("OpenAI meal/symptom extract requested. TranscriptLength={Length}", input.Transcript.Length);

            ChatCompletion completion = await client.CompleteChatAsync(
                [new UserChatMessage(prompt)],
                cancellationToken: cancellationToken);
            var raw = completion.Content[0].Text?.Trim() ?? string.Empty;
            var cleaned = StripMarkdownFences(raw);

            try
            {
                using var document = JsonDocument.Parse(cleaned);
                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return UnprocessableEntity(new { error = "AI response was not a JSON array." });
                }

                // Return typed JSON array (not a quoted string) for the Angular client.
                return Content(cleaned, "application/json");
            }
            catch (JsonException)
            {
                _logger.LogWarning("OpenAI returned unparseable JSON. Length={Length}", cleaned.Length);
                return UnprocessableEntity(new { error = "AI response could not be parsed as JSON." });
            }
        }

        private static string StripMarkdownFences(string text)
        {
            var trimmed = text.Trim();
            if (!trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                return trimmed;
            }

            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline < 0)
            {
                return trimmed;
            }

            trimmed = trimmed[(firstNewline + 1)..];
            var fence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (fence >= 0)
            {
                trimmed = trimmed[..fence];
            }

            return trimmed.Trim();
        }
    }

    public class TranscribedInput
    {
        public string Transcript { get; set; } = string.Empty;
    }
}
