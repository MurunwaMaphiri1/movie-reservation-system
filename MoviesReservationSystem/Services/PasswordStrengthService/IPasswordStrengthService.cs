namespace MoviesReservationSystem.Services.PasswordStrengthService
{
    public interface IPasswordStrengthService
    {
        bool CheckPasswordStrength(string password);
    }    
}

