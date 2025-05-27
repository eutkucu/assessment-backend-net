using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReportService.Domain.Models;

namespace ReportService.Infrastructure.Interfaces
{
    public interface IReportRepository
    {
        Task<Report> AddAsync(Report report);
        Task<Report> GetByIdAsync(Guid id);
        Task<IEnumerable<Report>> GetAllAsync();
        Task UpdateAsync(Report report);
        Task DeleteAsync(Guid id);
    }
} 