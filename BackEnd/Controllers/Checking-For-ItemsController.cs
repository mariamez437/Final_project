using Lost_and_Found.Interfaces;
using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.DTO.CheckDto;
using Lost_and_Found.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;
using MatchType = Lost_and_Found.Enums.MatchType;
using System.Net.Http;
using Lost_and_Found.Models.DTO.MobileCheckDto.MobileResponse;
using Lost_and_Found.Models.DTO.MobileCheckDto;
using System.Numerics;

namespace Lost_and_Found.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Checking_For_ItemsController : ControllerBase
    {
        private readonly HttpClient client = new HttpClient();

        private readonly IChecking_For_Items checking_For_Items;
        private readonly ILostCardService card_service;
        private readonly ILostPhoneService phone_service;
        public Checking_For_ItemsController(ILostCardService _LCServ, ILostPhoneService _LPServ)
        {
            card_service = _LCServ;
            phone_service = _LPServ;
        }

        //[Authorize]
        [HttpGet("get-all-items")]
        public async Task<IActionResult> GetCards([FromQuery] string email)
        {
            return Ok(await checking_For_Items.All_Items(email));
        }
        [HttpPost("CardMatch")]
        public async Task<IActionResult> MatchCard([FromQuery] string email, [FromQuery] MatchType matchType)
        {
            var lostCards = card_service.GetLostCardsOfEmail(email);
            if (lostCards == null || lostCards.Count == 0)
            {
                return BadRequest("No lost cards found for the provided email.");
            }

            var results = new List<MatchCardResponse>();

            var CardMatchDtos = lostCards.Select(card => new CardMatchRequest
            {
                MatchType = matchType,
                Lost = new CardLostMatchDto
                {
                    name = card.User?.UserName ?? "",
                    national_id = card.CardID,
                    governorate = card.Government,
                    city = card.Center,
                    street = card.Street,
                    contact = card.User?.PhoneNumber ?? "",
                    image_name = card.ImageName

                }
            }).ToList();

            var url = "http://127.0.0.1:8010/match";

            foreach (var checkMatchObj in CardMatchDtos)
            {
                var jsonData = new Dictionary<string, object>
                {
                    ["match_type"] = matchType.ToString().ToLower(),
                    ["lost"] = new Dictionary<string, object>
                    {
                        ["name"] = checkMatchObj.Lost?.name ?? "",
                        ["national_id"] = checkMatchObj.Lost?.national_id ?? "",
                        ["governorate"] = checkMatchObj.Lost?.governorate ?? "",
                        ["city"] = checkMatchObj.Lost?.city ?? "",
                        ["street"] = checkMatchObj.Lost?.street ?? "",
                        ["contact"] = checkMatchObj.Lost?.contact ?? "",
                        ["image_name"] = checkMatchObj.Lost?.image_name ?? ""
                    }
                };

                var jsonString = JsonConvert.SerializeObject(jsonData);

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                try
                {
                    var client = new HttpClient();
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var matchCardResponse = JsonConvert.DeserializeObject<MatchCardResponse>(responseBody);
                        if (matchCardResponse != null)
                            results.Add(matchCardResponse);
                    }


                }
                catch (Exception ex)
                {

                    return StatusCode(500, $"An error occurred while trying to match cards: {ex.Message}");
                }
            }

            return Ok(results);
        }



        [HttpPost("matchPhone")]
        public async Task<IActionResult> MatchPhone([FromQuery] string email, [FromQuery] MatchType matchType)
        {
            var lostPhones = phone_service.GetLostPhonesOfEmail(email);
            if (lostPhones == null || lostPhones.Count == 0)
            {
                return BadRequest("No lost phones found for the provided email.");
            }

            var results = new List<MobileMatchResponseDto>();

            var MobileMatchDtos = lostPhones.Select(phone => new MobileMatchRequest
            {
               
                MatchType = matchType,
                Lost = new MobileLostMatchDto
                {
                    governorate = phone.Government,
                    city = phone.Center,
                    street = phone.Street,
                    contact = phone.User?.PhoneNumber ?? "",
                    image_name = phone.ImageName,
                    brand = phone.Brand,
                    color = phone.Color,
                    image = phone.PhonePhoto ,
                }
            }).ToList();

            var url = "http://127.0.0.1:8004/match/";

            foreach (var checkMatchObj in MobileMatchDtos)
            {
                var jsonData = new Dictionary<string, object>
                {
                    ["match_type"] = matchType.ToString().ToLower(),
                    ["lost"] = new Dictionary<string, object>
                    {
                        ["governorate"] = checkMatchObj.Lost?.governorate ?? "",
                        ["city"] = checkMatchObj.Lost?.city ?? "",
                        ["street"] = checkMatchObj.Lost?.street ?? "",
                        ["contact"] = checkMatchObj.Lost?.contact ?? "",
                        ["image_name"] = checkMatchObj.Lost?.image_name ?? "",
                        ["brand"] = checkMatchObj.Lost?.brand ?? "",
                        ["color"] = checkMatchObj.Lost?.color ?? "",
                        ["image"] = Convert.ToBase64String(checkMatchObj.Lost.image ?? new byte[0]),

                    }
                };

                var jsonString = JsonConvert.SerializeObject(jsonData);

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                try
                {
                    var client = new HttpClient();
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var matchphoneResponse = JsonConvert.DeserializeObject<MobileMatchResponseDto>(responseBody);
                        if (matchphoneResponse != null)
                            results.Add(matchphoneResponse);
                    }


                }
                catch (Exception ex)
                {

                    return StatusCode(500, $"An error occurred while trying to match phones: {ex.Message}");
                }
            }

            return Ok(results);

        }
        public static IFormFile ConvertBytesToIFormFile(byte[] fileBytes, string fileName, string contentType = "image/jpeg")
        {
            var stream = new MemoryStream(fileBytes);
            return new FormFile(stream, 0, fileBytes.Length, "image", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }
    }
}
