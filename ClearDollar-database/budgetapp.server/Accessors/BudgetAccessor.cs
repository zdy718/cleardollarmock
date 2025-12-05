using BudgetAppCSCE361.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Server.Accessors
{
    public class BudgetAccessor : IBudgetAccessor
    {
        private readonly AppDbContext _context;

        public BudgetAccessor(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Budget>> GetAllAsync(Guid userId)
        {
            return await _context.Budgets
                .Where(b => b.UserId == userId)
                .Include(b => b.Tag)
                .OrderByDescending(b => b.Month)
                .ToListAsync();
        }

        public async Task<Budget?> GetByIdAsync(Guid userId, long budgetId)
        {
            return await _context.Budgets
                .Include(b => b.Tag)
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Id == budgetId);
        }

        public async Task<Budget> AddAsync(Guid userId, Budget budget)
        {
            budget.UserId = userId;
            
            // Validate that the tag belongs to the user
            var tagExists = await _context.Tags
                .AnyAsync(t => t.UserId == userId && t.Id == budget.TagId);
            
            if (!tagExists)
            {
                throw new ArgumentException("Tag does not exist or does not belong to user");
            }

            // Check if budget already exists for this tag and month
            var existingBudget = await _context.Budgets
                .FirstOrDefaultAsync(b => 
                    b.UserId == userId && 
                    b.TagId == budget.TagId && 
                    b.Month == budget.Month);

            if (existingBudget != null)
            {
                throw new InvalidOperationException("Budget already exists for this tag and month");
            }

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return budget;
        }

        public async Task<Budget> UpdateAsync(Guid userId, Budget budget)
        {
            var existing = await GetByIdAsync(userId, budget.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException("Budget not found");
            }

            // Validate new tag if it's being changed
            if (existing.TagId != budget.TagId)
            {
                var tagExists = await _context.Tags
                    .AnyAsync(t => t.UserId == userId && t.Id == budget.TagId);
                
                if (!tagExists)
                {
                    throw new ArgumentException("Tag does not exist or does not belong to user");
                }
            }

            existing.TagId = budget.TagId;
            existing.Month = budget.Month;
            existing.LimitCents = budget.LimitCents;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid userId, long budgetId)
        {
            var budget = await GetByIdAsync(userId, budgetId);
            if (budget == null)
            {
                return false;
            }

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return true;
        }

        // Additional helper methods
        public async Task<Budget?> GetByTagAndMonthAsync(Guid userId, long tagId, DateTime month)
        {
            // Normalize to first day of month
            var monthStart = new DateTime(month.Year, month.Month, 1);
            
            return await _context.Budgets
                .Include(b => b.Tag)
                .FirstOrDefaultAsync(b => 
                    b.UserId == userId && 
                    b.TagId == tagId && 
                    b.Month == monthStart);
        }

        public async Task<List<Budget>> GetByMonthAsync(Guid userId, DateTime month)
        {
            var monthStart = new DateTime(month.Year, month.Month, 1);
            
            return await _context.Budgets
                .Where(b => b.UserId == userId && b.Month == monthStart)
                .Include(b => b.Tag)
                .ToListAsync();
        }
    }
}