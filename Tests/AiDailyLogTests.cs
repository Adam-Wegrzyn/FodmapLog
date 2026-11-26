using Core.Ai;

namespace Tests
{
    public class AiDailyLogTests
    {
        [Theory]
        [InlineData("g", 1)]
        [InlineData("ml", 5)]
        [InlineData("łyżeczka", 6)]
        [InlineData("sztuka", 9)]
        [InlineData("Gram", 1)]
        public void Resolves_unit_aliases(string name, int expectedId)
        {
            Assert.Equal(expectedId, ReferenceAliases.ResolveUnitId(name));
        }

        [Theory]
        [InlineData("wzdęcia", 5)]
        [InlineData("Bloating", 5)]
        [InlineData("ciężkość", 5)]
        [InlineData("lekka ciężkość", 5)]
        [InlineData("dyskomfort", 5)]
        [InlineData("lekki dyskomfort żołądka", 5)]
        [InlineData("stomach discomfort", 5)]
        [InlineData("heaviness", 5)]
        [InlineData("ból brzucha", 6)]
        [InlineData("zgaga", 7)]
        [InlineData("biegunka", 3)]
        [InlineData("brain fog", 22)]
        [InlineData("mgła mózgowa", 22)]
        [InlineData("anxiety", 24)]
        [InlineData("joint pain", 25)]
        public void Resolves_symptom_aliases(string name, int expectedId)
        {
            Assert.Equal(expectedId, ReferenceAliases.ResolveSymptomTypeId(name));
        }

        [Theory]
        [InlineData(3, 3)]
        [InlineData(8, 4)]
        [InlineData(11, 5)]
        [InlineData(-2, 0)]
        public void Clamps_symptom_scale(int input, int expected)
        {
            Assert.Equal(expected, ReferenceAliases.ClampScale(input));
        }

        [Theory]
        [InlineData("lekka ciężkość", true)]
        [InlineData("lekki dyskomfort żołądka", true)]
        [InlineData("stomach discomfort", true)]
        [InlineData("Bloating", true)]
        [InlineData("ból brzucha", false)]
        [InlineData("Abdominal Pain", false)]
        public void Discomfort_is_not_pain(string name, bool discomfort)
        {
            Assert.Equal(discomfort, ReferenceAliases.IsDiscomfortNotPain(name));
        }

        [Theory]
        [InlineData("lekka ciężkość", "lekka ciężkość")]
        [InlineData("lekki dyskomfort żołądka", "lekki dyskomfort żołądka")]
        [InlineData("Bloating", null)]
        [InlineData("ból brzucha", null)]
        public void Keeps_discomfort_wording(string name, string? expected)
        {
            Assert.Equal(expected, ReferenceAliases.DiscomfortWording(name));
        }

        [Fact]
        public void Reads_events_wrapper_or_raw_array()
        {
            Assert.True(AiDailyLogNormalizer.TryReadEvents(
                """{ "events": [ { "date": "2026-04-11T08:00:00" } ] }""",
                out var wrapped,
                out _));
            Assert.Contains("2026-04-11", wrapped);

            Assert.True(AiDailyLogNormalizer.TryReadEvents(
                """[{ "date": "2026-04-11T10:00:00" }]""",
                out var raw,
                out _));
            Assert.Contains("2026-04-11T10:00:00", raw);

            Assert.False(AiDailyLogNormalizer.TryReadEvents("{ \"oops\": true }", out _, out var error));
            Assert.False(string.IsNullOrWhiteSpace(error));
        }
    }
}
