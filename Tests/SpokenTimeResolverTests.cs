using Core.Ai;

namespace Tests
{
    public class SpokenTimeResolverTests
    {
        private static readonly DateTime RecordedAt = new(2026, 11, 26, 10, 47, 0);

        private static RecordingClock Bangkok() =>
            new(RecordedAt, UserClock.FromClient("Asia/Bangkok", 420));

        private static ResolvedSpokenTime Resolve(string? phrase, DateTime? previous = null, string? modelDate = null)
        {
            DateTime? model = modelDate == null ? null : DateTime.Parse(modelDate);
            return SpokenTimeResolver.ResolveEvent(phrase, hint: null, model, Bangkok(), previous);
        }

        [Fact]
        public void Bangkok_diary_resolves_the_spoken_sequence()
        {
            var meal = Resolve("wczoraj o dwudziestej");
            AssertCivil(meal, 2026, 11, 25, 20, 0);
            AssertUtc(meal, 2026, 11, 25, 13, 0);

            var heaviness = Resolve("półtorej godziny później", meal.CivilLocal);
            AssertCivil(heaviness, 2026, 11, 25, 21, 30);
            AssertUtc(heaviness, 2026, 11, 25, 14, 30);

            var electrolytes = Resolve("przed snem", heaviness.CivilLocal);
            AssertCivil(electrolytes, 2026, 11, 25, 23, 0);
            AssertUtc(electrolytes, 2026, 11, 25, 16, 0);

            var morning = Resolve("rano następnego dnia", electrolytes.CivilLocal);
            AssertCivil(morning, 2026, 11, 26, 7, 30);
            AssertUtc(morning, 2026, 11, 26, 0, 30);
        }

        [Fact]
        public void English_diary_uses_the_same_reference_clock()
        {
            var meal = Resolve("yesterday at 8 pm");
            AssertCivil(meal, 2026, 11, 25, 20, 0);

            var later = Resolve("an hour and a half later", meal.CivilLocal);
            AssertCivil(later, 2026, 11, 25, 21, 30);

            var beforeBed = Resolve("before bed", later.CivilLocal);
            AssertCivil(beforeBed, 2026, 11, 25, 23, 0);

            var nextMorning = Resolve("the next morning", beforeBed.CivilLocal);
            AssertCivil(nextMorning, 2026, 11, 26, 7, 30);
        }

        [Fact]
        public void Hours_later_can_cross_midnight()
        {
            var previous = new DateTime(2026, 11, 25, 23, 0, 0);
            var twoHours = Resolve("2 godziny później", previous);
            AssertCivil(twoHours, 2026, 11, 26, 1, 0);
            AssertUtc(twoHours, 2026, 11, 25, 18, 0);

            var halfPast = new DateTime(2026, 11, 25, 23, 30, 0);
            var ninety = Resolve("półtorej godziny później", halfPast);
            AssertCivil(ninety, 2026, 11, 26, 1, 0);
            AssertUtc(ninety, 2026, 11, 25, 18, 0);

            var english = Resolve("2 hours later", previous);
            AssertCivil(english, 2026, 11, 26, 1, 0);
        }

        [Fact]
        public void Just_after_midnight_does_not_schedule_the_future()
        {
            var clock = new RecordingClock(
                new DateTime(2026, 11, 26, 0, 30, 0),
                UserClock.FromClient("Asia/Bangkok", 420));

            var beforeSleep = SpokenTimeResolver.ResolveEvent("przed snem", null, null, clock, null);
            Assert.Equal(new DateTime(2026, 11, 25, 23, 0, 0), beforeSleep.CivilLocal);

            var explicitTodayMorning = SpokenTimeResolver.ResolveEvent("dziś rano", null, null, clock, null);
            Assert.Equal(clock.LocalNow, explicitTodayMorning.CivilLocal);

            var bareMorning = SpokenTimeResolver.ResolveEvent("rano", null, null, clock, null);
            Assert.Equal(new DateTime(2026, 11, 25, 7, 30, 0), bareMorning.CivilLocal);
        }

