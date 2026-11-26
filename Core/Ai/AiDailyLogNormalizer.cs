using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Data.Common.DTO;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace Core.Ai
{
    public class AiExtractOutcome
    {
        public bool Success { get; init; }
        public IReadOnlyList<DailyLogDto> Logs { get; init; } = Array.Empty<DailyLogDto>();
        public string? Error { get; init; }

        public static AiExtractOutcome Ok(IReadOnlyList<DailyLogDto> logs) =>
            new() { Success = true, Logs = logs };

        public static AiExtractOutcome Fail(string error) =>
            new() { Success = false, Error = error };
    }

    public class AiDailyLogNormalizer
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IFodmapLogRepository _repository;

        public AiDailyLogNormalizer(IFodmapLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<AiExtractOutcome> NormalizeAsync(
            string rawJson,
            DateOnly? selectedDate,
            string userId,
            CancellationToken cancellationToken,
            RecordingClock? recordingClock = null)
        {
            _ = selectedDate;
            if (!TryReadEvents(rawJson, out var eventsJson, out var parseError))
            {
                return AiExtractOutcome.Fail(parseError!);
            }

            JsonElement events;
            try
            {
                using var document = JsonDocument.Parse(eventsJson);
                events = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                return AiExtractOutcome.Fail("AI response could not be parsed as daily logs.");
            }

            if (events.ValueKind != JsonValueKind.Array)
            {
                return AiExtractOutcome.Fail("AI response could not be parsed as daily logs.");
            }

            var clock = recordingClock ?? RecordingClock.FromClient(null, null, null);
            var units = (await _repository.GetAllUnits(cancellationToken)).ToList();
            var symptoms = (await _repository.GetAllSymptomTypes(userId, cancellationToken)).ToList();

            var valid = new List<DailyLogDto>();
            DateTime? previousCivil = null;
            foreach (var element in events.EnumerateArray())
            {
                if (!TryReadItem(element, out var item, out var expression, out var hint) || item == null)
                {
                    continue;
                }

                var modelDate = item.Date == default ? (DateTime?)null : item.Date;
                var resolved = SpokenTimeResolver.ResolveEvent(expression, hint, modelDate, clock, previousCivil);
                if (TryNormalizeItem(item, resolved.CivilLocal, units, symptoms, out var normalized))
                {
                    valid.Add(normalized);
                    previousCivil = resolved.CivilLocal;
                }
            }

            return AiExtractOutcome.Ok(valid);
        }

        public static bool TryReadEvents(string rawJson, out string eventsJson, out string? error)
        {
            eventsJson = string.Empty;
            error = null;
            try
            {
                using var document = JsonDocument.Parse(rawJson);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    eventsJson = root.GetRawText();
                    return true;
                }

                if (root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("events", out var events)
                    && events.ValueKind == JsonValueKind.Array)
                {
                    eventsJson = events.GetRawText();
                    return true;
                }

                error = "AI response was not a JSON array of daily logs.";
                return false;
            }
            catch (JsonException)
            {
                error = "AI response could not be parsed as JSON.";
                return false;
            }
        }

        private static bool TryReadItem(
            JsonElement element,
            out DailyLogDto? item,
            out string? expression,
            out SpokenTimeHint? hint)
        {
            item = null;
            expression = null;
            hint = null;
            if (element.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            expression = ReadString(element, "timeExpression");
            hint = ReadHint(element);

            JsonObject node;
            try
            {
                node = JsonNode.Parse(element.GetRawText())!.AsObject();
            }
            catch (JsonException)
            {
                return false;
            }

            ScrubDate(node, ref expression);
            if (node["mealLog"] is JsonObject meal)
            {
                ScrubDate(meal, ref expression);
            }

            if (node["symptomsLog"] is JsonObject symptoms)
            {
                ScrubDate(symptoms, ref expression);
            }

            try
            {
                item = node.Deserialize<DailyLogDto>(JsonOptions);
                return item != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static void ScrubDate(JsonObject obj, ref string? expression)
        {
            if (obj["date"] is not JsonValue value || !value.TryGetValue<string>(out var text))
            {
                return;
            }

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(expression))
            {
                expression = text;
            }

            obj.Remove("date");
        }

        private static string? ReadString(JsonElement obj, string name)
        {
            if (!TryProperty(obj, name, out var value) || value.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var text = value.GetString();
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        private static SpokenTimeHint? ReadHint(JsonElement element)
        {
            if (!TryProperty(element, "time", out var time) || time.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            int? dayOffset = TryProperty(time, "dayOffset", out var day) && day.TryGetInt32(out var dayValue)
                ? dayValue
                : null;
            int? minutes = TryProperty(time, "minutesAfterPrevious", out var after) && after.TryGetInt32(out var minuteValue)
                ? minuteValue
                : null;
            var hint = new SpokenTimeHint
            {
                DayOffset = dayOffset,
                MinutesAfterPrevious = minutes,
                Clock = ReadString(time, "clock"),
                PartOfDay = ReadString(time, "partOfDay")
            };
            return hint.HasValue ? hint : null;
        }

        private static bool TryProperty(JsonElement obj, string name, out JsonElement value)
        {
            if (obj.ValueKind == JsonValueKind.Object && obj.TryGetProperty(name, out value))
            {
                return true;
            }

            if (obj.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in obj.EnumerateObject())
                {
                    if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        private static bool TryNormalizeItem(
            DailyLogDto item,
            DateTime when,
            IReadOnlyList<Unit> units,
            IReadOnlyList<SymptomType> symptomTypes,
            out DailyLogDto normalized)
        {
            normalized = item;
            var hasMeal = item.MealLog?.ProductQuantity?.Any(pq =>
                !string.IsNullOrWhiteSpace(pq.Product?.Name)) == true;
            var hasSymptoms = item.SymptomsLog?.Symptoms?.Any(s =>
                !string.IsNullOrWhiteSpace(s.SymptomType?.Name)
                || s.SymptomType?.Id > 0
                || !string.IsNullOrWhiteSpace(s.SpokenName)) == true;

            if (!hasMeal && !hasSymptoms)
            {
                return false;
            }

            when = DateTime.SpecifyKind(when, DateTimeKind.Unspecified);
            item.Date = when;
            if (hasMeal && item.MealLog != null)
            {
                item.MealLog.Date = when;
                item.SymptomsLog = null;
                foreach (var pq in item.MealLog.ProductQuantity!)
                {
                    ResolveUnit(pq, units);
                }
            }
            else if (item.SymptomsLog != null)
            {
                item.SymptomsLog.Date = when;
                item.MealLog = null;
                foreach (var symptom in item.SymptomsLog.Symptoms)
                {
                    AdjustSymptom(symptom);
                    ResolveSymptom(symptom, symptomTypes);
                }
            }

            normalized = item;
            return true;
        }

        private static void AdjustSymptom(SymptomDto symptom)
        {
            var spoken = FirstText(symptom.SpokenName, symptom.SymptomType?.Name);
            var wording = ReferenceAliases.DiscomfortWording(symptom.SpokenName)
                ?? ReferenceAliases.DiscomfortWording(symptom.SymptomType?.Name);
            if (ReferenceAliases.IsDiscomfortNotPain(spoken))
            {
                symptom.SymptomType ??= new SymptomTypeDto();
                symptom.SymptomType.Id = ReferenceAliases.ResolveSymptomTypeId("Bloating") ?? 5;
                symptom.SymptomType.Name = "Bloating";
            }

            if (!string.IsNullOrWhiteSpace(wording))
            {
                symptom.SpokenName = wording;
            }

            var scale = ReferenceAliases.ClampScale((int)symptom.SymptomScale);
            if (ReferenceAliases.MentionsMild(spoken) && (scale == 0 || scale >= 3))
            {
                scale = 1;
            }

            symptom.SymptomScale = (DataAccess.Enums.SymptomScale)scale;
        }

        private static string? FirstText(string? preferred, string? fallback)
        {
            if (!string.IsNullOrWhiteSpace(preferred))
            {
                return preferred.Trim();
            }

            return string.IsNullOrWhiteSpace(fallback) ? null : fallback.Trim();
        }

        private static void ResolveUnit(ProductQuantityDto pq, IReadOnlyList<Unit> units)
        {
            var id = pq.Unit?.Id > 0 ? pq.Unit.Id : ReferenceAliases.ResolveUnitId(pq.Unit?.Name);
            if (id == null)
            {
                return;
            }

            var match = units.FirstOrDefault(u => u.Id == id);
            if (match == null)
            {
                return;
            }

            pq.Unit = new Unit { Id = match.Id, Name = match.Name };
        }

        private static void ResolveSymptom(SymptomDto symptom, IReadOnlyList<SymptomType> types)
        {
            var id = symptom.SymptomType?.Id > 0
                ? symptom.SymptomType.Id
                : ReferenceAliases.ResolveSymptomTypeId(symptom.SymptomType?.Name);
            SymptomType? match = id != null
                ? types.FirstOrDefault(t => t.Id == id)
                : null;

            if (match == null && !string.IsNullOrWhiteSpace(symptom.SymptomType?.Name))
            {
                var name = symptom.SymptomType.Name.Trim();
                match = types.FirstOrDefault(t =>
                    t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            if (match == null)
            {
                return;
            }

            symptom.SymptomType = new SymptomTypeDto
            {
                Id = match.Id,
                Name = match.Name,
                ScaleKind = match.ScaleKind
            };
        }
    }
}
