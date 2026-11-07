using DataAccess.Enums;

namespace DataAccess.Entities
{
    public class SymptomType: BaseEntity
    {
        public required string Name { get; set; }
        /// <summary>
        /// Null = shared catalog (seeded). Set = this user's private type.
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// Negative (default): 5 = serious. Positive: 5 = great (mood, energy, …).
        /// </summary>
        public SymptomScaleKind ScaleKind { get; set; } = SymptomScaleKind.Negative;
    }
}
