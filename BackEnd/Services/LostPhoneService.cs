using AutoMapper;
using Lost_and_Found.Interfaces;
using Lost_and_Found.Models;
using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Lost_and_Found.Services
{
    public class LostPhoneService : ILostPhoneService
    {
        private DataConnection con;
        private IMapper mp;
        public LostPhoneService(DataConnection con, IMapper mp)
        {
            this.con = con;
            this.mp = mp;
        }

        public List<LostPhone> GetLostPhones()
        {
            return con.LostPhones.ToList();
        }

        public List<int> GetLostPhonesOfID(string email)
        {
            return con.LostPhones
                     .Where(o => o.ForiegnKey_UserEmail == email)
                     .Select(o => o.ID)
                     .ToList();
        }
        public async Task<LostPhone> AddLostPhone(LostPhoneDTO phone)
        {

            byte[]? photoBytes = null;
            if (phone.PhonePhoto != null)
            {
                var stream = new MemoryStream();
                await phone.PhonePhoto.CopyToAsync(stream);
                photoBytes = stream.ToArray();

            }

            string prefix = "findphone";
            string uniquePart = Guid.NewGuid().ToString("N");
            string imageName = prefix + uniquePart + ".jpg";


            LostPhone lostphone = new()
            {
                ForiegnKey_UserEmail = phone.ForiegnKey_UserEmail,
                PhonePhoto = photoBytes,
                Color = phone.Color,
                Brand = phone.Brand,
                Center = phone.Center,
                Government = phone.Government,
                Street = phone.Street,
                ImageName = imageName,

            };

            con.LostPhones.Add(lostphone);
            con.SaveChanges();

            return lostphone;
        }
        public LostPhone UpdateLostPhone(LostPhoneDTO phone)
        {
            
            LostPhone phone1 = con.LostPhones.FirstOrDefault();

            using var stream = new MemoryStream();
            phone.PhonePhoto?.CopyTo(stream);

            phone1.PhonePhoto = stream.ToArray();
            phone1.Color = phone.Color;
            phone1.Brand = phone.Brand;
            phone1.Street = phone.Street;
            phone1.Government = phone.Government;
            phone1.Center = phone.Center;

            con.LostPhones.Update(phone1);
            con.SaveChanges();
            return phone1;
        }

        public string DeleteLostPhone(string email, string phonenum)
        {
           

            con.LostPhones.Remove(con.LostPhones.FirstOrDefault());
            con.SaveChanges();
            return $"Phone Number {phonenum} Deleted";
        }

        public List<LostPhone> GetLostPhonesOfEmail(string email)
        {
            return con.LostPhones
                .Include(p => p.User)
                .Where(p => p.ForiegnKey_UserEmail == email)
                .ToList();
        }
    }
}