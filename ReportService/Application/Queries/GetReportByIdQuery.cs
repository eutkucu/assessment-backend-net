using System;
using MediatR;
using ReportService.Domain.Models;

namespace ReportService.Application.Queries
{
    public class GetReportByIdQuery : IRequest<Report>
    {
        public Guid Id { get; set; }
    }
} 