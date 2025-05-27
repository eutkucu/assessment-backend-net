using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ReportService.Domain.Models;
using ReportService.Infrastructure.Interfaces;
using Newtonsoft.Json;
using ReportService.Infrastructure.Kafka;
using ReportService.Domain.Enums;

namespace ReportService.Application.Commands
{
    public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, Guid>
    {
        private readonly IReportRepository _reportRepository;
        private readonly KafkaProducer _kafkaProducer;

        public CreateReportCommandHandler(IReportRepository reportRepository, KafkaProducer kafkaProducer)
        {
            _reportRepository = reportRepository;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<Guid> Handle(CreateReportCommand request, CancellationToken cancellationToken)
        {
            var report = new Report
            {
                Id = Guid.NewGuid(),
                RequestDate = DateTime.UtcNow,
                Status = ReportStatus.Preparing,
                Location = request.Location,
                PersonCount = 0,
                PhoneNumberCount = 0
            };

            await _reportRepository.AddAsync(report);

            // Kafka'ya rapor isteği gönder
            var message = JsonConvert.SerializeObject(new
            {
                ReportId = report.Id,
                Location = report.Location
            });
            await _kafkaProducer.ProduceAsync(report.Id.ToString(), message);

            return report.Id;
        }
    }
} 