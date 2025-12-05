using BudgetAppCSCE361.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Server.Accessors
{
    public class TransactionAccessor : ITransactionAccessor
    {
        private readonly AppDbContext _context;

        public TransactionAccessor(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetAllAsync(Guid userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.TransactionTags)
                    .ThenInclude(tt => tt.Tag)
                .OrderByDescending(t => t.PostedAt)
                .ToListAsync();
        }

        public async Task<Transaction?> GetByIdAsync(Guid userId, long transactionId)
        {
            return await _context.Transactions
                .Include(t => t.TransactionTags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == transactionId);
        }

        public async Task<List<Transaction>> GetByDateRangeAsync(Guid userId, DateTime start, DateTime end)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId && t.PostedAt >= start && t.PostedAt <= end)
                .Include(t => t.TransactionTags)
                    .ThenInclude(tt => tt.Tag)
                .OrderByDescending(t => t.PostedAt)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetByTagAsync(Guid userId, long tagId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId && 
                       t.TransactionTags.Any(tt => tt.TagId == tagId))
                .Include(t => t.TransactionTags)
                    .ThenInclude(tt => tt.Tag)
                .OrderByDescending(t => t.PostedAt)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetUntaggedAsync(Guid userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId && !t.TransactionTags.Any())
                .OrderByDescending(t => t.PostedAt)
                .ToListAsync();
        }

        public async Task<Transaction> AddAsync(Guid userId, Transaction transaction)
        {
            transaction.UserId = userId;
            
            // Validate tags if any are provided
            if (transaction.TransactionTags != null && transaction.TransactionTags.Any())
            {
                var tagIds = transaction.TransactionTags.Select(tt => tt.TagId).Distinct().ToList();
                var validTags = await _context.Tags
                    .Where(t => t.UserId == userId && tagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                if (validTags.Count != tagIds.Count)
                {
                    throw new ArgumentException("One or more tags do not exist or do not belong to user");
                }

                // Clear and rebuild transaction tags to avoid duplicates
                var tagList = transaction.TransactionTags.Select(tt => tt.TagId).Distinct().ToList();
                transaction.TransactionTags.Clear();
                
                foreach (var tagId in tagList)
                {
                    transaction.TransactionTags.Add(new TransactionTag
                    {
                        TagId = tagId
                    });
                }
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            
            // Reload with tags
            return (await GetByIdAsync(userId, transaction.Id))!;
        }

        public async Task<List<Transaction>> AddBulkAsync(Guid userId, List<Transaction> transactions)
        {
            if (transactions == null || !transactions.Any())
            {
                return new List<Transaction>();
            }

            // Set userId for all transactions
            foreach (var transaction in transactions)
            {
                transaction.UserId = userId;
            }

            // Validate all tags at once
            var allTagIds = transactions
                .Where(t => t.TransactionTags != null)
                .SelectMany(t => t.TransactionTags.Select(tt => tt.TagId))
                .Distinct()
                .ToList();

            if (allTagIds.Any())
            {
                var validTags = await _context.Tags
                    .Where(t => t.UserId == userId && allTagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                // Validate and clean up tags
                foreach (var transaction in transactions)
                {
                    if (transaction.TransactionTags != null && transaction.TransactionTags.Any())
                    {
                        var validTransactionTags = transaction.TransactionTags
                            .Where(tt => validTags.Contains(tt.TagId))
                            .Select(tt => tt.TagId)
                            .Distinct()
                            .ToList();

                        if (validTransactionTags.Count != transaction.TransactionTags.Select(tt => tt.TagId).Distinct().Count())
                        {
                            throw new ArgumentException($"Transaction contains invalid tags that don't belong to user");
                        }

                        // Rebuild tags without duplicates
                        transaction.TransactionTags.Clear();
                        foreach (var tagId in validTransactionTags)
                        {
                            transaction.TransactionTags.Add(new TransactionTag { TagId = tagId });
                        }
                    }
                }
            }

            _context.Transactions.AddRange(transactions);
            await _context.SaveChangesAsync();
            
            return transactions;
        }

        public async Task<Transaction> UpdateAsync(Guid userId, Transaction transaction)
        {
            var existing = await GetByIdAsync(userId, transaction.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException("Transaction not found");
            }

            existing.PostedAt = transaction.PostedAt;
            existing.Description = transaction.Description;
            existing.AmountCents = transaction.AmountCents;

            // Update tags if provided
            if (transaction.TransactionTags != null)
            {
                // Validate new tags
                var tagIds = transaction.TransactionTags.Select(tt => tt.TagId).Distinct().ToList();
                if (tagIds.Any())
                {
                    var validTags = await _context.Tags
                        .Where(t => t.UserId == userId && tagIds.Contains(t.Id))
                        .Select(t => t.Id)
                        .ToListAsync();

                    if (validTags.Count != tagIds.Count)
                    {
                        throw new ArgumentException("One or more tags do not exist or do not belong to user");
                    }
                }

                // Remove old tags
                _context.TransactionTags.RemoveRange(existing.TransactionTags);

                // Add new tags (without duplicates)
                foreach (var tagId in tagIds)
                {
                    existing.TransactionTags.Add(new TransactionTag
                    {
                        TransactionId = existing.Id,
                        TagId = tagId
                    });
                }
            }

            await _context.SaveChangesAsync();
            
            // Reload with tags
            return (await GetByIdAsync(userId, existing.Id))!;
        }

        public async Task<bool> DeleteAsync(Guid userId, long transactionId)
        {
            var transaction = await GetByIdAsync(userId, transactionId);
            if (transaction == null)
            {
                return false;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        // Additional helper methods
        public async Task<bool> AddTagToTransactionAsync(Guid userId, long transactionId, long tagId)
        {
            var transaction = await GetByIdAsync(userId, transactionId);
            if (transaction == null)
            {
                return false;
            }

            var tagExists = await _context.Tags
                .AnyAsync(t => t.UserId == userId && t.Id == tagId);
            
            if (!tagExists)
            {
                return false;
            }

            // Check if tag is already added
            var alreadyTagged = transaction.TransactionTags.Any(tt => tt.TagId == tagId);
            if (alreadyTagged)
            {
                return true; // Already tagged, consider it success
            }

            transaction.TransactionTags.Add(new TransactionTag
            {
                TransactionId = transactionId,
                TagId = tagId
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTagFromTransactionAsync(Guid userId, long transactionId, long tagId)
        {
            var transaction = await GetByIdAsync(userId, transactionId);
            if (transaction == null)
            {
                return false;
            }

            var transactionTag = transaction.TransactionTags
                .FirstOrDefault(tt => tt.TagId == tagId);
            
            if (transactionTag == null)
            {
                return false;
            }

            _context.TransactionTags.Remove(transactionTag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<long, int>> GetTagSummaryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Transactions
                .Where(t => t.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(t => t.PostedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.PostedAt <= endDate.Value);
            }

            var summary = await query
                .SelectMany(t => t.TransactionTags)
                .GroupBy(tt => tt.TagId)
                .Select(g => new { TagId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TagId, x => x.Count);

            return summary;
        }

        public async Task<int> GetTotalSpentByTagAsync(Guid userId, long tagId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Transactions
                .Where(t => t.UserId == userId && t.TransactionTags.Any(tt => tt.TagId == tagId));

            if (startDate.HasValue)
            {
                query = query.Where(t => t.PostedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.PostedAt <= endDate.Value);
            }

            return await query.SumAsync(t => t.AmountCents);
        }
    }
}