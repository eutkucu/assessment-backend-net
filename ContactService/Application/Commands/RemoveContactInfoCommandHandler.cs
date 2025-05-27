using System;
using System.Threading;
using System.Threading.Tasks;
using ContactService.Domain.Entities;
using ContactService.Infrastructure.Interfaces;
using MediatR;

namespace ContactService.Application.Commands
{
    public class RemoveContactInfoCommandHandler : IRequestHandler<RemoveContactInfoCommand, bool>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IContactInfoRepository _contactInfoRepository;

        public RemoveContactInfoCommandHandler(IPersonRepository personRepository, IContactInfoRepository contactInfoRepository)
        {
            _personRepository = personRepository;
            _contactInfoRepository = contactInfoRepository;
        }

        public async Task<bool> Handle(RemoveContactInfoCommand request, CancellationToken cancellationToken)
        {
            var person = await _personRepository.GetByIdAsync(request.PersonId);
            if (person == null)
                return false;

            var contactInfo = await _contactInfoRepository.GetByIdAsync(request.ContactInfoId);
            if (contactInfo == null || contactInfo.PersonId != request.PersonId)
                return false;

            await _contactInfoRepository.DeleteAsync(request.ContactInfoId);
            return true;
        }
    }
} 