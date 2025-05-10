using DataAccess.Enums;
using System.Text.Json.Serialization;

namespace Data.Common.DTO
{
    public class SymptomDto
    {
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public SymptomType SymptomType { get; set; }
        public SymptomScale SymptomScale { get; set; }
    }
}