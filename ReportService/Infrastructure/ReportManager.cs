using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using ReportService.Domain.Models;
using ReportService.Domain.Enums;
using ReportService.Infrastructure.Interfaces;
using ReportService.Services;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Text.Json;

namespace ReportService.Infrastructure
{
    public class ReportManager
    {
        private readonly IReportRepository _reportRepository;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ContactReportService _contactReportService;
        private readonly ILogger<ReportManager> _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public ReportManager(
            IReportRepository reportRepository,
            IKafkaProducer kafkaProducer,
            ContactReportService contactReportService,
            ILogger<ReportManager> logger)
        {
            _reportRepository = reportRepository;
            _kafkaProducer = kafkaProducer;
            _contactReportService = contactReportService;
            _logger = logger;
        }

        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            try
            {
                return await _reportRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Raporlar listelenirken hata oluştu");
                throw;
            }
        }

        public async Task<Report> GetReportByIdAsync(Guid id)
        {
            try
            {
                var report = await _reportRepository.GetByIdAsync(id);
                if (report == null)
                {
                    _logger.LogWarning($"Rapor bulunamadı: {id}");
                    throw new KeyNotFoundException($"Report with ID {id} not found");
                }
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rapor getirilirken hata oluştu. ID: {id}");
                throw;
            }
        }

        public async Task<Report> CreateReportAsync(CreateReportRequest request)
        {
            try
            {
                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    Location = request.Location,
                    Status = ReportStatus.Pending,
                    RequestDate = DateTime.UtcNow,
                    PersonCount = 0,
                    PhoneNumberCount = 0,
                    ErrorMessage = null
                };

                await _reportRepository.AddAsync(report);

                try
                {
                    var message = System.Text.Json.JsonSerializer.Serialize(new { ReportId = report.Id, Location = report.Location });
                    await _kafkaProducer.ProduceAsync(report.Id.ToString(), message);
                    _logger.LogInformation($"Rapor oluşturuldu ve Kafka'ya gönderildi. ID: {report.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Kafka'ya mesaj gönderilirken hata oluştu. Report ID: {report.Id}");
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rapor oluşturulurken hata oluştu");
                throw;
            }
        }

        public async Task ProcessReportAsync(Guid reportId)
        {
            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    var report = await _reportRepository.GetByIdAsync(reportId);
                    if (report == null)
                    {
                        _logger.LogWarning($"İşlenecek rapor bulunamadı: {reportId}");
                        return;
                    }

                    if (report.Status != ReportStatus.Pending)
                    {
                        _logger.LogWarning($"Rapor zaten işlenmiş durumda: {reportId}, Status: {report.Status}");
                        return;
                    }

                    report.Status = ReportStatus.Preparing;
                    await _reportRepository.UpdateAsync(report);

                    try
                    {
                        var stats = await _contactReportService.GetContactStatsAsync(report.Location);
                        report.PersonCount = stats.PersonCount;
                        report.PhoneNumberCount = stats.PhoneNumberCount;
                        report.Status = ReportStatus.Completed;
                        report.ErrorMessage = null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Contact servisinden veri alınırken hata oluştu. Report ID: {reportId}");
                        report.Status = ReportStatus.Failed;
                        report.ErrorMessage = ex.Message;
                    }

                    await _reportRepository.UpdateAsync(report);
                    _logger.LogInformation($"Rapor işlemi tamamlandı. ID: {reportId}, Status: {report.Status}");
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rapor işlenirken hata oluştu. Report ID: {reportId}");
                throw;
            }
        }
    }
} 