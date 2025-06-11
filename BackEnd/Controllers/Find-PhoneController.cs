using AutoMapper;
using Lost_and_Found.Interfaces;
using Lost_and_Found.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace Lost_and_Found.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Find_PhoneController : ControllerBase
    { 
            private readonly IFindPhoneService Find_Phonesserver;
            private readonly IMapper mp;
            public Find_PhoneController(IFindPhoneService Find_Phonesserver, IMapper mp)
            {
                this.Find_Phonesserver = Find_Phonesserver;
                this.mp = mp;
            }
        [Authorize(Roles = "Manager")]
        [HttpGet("Get All Founded Phones")]
            public IActionResult Get()
            {
                var findedphones = Find_Phonesserver.GetFoundedPhones();
            List<string> ret = [];
            foreach (var phone in findedphones)
                ret.Add(phone.Brand);
            
                return Ok(ret);
            }
        [Authorize]
        [HttpPost("Add find phone")]
            public IActionResult Post([FromForm] FindPhoneDTO findphoneDTO)
            {
                var findphone = Find_Phonesserver.AddFoundedPhone(findphoneDTO);
                if (findphone == null)
                    return BadRequest("Phone Number Already Exists");


                return Ok($"Added find phone {findphoneDTO.Brand}");
          }

        [Authorize(Roles = "Manager")]
        [HttpPut("Update found phone")]
            public IActionResult Update([FromForm] FindPhoneDTO foundphoneDTO)
            {
                var findphone = Find_Phonesserver.UpdateFoundedPhone(foundphoneDTO);
                if (findphone == null)
                    return BadRequest("Phone Number do not Exists");


                return Ok($"Updated find phone {foundphoneDTO.Brand}");
            }

        [Authorize(Roles = "Manager")]
        [HttpDelete("Delete find phone")]
            public IActionResult Delete([FromForm] string phonenumber)
            {
                var ret = Find_Phonesserver.DeleteFoundedPhone(phonenumber);
                if (ret == null)
                    return BadRequest("Phone Number Not Found");

                return Ok(ret);
            }
        }
}
