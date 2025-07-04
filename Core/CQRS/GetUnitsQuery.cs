using DataAccess.Entities;
using MediatR;
using System.Collections.Generic;

namespace Core.CQRS
{
    public class GetUnitsQuery : IRequest<IEnumerable<DataAccess.Entities.Unit>>
    {

    }
}