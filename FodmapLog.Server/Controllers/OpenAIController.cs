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
        private readonly bool _useAiStubs;
        private readonly ILogger<OpenAIController> _logger;
        private const int MaxTranscriptLength = 8000;

        public OpenAIController(IConfiguration configuration, ILogger<OpenAIController> logger)
        {
            _apiKey = configuration["openAIApiKey"];
            _useAiStubs = false; //configuration.GetValue("UseAiStubs", false);
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

            //if (_useAiStubs || string.IsNullOrWhiteSpace(_apiKey))
            //{
            //    if (_useAiStubs)
            //    {
            //        _logger.LogInformation("UseAiStubs enabled — returning local stub daily logs. TranscriptLength={Length}", input.Transcript.Length);
            //        await Task.Delay(350, cancellationToken);
            //        return Content(BuildStubDailyLogsJson(), "application/json");
            //    }

            //    return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "OpenAI is not configured." });
            //}

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

            var isPolish = IsPolish(input.Language);
            var languageHint = isPolish
                ? "The transcript may be in Polish. Understand Polish food and symptom descriptions."
                : "The transcript may be in English.";
            var unitList = "Gram, Kilogram, Milligram, Liter, Milliliter, Teaspoon, Tablespoon, Cup, Piece, Slice, Drop, Pinch, Ounce, Pound, Fluid Ounce, Pint, Quart, Gallon, Can, Package, Bottle";
            var symptomList = "Nausea, Burping, Diarrhea, Constipation, Bloating, Abdominal Pain, Heartburn, Gas, Cramps, Vomiting, Appetite, Headache, Fatigue, Mood, Energy, Sleep Quality, Stress, Concentration, Motivation, Physical Activity, General Well-being";

            var prompt =
                $@"Convert the following user input (Meal and symptom dairy) into a JSON format with no additional text,
                without any formatting, code blocks, or extra characters.. Return only the JSON.
                {languageHint}
                Keep product.name in the language the user used (Polish food names are fine).
                Always use ENGLISH names for unit.name from this exact list: {unitList}.
                Always use ENGLISH names for symptomType.name from this exact list: {symptomList}.
                User Input: '{input.Transcript}'
                Symptom scale please convert to int -> 0 (excellent) - 10 (the worst)
                JSON Format Example:
                {jsonExample}
                Output only the JSON in this format based on the provided input.";

            _logger.LogInformation(
                "OpenAI meal/symptom extract requested. TranscriptLength={Length} Language={Language}",
                input.Transcript.Length,
                input.Language ?? "en");

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

                return Content(cleaned, "application/json");
            }
            catch (JsonException)
            {
                _logger.LogWarning("OpenAI returned unparseable JSON. Length={Length}", cleaned.Length);
                return UnprocessableEntity(new { error = "AI response could not be parsed as JSON." });
            }
        }

        private static string BuildStubDailyLogsJson()
        {
            var day = DateTime.Today;
            var mealAt = day.AddHours(8);
            var symptomAt = day.AddHours(10);
            var mealIso = mealAt.ToString("yyyy-MM-ddTHH:mm:ss");
            var symptomIso = symptomAt.ToString("yyyy-MM-ddTHH:mm:ss");

            return $$"""
            [
              {
                "date": "{{mealIso}}",
                "mealLog": {
                  "date": "{{mealIso}}",
                  "productQuantity": [
                    {
                      "product": { "name": "Oatmeal" },
                      "quantity": 1,
                      "unit": { "name": "Bowl" }
                    },
                    {
                      "product": { "name": "Milk" },
                      "quantity": 200,
                      "unit": { "name": "ml" }
                    }
                  ]
                },
                "symptomsLog": null
              },
              {
                "date": "{{symptomIso}}",
                "mealLog": null,
                "symptomsLog": {
                  "date": "{{symptomIso}}",
                  "symptoms": [
                    {
                      "symptomType": { "name": "Bloating" },
                      "symptomScale": 3
                    }
                  ]
                }
              }
            ]
            """;
        }

        private static bool IsPolish(string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return false;
            }

            return language.Trim().StartsWith("pl", StringComparison.OrdinalIgnoreCase);
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
        /// <summary>App UI language: en or pl (also accepts pl-PL / en-US).</summary>
        public string? Language { get; set; }
    }
}
