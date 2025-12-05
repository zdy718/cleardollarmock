using BudgetAppCSCE361.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Server.Accessors
{
    public class UserAccessor : IUserAccessor
    {
        private readonly AppDbContext _context;

        public UserAccessor(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser?> GetByIdAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Transactions)
                .Include(u => u.Tags)
                .Include(u => u.Budgets)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Transactions)
                .Include(u => u.Tags)
                .Include(u => u.Budgets)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<AppUser> CreateAsync(AppUser user)
        {
            // Ensure UserId is set
            if (user.UserId == Guid.Empty)
            {
                user.UserId = Guid.NewGuid();
            }

            // Set CreatedAt if not already set
            if (user.CreatedAt == default)
            {
                user.CreatedAt = DateTime.UtcNow;
            }

            // Validate email is unique
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email);
            
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return user;
        }

        public async Task<AppUser> UpdateAsync(AppUser user)
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);
            
            if (existing == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Check if email is being changed and if it's already taken
            if (existing.Email != user.Email)
            {
                var emailTaken = await _context.Users
                    .AnyAsync(u => u.Email == user.Email && u.UserId != user.UserId);
                
                if (emailTaken)
                {
                    throw new InvalidOperationException("Email is already in use by another user");
                }
            }

            existing.Email = user.Email;
            // Note: CreatedAt should not be updated

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (user == null)
            {
                return false;
            }

            // Due to cascade delete, this will also delete:
            // - All transactions
            // - All tags (and their transaction tags)
            // - All budgets
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Additional helper methods
        public async Task<bool> ExistsAsync(Guid userId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<List<AppUser>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _context.Users.CountAsync();
        }
    }
}