using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactService.Domain;
using ContactService.Domain.Entities;
using ContactService.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace ContactService.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<PersonService> _logger;
        private readonly IContactInfoService _contactInfoService;

        public PersonService(IPersonRepository personRepository, ILogger<PersonService> logger, IContactInfoService contactInfoService)
        {
            _personRepository = personRepository;
            _logger = logger;
            _contactInfoService = contactInfoService;
        }

        public async Task<List<Person>> GetAllPersonsAsync()
        {
            var persons = await _personRepository.GetAllAsync();
            foreach (var person in persons)
            {
                person.ContactInfos = await _contactInfoService.GetAllByPersonIdAsync(person.Id);
            }
            return persons;
        }

        public async Task<Person> GetPersonByIdAsync(Guid id)
        {
            var person = await _personRepository.GetByIdAsync(id);
            if (person != null)
            {
                person.ContactInfos = await _contactInfoService.GetAllByPersonIdAsync(id);
            }
            return person;
        }

        public async Task<string> CreatePersonAsync(Person person)
        {
            await _personRepository.AddAsync(person);
            return person.Id.ToString();
        }

        public async Task UpdatePersonAsync(Person person)
        {
            await _personRepository.UpdateAsync(person);
        }

        public async Task DeletePersonAsync(Guid id)
        {
            await _personRepository.DeleteAsync(id);
            await _contactInfoService.DeleteByPersonIdAsync(id);
        }
    }
} 