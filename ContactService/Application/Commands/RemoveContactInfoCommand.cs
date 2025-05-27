using System;
using ContactService.Domain.Entities;
using MediatR;

namespace ContactService.Application.Commands
{
    public class RemoveContactInfoCommand : IRequest<bool>
    {
        public Guid PersonId { get; set; }
        public Guid ContactInfoId { get; set; }
    }
} 