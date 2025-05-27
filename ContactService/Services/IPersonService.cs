using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactService.Domain;
using ContactService.Domain.Entities;

namespace ContactService.Services
{
    public interface IPersonService
    {
        Task<List<Person>> GetAllPersonsAsync();
        Task<Person> GetPersonByIdAsync(Guid id);
        Task<string> CreatePersonAsync(Person person);
        Task UpdatePersonAsync(Person person);
        Task DeletePersonAsync(Guid id);
    }
} 