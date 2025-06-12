namespace Lost_and_Found.Models.DTO.MobileCheckDto
{
    public class ResetPasswordWithCodeDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
