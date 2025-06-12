using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;

namespace Lost_and_Found.Interfaces
{
    public interface IAuthService
    {
        public User Register(RegisterDTO reg);
        public Manager AddManager(Manager man);
        public object Login(LoginDTO log);
        public string CreateToken(LoginDTO log);
        public string GetTypeOfUser(string email);
        bool RequestPasswordReset(string email);
        bool ResetPassword(string token, string newPassword);
        Task<bool> ValidateCodeAsync(string email, string code);
        Task DeleteCodeAsync(string email);
        Task SaveCodeAsync(string email, string code);
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<bool> ResetPasswordAsync(User user, string token, string newPassword);
        Task<User> FindByEmailAsync(string email);
    }
}
