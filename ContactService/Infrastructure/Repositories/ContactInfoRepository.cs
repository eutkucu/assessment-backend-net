using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using ContactService.Domain;
using ContactService.Domain.Entities;
using ContactService.Infrastructure.Interfaces;
using ContactService.Infrastructure;

namespace ContactService.Infrastructure.Repositories
{
    public class ContactInfoRepository : IContactInfoRepository
    {
        private readonly IMongoCollection<ContactInfo> _contactInfos;

        public ContactInfoRepository(MongoDbContext context)
        {
            _contactInfos = context.ContactInfos;
        }

        public async Task<List<ContactInfo>> GetAllByPersonIdAsync(Guid personId)
        {
            return await _contactInfos.Find(ci => ci.PersonId == personId).ToListAsync();
        }

        public async Task<ContactInfo> GetByIdAsync(Guid id)
        {
            return await _contactInfos.Find(ci => ci.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(ContactInfo contactInfo)
        {
            await _contactInfos.InsertOneAsync(contactInfo);
        }

        public async Task UpdateAsync(ContactInfo contactInfo)
        {
            await _contactInfos.ReplaceOneAsync(ci => ci.Id == contactInfo.Id, contactInfo);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _contactInfos.DeleteOneAsync(ci => ci.Id == id);
        }

        public async Task DeleteByPersonIdAsync(Guid personId)
        {
            await _contactInfos.DeleteManyAsync(ci => ci.PersonId == personId);
        }
    }
} 