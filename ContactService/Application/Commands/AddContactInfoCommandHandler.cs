using System;
using System.Threading;
using System.Threading.Tasks;
using ContactService.Domain.Entities;
using ContactService.Domain.Enums;
using ContactService.Infrastructure.Interfaces;
using MediatR;

namespace ContactService.Application.Commands
{
    public class AddContactInfoCommandHandler : IRequestHandler<AddContactInfoCommand, bool>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IContactInfoRepository _contactInfoRepository;

        public AddContactInfoCommandHandler(IPersonRepository personRepository, IContactInfoRepository contactInfoRepository)
        {
            _personRepository = personRepository;
            _contactInfoRepository = contactInfoRepository;
        }

        public async Task<bool> Handle(AddContactInfoCommand request, CancellationToken cancellationToken)
        {
            var person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
                return false;

            var contactInfo = new ContactInfo
            {
                PersonId = request.PersonId,
                Type = request.InfoType,
                Value = request.InfoContent
            };

            await _contactInfoRepository.AddAsync(contactInfo);
            return true;
        }
    }
} 