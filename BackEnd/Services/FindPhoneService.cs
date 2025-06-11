using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;
using Lost_and_Found.Models;
using AutoMapper;
using Lost_and_Found.Interfaces;
using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace Lost_and_Found.Services
{
    public class FindPhoneService : IFindPhoneService
    {
        private DataConnection con;
        private IMapper mp;
        public FindPhoneService(DataConnection con, IMapper mp)
        {
            this.mp = mp;
            this.con = con;
        }

        public List<FindPhone> GetFoundedPhones()
        {
            return con.FindPhones.ToList();
        }

        public async Task<FindPhone> AddFoundedPhone(FindPhoneDTO phone)
        {
            if (con.FindPhones.FirstOrDefault(o => o.Brand == phone.Brand) != null
                || con.Users.FirstOrDefault(o => o.Email == phone.FinderEmail) == null)
                return null;

            using var stream = new MemoryStream();
            phone.PhonePhoto?.CopyTo(stream);


            FindPhone phone1 = new()
            {
               
                PhonePhoto = stream.ToArray(),
                Color = phone.Color,
                Brand = phone.Brand,
                Street = phone.Street,
                Government = phone.Government,
                Center = phone.Center,
                FinderEmail = phone.FinderEmail
            };
             con.FindPhones.Add(phone1);
             con.SaveChanges();

            using var httpClient = new HttpClient();

            var form = new MultipartFormDataContent();

            // الصورة بصيغة byte[] مفروض جاية من phone1.PhonePhoto
            var imageContent = new ByteArrayContent(phone1.PhonePhoto);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            // إضافة الصورة للحقل المطلوب
            form.Add(imageContent, "image", "photo.jpg");

            // إضافة الإيميل كـ form field
            form.Add(new StringContent(phone1.FinderEmail), "FounderEmail");

            // تنفيذ الطلب
            var response = await httpClient.PostAsync("http://localhost:9000/upload/", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send image to AI service. Status: {response.StatusCode}, Details: {error}");
            } 
            return phone1;
        }

        public FindPhone UpdateFoundedPhone(FindPhoneDTO phone)
        {
            if (con.FindPhones.FirstOrDefault(o => o.Brand == phone.Brand) == null
                || con.Users.FirstOrDefault(o => o.Email == phone.FinderEmail) == null)
                return null;

            FindPhone phone1 = con.FindPhones.FirstOrDefault(o => o.Brand == phone.Brand);

            using var stream = new MemoryStream();
            phone.PhonePhoto?.CopyTo(stream);


            phone1.PhonePhoto = stream.ToArray();
            phone1.Color = phone.Color;
            phone1.Brand = phone.Brand;
            phone1.Street = phone.Street;
            phone1.Center = phone.Center;
            phone1.FinderEmail = phone.FinderEmail;
            phone1.Government = phone.Government;

            con.FindPhones.Update(phone1);
            con.SaveChanges();
            return phone1;
        }

        public string DeleteFoundedPhone(string phonenum)
        {
            if (con.FindPhones.FirstOrDefault(o => o.Brand == phonenum) == null)
                return null;

            con.FindPhones.Remove(con.FindPhones.FirstOrDefault(o => o.Brand == phonenum));
            con.SaveChanges();
            return $"Phone Number {phonenum} Deleted";
        }
    }
}