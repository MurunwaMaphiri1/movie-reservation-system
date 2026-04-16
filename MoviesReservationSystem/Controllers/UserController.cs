using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesReservationSystem.Data;
using MoviesReservationSystem.Models.DTO;
using MoviesReservationSystem.Models.Entities;
using MoviesReservationSystem.Services.PasswordStrengthService;

namespace MoviesReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController: ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IPasswordStrengthService _passwordStrengthService;

        public UserController(IConfiguration configuration, ApplicationDbContext context,
            IPasswordStrengthService passwordStrengthService)
        {
            _configuration = configuration;
            _context = context;
            _passwordStrengthService = passwordStrengthService;
            Env.Load();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var allUsers = _context.Users.ToList();
            return Ok(allUsers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return Ok(user);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetUserByFullName([FromQuery] string FullName)
        {
            var user = await _context.Users
                .Where(u => u.FullName.Contains(FullName))
                .ToListAsync();
            
            if (!user.Any()) return new NotFoundResult();
            
            return Ok(user);
        }

        [HttpPatch("change-user-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleDTO changeUserRoleDTO)
        {
            var userToUpdate = await _context.Users.FindAsync(changeUserRoleDTO.Id);
            if (userToUpdate == null) return new NotFoundResult();
            userToUpdate.Role = changeUserRoleDTO.Role;
            await _context.SaveChangesAsync();
            return Ok(userToUpdate);
        }

        /*
        [HttpPost("register")]
        public async Task<IActionResult> registerUser([FromBody] Users users)
        {
            try
            {
                _passwordStrengthService.CheckPasswordStrength(users.Password);
            
                users.Password = BCrypt.Net.BCrypt.HashPassword(users.Password);

                var userEntity = new Users()
                {
                    FullName = users.FullName,
                    Email = users.Email,
                    Password = users.Password
                };
            
                _context.Users.Add(userEntity);
                await _context.SaveChangesAsync();
                return Ok(userEntity);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> loginUser([FromBody] LoginRequestDTO loginRequestDto)
        {
            var jwtKey = Env.GetString("JWT_SECRET_KEY");
            var existingUser = _context.Users.FirstOrDefault(x => x.Email == loginRequestDto.Email);

            if (existingUser == null)
            {
                return Unauthorized(new { message = "Incorrect email or password" });}
            
            bool isMatch = BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, existingUser.Password);
                
            if (!isMatch)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id", existingUser.Id.ToString()),
                new Claim("Name", existingUser.FullName),
                new Claim("Email", existingUser.Email),
                new Claim("Role", existingUser.Role)
            };
                
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["JwtConfig:Issuer"],
                _configuration["JwtConfig:Audience"],
                claims,
                signingCredentials: creds
            );
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { token = tokenString , user = new { userId = existingUser.Id, existingUser.FullName, existingUser.Email }});
        }*/
    }
}
