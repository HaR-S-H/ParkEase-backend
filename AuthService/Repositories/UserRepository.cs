using AuthService.Models;
using AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;
        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }
        public async Task DeleteByUserId(int UserId)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public Task<bool> ExistsByEmail(string Email)
        {
            return _context.Users.AnyAsync(u => u.Email == Email);
        }

        public Task<List<User>> FindAllByRole(string Role)
        {
            return _context.Users
                .AsNoTracking()
                .Where(u => u.Role == Role)
                .ToListAsync();
        }

        public Task<User?> FindByEmail(string Email)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == Email);
        }

        public Task<User?> FindByPhone(string Phone)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Phone == Phone);
        }

        public async Task<User> Create(User User)
        {
            _context.Users.Add(User);
            await _context.SaveChangesAsync();
            return User;
        }

        public async Task Update(User User)
        {
            _context.Users.Update(User);
            await _context.SaveChangesAsync();
        }

        public Task<User?> FindByUserId(int UserId)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == UserId);
        }

        public Task<User?> FindByVehiclePlate(string VehiclePlate)
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.VehiclePlate == VehiclePlate);
        }
    }
}