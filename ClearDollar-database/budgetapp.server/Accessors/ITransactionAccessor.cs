using System.Collections.Generic;
using BudgetAppCSCE361.Data;

namespace BudgetApp.Server.Accessors
{
    public interface ITransactionAccessor
    {
        Task<List<Transaction>> GetAllAsync(Guid userId);
        Task<Transaction?> GetByIdAsync(Guid userId, long transactionId);
        Task<List<Transaction>> GetByDateRangeAsync(Guid userId, DateTime start, DateTime end);
        Task<List<Transaction>> GetByTagAsync(Guid userId, long tagId);
        Task<List<Transaction>> GetUntaggedAsync(Guid userId);
        Task<Transaction> AddAsync(Guid userId, Transaction transaction);
        Task<List<Transaction>> AddBulkAsync(Guid userId, List<Transaction> transactions);
        Task<Transaction> UpdateAsync(Guid userId, Transaction transaction);
        Task<bool> DeleteAsync(Guid userId, long transactionId);
    }
}