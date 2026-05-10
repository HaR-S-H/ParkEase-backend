namespace AuthService.Helpers.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPassword(string Password);
        bool VerifyPassword(string Password, string HashedPassword);
    }
}