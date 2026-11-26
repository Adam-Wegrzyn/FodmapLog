using System.Collections.Generic;

namespace Data.Common.DTO
{
    public class InsightsResponseDto
    {
        public string Status { get; set; } = InsightsStatuses.Locked;
        public int Days { get; set; }
        public InsightsProgressDto Progress { get; set; } = new();
        public List<InsightFindingDto> Findings { get; set; } = new();
        public List<InsightRareFoodDto> RarelyFollowed { get; set; } = new();
    }

    public static class InsightsStatuses
    {
        public const string Locked = "locked";
        public const string Ready = "ready";
    }

    public class InsightsProgressDto
    {
        public int DaysWithEntries { get; set; }
        public int DaysRequired { get; set; }
        public int Meals { get; set; }
        public int MealsRequired { get; set; }
        public int SymptomLogs { get; set; }
        public int SymptomLogsRequired { get; set; }
    }

    public class InsightFindingDto
    {
        public string Product { get; set; } = "";
        public string FoodKey { get; set; } = "";
        public int Exposures { get; set; }
        public int Hits { get; set; }
        public int BaselineHits { get; set; }
        public int BaselineMeals { get; set; }
        /// <summary>"stronger pattern" or "weak signal".</summary>
        public string Label { get; set; } = "";
        /// <summary>Most common relevant symptom names in the hit window (English seed names when seeded).</summary>
        public List<string> SymptomNames { get; set; } = new();
        /// <summary>Display names of foods usually eaten in the same meals.</summary>
        public List<string> EatenWith { get; set; } = new();
    }

    public class InsightRareFoodDto
    {
        public string Product { get; set; } = "";
        public string FoodKey { get; set; } = "";
        public int Exposures { get; set; }
        public int Hits { get; set; }
        public int BaselineHits { get; set; }
        public int BaselineMeals { get; set; }
    }

    public class InsightFeedbackRequestDto
    {
        public string? FoodKey { get; set; }
        public string? Product { get; set; }
        public bool Helpful { get; set; }
    }
}
