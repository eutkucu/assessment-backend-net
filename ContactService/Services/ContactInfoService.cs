using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactService.Domain;
using ContactService.Domain.Entities;
using ContactService.Infrastructure.Interfaces;

namespace ContactService.Services
{
    public class ContactInfoService : IContactInfoService
    {
        private readonly IContactInfoRepository _contactInfoRepository;

        public ContactInfoService(IContactInfoRepository contactInfoRepository)
        {
            _contactInfoRepository = contactInfoRepository;
        }

        public async Task<List<ContactInfo>> GetAllByPersonIdAsync(Guid personId)
        {
            return await _contactInfoRepository.GetAllByPersonIdAsync(personId);
        }

        public async Task<ContactInfo> GetByIdAsync(Guid id)
        {
            return await _contactInfoRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(ContactInfo contactInfo)
        {
            await _contactInfoRepository.AddAsync(contactInfo);
        }

        public async Task UpdateAsync(ContactInfo contactInfo)
        {
            await _contactInfoRepository.UpdateAsync(contactInfo);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _contactInfoRepository.DeleteAsync(id);
        }

        public async Task DeleteByPersonIdAsync(Guid personId)
        {
            await _contactInfoRepository.DeleteByPersonIdAsync(personId);
        }
    }
} 