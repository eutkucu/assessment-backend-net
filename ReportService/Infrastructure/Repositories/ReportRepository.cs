using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using ReportService.Domain.Models;
using ReportService.Infrastructure.Interfaces;

namespace ReportService.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IMongoCollection<Report> _reports;

        public ReportRepository(IMongoDatabase database)
        {
            _reports = database.GetCollection<Report>("Reports");
        }

        public async Task<Report> AddAsync(Report report)
        {
            await _reports.InsertOneAsync(report);
            return report;
        }

        public async Task<Report> GetByIdAsync(Guid id)
        {
            return await _reports.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Report>> GetAllAsync()
        {
            return await _reports.Find(_ => true).ToListAsync();
        }

        public async Task UpdateAsync(Report report)
        {
            await _reports.ReplaceOneAsync(r => r.Id == report.Id, report);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _reports.DeleteOneAsync(r => r.Id == id);
        }
    }
} 