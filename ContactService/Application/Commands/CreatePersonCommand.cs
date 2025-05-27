using System;
using MediatR;

namespace ContactService.Application.Commands
{
    public class CreatePersonCommand : IRequest<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
    }
} 