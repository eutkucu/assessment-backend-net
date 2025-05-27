using System;
using ContactService.Domain.Entities;
using MediatR;

namespace ContactService.Application.Queries
{
    public class GetPersonByIdQuery : IRequest<Person>
    {
        public Guid Id { get; set; }
    }
} 