using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;
using Lost_and_Found.Models;
using AutoMapper;
using Lost_and_Found.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lost_and_Found.Services
{
    public class LostCardService:ILostCardService
    {
        private DataConnection con;
        private IMapper mp;
        public LostCardService(DataConnection con, IMapper mp)
        {
            this.con = con;
            this.mp = mp;
        }

        public List<LostCard> GetLostCards()
        {
            return con.LostCards.ToList();
        }

        public List<LostCard> GetLostCardsOfEmail(string email)
        {
            return con.LostCards
                      .Where(o => o.ForiegnKey_UserEmail == email)
                      .ToList();
        }
        public async Task<LostCard?> AddLostCard(LostCardsDTO lostCardDTO)
        {
            if (con.LostCards.Any(o => o.CardID == lostCardDTO.CardID) ||
                !con.Users.Any(o => o.Email == lostCardDTO.ForiegnKey_UserEmail))
            {
                return null;
            }

            byte[]? photoBytes = null;
            if (lostCardDTO.CardPhoto != null)
            {
                using var stream = new MemoryStream();
                await lostCardDTO.CardPhoto.CopyToAsync(stream);
                photoBytes = stream.ToArray();
            }

            string prefix = "lost";
            string uniquePart = Guid.NewGuid().ToString("N");
            string imageName = prefix + uniquePart + ".jpg";

            LostCard lostCard = new()
            {
                CardID = lostCardDTO.CardID,
                CardPhoto = photoBytes,
                Street = lostCardDTO.Street,
                Center = lostCardDTO.Center,
                Government = lostCardDTO.Government,
                ImageName = imageName,
                ForiegnKey_UserEmail = lostCardDTO.ForiegnKey_UserEmail
            };

            con.LostCards.Add(lostCard);
            await con.SaveChangesAsync();

            using var httpClient = new HttpClient();
            var form = new MultipartFormDataContent();

            if (photoBytes != null)
            {
                var imageContent = new ByteArrayContent(photoBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                form.Add(imageContent, "image", imageName);
            }

          //  form.Add(new StringContent(lostCardDTO.Name ?? ""), "name");
            form.Add(new StringContent(lostCardDTO.CardID), "national_id");
            form.Add(new StringContent(lostCardDTO.Government), "governorate");
            form.Add(new StringContent(lostCardDTO.Center), "city");
            form.Add(new StringContent(lostCardDTO.Street), "street");
            form.Add(new StringContent(lostCardDTO.ForiegnKey_UserEmail ?? ""), "contact");
            form.Add(new StringContent(imageName), "image_name");

            var response = await httpClient.PostAsync("http://localhost:9000/add_lost", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send data to AI service. Status: {response.StatusCode}, Details: {error}");
            }

            return lostCard;
        }


        public LostCard UpdateLostCard(LostCardsDTO card)
        {
            if (con.LostCards.FirstOrDefault(o => o.CardID == card.CardID) == null
                || con.Users.FirstOrDefault(o => o.Email == card.ForiegnKey_UserEmail) == null)
                return null;

            LostCard card1 = con.LostCards.FirstOrDefault(o => o.CardID == card.CardID);

            using var stream = new MemoryStream();
            card.CardPhoto?.CopyTo(stream);


            card1.CardID = card.CardID;
            card1.CardPhoto = stream.ToArray();
            card1.Street = card.Street;
            card1.Center = card.Center;
            card1.Government = card.Government;

            con.LostCards.Update(card1);
            con.SaveChanges();

            return card1;
        }

        public string DeleteLostCard(string email, string cardid)
        {
            if (con.LostCards.FirstOrDefault(o => o.ForiegnKey_UserEmail == email) == null
                || con.LostCards.FirstOrDefault(o => o.CardID == cardid) == null)
                return null;

            con.LostCards.Remove(con.LostCards.FirstOrDefault(o => o.CardID == cardid));
            con.SaveChanges();
            return $"Card Number {cardid} Deleted";
        }

      
    }
}
