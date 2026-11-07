using System.Text.Json;
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
            CancellationToken cancellationToken)
        {
            if (!TryReadEvents(rawJson, out var eventsJson, out var parseError))
            {
                return AiExtractOutcome.Fail(parseError!);
            }

            List<DailyLogDto>? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<List<DailyLogDto>>(eventsJson, JsonOptions);
            }
            catch (JsonException)
            {
                return AiExtractOutcome.Fail("AI response could not be parsed as daily logs.");
            }

            if (parsed == null)
            {
                return AiExtractOutcome.Fail("AI response could not be parsed as daily logs.");
            }

            var units = (await _repository.GetAllUnits(cancellationToken)).ToList();
            var symptoms = (await _repository.GetAllSymptomTypes(userId, cancellationToken)).ToList();

            var valid = new List<DailyLogDto>();
            foreach (var item in parsed)
            {
                if (TryNormalizeItem(item, selectedDate, units, symptoms, out var normalized))
                {
                    valid.Add(normalized);
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

        private static bool TryNormalizeItem(
            DailyLogDto item,
            DateOnly? selectedDate,
            IReadOnlyList<Unit> units,
            IReadOnlyList<SymptomType> symptomTypes,
            out DailyLogDto normalized)
        {
            normalized = item;
            var hasMeal = item.MealLog?.ProductQuantity?.Any(pq =>
                !string.IsNullOrWhiteSpace(pq.Product?.Name)) == true;
            var hasSymptoms = item.SymptomsLog?.Symptoms?.Any(s =>
                !string.IsNullOrWhiteSpace(s.SymptomType?.Name) || s.SymptomType?.Id > 0) == true;

            if (!hasMeal && !hasSymptoms)
            {
                return false;
            }

            var when = item.Date == default ? DateTime.Now : item.Date;
            if (selectedDate.HasValue)
            {
                when = selectedDate.Value.ToDateTime(TimeOnly.FromDateTime(when));
            }

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
                    ResolveSymptom(symptom, symptomTypes);
                    symptom.SymptomScale = (DataAccess.Enums.SymptomScale)ReferenceAliases.ClampScale((int)symptom.SymptomScale);
                }
            }

            normalized = item;
            return true;
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
