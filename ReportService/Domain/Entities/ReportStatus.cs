using System;

namespace ReportService.Domain.Entities
{
    public class ReportStatus
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 