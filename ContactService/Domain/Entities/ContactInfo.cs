using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ContactService.Domain.Enums;

namespace ContactService.Domain.Entities
{
    public class ContactInfo
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid PersonId { get; set; }

        public ContactType Type { get; set; }

        public string Value { get; set; }
    }
} 