using System.ComponentModel.DataAnnotations;

namespace Lost_and_Found.Models.Entites
{
    public class Manager
    {
        [Key]
        public string EmailManager { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string CardID { get; set; }
    }
}
