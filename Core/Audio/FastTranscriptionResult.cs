using System.Text.Json;

namespace Core.Audio
{
    /// <summary>
    /// Reads display text from Azure Speech Fast Transcription JSON.
    /// </summary>
    public static class FastTranscriptionResult
    {
        public static string ReadText(string? body)
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            var root = doc.RootElement;

            var combined = ReadPhraseTexts(root, "combinedPhrases");
            if (combined.Count > 0)
            {
                return string.Join(" ", combined);
            }

            var phrases = ReadPhraseTexts(root, "phrases");
            return phrases.Count == 0 ? string.Empty : string.Join(" ", phrases);
        }

        private static List<string> ReadPhraseTexts(JsonElement root, string propertyName)
        {
            var texts = new List<string>();
            if (!root.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                return texts;
            }

            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object
                    || !item.TryGetProperty("text", out var textEl))
                {
                    continue;
                }

                var text = textEl.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    texts.Add(text);
                }
            }

            return texts;
        }
    }
}
