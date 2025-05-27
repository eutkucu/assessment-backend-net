using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactService.Domain;
using ContactService.Domain.Entities;

namespace ContactService.Services
{
    public interface IContactInfoService
    {
        Task<List<ContactInfo>> GetAllByPersonIdAsync(Guid personId);
        Task<ContactInfo> GetByIdAsync(Guid id);
        Task AddAsync(ContactInfo contactInfo);
        Task UpdateAsync(ContactInfo contactInfo);
        Task DeleteAsync(Guid id);
        Task DeleteByPersonIdAsync(Guid personId);
    }
} 