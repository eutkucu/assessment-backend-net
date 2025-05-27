using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ContactService.Domain.Entities;

namespace ContactService.Infrastructure
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly int _maxRetryCount;
        private int _currentRetryCount;

        public MongoDbContext(IConfiguration configuration)
        {
            _maxRetryCount = configuration.GetValue<int>("MongoDb:MaxRetryCount");
            _currentRetryCount = 0;

            while (_currentRetryCount < _maxRetryCount)
            {
                try
                {
                    var client = new MongoClient(configuration["MongoDb:ConnectionString"]);
                    _database = client.GetDatabase(configuration["MongoDb:DatabaseName"]);
                    _currentRetryCount = 0;
                    break;
                }
                catch (Exception ex)
                {
                    _currentRetryCount++;
                    Console.WriteLine($"MongoDB bağlantı hatası. Deneme {_currentRetryCount}/{_maxRetryCount}");
                    
                    if (_currentRetryCount >= _maxRetryCount)
                    {
                        Console.WriteLine($"MongoDB'ye bağlanılamadı. Maksimum deneme sayısına ({_maxRetryCount}) ulaşıldı.");
                        throw;
                    }
                    
                    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                }
            }
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }

        public IMongoCollection<Person> Persons => _database.GetCollection<Person>("Persons");
        public IMongoCollection<ContactInfo> ContactInfos => _database.GetCollection<ContactInfo>("ContactInfos");
    }
} 