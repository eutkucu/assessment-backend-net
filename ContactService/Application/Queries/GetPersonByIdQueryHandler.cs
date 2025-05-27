using System.Threading;
using System.Threading.Tasks;
using ContactService.Domain.Entities;
using ContactService.Infrastructure.Interfaces;
using MediatR;

namespace ContactService.Application.Queries
{
    public class GetPersonByIdQueryHandler : IRequestHandler<GetPersonByIdQuery, Person>
    {
        private readonly IPersonRepository _personRepository;

        public GetPersonByIdQueryHandler(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<Person> Handle(GetPersonByIdQuery request, CancellationToken cancellationToken)
        {
            return await _personRepository.GetByIdAsync(request.Id);
        }
    }
} 