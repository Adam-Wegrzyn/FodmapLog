using Core.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenAI.Chat;

namespace FodmapLog.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("ai")]
    public class OpenAIController : ControllerBase
    {
        private readonly string? _apiKey;
        private readonly bool _useAiStubs;
        private readonly ILogger<OpenAIController> _logger;
        private readonly AiDailyLogNormalizer _normalizer;
        private const int MaxTranscriptLength = 8000;

        public OpenAIController(
            IConfiguration configuration,
            ILogger<OpenAIController> logger,
            AiDailyLogNormalizer normalizer)
        {
            _apiKey = configuration["openAIApiKey"];
            _useAiStubs = configuration.GetValue("UseAiStubs", false);
            _logger = logger;
            _normalizer = normalizer;
        }

        [HttpPost]
        [Route("GeneratemealLogFromAI")]
        public async Task<IActionResult> GeneratemealLogFromAI(
            [FromBody] TranscribedInput input,
            CancellationToken cancellationToken)
        {
            if (input?.Transcript is null || string.IsNullOrWhiteSpace(input.Transcript))
            {
                return BadRequest(new { error = "Transcript is required." });
            }

            if (input.Transcript.Length > MaxTranscriptLength)
            {
                return BadRequest(new { error = $"Transcript exceeds {MaxTranscriptLength} characters." });
            }

            var selectedDate = ParseSelectedDate(input.SelectedDate);
            var userId = this.RequireUserId();

            if (_useAiStubs || string.IsNullOrWhiteSpace(_apiKey))
            {
                if (_useAiStubs)
                {
                    _logger.LogInformation(
                        "UseAiStubs enabled — returning local stub daily logs. TranscriptLength={Length}",
                        input.Transcript.Length);
                    await Task.Delay(350, cancellationToken);
                    var stub = await _normalizer.NormalizeAsync(
                        BuildStubDailyLogsJson(selectedDate),
                        selectedDate,
                        userId,
                        cancellationToken);
                    return Ok(stub.Logs);
                }

                _logger.LogWarning("OpenAI is not configured (missing openAIApiKey).");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "AI is temporarily unavailable." });
            }

            _logger.LogInformation(
                "OpenAI meal/symptom extract requested. TranscriptLength={Length} Language={Language}",
                input.Transcript.Length,
                input.Language ?? "en");

            string raw;
            try
            {
                var client = new ChatClient(model: "gpt-4o", apiKey: _apiKey);
                var options = new ChatCompletionOptions
                {
                    Temperature = 0,
                    ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                };
                ChatCompletion completion = await client.CompleteChatAsync(
                    [
                        new SystemChatMessage(BuildSystemPrompt(input, selectedDate)),
                        new UserChatMessage(input.Transcript.Trim())
                    ],
                    options,
                    cancellationToken);
                raw = completion.Content[0].Text?.Trim() ?? string.Empty;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI request failed.");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "AI is temporarily unavailable." });
            }

            var cleaned = StripMarkdownFences(raw);
            var outcome = await _normalizer.NormalizeAsync(cleaned, selectedDate, userId, cancellationToken);
            if (!outcome.Success)
            {
                _logger.LogWarning("OpenAI JSON rejected. Error={Error} Length={Length}", outcome.Error, cleaned.Length);
                return UnprocessableEntity(new { error = outcome.Error });
            }

            return Ok(outcome.Logs);
        }

        private static string BuildSystemPrompt(TranscribedInput input, DateOnly? selectedDate)
        {
            var isPolish = IsPolish(input.Language);
            var languageHint = isPolish
                ? "The transcript may be in Polish. Understand Polish food and symptom wording."
                : "The transcript may be in English.";
            var day = selectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
            var now = string.IsNullOrWhiteSpace(input.ClientNow) ? DateTime.Now.ToString("o") : input.ClientNow.Trim();

            return
                $$"""
                You extract meals and gut/mood symptoms from a spoken food diary.
                {{languageHint}}
                The user is logging calendar day {{day}}. Use that date for every event. Times in the transcript belong to that day.
                The client's local now is {{now}}. If they say "just now" or omit a time, use that clock time.
                Return a JSON object only: { "events": [ ... ] }. No markdown.

                Each event:
                - date: ISO local datetime on {{day}} (example  {{day}}T08:00:00)
                - either mealLog OR symptomsLog; set the other to null
                - mealLog.productQuantity: product.name in the user's language; quantity as a number; unit.name from the unit list
                - symptomsLog.symptoms: symptomType.name from the lists below; symptomScale integer 0 through 5
                - Problem symptoms: 0 = none, 5 = worst/serious
                - How-I-feel symptoms: 0 = very bad, 5 = great (e.g. "mood was great" → Mood 5, not 0)

                Units (English names only): Gram, Kilogram, Milligram, Liter, Milliliter, Teaspoon, Tablespoon, Cup, Piece, Slice, Drop, Pinch, Ounce, Pound, Fluid Ounce, Pint, Quart, Gallon, Can, Package, Bottle
                Problem symptoms (5 = serious): Nausea, Burping, Diarrhea, Constipation, Bloating, Abdominal Pain, Heartburn, Gas, Cramps, Vomiting, Headache, Fatigue, Stress, Brain Fog, Dizziness, Anxiety, Joint Pain, Itching, Urgency, Incomplete Evacuation, Mucus, Palpitations, Irritability
                How-I-feel symptoms (5 = great): Appetite, Mood, Energy, Sleep Quality, Concentration, Motivation, Physical Activity, General Well-being

                Example events for a user who said they ate oatmeal with milk at 8 and felt bloated around 10:
                {
                  "events": [
                    {
                      "date": "{{day}}T08:00:00",
                      "mealLog": {
                        "date": "{{day}}T08:00:00",
                        "productQuantity": [
                          { "product": { "name": "oatmeal" }, "quantity": 50, "unit": { "name": "Gram" } },
                          { "product": { "name": "milk" }, "quantity": 200, "unit": { "name": "Milliliter" } }
                        ]
                      },
                      "symptomsLog": null
                    },
                    {
                      "date": "{{day}}T10:00:00",
                      "mealLog": null,
                      "symptomsLog": {
                        "date": "{{day}}T10:00:00",
                        "symptoms": [
                          { "symptomType": { "name": "Bloating" }, "symptomScale": 3 }
                        ]
                      }
                    }
                  ]
                }
                """;
        }

        private static DateOnly? ParseSelectedDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateOnly.TryParse(value, out var day) ? day : null;
        }

        private static string BuildStubDailyLogsJson(DateOnly? selectedDate)
        {
            var day = selectedDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today;
            var mealIso = day.AddHours(8).ToString("yyyy-MM-ddTHH:mm:ss");
            var symptomIso = day.AddHours(10).ToString("yyyy-MM-ddTHH:mm:ss");
            return $$"""
            {
              "events": [
                {
                  "date": "{{mealIso}}",
                  "mealLog": {
                    "date": "{{mealIso}}",
                    "productQuantity": [
                      { "product": { "name": "oatmeal" }, "quantity": 50, "unit": { "name": "Gram" } },
                      { "product": { "name": "milk" }, "quantity": 200, "unit": { "name": "Milliliter" } }
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
                      { "symptomType": { "name": "Bloating" }, "symptomScale": 3 }
                    ]
                  }
                }
              ]
            }
            """;
        }

        private static bool IsPolish(string? language) =>
            !string.IsNullOrWhiteSpace(language)
            && language.Trim().StartsWith("pl", StringComparison.OrdinalIgnoreCase);

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
        public string? Language { get; set; }
        /// <summary>Calendar day the user is viewing (yyyy-MM-dd).</summary>
        public string? SelectedDate { get; set; }
        /// <summary>Client local now (ISO).</summary>
        public string? ClientNow { get; set; }
    }
}
