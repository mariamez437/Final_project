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
            //if (con.FindPhones.FirstOrDefault(o => o.Brand == phone.Brand) != null
            //    || con.Users.FirstOrDefault(o => o.Email == phone.FinderEmail) == null)
            //    return null;
            byte[]? photoBytes = null;
            if(phone.PhonePhoto != null)
            {
                var stream = new MemoryStream();
                await phone.PhonePhoto.CopyToAsync(stream); 
                photoBytes = stream.ToArray();

            }

            string prefix = "findphone";
            string uniquePart = Guid.NewGuid().ToString("N");
            string imageName = prefix + uniquePart +".jpg";


            FindPhone phone1 = new()
            {  
                PhonePhoto = photoBytes,
                Color = phone.Color,
                Brand = phone.Brand,
                Street = phone.Street,
                Government = phone.Government,
                Center = phone.Center,
                FinderEmail = phone.FinderEmail,
                ImageName = imageName,  
                
            };
            try
            {
                con.FindPhones.Add(phone1);
                con.SaveChanges();
            }
            catch(Exception ex) {

                Console.WriteLine("Error: " + ex.Message);
                
            }

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            var form = new MultipartFormDataContent();
            if (photoBytes != null && photoBytes.Length > 0)
            {
                var imageContent = new ByteArrayContent(photoBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                // اسم الحقل "image" لازم يطابق اسم الباراميتر في FastAPI
                form.Add(imageContent, "image", imageName);
            }
            form.Add(new StringContent(phone1.Government), "governorate");
            form.Add(new StringContent(phone1.Center), "city");
            form.Add(new StringContent(phone1.Street), "street");
            form.Add(new StringContent(phone1.FinderEmail), "contact");
            form.Add(new StringContent(phone1.ImageName), "image_name"); 
            form.Add(new StringContent(phone1.Brand), "brand");
            form.Add(new StringContent(phone1.Color), "color");


            var response = await httpClient.PostAsync("http://127.0.0.1:8004/add_found", form);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {responseContent}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send data to AI service. Status: {response.StatusCode}");
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