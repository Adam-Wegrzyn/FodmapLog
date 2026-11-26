using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Ai
{
    public readonly record struct ResolvedSpokenTime(DateTime CivilLocal, DateTime Utc);

    /// <summary>
    /// Optional machine hint from the model. The phrase resolver wins when it parses;
    /// this is the fallback when the phrase is missing or ambiguous.
    /// </summary>
    public sealed class SpokenTimeHint
    {
        public int? DayOffset { get; init; }
        public string? Clock { get; init; }
        public string? PartOfDay { get; init; }
        public int? MinutesAfterPrevious { get; init; }

        public bool HasValue =>
            DayOffset.HasValue
            || MinutesAfterPrevious.HasValue
            || !string.IsNullOrWhiteSpace(Clock)
            || !string.IsNullOrWhiteSpace(PartOfDay);
    }

    /// <summary>
    /// Turns Polish and English time phrases into a civil local time and the matching UTC instant.
    /// "Now" is the recording clock, never the server clock. Results are never in the future.
    /// </summary>
    public static class SpokenTimeResolver
    {
        private static readonly (string Phrase, int Hour)[] PolishHours =
        [
            ("dwudziestej trzeciej", 23),
            ("dwudziestej drugiej", 22),
            ("dwudziestej pierwszej", 21),
            ("dwudziestej", 20),
            ("dziewietnastej", 19),
            ("osiemnastej", 18),
            ("siedemnastej", 17),
            ("szesnastej", 16),
            ("pietnastej", 15),
            ("czternastej", 14),
            ("trzynastej", 13),
            ("dwunastej", 12),
            ("jedenastej", 11),
            ("dziesiatej", 10),
            ("dziewiatej", 9),
            ("osmej", 8),
            ("siodmej", 7),
            ("szostej", 6),
            ("piatej", 5),
            ("czwartej", 4),
            ("trzeciej", 3),
            ("drugiej", 2),
            ("pierwszej", 1),
            ("polnocy", 0)
        ];

        private static readonly (string[] Phrases, TimeOnly Time)[] PartsOfDay =
        [
            (["przed snem", "przed spaniem", "before bed", "before sleep", "before i slept", "before going to sleep"], new TimeOnly(23, 0)),
            (["na sniadanie", "for breakfast", "at breakfast", "breakfast"], new TimeOnly(8, 0)),
            (["na obiad", "for lunch", "at lunch", "lunch"], new TimeOnly(13, 0)),
            (["na kolacje", "for dinner", "at dinner", "dinner"], new TimeOnly(19, 0)),
            (["w poludnie", "at noon", "noon"], new TimeOnly(12, 0)),
            (["rano", "in the morning", "this morning", "morning"], new TimeOnly(7, 30)),
            (["popoludniu", "po poludniu", "in the afternoon", "afternoon"], new TimeOnly(15, 0)),
            (["wieczorem", "wieczor", "in the evening", "evening"], new TimeOnly(19, 0))
        ];

        public static ResolvedSpokenTime ResolveEvent(
            string? expression,
            SpokenTimeHint? hint,
            DateTime? modelCivilOrUtc,
            RecordingClock clock,
            DateTime? previousCivil)
        {
            if (TryResolveExpression(expression, clock, previousCivil, out var fromPhrase))
            {
                return fromPhrase;
            }

            if (hint is { HasValue: true }
                && TryResolveHint(hint, clock, previousCivil, out var fromHint))
            {
                return fromHint;
            }

            if (TryInterpretModelDate(modelCivilOrUtc, clock, out var fromModel))
            {
                return Finish(fromModel, clock);
            }

            return Finish(clock.LocalNow, clock);
        }

        public static bool TryResolveExpression(
            string? expression,
            RecordingClock clock,
            DateTime? previousCivil,
            out ResolvedSpokenTime resolved)
        {
            resolved = default;
            var text = Fold(expression);
            if (text.Length == 0 || text.Length > 160)
            {
                return false;
            }

            if (TryRelativeMinutes(text, out var minutes))
            {
                var anchor = previousCivil ?? clock.LocalNow;
                return Finish(anchor.AddMinutes(minutes), clock, out resolved);
            }

            var nextDay = ContainsAny(text,
                "nastepnego dnia", "nastepny dzien", "na nastepny dzien", "nastepnego ranka",
                "next day", "the next day", "next morning");
            var dayBeforeYesterday = text.Contains("przedwczoraj", StringComparison.Ordinal)
                || text.Contains("day before yesterday", StringComparison.Ordinal);
            var yesterday = !dayBeforeYesterday
                && (text.Contains("wczoraj", StringComparison.Ordinal) || ContainsWord(text, "yesterday"));
            var today = ContainsWord(text, "dzis")
                || text.Contains("dzisiaj", StringComparison.Ordinal)
                || ContainsWord(text, "today");
            var tomorrow = ContainsWord(text, "jutro") || ContainsWord(text, "tomorrow");
            var dayCues = 0;
            int? dayOffset = null;
            if (dayBeforeYesterday)
            {
                dayOffset = -2;
                dayCues++;
            }

            if (yesterday)
            {
                dayOffset ??= -1;
                dayCues++;
            }

            if (today)
            {
                dayOffset ??= 0;
                dayCues++;
            }

            if (tomorrow)
            {
                dayOffset ??= 1;
                dayCues++;
            }

            if (nextDay)
            {
                dayCues++;
            }

            if (!TryClock(text, out var clockTime, out var clockCount))
            {
                return false;
            }

            if (!TryPartOfDay(text, out var partTime, out var partCount))
            {
                return false;
            }

            if (dayCues > 1 || clockCount > 1 || partCount > 1)
            {
                return false;
            }

            if (dayCues == 0 && clockCount == 0 && partCount == 0)
            {
                return false;
            }

            var civil = Compose(clock, previousCivil, dayOffset, nextDay, clockCount == 1 ? clockTime : null, partCount == 1 ? partTime : null);
            return Finish(civil, clock, out resolved);
        }

        public static bool TryResolveHint(
            SpokenTimeHint hint,
            RecordingClock clock,
            DateTime? previousCivil,
            out ResolvedSpokenTime resolved)
        {
            resolved = default;
            if (hint.MinutesAfterPrevious is int minutes)
            {
                var anchor = previousCivil ?? clock.LocalNow;
                return Finish(anchor.AddMinutes(minutes), clock, out resolved);
            }

            TimeOnly? clockTime = null;
            if (!string.IsNullOrWhiteSpace(hint.Clock)
                && TimeOnly.TryParse(hint.Clock.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedClock))
            {
                clockTime = parsedClock;
            }

            TimeOnly? partTime = null;
            if (!string.IsNullOrWhiteSpace(hint.PartOfDay))
            {
                partTime = PartToken(Fold(hint.PartOfDay));
            }

            if (hint.DayOffset is null && clockTime is null && partTime is null)
            {
                return false;
            }

            var dayOffset = hint.DayOffset is >= -7 and <= 1 ? hint.DayOffset : null;
            var civil = Compose(clock, previousCivil, dayOffset, nextDay: false, clockTime, partTime);
            return Finish(civil, clock, out resolved);
        }

        private static DateTime Compose(
            RecordingClock clock,
            DateTime? previousCivil,
            int? dayOffset,
            bool nextDay,
            TimeOnly? clockTime,
            TimeOnly? partTime)
        {
            DateOnly date;
            if (nextDay)
            {
                var anchor = previousCivil ?? clock.LocalNow;
                date = DateOnly.FromDateTime(anchor).AddDays(1);
            }
            else if (dayOffset.HasValue)
            {
                date = DateOnly.FromDateTime(clock.LocalNow).AddDays(dayOffset.Value);
            }
            else if (previousCivil.HasValue && (clockTime.HasValue || partTime.HasValue))
            {
                date = DateOnly.FromDateTime(previousCivil.Value);
            }
            else
            {
                date = DateOnly.FromDateTime(clock.LocalNow);
            }

            var time = clockTime ?? partTime ?? TimeOnly.FromDateTime(clock.LocalNow);
            var civil = date.ToDateTime(time);

            if (!dayOffset.HasValue
                && !nextDay
                && previousCivil.HasValue
                && civil <= previousCivil.Value
                && partTime.HasValue
                && !clockTime.HasValue
                && partTime.Value.Hour < 12
                && previousCivil.Value.TimeOfDay >= TimeSpan.FromHours(12))
            {
                civil = civil.AddDays(1);
            }

            if (!dayOffset.HasValue && !nextDay && civil > clock.LocalNow)
            {
                var stepped = civil.AddDays(-1);
                if (stepped <= clock.LocalNow)
                {
                    civil = stepped;
                }
            }

            return civil;
        }

        private static bool Finish(DateTime civil, RecordingClock clock, out ResolvedSpokenTime resolved)
        {
            if (civil > clock.LocalNow)
            {
                civil = clock.LocalNow;
            }

            civil = DateTime.SpecifyKind(civil, DateTimeKind.Unspecified);
            var utc = DateTime.SpecifyKind(clock.ToUtc(civil), DateTimeKind.Utc);
            resolved = new ResolvedSpokenTime(civil, utc);
            return true;
        }

        private static ResolvedSpokenTime Finish(DateTime civil, RecordingClock clock)
        {
            Finish(civil, clock, out var resolved);
            return resolved;
        }

        private static bool TryInterpretModelDate(DateTime? model, RecordingClock clock, out DateTime civil)
        {
            civil = default;
            if (model is null || model.Value == default)
            {
                return false;
            }

            var value = model.Value;
            if (value.Kind == DateTimeKind.Utc)
            {
                civil = clock.Zone.IsKnown
                    ? clock.Zone.ToCivil(value)
                    : DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
                return true;
            }

            civil = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            return true;
        }

        private static bool TryRelativeMinutes(string text, out double minutes)
        {
            minutes = 0;
            var later = ContainsWord(text, "pozniej")
                || ContainsWord(text, "later")
                || ContainsWord(text, "after");
            if (!later)
            {
                return false;
            }

            if (text.Contains("poltorej godziny", StringComparison.Ordinal)
                || text.Contains("hour and a half", StringComparison.Ordinal)
                || text.Contains("1.5 hour", StringComparison.Ordinal)
                || text.Contains("90 minute", StringComparison.Ordinal))
            {
                minutes = 90;
                return true;
            }

            if (text.Contains("pol godziny", StringComparison.Ordinal)
                || text.Contains("half an hour", StringComparison.Ordinal)
                || text.Contains("half hour", StringComparison.Ordinal))
            {
                minutes = 30;
                return true;
            }

            if (text.Contains("kwadrans", StringComparison.Ordinal)
                || text.Contains("quarter of an hour", StringComparison.Ordinal)
                || text.Contains("15 minute", StringComparison.Ordinal))
            {
                minutes = 15;
                return true;
            }

            var numeric = Regex.Match(text, @"(\d+(?:[.,]\d+)?)\s*(godzin\w*|hours?|h)\b");
            if (numeric.Success && double.TryParse(numeric.Groups[1].Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var hours))
            {
                minutes = hours * 60;
                return minutes > 0;
            }

            numeric = Regex.Match(text, @"(\d+)\s*(minut\w*|minutes?|min)\b");
            if (numeric.Success && double.TryParse(numeric.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var mins))
            {
                minutes = mins;
                return minutes > 0;
            }

            foreach (var (word, value) in NumberWords())
            {
                if (Regex.IsMatch(text, $@"\b{word}\s+godzin"))
                {
                    minutes = value * 60;
                    return true;
                }
            }

            if (Regex.IsMatch(text, @"\bgodzine\b") || Regex.IsMatch(text, @"\ban hour\b") || Regex.IsMatch(text, @"\bone hour\b"))
            {
                minutes = 60;
                return true;
            }

            return false;
        }

        private static IEnumerable<(string Word, double Value)> NumberWords()
        {
            yield return ("poltorej", 1.5);
            yield return ("pol", 0.5);
            yield return ("jedna", 1);
            yield return ("jedne", 1);
            yield return ("dwie", 2);
            yield return ("dwoch", 2);
            yield return ("trzy", 3);
            yield return ("cztery", 4);
            yield return ("piec", 5);
            yield return ("szesc", 6);
            yield return ("siedem", 7);
            yield return ("osiem", 8);
            yield return ("dziewiec", 9);
            yield return ("dziesiec", 10);
        }

        private static bool TryClock(string text, out TimeOnly time, out int count)
        {
            time = default;
            var seen = 0;
            TimeOnly? found = null;

            void Add(TimeOnly candidate)
            {
                if (found is null)
                {
                    found = candidate;
                    seen = 1;
                    return;
                }

                if (found.Value != candidate)
                {
                    seen++;
                }
            }

            foreach (var (phrase, hour) in PolishHours)
            {
                if (Regex.IsMatch(text, $@"\b{phrase}\b"))
                {
                    Add(new TimeOnly(hour, 0));
                    break;
                }
            }

            foreach (Match match in Regex.Matches(text, @"\b(?:o|at)\s+(\d{1,2})(?:[:.](\d{2}))?\s*(am|pm)?\b"))
            {
                if (TryHourMinute(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, out var parsed))
                {
                    Add(parsed);
                }
            }

            foreach (Match match in Regex.Matches(text, @"\b(\d{1,2}):(\d{2})\b"))
            {
                if (TryHourMinute(match.Groups[1].Value, match.Groups[2].Value, null, out var parsed))
                {
                    Add(parsed);
                }
            }

            foreach (Match match in Regex.Matches(text, @"\b(\d{1,2})\s*(am|pm)\b"))
            {
                if (TryHourMinute(match.Groups[1].Value, "00", match.Groups[2].Value, out var parsed))
                {
                    Add(parsed);
                }
            }

            count = seen;
            if (found is TimeOnly value && seen == 1)
            {
                time = value;
                return true;
            }

            return seen == 0;
        }

        private static bool TryHourMinute(string hourText, string minuteText, string? ampm, out TimeOnly time)
        {
            time = default;
            if (!int.TryParse(hourText, out var hour))
            {
                return false;
            }

            var minute = 0;
            if (!string.IsNullOrEmpty(minuteText) && !int.TryParse(minuteText, out minute))
            {
                return false;
            }

            var mer = ampm?.ToLowerInvariant();
            if (mer == "pm" && hour < 12)
            {
                hour += 12;
            }

            if (mer == "am" && hour == 12)
            {
                hour = 0;
            }

            if (hour is < 0 or > 23 || minute is < 0 or > 59)
            {
                return false;
            }

            time = new TimeOnly(hour, minute);
            return true;
        }

        private static bool TryPartOfDay(string text, out TimeOnly time, out int count)
        {
            time = default;
            count = 0;
            TimeOnly? found = null;
            foreach (var (phrases, partTime) in PartsOfDay)
            {
                if (!phrases.Any(phrase => HasPhrase(text, phrase)))
                {
                    continue;
                }

                if (found is null)
                {
                    found = partTime;
                    count = 1;
                    continue;
                }

                if (found.Value != partTime)
                {
                    count++;
                }
            }

            // "next morning" / "rano nastepnego dnia" is one part plus a day cue.
            if (text.Contains("next morning", StringComparison.Ordinal) && found == new TimeOnly(7, 30))
            {
                count = 1;
            }

            if (found is TimeOnly value && count == 1)
            {
                time = value;
                return true;
            }

            return count == 0;
        }

        private static TimeOnly? PartToken(string token)
        {
            return token switch
            {
                "morning" or "rano" => new TimeOnly(7, 30),
                "noon" or "poludnie" => new TimeOnly(12, 0),
                "afternoon" or "popoludnie" => new TimeOnly(15, 0),
                "evening" or "wieczor" => new TimeOnly(19, 0),
                "beforesleep" or "beforebed" or "przedsnem" or "sleep" => new TimeOnly(23, 0),
                "breakfast" or "sniadanie" => new TimeOnly(8, 0),
                "lunch" or "obiad" => new TimeOnly(13, 0),
                "dinner" or "kolacja" or "kolacje" => new TimeOnly(19, 0),
                _ => null
            };
        }

        internal static string Fold(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var replaced = value.Trim().Replace('ł', 'l').Replace('Ł', 'l');
            var decomposed = replaced.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposed.Length);
            foreach (var ch in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(char.ToLowerInvariant(ch));
                }
            }

            return Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), @"\s+", " ");
        }

        private static bool HasPhrase(string text, string phrase)
        {
            if (phrase.Contains(' '))
            {
                return text.Contains(phrase, StringComparison.Ordinal);
            }

            return Regex.IsMatch(text, $@"\b{Regex.Escape(phrase)}\b");
        }

        private static bool ContainsAny(string text, params string[] needles) =>
            needles.Any(needle => text.Contains(needle, StringComparison.Ordinal));

        private static bool ContainsWord(string text, string word) =>
            Regex.IsMatch(text, $@"\b{Regex.Escape(word)}\b");
    }
}
