using System.Collections.Generic;
using MediatR;
using ReportService.Domain.Models;

namespace ReportService.Application.Queries
{
    public class GetAllReportsQuery : IRequest<List<Report>>
    {
    }
} 