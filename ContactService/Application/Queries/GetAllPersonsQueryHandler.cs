using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ContactService.Domain.Entities;
using ContactService.Infrastructure.Interfaces;
using MediatR;

namespace ContactService.Application.Queries
{
    public class GetAllPersonsQueryHandler : IRequestHandler<GetAllPersonsQuery, List<Person>>
    {
        private readonly IPersonRepository _personRepository;

        public GetAllPersonsQueryHandler(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<List<Person>> Handle(GetAllPersonsQuery request, CancellationToken cancellationToken)
        {
            return await _personRepository.GetAllAsync();
        }
    }
} 