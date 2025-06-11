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
        [HttpPost("matchCard")]
        public async Task<IActionResult> MatchCard(string email, MatchType matchType)
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
                    Name = card.User?.UserName ?? "",
                    NationalId = card.CardID,
                    Governorate = card.Government,
                    City = card.Center,
                    Street = card.Street,
                    Contact = card.User?.PhoneNumber ?? "",
                    ImageUrl = $"/static/lostedcard/{card.ImageName}"
                }
            }).ToList();

            var url = "http://localhost:9000/match/";

            foreach (var checkMatchObj in CardMatchDtos)
            {
                var jsonData = new
                {
                    MatchType = matchType.ToString(),
                    Lost = new CardLostMatchDto
                    {
                        Name = checkMatchObj.Lost?.Name ?? "",
                        NationalId = checkMatchObj.Lost?.NationalId ?? "",
                        Governorate = checkMatchObj.Lost?.Governorate ?? "",
                        City = checkMatchObj.Lost?.City ?? "",
                        Street = checkMatchObj.Lost?.Street ?? "",
                        Contact = checkMatchObj.Lost?.Contact ?? "",
                        ImageUrl = checkMatchObj.Lost?.ImageUrl
                    }
                };

                var jsonString = JsonConvert.SerializeObject(jsonData);  

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var matchCardResponse = JsonConvert.DeserializeObject<MatchCardResponse>(responseBody);
                        if (matchCardResponse != null)
                            results.Add(matchCardResponse);
                    }
                    else
                    {
                      
                    }
                }
                catch (Exception ex)
                {
                   
                    return StatusCode(500, $"An error occurred while trying to match cards: {ex.Message}");
                }
            }

            return Ok(results);
        }
        


        //[HttpPost("matchPhone")]
        //public async Task<IActionResult> MatchPhone(string email)
        //{
        //    List<LostPhone> lostPhones = phone_service.GetLostPhonesOfEmail(email);

        //    List<LostMa> lostPhoneDtos = lostPhones.Select(phone => new LostPhoneDto
        //    {
        //        Id = phone.Id,
        //        PhoneID = phone.PhoneID,
        //        PhonePhotoBase64 = Convert.ToBase64String(phone.PhonePhoto), // لو صورة باينري
        //        Brand = phone.Brand,
        //        Model = phone.Model,
        //        Description = phone.Description
        //    }).ToList();

        //    var url = "http://localhost:9000/match/";
        //    using var client = new HttpClient();

        //    foreach (var lostPhoneDto in lostPhoneDtos)
        //    {
        //        var jsonData = new
        //        {
        //            match_type = "image",
        //            lost_image = lostPhoneDto.PhonePhotoBase64
        //        };

        //        var jsonString = System.Text.Json.JsonSerializer.Serialize(jsonData);
        //        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        //        try
        //        {
        //            var response = await client.PostAsync(url, content);
        //            if (response.IsSuccessStatusCode)
        //            {
        //                var responseBody = await response.Content.ReadAsStringAsync();
        //                Console.WriteLine($"Response for PhoneID {lostPhoneDto.PhoneID}: {responseBody}");
        //            }
        //            else
        //            {
        //                Console.WriteLine($"Error for PhoneID {lostPhoneDto.PhoneID}: {response.StatusCode}");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Exception for PhoneID {lostPhoneDto.PhoneID}: {ex.Message}");
        //        }
        //    }

        //    return Ok(lostPhoneDtos);
        //}
        private async Task<byte[]?> ConvertFormFileToBytesAsync(IFormFile? file)
        {
            if (file == null)
                return null;

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
        //public IFormFile ByteArrayToIFormFile(byte[] fileBytes, string fileName)
        //{
        //    var stream = new MemoryStream(fileBytes);
        //    IFormFile file = new FormFile(stream, 0, fileBytes.Length, "name", fileName)
        //    {
        //        Headers = new HeaderDictionary(),
        //        ContentType = "image/jpeg" 
        //    };
        //    return file;
        //}
    }
}