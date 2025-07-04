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
    public class GetUnitsQueryHandler : IRequestHandler<GetUnitsQuery, IEnumerable<DataAccess.Entities.Unit>>
    {
        private readonly IFodmapLogRepository _repository;

        public GetUnitsQueryHandler(IFodmapLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DataAccess.Entities.Unit>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllUnits(cancellationToken);
        }
    }
}