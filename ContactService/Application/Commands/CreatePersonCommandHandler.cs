using System;
using System.Threading;
using System.Threading.Tasks;
using ContactService.Domain.Entities;
using ContactService.Infrastructure.Interfaces;
using MediatR;

namespace ContactService.Application.Commands
{
    public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, string>
    {
        private readonly IPersonRepository _personRepository;

        public CreatePersonCommandHandler(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<string> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            var person = new Person
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Company = request.Company
            };

            await _personRepository.AddAsync(person);
            return person.Id.ToString();
        }
    }
} 