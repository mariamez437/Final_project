namespace Lost_and_Found.Models.DTO
{
    public class LostPhoneDTO
    {
        public IFormFile? PhonePhoto { get; set; }
        public string Color { get; set; }
        public string Brand { get; set; }
        public string Government { get; set; }
        public string Center { get; set; }
        public string Street { get; set; }
        public string ForiegnKey_UserEmail { get; set; }
        public string? ImageName { get; set; }   
    }
}
