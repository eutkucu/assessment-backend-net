using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactService.Domain;
using ContactService.Domain.Entities;

namespace ContactService.Infrastructure.Interfaces
{
    public interface IPersonRepository
    {
        Task<List<Person>> GetAllAsync();
        Task<Person> GetByIdAsync(Guid id);
        Task AddAsync(Person person);
        Task UpdateAsync(Person person);
        Task DeleteAsync(Guid id);
    }
} 