using Core.Insights;
using Data.Common.DTO;
using DataAccess.Enums;

namespace Tests
{
    public class InsightsCalculatorTests
    {
        private static readonly DateTime Day0 = new(2026, 1, 1);

        [Theory]
        [InlineData("Cebula", "onion")]
        [InlineData("cebulka", "onion")]
        [InlineData("  Onion ", "onion")]
        [InlineData("onions", "onion")]
        [InlineData("Jabłko", "apple")]
        [InlineData("płatki owsiane", "oats")]
        [InlineData("Red  Onion", "red onion")]
        [InlineData("   ", null)]
        [InlineData(null, null)]
        public void CanonicalKey_folds_case_space_diacritics_and_basic_synonyms(string? raw, string? expected)
        {
            Assert.Equal(expected, FoodNameNormalizer.CanonicalKey(raw));
        }

        [Fact]
        public void Window_is_inclusive_14_days_and_symptom_fetch_includes_the_next_day()
        {
            var today = new DateTime(2026, 1, 14);
            var (start, end, symptomEnd) = InsightsCalculator.WindowFor(today, 14);

            Assert.Equal(new DateTime(2026, 1, 1), start);
            Assert.Equal(today, end);
            Assert.Equal(new DateTime(2026, 1, 15), symptomEnd);
            Assert.True(InsightsCalculator.InProgressWindow(today.AddHours(23), start, end));
            Assert.False(InsightsCalculator.InProgressWindow(symptomEnd.AddHours(2), start, end));
        }

        [Fact]
        public void Symptom_at_2h_and_6h_counts_just_outside_does_not()
        {
            var at2h = ComputePattern(TimeSpan.FromHours(2), onionHits: 4);
            var at6h = ComputePattern(TimeSpan.FromHours(6), onionHits: 4);
            var before = ComputePattern(TimeSpan.FromHours(2).Subtract(TimeSpan.FromMinutes(1)), onionHits: 4);
            var after = ComputePattern(TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(1)), onionHits: 4);

            Assert.Equal(4, Only(at2h, "onion").Hits);
            Assert.Equal(4, Only(at6h, "onion").Hits);
            Assert.DoesNotContain(before.Findings, f => f.FoodKey == "onion");
            Assert.Contains(before.RarelyFollowed, r => r.FoodKey == "onion" && r.Hits == 0);
            Assert.DoesNotContain(after.Findings, f => f.FoodKey == "onion");
            Assert.Contains(after.RarelyFollowed, r => r.FoodKey == "onion");
        }

        [Fact]
        public void Only_negative_symptoms_at_severity_2_or_more_are_hits()
        {
            var mild = ComputePattern(TimeSpan.FromHours(4), onionHits: 4, severity: 2);
            var low = ComputePattern(TimeSpan.FromHours(4), onionHits: 4, severity: 1);
            var positive = ComputePattern(
                TimeSpan.FromHours(4),
                onionHits: 4,
                severity: 5,
                kind: SymptomScaleKind.Positive);
            var missingKind = ComputePattern(
                TimeSpan.FromHours(4),
                onionHits: 4,
                severity: 5,
                kind: null);

            Assert.Equal(InsightsThresholds.LabelStronger, Only(mild, "onion").Label);
            Assert.Equal(4, Only(mild, "onion").Hits);
            Assert.DoesNotContain(low.Findings, f => f.FoodKey == "onion");
            Assert.DoesNotContain(positive.Findings, f => f.FoodKey == "onion");
            Assert.DoesNotContain(missingKind.Findings, f => f.FoodKey == "onion");
            Assert.Contains(positive.RarelyFollowed, r => r.FoodKey == "onion");
        }

        [Fact]
        public void Stronger_pattern_uses_meals_without_the_food_as_baseline()
        {
            var result = ComputePattern(TimeSpan.FromHours(4), onionHits: 4);

            Assert.Equal(InsightsThresholds.StatusReady, result.Status);
            var onion = Only(result, "onion");
            Assert.Equal("onion", onion.Product);
            Assert.Equal(6, onion.Exposures);
            Assert.Equal(4, onion.Hits);
            Assert.Equal(7, onion.BaselineMeals);
            Assert.Equal(0, onion.BaselineHits);
            Assert.Equal(InsightsThresholds.LabelStronger, onion.Label);
            Assert.Contains("Bloating", onion.SymptomNames);
        }

