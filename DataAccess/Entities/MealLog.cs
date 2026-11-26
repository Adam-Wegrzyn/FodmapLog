using System;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class MealLog : BaseEntity
    {
        /// <summary>
        /// ASP.NET Identity user id (JWT <c>sub</c>). Null isolates legacy rows created before user scoping.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// When <see cref="UtcInstant"/> is true this is a UTC instant. Older rows are civil local wall time.
        /// </summary>
        public DateTime Date { get; set; }

        public bool UtcInstant { get; set; }
        public ICollection<ProductQuantity> ProductQuantity { get; set; }
    }
}
