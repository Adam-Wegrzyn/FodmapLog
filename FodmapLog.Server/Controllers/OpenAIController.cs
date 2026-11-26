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
            var recordingClock = RecordingClock.FromClient(input.ClientNow, input.TimeZone, input.UtcOffsetMinutes);

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
                        cancellationToken,
                        recordingClock);
                    return Ok(stub.Logs);
                }

                _logger.LogWarning("OpenAI is not configured (missing openAIApiKey).");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "AI is temporarily unavailable." });
            }

            _logger.LogInformation(
                "OpenAI meal/symptom extract requested. TranscriptLength={Length} Language={Language} TimeZone={TimeZone} UtcOffsetMinutes={UtcOffsetMinutes}",
                input.Transcript.Length,
                input.Language ?? "en",
                input.TimeZone ?? "",
                input.UtcOffsetMinutes);

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
                        new SystemChatMessage(BuildSystemPrompt(input, selectedDate, recordingClock)),
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
            var outcome = await _normalizer.NormalizeAsync(cleaned, selectedDate, userId, cancellationToken, recordingClock);
            if (!outcome.Success)
            {
                _logger.LogWarning("OpenAI JSON rejected. Error={Error} Length={Length}", outcome.Error, cleaned.Length);
                return UnprocessableEntity(new { error = outcome.Error });
            }

            return Ok(outcome.Logs);
        }

        private static string BuildSystemPrompt(TranscribedInput input, DateOnly? selectedDate, RecordingClock clock)
        {
            var isPolish = IsPolish(input.Language);
            var languageHint = isPolish
                ? "The transcript may be in Polish. Understand Polish food and symptom wording."
                : "The transcript may be in English.";
            var openDay = selectedDate?.ToString("yyyy-MM-dd") ?? clock.LocalNow.ToString("yyyy-MM-dd");
            var localNow = clock.LocalNow.ToString("yyyy-MM-ddTHH:mm:ss");
            var zone = string.IsNullOrWhiteSpace(input.TimeZone) ? "unknown" : input.TimeZone.Trim();
            var offset = clock.Zone.Offset;
            var offsetText = (offset < TimeSpan.Zero ? "-" : "+")
                + offset.Duration().ToString(@"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
            var yesterday = clock.LocalNow.Date.AddDays(-1).ToString("yyyy-MM-dd");

            return
                $$"""
                You extract meals and gut/mood symptoms from a spoken food diary.
                {{languageHint}}

                Recording reference. This is the user's wall clock when they recorded, not UTC:
                - Local date-time: {{localNow}}
                - IANA timezone: {{zone}}
                - UTC offset: {{offsetText}}
                The calendar day open in the app is {{openDay}}. That is only where they are looking. Do not put every event on that day.

                Give each event its own local date and clock time, resolved against the recording reference:
                - wczoraj / yesterday = the previous local calendar day
                - przedwczoraj / day before yesterday = two local days before
                - dziś / dzisiaj / today = the recording's local day
                - rano / morning, when no clock was said = 07:30
                - w południe / noon = 12:00
                - wieczorem / evening = 19:00
                - przed snem / before bed / before sleep = 23:00
                - na śniadanie / breakfast = 08:00
                - na obiad / lunch = 13:00
                - na kolację / dinner = 19:00
                - "X godzin później" / "X hours later" / "półtorej godziny później" / "an hour and a half later" = that long after the previous event, even across midnight
                - explicit clocks ("o dwudziestej", "o 20", "o 9", "at 8", "at 8 pm") = that clock
                - "rano następnego dnia" / "the next morning" = 07:30 on the day after the previous event
                - If they gave no time at all, use the recording local time {{localNow}}
                - Never output a time after {{localNow}}

                Return a JSON object only: { "events": [ ... ] } in chronological order. No markdown.

                Each event:
                - timeExpression: the short time phrase copied from the user. Examples: "wczoraj o dwudziestej", "półtorej godziny później", "przed snem", "rano następnego dnia". Use "" only when they said no time.
                - time: a structured hint so the server can recompute the instant. dayOffset is -2, -1, or 0 relative to the recording day (omit it when the phrase is only "X hours later" or "the next morning"). clock is "HH:mm" or null. partOfDay is one of morning, noon, afternoon, evening, beforeSleep, breakfast, lunch, dinner, or null. minutesAfterPrevious is an integer or null.
                - date: that moment as local ISO with no Z and no offset (example {{yesterday}}T20:00:00). Wall clock, not UTC.
                - either mealLog OR symptomsLog; set the other to null
                - mealLog.date and symptomsLog.date: the same local datetime
                - mealLog.productQuantity: product.name in the user's language; quantity as a number; unit.name from the unit list
                - symptomsLog.symptoms: symptomType.name from the lists; spokenName is the user's own short wording; symptomScale integer 0 through 5
                - Do not upgrade wording. ciężkość, dyskomfort, heaviness, discomfort, "lekka ciężkość", "lekki dyskomfort żołądka" map to Bloating, never Abdominal Pain. Use Abdominal Pain only when they said ból, pain, or bolało.
                - lekki / slight / mild → symptomScale 1
                - Do not emit an event for "the rest was fine", "reszta było dobrze", or "było dobrze"
                - Problem symptoms: 0 = none, 5 = worst/serious
                - How-I-feel symptoms: 0 = very bad, 5 = great (e.g. "mood was great" → Mood 5, not 0)

                Units (English names only): Gram, Kilogram, Milligram, Liter, Milliliter, Teaspoon, Tablespoon, Cup, Piece, Slice, Drop, Pinch, Ounce, Pound, Fluid Ounce, Pint, Quart, Gallon, Can, Package, Bottle
                Problem symptoms (5 = serious): Nausea, Burping, Diarrhea, Constipation, Bloating, Abdominal Pain, Heartburn, Gas, Cramps, Vomiting, Headache, Fatigue, Stress, Brain Fog, Dizziness, Anxiety, Joint Pain, Itching, Urgency, Incomplete Evacuation, Mucus, Palpitations, Irritability
                How-I-feel symptoms (5 = great): Appetite, Mood, Energy, Sleep Quality, Concentration, Motivation, Physical Activity, General Well-being

                Example for a note recorded at {{localNow}} whose speaker ate rice yesterday at 20:00 and felt slight heaviness an hour and a half later:
                {
                  "events": [
                    {
                      "timeExpression": "wczoraj o dwudziestej",
                      "time": { "dayOffset": -1, "clock": "20:00", "partOfDay": null, "minutesAfterPrevious": null },
                      "date": "{{yesterday}}T20:00:00",
                      "mealLog": {
                        "date": "{{yesterday}}T20:00:00",
                        "productQuantity": [
                          { "product": { "name": "ryż" }, "quantity": 100, "unit": { "name": "Gram" } }
                        ]
                      },
                      "symptomsLog": null
                    },
                    {
                      "timeExpression": "półtorej godziny później",
                      "time": { "dayOffset": null, "clock": null, "partOfDay": null, "minutesAfterPrevious": 90 },
                      "date": "{{yesterday}}T21:30:00",
                      "mealLog": null,
                      "symptomsLog": {
                        "date": "{{yesterday}}T21:30:00",
                        "symptoms": [
                          { "symptomType": { "name": "Bloating" }, "spokenName": "lekka ciężkość", "symptomScale": 1 }
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
        /// <summary>Client local now, wall clock without Z (ISO). A trailing Z is accepted and converted.</summary>
        public string? ClientNow { get; set; }
        /// <summary>IANA timezone, for example Asia/Bangkok.</summary>
        public string? TimeZone { get; set; }
        /// <summary>Minutes east of UTC. Asia/Bangkok is 420.</summary>
        public int? UtcOffsetMinutes { get; set; }
    }
}
