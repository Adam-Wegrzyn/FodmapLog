using Data.Common.DTO;
using DataAccess.Enums;

namespace Core.Insights
{
    /// <summary>
    /// Deterministic food/symptom patterns. No model calls.
    /// A hit is a negative symptom at severity &gt;= 2 logged from 2h through 6h after the meal (both ends included).
    /// </summary>
    public static class InsightsCalculator
    {
        public static InsightsResponseDto Compute(
            IEnumerable<InsightMeal>? meals,
            IEnumerable<InsightSymptomLog>? symptomLogs,
            int windowDays)
        {
            var mealList = (meals ?? Array.Empty<InsightMeal>()).ToList();
            var logList = (symptomLogs ?? Array.Empty<InsightSymptomLog>()).ToList();

            var progress = BuildProgress(mealList, logList);
            var unlocked = progress.DaysWithEntries >= InsightsThresholds.MinDaysWithEntries
                && progress.Meals >= InsightsThresholds.MinMeals
                && progress.SymptomLogs >= InsightsThresholds.MinSymptomLogs;

            var response = new InsightsResponseDto
            {
                Status = unlocked ? InsightsThresholds.StatusReady : InsightsThresholds.StatusLocked,
                Days = windowDays,
                Progress = progress
            };

            if (!unlocked)
            {
                return response;
            }

            var (findings, rarely) = BuildFoods(mealList, logList);
            response.Findings = findings;
            response.RarelyFollowed = rarely;
            return response;
        }

        /// <summary>
        /// Calendar window ending on <paramref name="today"/>, inclusive.
        /// Symptom fetch extends one extra day so a late meal still has its 2–6h tail.
        /// </summary>
        public static (DateTime Start, DateTime EndInclusive, DateTime SymptomEndInclusive) WindowFor(DateTime today, int days)
        {
            var end = today.Date;
            var start = end.AddDays(1 - days);
            return (start, end, end.AddDays(1));
        }

        public static bool InProgressWindow(DateTime at, DateTime start, DateTime endInclusive)
        {
            return at >= start && at < endInclusive.AddDays(1);
        }

        private static InsightsProgressDto BuildProgress(
            List<InsightMeal> meals,
            List<InsightSymptomLog> logs)
        {
            var days = new HashSet<DateTime>();
            foreach (var meal in meals)
            {
                days.Add(meal.At.Date);
            }

            var symptomLogs = 0;
            foreach (var log in logs)
            {
                if (!log.InWindow)
                {
                    continue;
                }

                symptomLogs++;
                days.Add(log.At.Date);
            }

            return new InsightsProgressDto
            {
                DaysWithEntries = days.Count,
                DaysRequired = InsightsThresholds.MinDaysWithEntries,
                Meals = meals.Count,
                MealsRequired = InsightsThresholds.MinMeals,
                SymptomLogs = symptomLogs,
                SymptomLogsRequired = InsightsThresholds.MinSymptomLogs
            };
        }

        private static (List<InsightFindingDto> findings, List<InsightRareFoodDto> rarely) BuildFoods(
            List<InsightMeal> meals,
            List<InsightSymptomLog> logs)
        {
            var relevant = new List<RelevantEvent>();
            foreach (var log in logs)
            {
                var names = new List<string>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var symptom in log.Symptoms ?? Array.Empty<InsightSymptom>())
                {
                    if (!IsRelevant(symptom))
                    {
                        continue;
                    }

                    var name = (symptom.Name ?? string.Empty).Trim();
                    if (name.Length > 0 && seen.Add(name))
                    {
                        names.Add(name);
                    }
                    else if (name.Length == 0 && names.Count == 0 && seen.Count == 0)
                    {
                        // Relevant, but unnamed. Still a hit; no label to show.
                        seen.Add(string.Empty);
                    }
                }

                if (seen.Count > 0)
                {
                    relevant.Add(new RelevantEvent(log.At, names));
                }
            }

            var mealFoods = new List<HashSet<string>>(meals.Count);
            var buckets = new Dictionary<string, FoodBucket>(StringComparer.Ordinal);

