using System.Collections.Generic;
using BudgetAppCSCE361.Data;

namespace BudgetApp.Server.Accessors
{
    public interface IUserAccessor
    {
        Task<AppUser?> GetByIdAsync(Guid userId);
        Task<AppUser?> GetByEmailAsync(string email);
        Task<AppUser> CreateAsync(AppUser user);
        Task<AppUser> UpdateAsync(AppUser user);
        Task<bool> DeleteAsync(Guid userId);
    }
}