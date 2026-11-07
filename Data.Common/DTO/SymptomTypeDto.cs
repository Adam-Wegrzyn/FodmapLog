using DataAccess.Enums;

namespace Data.Common.DTO
{
    public class SymptomTypeDto: BaseDto
    {
        public string Name { get; set; }
        public SymptomScaleKind ScaleKind { get; set; }
    }
}
