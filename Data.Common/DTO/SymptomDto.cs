using DataAccess.Entities;
using DataAccess.Enums;
using System.Text.Json.Serialization;

namespace Data.Common.DTO
{
    public class SymptomDto
    {
        public SymptomTypeDto SymptomType { get; set; }
        public SymptomScale SymptomScale { get; set; }
    }
}