        [Fact]
        public void Exact_30_point_gap_is_stronger_and_just_under_is_weak()
        {
            var stronger = RateCase(exposures: 6, hits: 4, baselineMeals: 30, baselineHits: 11);
            var weak = RateCase(exposures: 6, hits: 4, baselineMeals: 30, baselineHits: 12);

            // 4/6 − 11/30 = 30pp exactly; 4/6 − 12/30 = 26.6pp.
            Assert.Equal(InsightsThresholds.LabelStronger, Only(stronger, "onion").Label);
            Assert.Equal(6, Only(stronger, "onion").Exposures);
            Assert.Equal(4, Only(stronger, "onion").Hits);
            Assert.Equal(11, Only(stronger, "onion").BaselineHits);
            Assert.Equal(30, Only(stronger, "onion").BaselineMeals);
            Assert.Equal(InsightsThresholds.LabelWeak, Only(weak, "onion").Label);
        }

        [Fact]
        public void Exact_20_point_gap_is_weak_and_10_points_is_hidden()
        {
            var weak = RateCase(exposures: 4, hits: 2, baselineMeals: 10, baselineHits: 3);
            var hidden = RateCase(exposures: 4, hits: 2, baselineMeals: 10, baselineHits: 4);

            Assert.Equal(InsightsThresholds.LabelWeak, Only(weak, "onion").Label);
            Assert.DoesNotContain(hidden.Findings, f => f.FoodKey == "onion");
            Assert.DoesNotContain(hidden.RarelyFollowed, r => r.FoodKey == "onion");
        }

        [Fact]
        public void Food_needs_three_distinct_days_and_five_baseline_meals()
        {
            var fewDays = new List<InsightMeal>();
            var logs = UnlockSymptoms();
            for (var i = 0; i < 4; i++)
            {
                var at = Day0.AddDays(i % 2).AddHours(8 + i);
                fewDays.Add(Meal(at, "onion"));
                logs.Add(Hit(at, "Bloating"));
            }

            for (var i = 0; i < 10; i++)
            {
                fewDays.Add(Meal(Day0.AddDays(i % 7).AddHours(18), "rice"));
            }

            var byDays = InsightsCalculator.Compute(fewDays, logs, 14);
            Assert.Equal(InsightsThresholds.StatusReady, byDays.Status);
            Assert.DoesNotContain(byDays.Findings, f => f.FoodKey == "onion");

            var fewBaseline = new List<InsightMeal>();
            var baselineLogs = UnlockSymptoms();
            for (var d = 0; d < 6; d++)
            {
                var at = Day0.AddDays(d).AddHours(8);
                fewBaseline.Add(Meal(at, "onion"));
                baselineLogs.Add(Hit(at, "Bloating"));
            }

            for (var d = 0; d < 4; d++)
            {
                fewBaseline.Add(Meal(Day0.AddDays(d).AddHours(18), "rice"));
            }

            var byBaseline = InsightsCalculator.Compute(fewBaseline, baselineLogs, 14);
            Assert.Equal(InsightsThresholds.StatusReady, byBaseline.Status);
            Assert.DoesNotContain(byBaseline.Findings, f => f.FoodKey == "onion");
            Assert.DoesNotContain(byBaseline.RarelyFollowed, r => r.FoodKey == "onion");
        }

