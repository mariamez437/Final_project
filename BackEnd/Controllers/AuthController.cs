using AutoMapper;
using Lost_and_Found.Interfaces;
using Lost_and_Found.Models.DTO;
using Lost_and_Found.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
