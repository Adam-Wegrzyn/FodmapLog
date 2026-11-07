using DataAccess;
using DataAccess.Entities;
using DataAccess.Enums;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class PersonalSymptomTypeTests
    {
        private static FodmapLogRepository CreateRepo(out FodmapLogDbContext context)
        {
            var options = new DbContextOptionsBuilder<FodmapLogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            context = new FodmapLogDbContext(options);
            context.SymptomTypes.AddRange(
                new SymptomType { Name = "Nausea" },
                new SymptomType { Name = "Bloating" });
            context.SaveChanges();
            return new FodmapLogRepository(context);
        }

        [Fact]
        public async Task GetAll_returns_seeded_and_own_types_not_other_users()
        {
            var repo = CreateRepo(out var context);
            await repo.AddPersonalSymptomType("Brain fog", "user-a", CancellationToken.None);
            await repo.AddPersonalSymptomType("Joint pain", "user-b", CancellationToken.None);

            var forA = (await repo.GetAllSymptomTypes("user-a", CancellationToken.None)).ToList();

            Assert.Contains(forA, t => t.Name == "Nausea" && t.UserId == null);
            Assert.Contains(forA, t => t.Name == "Brain fog" && t.UserId == "user-a");
            Assert.DoesNotContain(forA, t => t.UserId == "user-b");
        }

        [Fact]
        public async Task Add_reuses_seeded_type_instead_of_duplicating()
        {
            var repo = CreateRepo(out _);
            var result = await repo.AddPersonalSymptomType("bloating", "user-a", CancellationToken.None);

            Assert.Equal("Bloating", result.Name);
            Assert.Null(result.UserId);
        }

        [Fact]
        public async Task Add_stores_positive_scale_kind_for_custom_wellbeing()
        {
            var repo = CreateRepo(out _);
            var created = await repo.AddPersonalSymptomType(
                "Calm", "user-a", CancellationToken.None, SymptomScaleKind.Positive);

            Assert.Equal(SymptomScaleKind.Positive, created.ScaleKind);
            Assert.Equal("user-a", created.UserId);
        }

        [Fact]
        public async Task Add_defaults_custom_types_to_negative_scale()
        {
            var repo = CreateRepo(out _);
            var created = await repo.AddPersonalSymptomType(
                "Brain fog", "user-a", CancellationToken.None);

            Assert.Equal(SymptomScaleKind.Negative, created.ScaleKind);
        }

        [Fact]
        public async Task Add_is_idempotent_for_the_same_user()
        {
            var repo = CreateRepo(out _);
            var first = await repo.AddPersonalSymptomType("Brain fog", "user-a", CancellationToken.None);
            var second = await repo.AddPersonalSymptomType(" brain  fog ", "user-a", CancellationToken.None);

            Assert.Equal(first.Id, second.Id);
            Assert.Equal("user-a", second.UserId);
        }

        [Fact]
        public async Task Saving_a_log_creates_a_personal_type_not_a_global_one()
        {
            var repo = CreateRepo(out var context);
            var log = new SymptomsLog
            {
                Date = DateTime.UtcNow,
                UserId = "user-a",
                Symptoms = new List<Symptom>
                {
                    new()
                    {
                        SymptomType = new SymptomType { Name = "Brain fog" },
                        SymptomScale = SymptomScale.Mild
                    }
                }
            };

            await repo.AddSymptomsLog(log, CancellationToken.None);

            var stored = Assert.Single(context.SymptomTypes.Where(t => t.Name == "Brain fog"));
            Assert.Equal("user-a", stored.UserId);
        }

        [Fact]
        public async Task Resolve_ignores_another_users_type_id()
        {
            var repo = CreateRepo(out var context);
            var other = await repo.AddPersonalSymptomType("Secret", "user-b", CancellationToken.None);

            var log = new SymptomsLog
            {
                Date = DateTime.UtcNow,
                UserId = "user-a",
                Symptoms = new List<Symptom>
                {
                    new()
                    {
                        SymptomType = new SymptomType { Id = other.Id, Name = "Secret" },
                        SymptomScale = SymptomScale.Low
                    }
                }
            };

            await repo.AddSymptomsLog(log, CancellationToken.None);

            var owned = context.SymptomTypes.Where(t => t.Name == "Secret").ToList();
            Assert.Equal(2, owned.Count);
            Assert.Contains(owned, t => t.UserId == "user-a" && t.Id != other.Id);
        }

        [Fact]
        public async Task Delete_account_removes_personal_types()
        {
            var repo = CreateRepo(out var context);
            await repo.AddPersonalSymptomType("Brain fog", "user-a", CancellationToken.None);
            await repo.AddPersonalSymptomType("Joint pain", "user-b", CancellationToken.None);

            await repo.DeleteAllLogsForUser("user-a", CancellationToken.None);

            Assert.DoesNotContain(context.SymptomTypes, t => t.UserId == "user-a");
            Assert.Contains(context.SymptomTypes, t => t.UserId == "user-b");
            Assert.Contains(context.SymptomTypes, t => t.Name == "Nausea" && t.UserId == null);
        }
    }
}
