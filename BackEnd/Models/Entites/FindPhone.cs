namespace Lost_and_Found.Models.Entites
{
    public class FindPhone
    {
        public int Id { get; set; }

        public string PhoneNumber { get; set; }
        public string ImageName { get; set; }


        public string Color { get; set; }
        public string Brand { get; set; }
        public byte[]? PhonePhoto { get; set; }
        public string Government { get; set; }
        public string Center { get; set; }
        public string Street { get; set; }
        public string FinderEmail { get; set; }
    }
}
