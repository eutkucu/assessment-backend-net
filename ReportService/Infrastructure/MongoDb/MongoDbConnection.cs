using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace ReportService.Infrastructure.MongoDb
{
    public class MongoDbConnection
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MongoDbConnection> _logger;
        private IMongoDatabase _database;

        public MongoDbConnection(IConfiguration configuration, ILogger<MongoDbConnection> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IMongoDatabase GetDatabase()
        {
            if (_database == null)
            {
                var retryCount = _configuration.GetValue<int>("MongoDbSettings:RetryCount");
                var retryIntervalSeconds = _configuration.GetValue<int>("MongoDbSettings:RetryIntervalSeconds");
                var currentRetry = 0;

                while (currentRetry < retryCount)
                {
                    try
                    {
                        var connectionString = _configuration.GetConnectionString("MongoDb");
                        var client = new MongoClient(connectionString);
                        _database = client.GetDatabase(_configuration["MongoDbSettings:DatabaseName"]);
                        return _database;
                    }
                    catch (Exception ex)
                    {
                        currentRetry++;
                        if (currentRetry >= retryCount)
                        {
                            _logger.LogError(ex, "Failed to connect to MongoDB after {RetryCount} attempts", retryCount);
                            throw;
                        }
                        _logger.LogWarning(ex, "Failed to connect to MongoDB. Retry {CurrentRetry} of {RetryCount}", currentRetry, retryCount);
                        Task.Delay(TimeSpan.FromSeconds(retryIntervalSeconds)).Wait();
                    }
                }
            }
            return _database;
        }
    }
} 