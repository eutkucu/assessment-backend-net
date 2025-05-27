using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ReportService.Domain.Models;
using ReportService.Domain.Enums;

namespace ReportService.Infrastructure
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase("ReportDB");
        }

        public IMongoCollection<Report> Reports => _database.GetCollection<Report>("Reports");

        public IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name);

        public async Task UpdatePendingReportsAsync()
        {
            var filter = Builders<Report>.Filter.Eq("Status", "Pending");
            var update = Builders<Report>.Update.Set("Status", ReportStatus.Preparing.ToString());
            await Reports.UpdateManyAsync(filter, update);
        }
    }
} 