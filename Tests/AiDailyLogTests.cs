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
        [InlineData("ból brzucha", 6)]
        [InlineData("zgaga", 7)]
        [InlineData("biegunka", 3)]
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