        [Fact]
        public void Synonyms_in_one_meal_count_as_one_exposure_and_share_a_finding()
        {
            var meals = new List<InsightMeal>();
            var logs = UnlockSymptoms();
            var spellings = new[] { "Cebula", "cebulka", "onion", "onion", "Onions", "cebula" };
            for (var d = 0; d < spellings.Length; d++)
            {
                var at = Day0.AddDays(d).AddHours(8);
                meals.Add(new InsightMeal(at, new[] { spellings[d], "Cebula" }));
                if (d < 4)
                {
                    logs.Add(Hit(at, "Bloating"));
                }

                meals.Add(Meal(Day0.AddDays(d).AddHours(18), "rice"));
            }

            meals.Add(Meal(Day0.AddDays(6).AddHours(12), "rice"));

            var result = InsightsCalculator.Compute(meals, logs, 14);
            var onion = Only(result, "onion");
            Assert.Equal(6, onion.Exposures);
            Assert.Equal(4, onion.Hits);
            Assert.Equal("Cebula", onion.Product);
            Assert.Equal(InsightsThresholds.LabelStronger, onion.Label);
        }

        [Fact]
        public void Findings_sort_by_gap_then_exposures_and_keep_three()
        {
            var meals = new List<InsightMeal>();
            var logs = new List<InsightSymptomLog>();
            var day = 0;

            void Food(string name, int exposures, int hits)
            {
                for (var i = 0; i < exposures; i++)
                {
                    var at = Day0.AddDays(day).AddHours(8);
                    day++;
                    meals.Add(Meal(at, name));
                    if (i < hits)
                    {
                        logs.Add(Hit(at, "Bloating"));
                    }
                }
            }

            Food("alpha", 6, 6);
            Food("beta", 6, 5);
            Food("delta", 4, 3);
            Food("gamma", 6, 4);
            for (var i = 0; i < 40; i++)
            {
                meals.Add(Meal(Day0.AddDays(day).AddHours(8), "plain"));
                day++;
            }

            var result = InsightsCalculator.Compute(meals, logs, 14);
            Assert.Equal(new[] { "alpha", "beta", "delta" }, result.Findings.Select(f => f.FoodKey).ToArray());
            Assert.Equal(InsightsThresholds.LabelWeak, result.Findings[2].Label);
            Assert.Equal(InsightsThresholds.LabelStronger, result.Findings[0].Label);
            Assert.DoesNotContain(result.Findings, f => f.FoodKey == "gamma");
        }

        [Fact]
        public void Foods_usually_eaten_together_are_named_when_overlap_is_mutual_80_percent()
        {
            var meals = new List<InsightMeal>();
            var logs = UnlockSymptoms();
            for (var d = 0; d < 5; d++)
            {
                var at = Day0.AddDays(d).AddHours(8);
                var withBread = d < 4;
                meals.Add(withBread
                    ? new InsightMeal(at, new[] { "onion", "bread" })
                    : Meal(at, "onion"));
                logs.Add(Hit(at, "Bloating", "Abdominal Pain"));
            }

            for (var d = 0; d < 8; d++)
            {
                meals.Add(Meal(Day0.AddDays(d % 7).AddHours(18), "rice"));
            }

            var result = InsightsCalculator.Compute(meals, logs, 14);
            var onion = Only(result, "onion");
            Assert.Contains("bread", onion.EatenWith);
            Assert.Equal(new[] { "Abdominal Pain", "Bloating" }, onion.SymptomNames.OrderBy(n => n).ToArray());

            var loose = new List<InsightMeal>();
            var looseLogs = UnlockSymptoms();
            for (var d = 0; d < 5; d++)
            {
                var at = Day0.AddDays(d).AddHours(8);
                loose.Add(d < 3
                    ? new InsightMeal(at, new[] { "onion", "garlic" })
                    : Meal(at, "onion"));
                looseLogs.Add(Hit(at, "Gas"));
            }

            for (var d = 0; d < 8; d++)
            {
                loose.Add(Meal(Day0.AddDays(d % 7).AddHours(18), "rice"));
            }

            var separated = InsightsCalculator.Compute(loose, looseLogs, 14);
            Assert.Empty(Only(separated, "onion").EatenWith);
        }

