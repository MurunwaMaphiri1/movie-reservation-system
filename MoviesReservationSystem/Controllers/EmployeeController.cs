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

namespace MoviesReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController: ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public EmployeeController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
            Env.Load();
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var allEmployees = _context.Employees.ToList();
            return Ok(allEmployees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            return Ok(employee);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetEmployeesByFullName(string FullName)
        {
            var employee = await _context.Employees
                .Where(e => e.FullName.Contains(FullName))
                .ToListAsync();
            return Ok(employee);
        }

        [HttpPatch("change-employee-role")]
        public async Task<IActionResult> ChangeEmployeeRole([FromBody] ChangeEmployeeRoleDTO changeEmployeeRoleDto)
        {
            var empToUpdate = await _context.Employees.FindAsync(changeEmployeeRoleDto.EmployeeId);
            if (empToUpdate == null) return NotFound();
            empToUpdate.Role = changeEmployeeRoleDto.Role;
            await _context.SaveChangesAsync();
            return Ok(empToUpdate);
        }

        [HttpPost("add-employee")]
        public async Task<IActionResult> AddEmployee([FromBody] Employees newEmployee)
        {
            newEmployee.Password = BCrypt.Net.BCrypt.HashPassword(newEmployee.Password);

            var empEntity = new Employees()
            {
                FullName = newEmployee.FullName,
                Email = newEmployee.Email,
                Password = newEmployee.Password,
                PhoneNumber = newEmployee.PhoneNumber,
                Role = newEmployee.Role
            };
            
            await _context.Employees.AddAsync(empEntity);
            await _context.SaveChangesAsync();
            return Ok(empEntity);
        }

        [HttpDelete("delete-employee/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var empToDelete = await _context.Employees.FindAsync(id);
            if (empToDelete == null) return NotFound();
            _context.Employees.Remove(empToDelete);
            await _context.SaveChangesAsync();
            return Ok(empToDelete);
        }

        [HttpPost("login")]
        public async Task<IActionResult> empLogin([FromBody] LoginRequestDTO loginRequestDto)
        {
            var jwtKey = Env.GetString("JWT_SECRET_KEY");
            var existingEmp = _context.Employees
                .FirstOrDefault(e => e.Email == loginRequestDto.Email);

            if (existingEmp == null)
            {
                return Unauthorized(new { message = "Incorrect email or password" });
            }

            bool isMatch = BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, existingEmp.Password);

            if (!isMatch)
            {
                return Unauthorized(new { message = "Incorrect password" });
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id", existingEmp.EmployeeId.ToString()),
                new Claim("Name", existingEmp.FullName),
                new Claim("Email", existingEmp.Email),
                new Claim("Role", existingEmp.Role)
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
            return Ok(new { token = tokenString, employee = new { existingEmp.EmployeeId,
                existingEmp.FullName, existingEmp.Email, existingEmp.Role } });
        }
    }
}