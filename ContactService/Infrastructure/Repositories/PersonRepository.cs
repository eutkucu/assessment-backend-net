using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using ContactService.Domain;
using ContactService.Domain.Entities;
using ContactService.Infrastructure.Interfaces;
using ContactService.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ContactService.Infrastructure.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly IMongoCollection<Person> _persons;
        private readonly ILogger<PersonRepository> _logger;

        public PersonRepository(MongoDbContext context, ILogger<PersonRepository> logger)
        {
            _persons = context.Persons;
            _logger = logger;
        }

        public async Task<List<Person>> GetAllAsync()
        {
            return await _persons.Find(_ => true).ToListAsync();
        }

        public async Task<Person> GetByIdAsync(Guid id)
        {
            return await _persons.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Person person)
        {
            try
            {
                _logger.LogInformation("MongoDB InsertOneAsync çağrılıyor. Id: {Id}", person.Id);
                await _persons.InsertOneAsync(person);
                _logger.LogInformation("MongoDB InsertOneAsync başarılı. Id: {Id}", person.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MongoDB InsertOneAsync sırasında hata oluştu. Id: {Id}", person.Id);
                throw;
            }
        }

        public async Task UpdateAsync(Person person)
        {
            await _persons.ReplaceOneAsync(p => p.Id == person.Id, person);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _persons.DeleteOneAsync(p => p.Id == id);
        }
    }
} 