        [Fact]
        public void Zero_hits_with_enough_exposures_go_to_rarely_not_findings()
        {
            var result = ComputePattern(TimeSpan.FromHours(4), onionHits: 0);

            Assert.DoesNotContain(result.Findings, f => f.FoodKey == "onion");
            var rare = Assert.Single(result.RarelyFollowed, r => r.FoodKey == "onion");
            Assert.Equal(0, rare.Hits);
            Assert.Equal(6, rare.Exposures);
        }

        [Fact]
        public void Tail_symptom_outside_the_progress_window_can_still_be_a_hit()
        {
            var (meals, logs) = Pattern(TimeSpan.FromHours(4), onionHits: 3);
            var lateMeal = Day0.AddDays(3).AddHours(22);
            meals.Add(Meal(lateMeal, "onion"));
            logs.Add(new InsightSymptomLog(
                lateMeal.AddHours(4),
                InWindow: false,
                new[] { new InsightSymptom("Bloating", SymptomScaleKind.Negative, 3) }));

            var result = InsightsCalculator.Compute(meals, logs, 14);
            var onion = Only(result, "onion");
            Assert.Equal(4, onion.Hits);
            Assert.Equal(7, onion.Exposures);
            Assert.Equal(6, result.Progress.SymptomLogs);
        }

        [Theory]
        [InlineData(6, 10, 3)]
        [InlineData(7, 9, 3)]
        [InlineData(7, 10, 2)]
        public void Locked_until_days_meals_and_symptom_logs_all_clear_the_bar(int days, int meals, int symptomLogs)
        {
            var result = LockedCase(days, meals, symptomLogs);

            Assert.Equal(InsightsThresholds.StatusLocked, result.Status);
            Assert.Empty(result.Findings);
            Assert.Empty(result.RarelyFollowed);
            Assert.Equal(days, result.Progress.DaysWithEntries);
            Assert.Equal(7, result.Progress.DaysRequired);
            Assert.Equal(meals, result.Progress.Meals);
            Assert.Equal(10, result.Progress.MealsRequired);
            Assert.Equal(symptomLogs, result.Progress.SymptomLogs);
            Assert.Equal(3, result.Progress.SymptomLogsRequired);
        }

        [Fact]
        public void Ready_with_no_qualifying_food_returns_an_empty_finding_list()
        {
            var result = LockedCase(days: 7, meals: 10, symptomLogs: 3);

            Assert.Equal(InsightsThresholds.StatusReady, result.Status);
            Assert.Empty(result.Findings);
            Assert.Empty(result.RarelyFollowed);
        }

        [Fact]
        public void A_day_with_only_a_symptom_still_counts_and_the_same_day_is_not_double_counted()
        {
            var meals = new List<InsightMeal> { Meal(Day0.AddHours(8), "rice") };
            var logs = new List<InsightSymptomLog>
            {
                new(Day0.AddHours(9), true, new[] { new InsightSymptom("Gas", SymptomScaleKind.Negative, 2) }),
                new(Day0.AddDays(1).AddHours(9), true, new[] { new InsightSymptom("Gas", SymptomScaleKind.Negative, 2) })
            };

            var result = InsightsCalculator.Compute(meals, logs, 14);

            Assert.Equal(2, result.Progress.DaysWithEntries);
            Assert.Equal(1, result.Progress.Meals);
            Assert.Equal(2, result.Progress.SymptomLogs);
        }

        private static InsightsResponseDto ComputePattern(
            TimeSpan symptomAfterMeal,
            int onionHits,
            int severity = 3,
            SymptomScaleKind? kind = SymptomScaleKind.Negative)
        {
            var (meals, logs) = Pattern(symptomAfterMeal, onionHits, severity, kind);
            return InsightsCalculator.Compute(meals, logs, 14);
        }