        [Theory]
        [InlineData("o 9", 9, 0)]
        [InlineData("o dziewiątej", 9, 0)]
        [InlineData("at 9", 9, 0)]
        [InlineData("na śniadanie", 8, 0)]
        [InlineData("for breakfast", 8, 0)]
        [InlineData("w południe", 12, 0)]
        public void Clock_and_meal_names_that_are_already_past(string phrase, int hour, int minute)
        {
            // 12:00 is still ahead of 10:47, so noon steps back to yesterday.
            var expectedDay = hour == 12 ? 25 : 26;
            var resolved = Resolve(phrase);
            AssertCivil(resolved, 2026, 11, expectedDay, hour, minute);
        }

        [Theory]
        [InlineData("o dwudziestej", 20, 0)]
        [InlineData("at 8 pm", 20, 0)]
        [InlineData("wieczorem", 19, 0)]
        [InlineData("na kolację", 19, 0)]
        [InlineData("for dinner", 19, 0)]
        [InlineData("przed snem", 23, 0)]
        [InlineData("before sleep", 23, 0)]
        public void Future_part_of_today_uses_the_most_recent_past_occurrence(string phrase, int hour, int minute)
        {
            var resolved = Resolve(phrase);
            AssertCivil(resolved, 2026, 11, 25, hour, minute);
            Assert.True(resolved.CivilLocal <= RecordedAt);
        }

        [Fact]
        public void Day_before_yesterday_and_warsaw_offset()
        {
            var resolved = Resolve("przedwczoraj rano");
            AssertCivil(resolved, 2026, 11, 24, 7, 30);

            var warsaw = new RecordingClock(
                RecordedAt,
                UserClock.FromClient("Europe/Warsaw", 60));
            var meal = SpokenTimeResolver.ResolveEvent("wczoraj o dwudziestej", null, null, warsaw, null);
            Assert.Equal(new DateTime(2026, 11, 25, 20, 0, 0), meal.CivilLocal);
            Assert.Equal(new DateTime(2026, 11, 25, 19, 0, 0, DateTimeKind.Utc), meal.Utc);
        }

        [Fact]
        public void Missing_time_uses_the_recording_clock_and_clamps_the_future()
        {
            var none = Resolve(null);
            Assert.Equal(RecordedAt, none.CivilLocal);

            var future = Resolve(null, modelDate: "2026-11-26T18:00:00");
            Assert.Equal(RecordedAt, future.CivilLocal);

            var past = Resolve(null, modelDate: "2026-11-25T20:00:00");
            AssertCivil(past, 2026, 11, 25, 20, 0);
            AssertUtc(past, 2026, 11, 25, 13, 0);
        }

        [Fact]
        public void Structured_hint_is_used_when_the_phrase_is_ambiguous()
        {
            var clock = Bangkok();
            var hint = new SpokenTimeHint { DayOffset = -1, Clock = "20:00" };
            var resolved = SpokenTimeResolver.ResolveEvent(
                "wczoraj o dwudziestej i przed snem",
                hint,
                DateTime.Parse("2026-11-26T03:47:00"),
                clock,
                previousCivil: null);

            AssertCivil(resolved, 2026, 11, 25, 20, 0);
        }

        [Fact]
        public void Utc_client_now_is_converted_into_the_stated_zone()
        {
            var clock = RecordingClock.FromClient(
                "2026-11-26T03:47:00.000Z",
                "Asia/Bangkok",
                420);
            Assert.Equal(RecordedAt, clock.LocalNow);

            var local = RecordingClock.FromClient("2026-11-26T10:47:00", "Asia/Bangkok", 420);
            Assert.Equal(RecordedAt, local.LocalNow);
        }

        private static void AssertCivil(ResolvedSpokenTime resolved, int year, int month, int day, int hour, int minute)
        {
            Assert.Equal(new DateTime(year, month, day, hour, minute, 0), resolved.CivilLocal);
            Assert.Equal(DateTimeKind.Unspecified, resolved.CivilLocal.Kind);
        }

        private static void AssertUtc(ResolvedSpokenTime resolved, int year, int month, int day, int hour, int minute)
        {
            Assert.Equal(new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc), resolved.Utc);
        }
    }
}
