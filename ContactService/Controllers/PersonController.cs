using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactService.Domain;
using ContactService.Domain.Entities;
using ContactService.Models;
using ContactService.Services;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ContactService.Application.Commands;
using System.ComponentModel.DataAnnotations;

namespace ContactService.Controllers
{
    /// <summary>
    /// Kişi işlemlerini yöneten controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;
        private readonly IMediator _mediator;

        public PersonController(IPersonService personService, IMediator mediator)
        {
            _personService = personService;
            _mediator = mediator;
        }

        /// <summary>
        /// Tüm kişileri listeler
        /// </summary>
        /// <returns>Kişi listesi</returns>
        /// <response code="200">Kişiler başarıyla listelendi</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<Person>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<Person>>> GetAll()
        {
            var result = await _personService.GetAllPersonsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kişiyi getirir
        /// </summary>
        /// <param name="id">Kişi ID'si</param>
        /// <returns>Kişi detayları</returns>
        /// <response code="200">Kişi başarıyla getirildi</response>
        /// <response code="404">Kişi bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Person), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Person>> GetById([Required] Guid id)
        {
            var result = await _personService.GetPersonByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Yeni bir kişi oluşturur
        /// </summary>
        /// <param name="dto">Kişi bilgileri</param>
        /// <returns>Oluşturulan kişinin ID'si</returns>
        /// <response code="200">Kişi başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<string>> Create([FromBody] CreatePersonDto dto)
        {
            var person = new Person
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Company = dto.Company
            };
            
            var result = await _personService.CreatePersonAsync(person);
            return Ok(result);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kişiyi günceller
        /// </summary>
        /// <param name="id">Kişi ID'si</param>
        /// <param name="dto">Güncellenecek kişi bilgileri</param>
        /// <returns>Güncelleme işlemi sonucu</returns>
        /// <response code="200">Kişi başarıyla güncellendi</response>
        /// <response code="404">Kişi bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> Update([Required] Guid id, [FromBody] UpdatePersonDto dto)
        {
            var person = new Person
            {
                Id = id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Company = dto.Company
            };
            
            await _personService.UpdatePersonAsync(person);
            return Ok(true);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kişiyi siler
        /// </summary>
        /// <param name="id">Kişi ID'si</param>
        /// <returns>Silme işlemi sonucu</returns>
        /// <response code="200">Kişi başarıyla silindi</response>
        /// <response code="404">Kişi bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> Delete([Required] Guid id)
        {
            await _personService.DeletePersonAsync(id);
            return Ok(true);
        }
    }
} 