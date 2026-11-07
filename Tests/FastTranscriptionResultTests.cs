using Core.Audio;

namespace Tests
{
    public class FastTranscriptionResultTests
    {
        [Fact]
        public void Reads_combinedPhrases_text()
        {
            const string json = """
                {
                  "durationMilliseconds": 120000,
                  "combinedPhrases": [
                    { "text": "I had oatmeal at eight. Then bloating around ten." }
                  ]
                }
                """;

            Assert.Equal(
                "I had oatmeal at eight. Then bloating around ten.",
                FastTranscriptionResult.ReadText(json));
        }

        [Fact]
        public void Joins_phrase_fallback_when_combined_missing()
        {
            const string json = """
                {
                  "phrases": [
                    { "text": "Zjadłem owsiankę." },
                    { "text": "Potem miałem wzdęcia." }
                  ]
                }
                """;

            Assert.Equal(
                "Zjadłem owsiankę. Potem miałem wzdęcia.",
                FastTranscriptionResult.ReadText(json));
        }

        [Fact]
        public void Empty_body_returns_empty_string()
        {
            Assert.Equal(string.Empty, FastTranscriptionResult.ReadText("{}"));
            Assert.Equal(string.Empty, FastTranscriptionResult.ReadText(null));
        }
    }
}
