using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReportService.Domain.Enums;

namespace ReportService.Domain.Models
{
    public class Report
    {
        [BsonId]
        public Guid Id { get; set; }

        public DateTime RequestDate { get; set; }

        public ReportStatus Status { get; set; }

        public string? Location { get; set; }

        public int PersonCount { get; set; }

        public int PhoneNumberCount { get; set; }

        public string? ErrorMessage { get; set; }
    }
} 