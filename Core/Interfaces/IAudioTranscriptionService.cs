namespace Core.Interfaces
{
    public interface IAudioTranscriptionService
    {
        Task<string> TranscribeAsync(string audioBase64, string speechLocale = "en-US");
    }
}
