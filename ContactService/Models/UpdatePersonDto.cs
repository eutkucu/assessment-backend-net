using System;
using System.ComponentModel.DataAnnotations;

namespace ContactService.Models
{
    public class UpdatePersonDto
    {
        [Required(ErrorMessage = "The FirstName field is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "The LastName field is required.")]
        public string LastName { get; set; }

        public string Company { get; set; }
    }
} 