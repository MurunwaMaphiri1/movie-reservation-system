using MoviesReservationSystem.Models.DTO;
using MoviesReservationSystem.Models.Entities;

namespace MoviesReservationSystem.Services.AuthService
{
    public interface IAuthService
    {
        Task<Users> RegisterUser(Users user);
        Task<string> LoginUser(LoginRequestDTO loginRequestDto);
    }    
}

