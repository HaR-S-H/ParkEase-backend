using AuthService.Models;

namespace AuthService.Repositories
{
    public interface IUserRepository
    {
        Task<User?> FindByEmail(string Email);
        Task<User?> FindByUserId(int UserId);
        Task<bool> ExistsByEmail(string Email);
        Task<List<User>> FindAllByRole(string Role);
        Task<User?> FindByVehiclePlate(string VehiclePlate);
        Task<User?> FindByPhone(string Phone);
        Task<User> Create(User User);
        Task Update(User User);
        Task DeleteByUserId(int UserId);
        
    }
}