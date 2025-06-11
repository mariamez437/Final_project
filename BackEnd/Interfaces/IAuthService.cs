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
    }
}
