using DataAccess.Entities;
using MediatR;

namespace Core.CQRS
{
    public class AddPersonalSymptomTypeCommand : IRequest<SymptomType>
    {
        public required string Name { get; init; }
        public required string UserId { get; init; }
    }
}
