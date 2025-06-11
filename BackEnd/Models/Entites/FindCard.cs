namespace Lost_and_Found.Models.Entites
{
    public class FindCard
    {
        public int Id { get; set; }
        public string CardID { get; set; }
        public byte[]? CardPhoto { get; set; }
        public string ImageName { get; set; }
        public string Government { get; set; }
        public string Center { get; set; }
        public string Street { get; set; }
        public string FinderEmail { get; set; }
    }
}