using DataAccess.Entities;
using MediatR;
using System.Collections.Generic;

namespace Core.CQRS
{
    public class GetSymptomTypesQuery : IRequest<IEnumerable<SymptomType>>
    {

    }
}