        /// <summary>
        /// Six onion meals at 08:00 across six days, rice meals at 18:00 that do not share the 2–6h window,
        /// plus a seventh day so the unlock rule can pass.
        /// </summary>
        private static (List<InsightMeal> meals, List<InsightSymptomLog> logs) Pattern(
            TimeSpan symptomAfterMeal,
            int onionHits,
            int severity = 3,
            SymptomScaleKind? kind = SymptomScaleKind.Negative)
        {
            var meals = new List<InsightMeal>();
            var logs = UnlockSymptoms();
            for (var d = 0; d < 6; d++)
            {
                var at = Day0.AddDays(d).AddHours(8);
                meals.Add(Meal(at, "onion"));
                if (d < onionHits)
                {
                    logs.Add(new InsightSymptomLog(
                        at.Add(symptomAfterMeal),
                        true,
                        new[] { new InsightSymptom("Bloating", kind, severity) }));
                }

                meals.Add(Meal(Day0.AddDays(d).AddHours(18), "rice"));
            }

            meals.Add(Meal(Day0.AddDays(6).AddHours(12), "rice"));
            return (meals, logs);
        }

        private static InsightsResponseDto RateCase(int exposures, int hits, int baselineMeals, int baselineHits)
        {
            var meals = new List<InsightMeal>();
            var logs = new List<InsightSymptomLog>();
            for (var i = 0; i < exposures; i++)
            {
                var at = Day0.AddDays(i).AddHours(8);
                meals.Add(Meal(at, "onion"));
                if (i < hits)
                {
                    logs.Add(Hit(at, "Bloating"));
                }
            }

            for (var i = 0; i < baselineMeals; i++)
            {
                var at = Day0.AddDays(exposures + i).AddHours(8);
                meals.Add(Meal(at, "rice"));
                if (i < baselineHits)
                {
                    logs.Add(Hit(at, "Bloating"));
                }
            }

            if (logs.Count < 3)
            {
                logs.AddRange(UnlockSymptoms());
            }

            return InsightsCalculator.Compute(meals, logs, 14);
        }

        private static InsightsResponseDto LockedCase(int days, int meals, int symptomLogs)
        {
            var mealList = new List<InsightMeal>();
            for (var i = 0; i < meals; i++)
            {
                mealList.Add(Meal(Day0.AddDays(i % Math.Max(days, 1)).AddHours(8 + (i % 5)), $"food{i}"));
            }

            var logs = new List<InsightSymptomLog>();
            for (var i = 0; i < symptomLogs; i++)
            {
                var at = Day0.AddDays(i % Math.Max(days, 1)).AddHours(12);
                logs.Add(new InsightSymptomLog(
                    at,
                    true,
                    new[] { new InsightSymptom("Gas", SymptomScaleKind.Negative, 1) }));
            }

            // Symptom-only days extend the day count when meals did not already cover them.
            var covered = mealList.Select(m => m.At.Date).ToHashSet();
            var cursor = 0;
            while (covered.Count < days)
            {
                var date = Day0.AddDays(100 + cursor);
                cursor++;
                covered.Add(date);
                logs.Add(new InsightSymptomLog(
                    date.AddHours(9),
                    true,
                    new[] { new InsightSymptom("Gas", SymptomScaleKind.Negative, 1) }));
            }

            return InsightsCalculator.Compute(mealList, logs, 14);
        }

        private static List<InsightSymptomLog> UnlockSymptoms()
        {
            var day = Day0.AddDays(6).AddHours(12);
            return new List<InsightSymptomLog>
            {
                new(day.AddMinutes(10), true, new[] { new InsightSymptom("Headache", SymptomScaleKind.Negative, 2) }),
                new(day.AddMinutes(20), true, new[] { new InsightSymptom("Gas", SymptomScaleKind.Negative, 2) }),
                new(day.AddMinutes(30), true, new[] { new InsightSymptom("Nausea", SymptomScaleKind.Negative, 2) })
            };
        }

        private static InsightMeal Meal(DateTime at, params string[] products) => new(at, products);

        private static InsightSymptomLog Hit(DateTime mealAt, params string[] names) =>
            new(
                mealAt.AddHours(4),
                true,
                names.Select(n => new InsightSymptom(n, SymptomScaleKind.Negative, 3)).ToList());

        private static InsightFindingDto Only(InsightsResponseDto result, string key) =>
            Assert.Single(result.Findings, f => f.FoodKey == key);
    }
}
