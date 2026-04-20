namespace AuthService.Helpers.Implementations
{
  public interface IJwtService
    {
        string GenerateToken(int UserId, string Email, string Role);
    }
}