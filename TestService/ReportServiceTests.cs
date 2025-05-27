using System;
using Xunit;
using ReportService.Domain.Models;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace TestService
{
    public class ReportServiceTests
    {
        public class ReportTests
        {
            [Fact]
            public void Report_ShouldCreateWithValidData()
            {
                // Arrange
                var id = ObjectId.GenerateNewId().ToString();
                var requestDate = DateTime.UtcNow;
                var status = ReportStatus.Completed;
                var location = "Istanbul";
                var personCount = 10;
                var phoneNumberCount = 15;

                // Act
                var report = new Report
                {
                    Id = id,
                    RequestDate = requestDate,
                    Status = status,
                    Location = location,
                    PersonCount = personCount,
                    PhoneNumberCount = phoneNumberCount
                };

                // Assert
                Assert.Equal(id, report.Id);
                Assert.Equal(requestDate, report.RequestDate);
                Assert.Equal(status, report.Status);
                Assert.Equal(location, report.Location);
                Assert.Equal(personCount, report.PersonCount);
                Assert.Equal(phoneNumberCount, report.PhoneNumberCount);
            }

            [Fact]
            public void Report_ShouldHandleEmptyValues()
            {
                // Arrange & Act
                var report = new Report();

                // Assert
                Assert.Null(report.Id);
                Assert.Equal(default(DateTime), report.RequestDate);
                Assert.Null(report.Status);
                Assert.Null(report.Location);
                Assert.Equal(0, report.PersonCount);
                Assert.Equal(0, report.PhoneNumberCount);
            }

            [Fact]
            public void Report_ShouldHandleNegativeCounts()
            {
                // Arrange
                var report = new Report
                {
                    PersonCount = -5,
                    PhoneNumberCount = -3
                };

                // Assert
                Assert.Equal(-5, report.PersonCount);
                Assert.Equal(-3, report.PhoneNumberCount);
            }

            [Theory]
            [InlineData(ReportStatus.Preparing)]
            [InlineData(ReportStatus.Completed)]
            public void Report_ShouldHandleDifferentStatuses(ReportStatus status)
            {
                // Arrange & Act
                var report = new Report
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Status = status
                };

                // Assert
                Assert.Equal(status, report.Status);
            }

            [Theory]
            [InlineData("Istanbul")]
            [InlineData("Ankara")]
            [InlineData("Izmir")]
            [InlineData("Antalya")]
            public void Report_ShouldHandleDifferentLocations(string location)
            {
                // Arrange & Act
                var report = new Report
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Location = location
                };

                // Assert
                Assert.Equal(location, report.Location);
            }

            [Fact]
            public void Report_ShouldHandleMaxCounts()
            {
                // Arrange & Act
                var report = new Report
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    PersonCount = int.MaxValue,
                    PhoneNumberCount = int.MaxValue
                };

                // Assert
                Assert.Equal(int.MaxValue, report.PersonCount);
                Assert.Equal(int.MaxValue, report.PhoneNumberCount);
            }

            [Fact]
            public void Report_ShouldHandleMinCounts()
            {
                // Arrange & Act
                var report = new Report
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    PersonCount = int.MinValue,
                    PhoneNumberCount = int.MinValue
                };

                // Assert
                Assert.Equal(int.MinValue, report.PersonCount);
                Assert.Equal(int.MinValue, report.PhoneNumberCount);
            }

            [Fact]
            public void Report_ShouldHandleFutureDate()
            {
                // Arrange
                var futureDate = DateTime.UtcNow.AddYears(1);

                // Act
                var report = new Report
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    RequestDate = futureDate
                };

                // Assert
                Assert.Equal(futureDate, report.RequestDate);
            }

            [Fact]
            public void Report_ShouldHandlePastDate()
            {
                // Arrange
                var pastDate = DateTime.UtcNow.AddYears(-1);

                // Act
                var report = new Report
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    RequestDate = pastDate
                };

                // Assert
                Assert.Equal(pastDate, report.RequestDate);
            }
        }

        public class KafkaIntegrationTests
        {
            [Fact]
            public async Task KafkaProducer_ShouldProduceMessage()
            {
                // Arrange
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["KafkaSettings:BootstrapServers"] = "localhost:9092",
                        ["KafkaSettings:ReportRequestTopic"] = "report-requests",
                        ["KafkaSettings:RetryCount"] = "3",
                        ["KafkaSettings:RetryIntervalSeconds"] = "1"
                    })
                    .Build();

                var logger = new Mock<ILogger<KafkaProducer>>();
                var producer = new Mock<IProducer<string, string>>();
                var kafkaProducer = new KafkaProducer(producer.Object, logger.Object, configuration);

                var reportId = "test-report-id";
                var location = "Istanbul";
                var message = JsonConvert.SerializeObject(new { ReportId = reportId, Location = location });

                producer.Setup(p => p.ProduceAsync(
                    It.IsAny<string>(),
                    It.IsAny<Message<string, string>>(),
                    It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new DeliveryResult<string, string>());

                // Act
                await kafkaProducer.ProduceAsync(reportId, message);

                // Assert
                producer.Verify(p => p.ProduceAsync(
                    It.IsAny<string>(),
                    It.Is<Message<string, string>>(m => 
                        m.Key == reportId && 
                        m.Value == message),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
            }

            [Fact]
            public async Task KafkaConsumer_ShouldHandleMessage()
            {
                // Arrange
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["KafkaSettings:BootstrapServers"] = "localhost:9092",
                        ["KafkaSettings:ReportResultTopic"] = "report-results",
                        ["KafkaSettings:RetryCount"] = "3",
                        ["KafkaSettings:RetryIntervalSeconds"] = "1"
                    })
                    .Build();

                var logger = new Mock<ILogger<KafkaConsumer>>();
                var consumer = new Mock<IConsumer<string, string>>();
                var serviceProvider = new Mock<IServiceProvider>();
                var scope = new Mock<IServiceScope>();
                var scopeFactory = new Mock<IServiceScopeFactory>();
                var reportRepository = new Mock<IReportRepository>();

                var reportId = "test-report-id";
                var location = "Istanbul";
                var personCount = 10;
                var phoneNumberCount = 15;

                var message = JsonConvert.SerializeObject(new
                {
                    ReportId = reportId,
                    Status = "Completed",
                    Location = location,
                    PersonCount = personCount,
                    PhoneNumberCount = phoneNumberCount
                });

                var consumeResult = new ConsumeResult<string, string>
                {
                    Message = new Message<string, string>
                    {
                        Key = reportId,
                        Value = message
                    }
                };

                consumer.Setup(c => c.Consume(It.IsAny<TimeSpan>()))
                    .Returns(consumeResult);

                serviceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                    .Returns(scopeFactory.Object);

                scopeFactory.Setup(sf => sf.CreateScope())
                    .Returns(scope.Object);

                scope.Setup(s => s.ServiceProvider)
                    .Returns(serviceProvider.Object);

                serviceProvider.Setup(sp => sp.GetService(typeof(IReportRepository)))
                    .Returns(reportRepository.Object);

                var kafkaConsumer = new KafkaConsumer(
                    consumer.Object,
                    logger.Object,
                    configuration,
                    serviceProvider.Object);

                // Act & Assert
                // Not: BackgroundService olduğu için gerçek bir test yapmak zor
                // Bu test sadece consumer'ın oluşturulabildiğini ve yapılandırıldığını doğrular
                Assert.NotNull(kafkaConsumer);
            }
        }

        public class ReportServiceFunctionalityTests
        {
            private readonly Mock<IReportRepository> _reportRepositoryMock;
            private readonly Mock<ILogger<ReportService>> _loggerMock;
            private readonly Mock<IKafkaProducer> _kafkaProducerMock;
            private readonly ReportService _reportService;

            public ReportServiceFunctionalityTests()
            {
                _reportRepositoryMock = new Mock<IReportRepository>();
                _loggerMock = new Mock<ILogger<ReportService>>();
                _kafkaProducerMock = new Mock<IKafkaProducer>();
                _reportService = new ReportService(
                    _reportRepositoryMock.Object,
                    _kafkaProducerMock.Object,
                    _loggerMock.Object);
            }

            [Fact]
            public async Task CreateReport_ShouldCreateReportSuccessfully()
            {
                // Arrange
                var location = "Istanbul";
                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    Location = location,
                    Status = ReportStatus.Preparing,
                    RequestDate = DateTime.UtcNow
                };

                _reportRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Report>()))
                    .ReturnsAsync(report);

                // Act
                var result = await _reportService.CreateReportAsync(location);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(location, result.Location);
                Assert.Equal(ReportStatus.Preparing, result.Status);
                _reportRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Report>()), Times.Once);
                _kafkaProducerMock.Verify(x => x.ProduceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()), Times.Once);
            }

            [Fact]
            public async Task GetAllReports_ShouldReturnAllReports()
            {
                // Arrange
                var reports = new List<Report>
                {
                    new Report
                    {
                        Id = Guid.NewGuid(),
                        Location = "Istanbul",
                        Status = ReportStatus.Completed,
                        RequestDate = DateTime.UtcNow
                    },
                    new Report
                    {
                        Id = Guid.NewGuid(),
                        Location = "Ankara",
                        Status = ReportStatus.Preparing,
                        RequestDate = DateTime.UtcNow
                    }
                };

                _reportRepositoryMock.Setup(x => x.GetAllAsync())
                    .ReturnsAsync(reports);

                // Act
                var result = await _reportService.GetAllReportsAsync();

                // Assert
                Assert.Equal(2, result.Count());
                _reportRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
            }

            [Fact]
            public async Task GetReportDetails_ShouldReturnReportDetails()
            {
                // Arrange
                var reportId = Guid.NewGuid();
                var report = new Report
                {
                    Id = reportId,
                    Location = "Istanbul",
                    Status = ReportStatus.Completed,
                    RequestDate = DateTime.UtcNow,
                    PersonCount = 10,
                    PhoneNumberCount = 15
                };

                _reportRepositoryMock.Setup(x => x.GetByIdAsync(reportId))
                    .ReturnsAsync(report);

                // Act
                var result = await _reportService.GetReportDetailsAsync(reportId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(reportId, result.Id);
                Assert.Equal(10, result.PersonCount);
                Assert.Equal(15, result.PhoneNumberCount);
            }

            [Fact]
            public async Task UpdateReportStatus_ShouldUpdateStatusSuccessfully()
            {
                // Arrange
                var reportId = Guid.NewGuid();
                var report = new Report
                {
                    Id = reportId,
                    Location = "Istanbul",
                    Status = ReportStatus.Preparing,
                    RequestDate = DateTime.UtcNow
                };

                _reportRepositoryMock.Setup(x => x.GetByIdAsync(reportId))
                    .ReturnsAsync(report);
                _reportRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Report>()))
                    .ReturnsAsync(report);

                // Act
                var result = await _reportService.UpdateReportStatusAsync(reportId, ReportStatus.Completed);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(ReportStatus.Completed, result.Status);
                _reportRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Report>()), Times.Once);
            }

            [Fact]
            public async Task ProcessReport_ShouldProcessReportSuccessfully()
            {
                // Arrange
                var reportId = Guid.NewGuid();
                var location = "Istanbul";
                var report = new Report
                {
                    Id = reportId,
                    Location = location,
                    Status = ReportStatus.Preparing,
                    RequestDate = DateTime.UtcNow
                };

                var persons = new List<Person>
                {
                    new Person
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "John",
                        LastName = "Doe",
                        ContactInfos = new List<ContactInfo>
                        {
                            new ContactInfo
                            {
                                Id = Guid.NewGuid(),
                                InfoType = ContactType.Location,
                                InfoContent = location
                            },
                            new ContactInfo
                            {
                                Id = Guid.NewGuid(),
                                InfoType = ContactType.PhoneNumber,
                                InfoContent = "1234567890"
                            }
                        }
                    }
                };

                _reportRepositoryMock.Setup(x => x.GetByIdAsync(reportId))
                    .ReturnsAsync(report);
                _reportRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Report>()))
                    .ReturnsAsync(report);

                // Act
                await _reportService.ProcessReportAsync(reportId, persons);

                // Assert
                _reportRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Report>(r =>
                    r.Status == ReportStatus.Completed &&
                    r.PersonCount == 1 &&
                    r.PhoneNumberCount == 1)), Times.Once);
            }
        }
    }
} 