using System.Collections.Generic;
using ContactService.Domain.Entities;
using MediatR;

namespace ContactService.Application.Queries
{
    public class GetAllPersonsQuery : IRequest<List<Person>>
    {
    }
} 