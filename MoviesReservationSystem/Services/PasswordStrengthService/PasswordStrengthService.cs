using System.Text.RegularExpressions;

namespace MoviesReservationSystem.Services.PasswordStrengthService
{
    public interface IPasswordStrengthService
    {
        bool CheckPasswordStrength(string password);
    }

    public class PasswordStrengthService : IPasswordStrengthService
    {
        private const string Pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&^#()[\]{}|\\/\-+_.:;=,~`])[^\s<>]{8,}$";

        public bool CheckPasswordStrength(string password)
        {
            if (Regex.IsMatch(password, Pattern))
                return true;

            throw new Exception("Password does not meet requirements. Password must:" +
                                "be at least 8 characters long, " + 
                                "contain at least one lowercase letter (a-z), " +
                                "contain at least one uppercase letter (A-Z), " +
                                "contain at least one number (0-9), " + 
                                "contain at least one special character (@$!%*?&^#()[]{}|\\\\/−+_.:;=,~`), " +
                                "and must not contain spaces, '<' or '>'."
            );
        }
    }
}