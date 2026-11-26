using DataAccess.Enums;

namespace Core.Insights
{
    public sealed record InsightMeal(DateTime At, IReadOnlyList<string>? ProductNames);

    public sealed record InsightSymptom(string? Name, SymptomScaleKind? ScaleKind, int Severity);

    /// <param name="InWindow">False for the extra few hours after the analysis window, kept only so a late meal can still match.</param>
    public sealed record InsightSymptomLog(DateTime At, bool InWindow, IReadOnlyList<InsightSymptom>? Symptoms);
}
