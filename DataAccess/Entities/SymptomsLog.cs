using System;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class SymptomsLog : BaseEntity
    {
        /// <summary>
        /// ASP.NET Identity user id (JWT <c>sub</c>). Null isolates legacy rows created before user scoping.
        /// </summary>
        public string? UserId { get; set; }

        public DateTime Date { get; set; }
        public required List<Symptom> Symptoms { get; set; }
    }
}
