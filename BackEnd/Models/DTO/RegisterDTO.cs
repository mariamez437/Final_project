namespace Lost_and_Found.Models.DTO
{
    public class RegisterDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CardID { get; set; }
        public string PhoneNumber { get; set; }
        public IFormFile? PhonePhoto { get; set; }
        public IFormFile? CardPhoto { get; set; }
    }
}
