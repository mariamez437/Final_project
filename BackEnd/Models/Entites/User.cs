namespace Lost_and_Found.Models.Entites
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public string CardID { get; set; }
        public string PhoneNumber { get; set; }
        public byte[]? PhonePhoto { get; set; }
        public byte[]? CardPhoto { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }
        public List<LostPhone> LostPhones { get; set; }        // One To Many relation
        public List<LostCard> LostCards { get; set; }           // One To Many relation
    }
}
