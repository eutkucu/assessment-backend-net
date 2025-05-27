using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace ReportService.Infrastructure
{
    public class ContactDbContext
    {
        private readonly IMongoDatabase _database;

        public ContactDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["ContactDb:ConnectionString"]);
            _database = client.GetDatabase(configuration["ContactDb:DatabaseName"]);
        }

        public IMongoCollection<dynamic> Persons => _database.GetCollection<dynamic>("Persons");
        public IMongoCollection<dynamic> ContactInfos => _database.GetCollection<dynamic>("ContactInfos");
    }
} 