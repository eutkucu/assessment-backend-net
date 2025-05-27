using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReportService.Infrastructure.Interfaces;

namespace ReportService.Infrastructure.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;
        private readonly string _topic;
        private readonly int _retryCount;
        private readonly int _retryIntervalSeconds;
        private readonly int _maxRetryIntervalSeconds;
        private bool _isCircuitOpen;
        private DateTime _circuitOpenTime;
        private readonly int _circuitBreakerTimeoutSeconds;

        public KafkaProducer(
            IProducer<string, string> producer,
            ILogger<KafkaProducer> logger,
            IConfiguration configuration)
        {
            _producer = producer;
            _logger = logger;
            _topic = configuration["KafkaSettings:ReportRequestTopic"] ?? "report-requests";
            _retryCount = configuration.GetValue<int>("KafkaSettings:RetryCount", 5);
            _retryIntervalSeconds = configuration.GetValue<int>("KafkaSettings:RetryIntervalSeconds", 2);
            _maxRetryIntervalSeconds = configuration.GetValue<int>("KafkaSettings:MaxRetryIntervalSeconds", 30);
            _circuitBreakerTimeoutSeconds = configuration.GetValue<int>("KafkaSettings:CircuitBreakerTimeoutSeconds", 60);
            _isCircuitOpen = false;
            
            _logger.LogInformation("KafkaProducer initialized with topic: {Topic}", _topic);
        }

        public async Task ProduceAsync(string key, string value)
        {
            if (_isCircuitOpen)
            {
                if ((DateTime.UtcNow - _circuitOpenTime).TotalSeconds >= _circuitBreakerTimeoutSeconds)
                {
                    _logger.LogInformation("Circuit breaker timeout reached, attempting to reconnect...");
                    _isCircuitOpen = false;
                }
                else
                {
                    throw new Exception("Kafka bağlantısı şu anda kapalı. Lütfen daha sonra tekrar deneyin.");
                }
            }

            var currentRetry = 0;
            var currentDelay = _retryIntervalSeconds;

            while (currentRetry < _retryCount)
            {
                try
                {
                    _logger.LogInformation($"Producing message to topic {_topic} with key: {key}");
                    
                    var message = new Message<string, string>
                    {
                        Key = key,
                        Value = value
                    };

                    var result = await _producer.ProduceAsync(_topic, message);
                    _logger.LogInformation($"Message produced to topic {_topic}, partition: {result.Partition}, offset: {result.Offset}");
                    return;
                }
                catch (ProduceException<string, string> ex)
                {
                    currentRetry++;
                    if (currentRetry >= _retryCount)
                    {
                        _logger.LogError(ex, "Kafka bağlantısı {RetryCount} deneme sonunda başarısız oldu. Circuit breaker açılıyor.", _retryCount);
                        _isCircuitOpen = true;
                        _circuitOpenTime = DateTime.UtcNow;
                        throw;
                    }

                    _logger.LogWarning(ex, "Kafka bağlantısı başarısız. Deneme {CurrentRetry}/{RetryCount}. {CurrentDelay} saniye sonra tekrar denenecek.", 
                        currentRetry, _retryCount, currentDelay);

                    await Task.Delay(TimeSpan.FromSeconds(currentDelay));
                    currentDelay = Math.Min(currentDelay * 2, _maxRetryIntervalSeconds);
                }
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
} 