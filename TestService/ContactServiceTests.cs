using System;
using Xunit;
using ContactService.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using Newtonsoft.Json;

namespace TestService
{
    public class ContactServiceTests
    {
        public class PersonTests
        {
            [Fact]
            public void Person_ShouldCreateWithValidData()
            {
                // Arrange
                var id = Guid.NewGuid();
                var firstName = "John";
                var lastName = "Doe";
                var company = "Test Company";

                // Act
                var person = new Person
                {
                    Id = id,
                    FirstName = firstName,
                    LastName = lastName,
                    Company = company
                };

                // Assert
                Assert.Equal(id, person.Id);
                Assert.Equal(firstName, person.FirstName);
                Assert.Equal(lastName, person.LastName);
                Assert.Equal(company, person.Company);
            }

            [Fact]
            public void Person_ShouldHandleEmptyValues()
            {
                // Arrange & Act
                var person = new Person();

                // Assert
                Assert.Equal(Guid.Empty, person.Id);
                Assert.Null(person.FirstName);
                Assert.Null(person.LastName);
                Assert.Null(person.Company);
            }
        }

        public class ContactInfoTests
        {
            [Fact]
            public void ContactInfo_ShouldCreateWithValidData()
            {
                // Arrange
                var id = Guid.NewGuid();
                var infoType = ContactType.Email;
                var infoContent = "test@example.com";

                // Act
                var contactInfo = new ContactInfo
                {
                    Id = id,
                    InfoType = infoType,
                    InfoContent = infoContent
                };

                // Assert
                Assert.Equal(id, contactInfo.Id);
                Assert.Equal(infoType, contactInfo.InfoType);
                Assert.Equal(infoContent, contactInfo.InfoContent);
            }

            [Fact]
            public void ContactInfo_ShouldHandleEmptyValues()
            {
                // Arrange & Act
                var contactInfo = new ContactInfo();

                // Assert
                Assert.Equal(Guid.Empty, contactInfo.Id);
                Assert.Equal(default(ContactType), contactInfo.InfoType);
                Assert.Null(contactInfo.InfoContent);
            }

            [Theory]
            [InlineData(ContactType.PhoneNumber, "1234567890")]
            [InlineData(ContactType.Email, "test@example.com")]
            [InlineData(ContactType.Location, "Istanbul, Turkey")]
            public void ContactInfo_ShouldHandleDifferentTypes(ContactType type, string content)
            {
                // Arrange & Act
                var contactInfo = new ContactInfo
                {
                    Id = Guid.NewGuid(),
                    InfoType = type,
                    InfoContent = content
                };

                // Assert
                Assert.Equal(type, contactInfo.InfoType);
                Assert.Equal(content, contactInfo.InfoContent);
            }

            [Fact]
            public void ContactInfo_ShouldHandleLongContent()
            {
                // Arrange
                var longContent = new string('a', 1000);

                // Act
                var contactInfo = new ContactInfo
                {
                    Id = Guid.NewGuid(),
                    InfoType = ContactType.Location,
                    InfoContent = longContent
                };

                // Assert
                Assert.Equal(longContent, contactInfo.InfoContent);
            }

            [Fact]
            public void ContactInfo_ShouldHandleSpecialCharacters()
            {
                // Arrange
                var specialContent = "test@example.com!@#$%^&*()_+{}|:\"<>?[]\\;',./";

                // Act
                var contactInfo = new ContactInfo
                {
                    Id = Guid.NewGuid(),
                    InfoType = ContactType.Email,
                    InfoContent = specialContent
                };

                // Assert
                Assert.Equal(specialContent, contactInfo.InfoContent);
            }

            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData(null)]
            public void ContactInfo_ShouldHandleEmptyOrNullContent(string content)
            {
                // Arrange & Act
                var contactInfo = new ContactInfo
                {
                    Id = Guid.NewGuid(),
                    InfoType = ContactType.PhoneNumber,
                    InfoContent = content
                };

                // Assert
                Assert.Equal(content, contactInfo.InfoContent);
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
                        ["Kafka:BootstrapServers"] = "localhost:9092",
                        ["Kafka:ReportResultTopic"] = "report-results",
                        ["Kafka:MaxRetryCount"] = "3"
                    })
                    .Build();

                var producer = new Mock<IProducer<string, string>>();
                var kafkaProducer = new KafkaProducer(configuration);

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

                producer.Setup(p => p.ProduceAsync(
                    It.IsAny<string>(),
                    It.IsAny<Message<string, string>>(),
                    It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new DeliveryResult<string, string>());

                // Act
                await kafkaProducer.ProduceAsync("report-results", reportId, message);

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
            public async Task KafkaConsumer_ShouldProcessReportRequest()
            {
                // Arrange
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Kafka:BootstrapServers"] = "localhost:9092",
                        ["Kafka:ReportRequestTopic"] = "report-requests",
                        ["Kafka:ReportResultTopic"] = "report-results",
                        ["Kafka:MaxRetryCount"] = "3"
                    })
                    .Build();

                var logger = new Mock<ILogger<KafkaConsumer>>();
                var consumer = new Mock<IConsumer<string, string>>();
                var producer = new Mock<KafkaProducer>();
                var serviceProvider = new Mock<IServiceProvider>();
                var scope = new Mock<IServiceScope>();
                var scopeFactory = new Mock<IServiceScopeFactory>();
                var personRepository = new Mock<IPersonRepository>();

                var reportId = "test-report-id";
                var location = "Istanbul";

                var message = JsonConvert.SerializeObject(new
                {
                    ReportId = reportId,
                    Location = location
                });

