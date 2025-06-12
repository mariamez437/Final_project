using AutoMapper;
using Lost_and_Found.Interfaces;
using Lost_and_Found.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lost_and_Found.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Lost_CardController : ControllerBase
    {
        private readonly ILostCardService lost_CardService;
        private readonly IMapper mp;
        public Lost_CardController(ILostCardService lost_CardService, IMapper mp)
        {
            this.lost_CardService = lost_CardService;
            this.mp = mp;
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("Get All Losted Cards")]
        public IActionResult Get()
        {
            var lost_Cards = lost_CardService.GetLostCards();
            List<string> ret = [];

            foreach (var i in lost_Cards)
                ret.Add(i.CardID);

            return Ok(ret);
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("Get Losted Cards By Email")]
        public IActionResult Getbyemail([FromForm] string email)
        {
            var lost_Cards = lost_CardService.GetLostCardsOfEmail(email);
            if (lost_Cards == null)
                return BadRequest("No Losted Cards Found");

            return Ok(lost_Cards);
        }

        [Authorize]
        [HttpPost("Add_Losted_Card")]
        public async Task<IActionResult> Post([FromForm] LostCardsDTO lostCardDTO)
        {
            var lostcard = await lost_CardService.AddLostCard(lostCardDTO);
            if (lostcard == null)
                return BadRequest("Card Number Already Exists or invalid email");

            return Ok($"Added lost card with id = {lostCardDTO.CardID}");
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("Update Losted Card")]
        public IActionResult Update([FromForm] LostCardsDTO lostCardDTO)
        {
            var lostcard = lost_CardService.UpdateLostCard(lostCardDTO);
            if (lostcard == null)
                return BadRequest("Card Number or email do not Exists");

            return Ok($"Updated lost card with id = {lostCardDTO.CardID}");
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete("Delete Lost Card")]
        public IActionResult Delete([FromForm] string email, [FromForm] string cardid)
        {
            var ret = lost_CardService.DeleteLostCard(email, cardid);
            if (ret == null)
                return BadRequest("Card Number or Email Not Found");

            return Ok(ret);
        }

    }
}
