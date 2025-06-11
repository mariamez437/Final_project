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
    public class Find_CardController : ControllerBase
    {

        private readonly IFindCardService Find_CardServices;
        private readonly IMapper mp;
        public Find_CardController(IFindCardService lost_CardService, IMapper mp)
        {
            this.Find_CardServices = lost_CardService;
            this.mp = mp;
        }
        [Authorize(Roles = "Manager")]
        [HttpGet("Get_All_Founded_Cards")]
        public IActionResult Get()
        {
            var findcards = Find_CardServices.GetFoundedCards();
            List<string> ret = [];
            foreach (var card in findcards)
                ret.Add(card.CardID);

            return Ok(ret);
        }
        [Authorize]
        [HttpPost("Add_Find_Card")] // افصل لوجك  ai خله ف تشكنج
        public IActionResult Post([FromForm] FindCardDTO findCardDTO)
        {
            var findcard = Find_CardServices.AddFoundedCard(findCardDTO);
            if (findcard == null)
                return BadRequest("Card Number Already Exists");

            return Ok($"Added find card {findCardDTO.CardID}");
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("Update_Find_Card")]
        public IActionResult Update([FromForm] FindCardDTO lostCardDTO)
        {
            var findcard = Find_CardServices.UpdateFoundedCard(lostCardDTO);
            if (findcard == null)
                return BadRequest("Card Number do not Exists");

            return Ok($"Updated find card {lostCardDTO.CardID}");
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete("Delete_find_Card")]
        public IActionResult Delete([FromForm] string cardid)
        {
            var ret = Find_CardServices.DeleteFoundedCard(cardid);
            if (ret == null)
                return BadRequest("Card Number Not Found");

            return Ok(ret);
        }
    }
}
