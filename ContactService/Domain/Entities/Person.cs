using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ContactService.Domain.Entities
{
    public class Person
    {
        [BsonId]
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Company { get; set; }

        public List<ContactInfo> ContactInfos { get; set; } = new List<ContactInfo>();
    }
} 