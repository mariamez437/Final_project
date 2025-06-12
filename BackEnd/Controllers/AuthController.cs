using AutoMapper;
using Lost_and_Found.Interfaces;
using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.DTO.MobileCheckDto;
using Lost_and_Found.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Lost_and_Found.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuthService authserver;
        private IMapper mp;
        public AuthController(IAuthService authService,IMapper mp)
        {
            this.authserver = authService;
            this.mp = mp;
        }

        [HttpPost("Register")]
        public IActionResult Register([FromForm]RegisterDTO request)
        {
            var res = authserver.Register(request);
            if (res == null)
                return BadRequest("the person is already exists");

            return Ok($"Added user with email = {request.Email}");
        }

        [HttpPost("Login")]
        public IActionResult Login([FromForm]LoginDTO log)
        {
            var res = authserver.Login(log);

            if (res is null)
                return BadRequest("Not Found Person");

            return Ok(res);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("AddManager")]
        public IActionResult AddManager([FromForm]Manager man)
        {
            var res = authserver.AddManager(man);

            if (res is null)
                return BadRequest("Manager is exists");

            return Ok(res);
        }
     

        [HttpPost("request-reset-code")]
        public async Task<IActionResult> RequestResetCode([FromBody] PasswordResetRequestDTO dto)
        {
            var user = await authserver.FindByEmailAsync(dto.Email);
            if (user == null)
                return Ok("If this email exists, a code will be sent.");

            var code = new Random().Next(100000, 999999).ToString();

            await authserver.SaveCodeAsync(dto.Email, code);
            await authserver.SendEmailAsync(dto.Email, "Password Reset Code", $"Your reset code is: {code}");

            return Ok("A reset code has been sent to your email.");
        }

        [HttpPost("reset-password-with-code")]
        public async Task<IActionResult> ResetPasswordWithCode([FromBody] ResetPasswordWithCodeDto dto)
        {
            var isValid = await authserver.ValidateCodeAsync(dto.Email, dto.Code);
            if (!isValid)
                return BadRequest("Invalid or expired code.");

            var user = await authserver.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest("User not found.");

            var token = await authserver.GeneratePasswordResetTokenAsync(user);
            var result = await authserver.ResetPasswordAsync(user, token, dto.NewPassword);

     

            await authserver.DeleteCodeAsync(dto.Email);

            return Ok("Password has been reset successfully.");
        }



    }
}
