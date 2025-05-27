using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ReportService.Infrastructure.Interfaces;
using ReportService.Domain.Models;

namespace ReportService.Application.Queries
{
    public class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, Report>
    {
        private readonly IReportRepository _reportRepository;

        public GetReportByIdQueryHandler(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<Report> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
        {
            return await _reportRepository.GetByIdAsync(request.Id);
        }
    }
} 