            for (var i = 0; i < meals.Count; i++)
            {
                var set = new HashSet<string>(StringComparer.Ordinal);
                foreach (var raw in meals[i].ProductNames ?? Array.Empty<string>())
                {
                    var key = FoodNameNormalizer.CanonicalKey(raw);
                    if (key == null)
                    {
                        continue;
                    }

                    if (!buckets.TryGetValue(key, out var bucket))
                    {
                        bucket = new FoodBucket(key);
                        buckets[key] = bucket;
                    }

                    var display = raw.Trim();
                    if (display.Length > 0)
                    {
                        bucket.DisplayCounts[display] = bucket.DisplayCounts.GetValueOrDefault(display) + 1;
                    }

                    if (!set.Add(key))
                    {
                        continue;
                    }

                    bucket.MealIndexes.Add(i);
                    bucket.Days.Add(meals[i].At.Date);
                }

                mealFoods.Add(set);
            }

            var hitNames = new List<string>?[meals.Count];
            var mealIsHit = new bool[meals.Count];
            for (var i = 0; i < meals.Count; i++)
            {
                hitNames[i] = NamesInWindow(meals[i].At, relevant);
                mealIsHit[i] = hitNames[i] != null;
            }

            var candidates = new List<FoodScore>();
            foreach (var bucket in buckets.Values)
            {
                var exposures = bucket.MealIndexes.Count;
                var hits = 0;
                var symptomCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var index in bucket.MealIndexes)
                {
                    if (!mealIsHit[index])
                    {
                        continue;
                    }

                    hits++;
                    foreach (var name in hitNames[index]!)
                    {
                        symptomCounts[name] = symptomCounts.GetValueOrDefault(name) + 1;
                    }
                }

                var baselineMeals = meals.Count - exposures;
                var baselineHits = 0;
                for (var i = 0; i < meals.Count; i++)
                {
                    if (mealFoods[i].Contains(bucket.Key))
                    {
                        continue;
                    }

                    if (mealIsHit[i])
                    {
                        baselineHits++;
                    }
                }

                candidates.Add(new FoodScore(
                    bucket,
                    exposures,
                    hits,
                    baselineMeals,
                    baselineHits,
                    TopNames(symptomCounts)));
            }

            var findings = new List<ScoredFinding>();
            var rarely = new List<InsightRareFoodDto>();
            foreach (var score in candidates)
            {
                var label = LabelFor(score);
                if (label != null)
                {
                    findings.Add(new ScoredFinding(score, label, Diff(score)));
                    continue;
                }

                if (score.Hits == 0
                    && score.Exposures >= InsightsThresholds.MinExposures
                    && score.Bucket.Days.Count >= InsightsThresholds.MinDistinctDays)
                {
                    rarely.Add(ToRare(score));
                }
            }

            var top = findings
                .OrderByDescending(f => f.Diff)
                .ThenByDescending(f => f.Score.Exposures)
                .ThenBy(f => f.Score.Bucket.Display, StringComparer.Ordinal)
                .Take(InsightsThresholds.MaxFindings)
                .Select(f => ToFinding(f, buckets, mealFoods))
                .ToList();

            var rareList = rarely
                .OrderByDescending(r => r.Exposures)
                .ThenBy(r => r.Product, StringComparer.Ordinal)
                .Take(InsightsThresholds.MaxRarely)
                .ToList();

