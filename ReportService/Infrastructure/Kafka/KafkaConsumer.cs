using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using ReportService.Infrastructure;
using System.Text.Json;

namespace ReportService.Infrastructure.Kafka
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ReportManager _reportManager;
        private readonly ILogger<KafkaConsumer> _logger;
        private bool _isSubscribed = false;

        public KafkaConsumer(
            IConsumer<string, string> consumer,
            ReportManager reportManager,
            ILogger<KafkaConsumer> logger)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _reportManager = reportManager ?? throw new ArgumentNullException(nameof(reportManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka consumer starting...");

            try
            {
                if (!_isSubscribed)
                {
                    _consumer.Subscribe("report-requests");
                    _isSubscribed = true;
                    _logger.LogInformation("Subscribed to report-requests topic");
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogDebug("Waiting for message...");
                        var result = _consumer.Consume(TimeSpan.FromSeconds(1));
                        
                        if (result == null)
                        {
                            _logger.LogDebug("No message received");
                            continue;
                        }

                        if (string.IsNullOrEmpty(result.Message?.Value))
                        {
                            _logger.LogWarning("Received null or empty message");
                            continue;
                        }

                        _logger.LogInformation($"Received message: {result.Message.Value}");
                        
                        var message = JsonSerializer.Deserialize<ReportMessage>(result.Message.Value);
                        if (message == null)
                        {
                            _logger.LogWarning("Failed to deserialize message");
                            continue;
                        }

                        if (message.ReportId == Guid.Empty)
                        {
                            _logger.LogWarning("Invalid ReportId in message");
                            continue;
                        }

                        _logger.LogInformation($"Processing report {message.ReportId}");
                        await _reportManager.ProcessReportAsync(message.ReportId);
                        _consumer.Commit(result);
                        _logger.LogInformation($"Successfully processed report {message.ReportId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in Kafka consumer");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Stopping Kafka consumer...");
                if (_isSubscribed)
                {
                    _consumer.Close();
                    _isSubscribed = false;
                }
                await base.StopAsync(cancellationToken);
                _logger.LogInformation("Kafka consumer stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Kafka consumer");
                throw;
            }
        }
    }

    public class ReportMessage
    {
        public Guid ReportId { get; set; }
        public string Location { get; set; }
    }
} 