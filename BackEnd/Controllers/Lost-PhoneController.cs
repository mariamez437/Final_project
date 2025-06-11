using AutoMapper;
using Lost_and_Found.Interfaces;
using Lost_and_Found.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lost_and_Found.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Lost_PhoneController : ControllerBase
    {
        private readonly ILostPhoneService lost_PhoneService;
        private readonly IMapper mp;
        public Lost_PhoneController(ILostPhoneService lost_PhoneService, IMapper mp)
        {
            this.lost_PhoneService = lost_PhoneService;
            this.mp = mp;
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("Get All Losted Phones")]
        public IActionResult Get()
        {
            var lost_Phones = lost_PhoneService.GetLostPhones();

            return Ok(lost_Phones.Select(o => new { PhoneNumber = o.PhoneNumber }).ToList());
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("Get Losted Phones By Email")]
        public IActionResult Getbyemail([FromForm] string email)
        {
            var lost_Phones = lost_PhoneService.GetLostPhonesOfID(email);
            if (lost_Phones == null)
                return BadRequest("No Losted Phones Found");

            return Ok(lost_Phones);
        }

        [Authorize]
        [HttpPost("Add_Losted_Phone")]
        public IActionResult Post([FromForm] LostPhoneDTO lostPhoneDTO)
        {
            var lostPhone = lost_PhoneService.AddLostPhone(lostPhoneDTO);
            if (lostPhone == null)
                return BadRequest("Phone Number Already Exists or invalid email");

            return Ok($"Added lost phone {lostPhoneDTO.PhoneNumber}");
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("Update Losted Phone")]
        public IActionResult Update([FromForm] LostPhoneDTO lostPhoneDTO)
        {
            var lostPhone = lost_PhoneService.UpdateLostPhone(lostPhoneDTO);
            if (lostPhone == null)
                return BadRequest("Phone Number or email do not Exists");

            return Ok($"Updated lost phone {lostPhoneDTO.PhoneNumber}");
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete("Delete Lost Phone")]
        public IActionResult Delete([FromForm] string email, [FromForm] string phonenum)
        {
            var ret = lost_PhoneService.DeleteLostPhone(email, phonenum);
            if (ret == null)
                return BadRequest("Phone Number or Email Not Found");

            return Ok(ret);
        }

    }
}