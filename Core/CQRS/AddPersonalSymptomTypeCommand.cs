using DataAccess.Entities;
using DataAccess.Enums;
using MediatR;

namespace Core.CQRS
{
    public class AddPersonalSymptomTypeCommand : IRequest<SymptomType>
    {
        public required string Name { get; init; }
        public required string UserId { get; init; }
        public SymptomScaleKind ScaleKind { get; init; } = SymptomScaleKind.Negative;
    }
}
