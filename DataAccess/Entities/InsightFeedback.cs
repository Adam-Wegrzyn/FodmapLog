using System;

namespace DataAccess.Entities
{
    /// <summary>
    /// Thumbs up/down on one insights finding. Append-only; scoped to the signed-in user.
    /// </summary>
    public class InsightFeedback : BaseEntity
    {
        public string UserId { get; set; } = "";
        public string FoodKey { get; set; } = "";
        public string ProductName { get; set; } = "";
        public bool Helpful { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