            return (top, rareList);
        }

        private static string? LabelFor(FoodScore score)
        {
            if (score.Exposures < InsightsThresholds.MinExposures
                || score.Bucket.Days.Count < InsightsThresholds.MinDistinctDays
                || score.BaselineMeals < InsightsThresholds.MinBaselineMeals)
            {
                return null;
            }

            var diff = Diff(score);
            if (score.Exposures >= InsightsThresholds.StrongerMinExposures
                && RateAtLeast(score.Hits, score.Exposures, InsightsThresholds.StrongerMinRate)
                && diff >= InsightsThresholds.StrongerMinDiffPercentagePoints)
            {
                return InsightsThresholds.LabelStronger;
            }

            if (diff >= InsightsThresholds.WeakMinDiffPercentagePoints)
            {
                return InsightsThresholds.LabelWeak;
            }

            return null;
        }

        private static decimal Diff(FoodScore score)
        {
            if (score.Exposures == 0 || score.BaselineMeals == 0)
            {
                return 0m;
            }

            // Cross-multiply so an exact 30pp / 20pp gap stays exact in decimal.
            var numerator = ((decimal)score.Hits * score.BaselineMeals - (decimal)score.BaselineHits * score.Exposures) * 100m;
            var denominator = (decimal)score.Exposures * score.BaselineMeals;
            return numerator / denominator;
        }

        private static bool RateAtLeast(int hits, int exposures, decimal minRate)
        {
            if (exposures <= 0)
            {
                return false;
            }

            return hits * 100m >= exposures * (minRate * 100m);
        }

        private static InsightFindingDto ToFinding(
            ScoredFinding scored,
            Dictionary<string, FoodBucket> buckets,
            List<HashSet<string>> mealFoods)
        {
            var score = scored.Score;
            return new InsightFindingDto
            {
                Product = score.Bucket.Display,
                FoodKey = score.Bucket.Key,
                Exposures = score.Exposures,
                Hits = score.Hits,
                BaselineHits = score.BaselineHits,
                BaselineMeals = score.BaselineMeals,
                Label = scored.Label,
                SymptomNames = score.SymptomNames.ToList(),
                EatenWith = Companions(score.Bucket, buckets, mealFoods)
            };
        }

        private static List<string> Companions(
            FoodBucket food,
            Dictionary<string, FoodBucket> buckets,
            List<HashSet<string>> mealFoods)
        {
            var companions = new List<(string Name, int Overlap)>();
            foreach (var other in buckets.Values)
            {
                if (other.Key == food.Key || other.MealIndexes.Count == 0)
                {
                    continue;
                }

                var overlap = 0;
                foreach (var index in food.MealIndexes)
                {
                    if (mealFoods[index].Contains(other.Key))
                    {
                        overlap++;
                    }
                }

                if (Share(overlap, food.MealIndexes.Count) >= InsightsThresholds.CoOccurrenceMinShare
                    && Share(overlap, other.MealIndexes.Count) >= InsightsThresholds.CoOccurrenceMinShare)
                {
                    companions.Add((other.Display, overlap));
                }
            }

            return companions
                .OrderByDescending(c => c.Overlap)
                .ThenBy(c => c.Name, StringComparer.Ordinal)
                .Take(InsightsThresholds.MaxCompanions)
                .Select(c => c.Name)
                .ToList();
        }

        private static decimal Share(int overlap, int meals) =>
            meals == 0 ? 0m : (decimal)overlap / meals;

        private static InsightRareFoodDto ToRare(FoodScore score) => new()
        {
            Product = score.Bucket.Display,
            FoodKey = score.Bucket.Key,
            Exposures = score.Exposures,
            Hits = score.Hits,
            BaselineHits = score.BaselineHits,
            BaselineMeals = score.BaselineMeals
        };

        private static List<string> TopNames(Dictionary<string, int> counts) =>
            counts
                .OrderByDescending(p => p.Value)
                .ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .Take(InsightsThresholds.MaxSymptomNames)
                .Select(p => p.Key)
                .ToList();

        /// <summary>Null when the meal has no relevant symptom in the window. Empty list when the symptom has no name.</summary>
        private static List<string>? NamesInWindow(DateTime mealAt, List<RelevantEvent> events)
        {
            var from = mealAt.AddHours(InsightsThresholds.WindowStartHours);
            var to = mealAt.AddHours(InsightsThresholds.WindowEndHours);
            List<string>? names = null;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var ev in events)
            {
                if (ev.At < from || ev.At > to)
                {
                    continue;
                }

                names ??= new List<string>();
                foreach (var name in ev.Names)
                {
                    if (seen.Add(name))
                    {
                        names.Add(name);
                    }
                }

                if (ev.Names.Count == 0)
                {
                    names ??= new List<string>();
                }
            }

            return names;
        }

        private static bool IsRelevant(InsightSymptom symptom) =>
            symptom.ScaleKind == SymptomScaleKind.Negative
            && symptom.Severity >= InsightsThresholds.MinRelevantSeverity;

        private sealed class FoodBucket(string key)
        {
            public string Key { get; } = key;
            public List<int> MealIndexes { get; } = new();
            public HashSet<DateTime> Days { get; } = new();
            public Dictionary<string, int> DisplayCounts { get; } = new(StringComparer.Ordinal);

            public string Display =>
                DisplayCounts
                    .OrderByDescending(p => p.Value)
                    .ThenBy(p => p.Key.Length)
                    .ThenBy(p => p.Key, StringComparer.Ordinal)
                    .Select(p => p.Key)
                    .FirstOrDefault() ?? Key;
        }

        private readonly record struct RelevantEvent(DateTime At, List<string> Names);

        private sealed record FoodScore(
            FoodBucket Bucket,
            int Exposures,
            int Hits,
            int BaselineMeals,
            int BaselineHits,
            List<string> SymptomNames);

        private sealed record ScoredFinding(FoodScore Score, string Label, decimal Diff);
    }
}
