using System.Collections.Generic;
using BudgetAppCSCE361.Data;

namespace BudgetApp.Server.Accessors
{
    public interface IBudgetAccessor
    {
        Task<List<Budget>> GetAllAsync(Guid userId);
        Task<Budget?> GetByIdAsync(Guid userId, long budgetId);
        Task<Budget> AddAsync(Guid userId, Budget budget);
        Task<Budget> UpdateAsync(Guid userId, Budget budget);
        Task<bool> DeleteAsync(Guid userId, long budgetId);
    }
}