using DataAccess.Entities;
using DataAccess.Interfaces;
using MediatR;

namespace Core.CQRS
{
    public class AddPersonalSymptomTypeCommandHandler
        : IRequestHandler<AddPersonalSymptomTypeCommand, SymptomType>
    {
        private readonly IFodmapLogRepository _repository;

        public AddPersonalSymptomTypeCommandHandler(IFodmapLogRepository repository)
        {
            _repository = repository;
        }

        public Task<SymptomType> Handle(
            AddPersonalSymptomTypeCommand request,
            CancellationToken cancellationToken)
        {
            return _repository.AddPersonalSymptomType(request.Name, request.UserId, cancellationToken);
        }
    }
}
