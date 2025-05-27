using System;
using MediatR;

namespace ReportService.Application.Commands
{
    public class CreateReportCommand : IRequest<Guid>
    {
        public string Location { get; set; }
    }
} 