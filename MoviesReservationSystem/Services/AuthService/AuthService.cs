using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MoviesReservationSystem.Data;
using MoviesReservationSystem.Models.DTO;
using MoviesReservationSystem.Models.Entities;
using MoviesReservationSystem.Services.PasswordStrengthService;

namespace MoviesReservationSystem.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IPasswordStrengthService  _passwordStrengthService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(IPasswordStrengthService passwordStrengthService, ApplicationDbContext context, 
            IConfiguration configuration)
        {
            _passwordStrengthService = passwordStrengthService;
            _context = context;
            _configuration = configuration;
        }

        public async Task<Users> RegisterUser(Users user)
        {
            _passwordStrengthService.CheckPasswordStrength(user.Password);
            
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            var userEntity = new Users
            {
                FullName = user.FullName,
                Email = user.Email,
                Password = user.Password
            };
            
            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync();
            return userEntity;
        }

        public async Task<string> LoginUser(LoginRequestDTO loginRequestDto)
        {
            var jwtKey = Env.GetString("JWT_SECRET_KEY");
            var existingUser = _context.Users.FirstOrDefault(x => x.Email == loginRequestDto.Email);

            if (existingUser == null)
                throw new UnauthorizedAccessException("Incorrect email or password");
            
            bool isMatch = BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, existingUser.Password);
                
            if (!isMatch)
                throw new UnauthorizedAccessException("Incorrect email or password");

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
            
            return new JwtSecurityTokenHandler().WriteToken(token);
            /*string tokenString = new JwtSecurityTokenHandler().WriteToken(token);*/
            /*return Ok(new { token = tokenString , user = new { userId = existingUser.Id, existingUser.FullName, 
                existingUser.Email }});*/
        }
    }    
}

