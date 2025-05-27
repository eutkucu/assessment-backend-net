using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ReportService.Infrastructure;
using ReportService.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ReportService.Controllers
{
    /// <summary>
    /// Rapor işlemlerini yöneten controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReportController : ControllerBase
    {
        private readonly ReportManager _reportManager;
        private readonly ILogger<ReportController> _logger;

        public ReportController(ReportManager reportManager, ILogger<ReportController> logger)
        {
            _reportManager = reportManager;
            _logger = logger;
        }

        /// <summary>
        /// Tüm raporları listeler
        /// </summary>
        /// <returns>Rapor listesi</returns>
        /// <response code="200">Raporlar başarıyla listelendi</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<Report>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<Report>>> GetAllReports()
        {
            try
            {
                var reports = await _reportManager.GetAllReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Raporlar listelenirken hata oluştu");
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip raporu getirir
        /// </summary>
        /// <param name="id">Rapor ID'si</param>
        /// <returns>Rapor detayları</returns>
        /// <response code="200">Rapor başarıyla getirildi</response>
        /// <response code="404">Rapor bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Report), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Report>> GetReport(Guid id)
        {
            try
            {
                var report = await _reportManager.GetReportByIdAsync(id);
                return Ok(report);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Report not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rapor getirilirken hata oluştu. ID: {id}");
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
            }
        }

        /// <summary>
        /// Yeni bir rapor talebi oluşturur
        /// </summary>
        /// <param name="request">Rapor talebi bilgileri</param>
        /// <returns>Oluşturulan rapor</returns>
        /// <response code="200">Rapor talebi başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost]
        [ProducesResponseType(typeof(Report), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Report>> CreateReport([FromBody] ReportService.Domain.Models.CreateReportRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Location))
                {
                    return BadRequest(new { error = "Location is required" });
                }

                var report = await _reportManager.CreateReportAsync(request);
                return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rapor oluşturulurken hata oluştu");
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
            }
        }
    }

    public class CreateReportRequest
    {
        public string Location { get; set; }
    }
} 