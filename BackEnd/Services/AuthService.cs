using Lost_and_Found.Interfaces;
using Lost_and_Found.Models;
using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lost_and_Found.Services
{
    public class AuthService : IAuthService
    {
        private readonly DataConnection con;
        private readonly IConfiguration config;
        public AuthService(DataConnection con, IConfiguration config)
        {
            this.con = con;
            this.config = config;
        }

        public string GetTypeOfUser(string email)
        {
            if (con.Users.Any(o => o.Email == email))
            {
                return "User";
            }
            else if (con.Managers.Any(o => o.EmailManager == email))
            {
                return "Manager";
            }

            return "Invalid";
        }

        public string CreateToken(LoginDTO log)
        {
            string role = GetTypeOfUser(log.Email);
            var Claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, log.Email),
                new Claim(ClaimTypes.Role, role),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokendescription = new JwtSecurityToken(
                    issuer: config.GetValue<string>("AppSettings:Issuer"),
                    audience: config.GetValue<string>("AppSettings:Audience"),
                    claims: Claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                    );

            
            return new JwtSecurityTokenHandler().WriteToken(tokendescription);
        }

        public User Register(RegisterDTO request)
        {
            var ok = con.Users.FirstOrDefault(o => o.Email == request.Email);

            if (ok != null || con.Managers.FirstOrDefault(o => o.EmailManager == request.Email) != null
                || con.Managers.FirstOrDefault(o => o.PhoneNumber == request.PhoneNumber) != null
                || con.Managers.FirstOrDefault(o => o.CardID == request.CardID) != null
                || con.Users.Any(o => o.CardID == request.CardID) == true
                || con.Users.Any(o => o.PhoneNumber == request.PhoneNumber) == true)
                return null;

            var user = new User();

            var hashedpass = new PasswordHasher<User>()
                                    .HashPassword(user, request.Password);

            user.Email = request.Email;
            user.HashedPassword = hashedpass;
            user.UserName = request.UserName;
            user.PhoneNumber = request.PhoneNumber;
            user.CardID = request.CardID;

            con.Users.Add(user);
            con.SaveChanges();

            return user;
        }

        public object Login(LoginDTO request)
        {

            if (GetTypeOfUser(request.Email) == "Invalid")
                return null;

            var user = con.Users.FirstOrDefault(o => o.Email == request.Email);
            if (user != null && new PasswordHasher<User>()
             .VerifyHashedPassword(user, user.HashedPassword, request.Password) == PasswordVerificationResult.Failed)
                return null;

            return new { Token = CreateToken(request), Type = GetTypeOfUser(request.Email) };
        }

        public Manager AddManager(Manager man)
        {
            if (con.Managers.FirstOrDefault(o => o.EmailManager == man.EmailManager) != null
                || con.Managers.FirstOrDefault(o => o.CardID == man.CardID) != null
                || con.Managers.FirstOrDefault(o => o.PhoneNumber == man.PhoneNumber) != null
                || con.Users.FirstOrDefault(o => o.Email == man.EmailManager) != null
                || con.Users.FirstOrDefault(o => o.PhoneNumber == man.PhoneNumber) != null
                || con.Users.FirstOrDefault(o => o.CardID == man.CardID) != null)
                return null;

            con.Managers.Add(man);
            con.SaveChanges();

            return man;
        }
    }
}
