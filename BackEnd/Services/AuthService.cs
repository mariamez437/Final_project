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
using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.Extensions.Options;



namespace Lost_and_Found.Services
{

    public class AuthService : IAuthService
    {
        private readonly EmailSettings _emailSettings;

        private readonly DataConnection con;
        private readonly IConfiguration config;
        public AuthService(DataConnection con, IConfiguration config, IOptions<EmailSettings> options)
        {
            this.con = con;
            this.config = config;
            _emailSettings = options.Value;

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

            // هاش كلمة السر بـ BCrypt
            user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            user.Email = request.Email;
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

            if (user == null)
                return null;

            // التحقق من كلمة السر باستخدام BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword);

            if (!isPasswordValid)
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

        public bool RequestPasswordReset(string email)
        {
            var user = con.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return false;

            // توليد توكن مؤقت (مثلاً GUID)
            string resetToken = Guid.NewGuid().ToString();

            // خزنه مع اليوزر (لو مش مخزن، أضف حقل في جدول المستخدمين أو جدول خاص)
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            con.SaveChanges();

             //هنا ترسل إيميل فيه الرابط: https://yourfrontend.com/reset-password?token=resetToken
            SendResetEmail(user.Email, resetToken);

            return true;
        }

        public bool ResetPassword(string token, string newPassword)
        {
            var user = con.Users.FirstOrDefault(u => u.ResetPasswordToken == token
                                                     && u.ResetPasswordExpiry > DateTime.Now);
            if (user == null) return false;

            var hashedpass = new PasswordHasher<User>().HashPassword(user, newPassword);
            user.HashedPassword = hashedpass;

            // امسح التوكن بعد نجاح العملية
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;

            con.SaveChanges();

            return true;
        }
        public void SendResetEmail(string toEmail, string token)
        {
            var fromEmail = config["EmailSettings:Email"];
            var fromPassword = config["EmailSettings:Password"];
            var resetLink = $"https://localhost:5194/htmlStaticFiles/RegisterationPages/ResetPassword.htmlResetPassword.html?token={token}"; // غير اللينك حسب موقعك

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Reset Your Password",
                Body = $"Click the link to reset your password: {resetLink}",
                IsBodyHtml = true
            };

            smtpClient.Send(message);
        }

        public async Task SaveCodeAsync(string email, string code)
        {
            // امسح الأكواد القديمة الغير مستخدمة للإيميل ده (اختياري، للتنضيف)
            var oldCodes = con.PasswordResetCodes
                .Where(p => p.Email == email && !p.IsUsed);
            con.PasswordResetCodes.RemoveRange(oldCodes);

            var resetCode = new PasswordResetCode
            {
                Email = email,
                Code = code,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await con.PasswordResetCodes.AddAsync(resetCode);
            await con.SaveChangesAsync();
        }


        public async Task<bool> ValidateCodeAsync(string email, string code)
        {
            var match = await con.PasswordResetCodes
                .Where(p => p.Email == email &&
                            p.Code == code &&
                            !p.IsUsed &&
                            p.ExpirationTime > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (match == null)
                return false;

            match.IsUsed = true; // علم الكود إنه اتستخدم
            await con.SaveChangesAsync();
            return true;
        }

        public async Task DeleteCodeAsync(string email)
        {
            var codes = await con.PasswordResetCodes
                .Where(p => p.Email == email)
                .ToListAsync();

            con.PasswordResetCodes.RemoveRange(codes);
            await con.SaveChangesAsync();
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email is required.", nameof(toEmail));

            var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
            {
                Port = _emailSettings.SmtpPort,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = string.IsNullOrWhiteSpace(subject) ? "(No Subject)" : subject,
                Body = string.IsNullOrWhiteSpace(body) ? "(No Body)" : body,
                IsBodyHtml = false
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            var code = new Random().Next(100000, 999999).ToString();

            var resetCode = new PasswordResetCode
            {
                Email = user.Email,
                Code = code,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10)
            };

            con.PasswordResetCodes.Add(resetCode);
            await con.SaveChangesAsync();

            return code;
        }
        public async Task<bool> ResetPasswordAsync(User user, string token, string newPassword)
        {
         

            user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword); // لو بتستخدم hashing
            con.Users.Update(user);
            await con.SaveChangesAsync();


            return true;
        }
        public async Task<User> FindByEmailAsync(string email)
        {
            return await con.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
