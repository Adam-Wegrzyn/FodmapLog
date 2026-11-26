namespace Core.Insights
{
    /// <summary>
    /// Calibration knobs for the insights MVP. Change them here only.
    /// The 2–6h window matches the day-timeline highlight.
    /// </summary>
    public static class InsightsThresholds
    {
        public const int DefaultWindowDays = 14;
        public const int MinWindowDays = 1;
        public const int MaxWindowDays = 90;

        public const int WindowStartHours = 2;
        public const int WindowEndHours = 6;

        /// <summary>Negative-scale symptoms at this severity or higher count as relevant. Matches <see cref="DataAccess.Enums.SymptomScale.Mild"/>.</summary>
        public const int MinRelevantSeverity = 2;

        public const int MinDaysWithEntries = 7;
        public const int MinMeals = 10;
        public const int MinSymptomLogs = 3;

        public const int MinExposures = 4;
        public const int MinDistinctDays = 3;
        public const int MinBaselineMeals = 5;

        public const int StrongerMinExposures = 6;
        public const decimal StrongerMinRate = 0.60m;
        public const decimal StrongerMinDiffPercentagePoints = 30m;
        public const decimal WeakMinDiffPercentagePoints = 20m;

        public const int MaxFindings = 3;
        public const int MaxRarely = 3;
        public const int MaxSymptomNames = 2;
        public const int MaxCompanions = 2;

        /// <summary>
        /// Two foods are "usually eaten together" when each appears in at least this share of the other's meals.
        /// </summary>
        public const decimal CoOccurrenceMinShare = 0.80m;

        public const string StatusLocked = "locked";
        public const string StatusReady = "ready";
        public const string LabelStronger = "stronger pattern";
        public const string LabelWeak = "weak signal";
    }
}
