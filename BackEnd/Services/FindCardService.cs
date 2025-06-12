using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;
using Lost_and_Found.Models;
using AutoMapper;
using Lost_and_Found.Interfaces;
using System.Text.Json;
using System.Text;

namespace Lost_and_Found.Services
{
    public class FindCardService : IFindCardService
    {
            private readonly DataConnection con;
            private IMapper mp;
            public FindCardService(DataConnection con, IMapper mp)
            {
                this.con = con;
                this.mp = mp;
            }

            public List<FindCard> GetFoundedCards()
            {
                return con.FindCards.ToList();
            }

        public async Task<FindCard?> AddFoundedCard(FindCardDTO card)
        {
            byte[]? photoBytes = null;
            if (card.CardPhoto != null)
            {
                var stream = new MemoryStream();
                await card.CardPhoto.CopyToAsync(stream);
                photoBytes = stream.ToArray();
            }

            string prefix = "find";
            string uniquePart = Guid.NewGuid().ToString("N");
            string imageName = prefix + uniquePart + ".jpg";

            FindCard card1 = new()
            {
                CardPhoto = photoBytes,
                CardID = card.CardID,
                Street = card.Street,
                Government = card.Government,
                Center = card.Center,
                FinderEmail = card.FinderEmail,
                ImageName = imageName
            };

            try
            {
                con.FindCards.Add(card1);
                await con.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            var form = new MultipartFormDataContent();

            // إضافة الصورة إذا كانت موجودة
            if (photoBytes != null && photoBytes.Length > 0)
            {
                var imageContent = new ByteArrayContent(photoBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                form.Add(imageContent, "image", imageName);
            }

            // إضافة البيانات النصية مع التأكد من عدم وجود قيم null
            form.Add(new StringContent(card1.FinderEmail ?? "Unknown"), "name");
            form.Add(new StringContent(card1.CardID ?? ""), "national_id");
            form.Add(new StringContent(card1.Government ?? ""), "governorate");
            form.Add(new StringContent(card1.Center ?? ""), "city");
            form.Add(new StringContent(card1.Street ?? ""), "street");
            form.Add(new StringContent(card1.FinderEmail ?? ""), "contact");
            form.Add(new StringContent(card1.ImageName ?? ""), "image_name");

            try
            {
                var response = await httpClient.PostAsync("http://127.0.0.1:8010/add_found", form);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    // طباعة تفاصيل أكثر للخطأ
                    Console.WriteLine($"Request failed with status: {response.StatusCode}");
                    Console.WriteLine($"Response content: {responseContent}");

                    // محاولة parse الخطأ من FastAPI
                    try
                    {
                        dynamic errorDetails = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
                        Console.WriteLine($"Error details: {errorDetails}");
                    }
                    catch
                    {
                        Console.WriteLine("Could not parse error response");
                    }

                    throw new Exception($"Failed to send data to AI service. Status: {response.StatusCode}, Response: {responseContent}");
                }

                return card1;
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
                throw new Exception($"Network error when calling AI service: {httpEx.Message}");
            }
            catch (TaskCanceledException tcEx)
            {
                Console.WriteLine($"Request Timeout: {tcEx.Message}");
                throw new Exception("Request to AI service timed out");
            }
        }

        public FindCard UpdateFoundedCard(FindCardDTO card)
        {
            if (con.FindCards.FirstOrDefault(o => o.CardID == card.CardID) == null 
                || con.Users.FirstOrDefault(o => o.Email == card.FinderEmail) == null)
                return null;

            FindCard card1 = con.FindCards.FirstOrDefault(o => o.CardID == card.CardID);

            var stream = new MemoryStream();
            card.CardPhoto?.CopyTo(stream);

            card1.CardPhoto = stream.ToArray();
            card1.CardID = card.CardID;
            card1.Street = card.Street;
            card1.Government = card.Government;
            card1.Center = card.Center;
            card1.FinderEmail = card.FinderEmail;
           

            con.FindCards.Update(card1);
            con.SaveChanges();

            return card1;
        }

        public string DeleteFoundedCard(string card)
            {
                if (con.FindCards.FirstOrDefault(o => o.CardID == card) == null)
                    return null;

                con.FindCards.Remove(con.FindCards.FirstOrDefault(o => o.CardID == card));
                con.SaveChanges();
                return $"Card Number {card} Deleted";
            }
        }
}
