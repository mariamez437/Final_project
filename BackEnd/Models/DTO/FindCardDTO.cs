namespace Lost_and_Found.Models.DTO
{
    public class FindCardDTO
    {
        public string CardID { get; set; }
        public IFormFile? CardPhoto { get; set; }
        public string Government { get; set; }
        public string Center { get; set; }
        public string Street { get; set; }
        public string FinderEmail { get; set; }
    }
}
