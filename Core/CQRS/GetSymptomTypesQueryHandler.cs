using Core.CQRS;
using Core.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.CQRS
{
    public class GetSymptomTypesQueryHandler : IRequestHandler<GetSymptomTypesQuery, IEnumerable<SymptomType>>
    {
        private readonly IFodmapLogRepository _repository;

        public GetSymptomTypesQueryHandler(IFodmapLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SymptomType>> Handle(GetSymptomTypesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllSymptomTypes(cancellationToken);
        }
    }
}