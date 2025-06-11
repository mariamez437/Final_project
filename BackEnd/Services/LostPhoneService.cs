using AutoMapper;
using Lost_and_Found.Interfaces;
using Lost_and_Found.Models;
using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;

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

        public List<string> GetLostPhonesOfID(string email)
        {
            return con.LostPhones.Where(o => o.ForiegnKey_UserEmail == email).Select(o => o.PhoneNumber).ToList();
        }
        public LostPhone AddLostPhone(LostPhoneDTO lostPhoneDTO)
        {
            if (con.LostPhones.FirstOrDefault(o => o.PhoneNumber == lostPhoneDTO.PhoneNumber) != null
                || con.Users.FirstOrDefault(o => o.Email == lostPhoneDTO.ForiegnKey_UserEmail) == null)
                return null;

            using var stream = new MemoryStream();
            lostPhoneDTO.PhonePhoto?.CopyTo(stream);


            LostPhone lostphone = new()
            {
                PhoneNumber = lostPhoneDTO.PhoneNumber,
                ForiegnKey_UserEmail = lostPhoneDTO.ForiegnKey_UserEmail,
                PhonePhoto = stream.ToArray(),
                Color = lostPhoneDTO.Color,
                Brand = lostPhoneDTO.Brand,
                Center = lostPhoneDTO.Center,
                Government = lostPhoneDTO.Government,
                Street = lostPhoneDTO.Street,
            };

            con.LostPhones.Add(lostphone);
            con.SaveChanges();

            return lostphone;
        }
        public LostPhone UpdateLostPhone(LostPhoneDTO phone)
        {
            if (con.LostPhones.FirstOrDefault(o => o.PhoneNumber == phone.PhoneNumber) == null
                || con.Users.FirstOrDefault(o => o.Email == phone.ForiegnKey_UserEmail) == null)
                return null;

            LostPhone phone1 = con.LostPhones.FirstOrDefault(o => o.PhoneNumber == phone.PhoneNumber);

            using var stream = new MemoryStream();
            phone.PhonePhoto?.CopyTo(stream);

            phone1.PhoneNumber = phone.PhoneNumber;
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
            if (con.LostPhones.FirstOrDefault(o => o.ForiegnKey_UserEmail == email) == null
                || con.LostPhones.FirstOrDefault(o => o.PhoneNumber == phonenum) == null)
                return null;

            con.LostPhones.Remove(con.LostPhones.FirstOrDefault(o => o.PhoneNumber == phonenum));
            con.SaveChanges();
            return $"Phone Number {phonenum} Deleted";
        }
    }
}