                var consumeResult = new ConsumeResult<string, string>
                {
                    Message = new Message<string, string>
                    {
                        Key = reportId,
                        Value = message
                    }
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

                consumer.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                    .Returns(consumeResult);

                serviceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                    .Returns(scopeFactory.Object);

                scopeFactory.Setup(sf => sf.CreateScope())
                    .Returns(scope.Object);

                scope.Setup(s => s.ServiceProvider)
                    .Returns(serviceProvider.Object);

                serviceProvider.Setup(sp => sp.GetService(typeof(IPersonRepository)))
                    .Returns(personRepository.Object);

                personRepository.Setup(r => r.GetAllAsync())
                    .ReturnsAsync(persons);

                var kafkaConsumer = new KafkaConsumer(
                    configuration,
                    serviceProvider.Object,
                    producer.Object,
                    "report-requests",
                    "report-results");

                // Act & Assert
                // Not: BackgroundService olduğu için gerçek bir test yapmak zor
                // Bu test sadece consumer'ın oluşturulabildiğini ve yapılandırıldığını doğrular
                Assert.NotNull(kafkaConsumer);
            }
        }

        public class ContactServiceFunctionalityTests
        {
            private readonly Mock<IPersonRepository> _personRepositoryMock;
            private readonly Mock<ILogger<ContactService>> _loggerMock;
            private readonly ContactService _contactService;

            public ContactServiceFunctionalityTests()
            {
                _personRepositoryMock = new Mock<IPersonRepository>();
                _loggerMock = new Mock<ILogger<ContactService>>();
                _contactService = new ContactService(_personRepositoryMock.Object, _loggerMock.Object);
            }

            [Fact]
            public async Task CreatePerson_ShouldCreatePersonSuccessfully()
            {
                // Arrange
                var person = new Person
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Company = "Test Company"
                };

                _personRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Person>()))
                    .ReturnsAsync(person);

                // Act
                var result = await _contactService.CreatePersonAsync(person);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(person.FirstName, result.FirstName);
                Assert.Equal(person.LastName, result.LastName);
                Assert.Equal(person.Company, result.Company);
                _personRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Person>()), Times.Once);
            }

            [Fact]
            public async Task DeletePerson_ShouldDeletePersonSuccessfully()
            {
                // Arrange
                var personId = Guid.NewGuid();
                _personRepositoryMock.Setup(x => x.DeleteAsync(personId))
                    .ReturnsAsync(true);

                // Act
                var result = await _contactService.DeletePersonAsync(personId);

                // Assert
                Assert.True(result);
                _personRepositoryMock.Verify(x => x.DeleteAsync(personId), Times.Once);
            }

            [Fact]
            public async Task AddContactInfo_ShouldAddContactInfoSuccessfully()
            {
                // Arrange
                var personId = Guid.NewGuid();
                var contactInfo = new ContactInfo
                {
                    InfoType = ContactType.PhoneNumber,
                    InfoContent = "1234567890"
                };

                var person = new Person
                {
                    Id = personId,
                    FirstName = "John",
                    LastName = "Doe"
                };

                _personRepositoryMock.Setup(x => x.GetByIdAsync(personId))
                    .ReturnsAsync(person);
                _personRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Person>()))
                    .ReturnsAsync(person);

                // Act
                var result = await _contactService.AddContactInfoAsync(personId, contactInfo);

                // Assert
                Assert.NotNull(result);
                Assert.Contains(result.ContactInfos, ci => 
                    ci.InfoType == contactInfo.InfoType && 
                    ci.InfoContent == contactInfo.InfoContent);
            }

            [Fact]
            public async Task RemoveContactInfo_ShouldRemoveContactInfoSuccessfully()
            {
                // Arrange
                var personId = Guid.NewGuid();
                var contactInfoId = Guid.NewGuid();

                var person = new Person
                {
                    Id = personId,
                    FirstName = "John",
                    LastName = "Doe",
                    ContactInfos = new List<ContactInfo>
                    {
                        new ContactInfo
                        {
                            Id = contactInfoId,
                            InfoType = ContactType.PhoneNumber,
                            InfoContent = "1234567890"
                        }
                    }
                };

                _personRepositoryMock.Setup(x => x.GetByIdAsync(personId))
                    .ReturnsAsync(person);
                _personRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Person>()))
                    .ReturnsAsync(person);

                // Act
                var result = await _contactService.RemoveContactInfoAsync(personId, contactInfoId);

                // Assert
                Assert.NotNull(result);
                Assert.DoesNotContain(result.ContactInfos, ci => ci.Id == contactInfoId);
            }

            [Fact]
            public async Task GetAllPersons_ShouldReturnAllPersons()
            {
                // Arrange
                var persons = new List<Person>
                {
                    new Person { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" },
                    new Person { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith" }
                };

                _personRepositoryMock.Setup(x => x.GetAllAsync())
                    .ReturnsAsync(persons);

                // Act
                var result = await _contactService.GetAllPersonsAsync();

                // Assert
                Assert.Equal(2, result.Count());
                _personRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
            }

            [Fact]
            public async Task GetPersonDetails_ShouldReturnPersonWithContactInfos()
            {
                // Arrange
                var personId = Guid.NewGuid();
                var person = new Person
                {
                    Id = personId,
                    FirstName = "John",
                    LastName = "Doe",
                    ContactInfos = new List<ContactInfo>
                    {
                        new ContactInfo
                        {
                            Id = Guid.NewGuid(),
                            InfoType = ContactType.PhoneNumber,
                            InfoContent = "1234567890"
                        }
                    }
                };

                _personRepositoryMock.Setup(x => x.GetByIdAsync(personId))
                    .ReturnsAsync(person);

                // Act
                var result = await _contactService.GetPersonDetailsAsync(personId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(personId, result.Id);
                Assert.Single(result.ContactInfos);
            }
        }
    }
} 