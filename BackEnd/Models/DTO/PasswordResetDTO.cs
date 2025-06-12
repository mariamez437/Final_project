namespace Lost_and_Found.Models.DTO
{
    public class PasswordResetDTO
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
