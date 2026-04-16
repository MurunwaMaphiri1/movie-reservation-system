using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoviesReservationSystem.Models.DTO;
using MoviesReservationSystem.Models.Entities;
using MoviesReservationSystem.Services.AuthService;

namespace MoviesReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] Users user)
        {
            var createdUser = await _authService.RegisterUser(user);
            return Ok(createdUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequestDTO login)
        {
            var token = await _authService.LoginUser(login);
            return Ok(new { token });
        }
    }    
}