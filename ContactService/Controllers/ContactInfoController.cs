using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactService.Domain;
using ContactService.Domain.Entities;
using ContactService.Models;
using ContactService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContactService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactInfoController : ControllerBase
    {
        private readonly IContactInfoService _contactInfoService;

        public ContactInfoController(IContactInfoService contactInfoService)
        {
            _contactInfoService = contactInfoService;
        }

        [HttpGet("{personId}")]
        public async Task<ActionResult<List<ContactInfo>>> GetAllByPersonId(Guid personId)
        {
            var result = await _contactInfoService.GetAllByPersonIdAsync(personId);
            return Ok(result);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<ContactInfo>> GetById(Guid id)
        {
            var result = await _contactInfoService.GetByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> AddContactInfo([FromBody] AddContactInfoDto dto)
        {
            var contactInfo = new ContactInfo
            {
                PersonId = dto.PersonId,
                Type = dto.InfoType,
                Value = dto.InfoContent
            };
            
            await _contactInfoService.AddAsync(contactInfo);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Update(ContactInfo contactInfo)
        {
            await _contactInfoService.UpdateAsync(contactInfo);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _contactInfoService.DeleteAsync(id);
            return Ok();
        }
    }
} 