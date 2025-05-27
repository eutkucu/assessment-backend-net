using System;
using ContactService.Domain.Enums;

namespace ContactService.Models
{
    public class AddContactInfoDto
    {
        public Guid PersonId { get; set; }
        public ContactType InfoType { get; set; }
        public string InfoContent { get; set; }
    }
} 