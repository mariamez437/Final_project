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
            private DataConnection con;
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
            if (con.FindCards.Any(o => o.CardID == card.CardID) ||
                !con.Users.Any(o => o.Email == card.FinderEmail))
            {
                return null;
            }

            byte[]? photoBytes = null;
            if (card.CardPhoto != null)
            {
                using var stream = new MemoryStream();
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

            con.FindCards.Add(card1);
            await con.SaveChangesAsync();

            using var httpClient = new HttpClient();
            var form = new MultipartFormDataContent();

            if (photoBytes != null)
            {
                var imageContent = new ByteArrayContent(photoBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                form.Add(imageContent, "image", imageName);
            }

            form.Add(new StringContent(card1.CardID), "national_id");
            form.Add(new StringContent(card1.Government), "governorate");
            form.Add(new StringContent(card1.Center), "city");
            form.Add(new StringContent(card1.Street), "street");
            form.Add(new StringContent(card.FinderEmail), "contact");  

            form.Add(new StringContent(card1.ImageName), "image_name");
            form.Add(new StringContent(card1.FinderEmail), "name");

           

            var response = await httpClient.PostAsync("http://localhost:9000/add_found", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send data to AI service. Status: {response.StatusCode}, Details: {error}");
            }

            return card1;
        }


        public FindCard UpdateFoundedCard(FindCardDTO card)
        {
            if (con.FindCards.FirstOrDefault(o => o.CardID == card.CardID) == null 
                || con.Users.FirstOrDefault(o => o.Email == card.FinderEmail) == null)
                return null;

            FindCard card1 = con.FindCards.FirstOrDefault(o => o.CardID == card.CardID);

            using var stream = new MemoryStream();
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
