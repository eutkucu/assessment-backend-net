using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ReportService.Domain.Models;
using ReportService.Infrastructure.Interfaces;

namespace ReportService.Application.Queries
{
    public class GetAllReportsQueryHandler : IRequestHandler<GetAllReportsQuery, List<Report>>
    {
        private readonly IReportRepository _reportRepository;

        public GetAllReportsQueryHandler(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<Report>> Handle(GetAllReportsQuery request, CancellationToken cancellationToken)
        {
            var reports = await _reportRepository.GetAllAsync();
            return new List<Report>(reports);
        }
    }
} 