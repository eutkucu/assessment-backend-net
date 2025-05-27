using System;
using ContactService.Domain.Entities;
using ContactService.Domain.Enums;
using MediatR;

namespace ContactService.Application.Commands
{
    public class AddContactInfoCommand : IRequest<bool>
    {
        public Guid PersonId { get; set; }
        public ContactType InfoType { get; set; }
        public string InfoContent { get; set; }
    }
} 