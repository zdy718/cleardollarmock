using BudgetAppCSCE361.Data; 
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Server.Accessors
{
    public class TagAccessor : ITagAccessor
    {
        private readonly AppDbContext _context;

        public TagAccessor(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tag>> GetAllAsync(Guid userId)
        {
            return await _context.Tags
                .Where(t => t.UserId == userId)
                .Include(t => t.Parent)
                .Include(t => t.Children)
                .OrderBy(t => t.Level)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag?> GetByIdAsync(Guid userId, long tagId)
        {
            return await _context.Tags
                .Include(t => t.Parent)
                .Include(t => t.Children)
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == tagId);
        }

        public async Task<List<Tag>> GetByLevelAsync(Guid userId, int level)
        {
            if (level < 1 || level > 3)
            {
                throw new ArgumentException("Level must be between 1 and 3");
            }

            return await _context.Tags
                .Where(t => t.UserId == userId && t.Level == level)
                .Include(t => t.Parent)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<Tag>> GetChildrenAsync(Guid userId, long parentId)
        {
            // Verify parent exists and belongs to user
            var parentExists = await _context.Tags
                .AnyAsync(t => t.UserId == userId && t.Id == parentId);

            if (!parentExists)
            {
                throw new ArgumentException("Parent tag does not exist or does not belong to user");
            }

            return await _context.Tags
                .Where(t => t.UserId == userId && t.ParentId == parentId)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag> AddAsync(Guid userId, Tag tag)
        {
            tag.UserId = userId;

            // Validate level
            if (tag.Level < 1 || tag.Level > 3)
            {
                throw new ArgumentException("Level must be between 1 and 3");
            }

            // Validate parent if specified
            if (tag.ParentId.HasValue)
            {
                var parent = await _context.Tags
                    .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == tag.ParentId.Value);

                if (parent == null)
                {
                    throw new ArgumentException("Parent tag does not exist or does not belong to user");
                }

                // Validate hierarchy levels (optional business rule)
                // Level 2 tags should have Level 1 parents, Level 3 should have Level 2 parents
                if (tag.Level != parent.Level + 1)
                {
                    throw new ArgumentException($"Invalid hierarchy: Level {tag.Level} tag cannot have Level {parent.Level} parent");
                }
            }
            else if (tag.Level != 1)
            {
                throw new ArgumentException("Only Level 1 tags can have no parent");
            }

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            
            // Reload with relationships
            return (await GetByIdAsync(userId, tag.Id))!;
        }

        public async Task<Tag> UpdateAsync(Guid userId, Tag tag)
        {
            var existing = await GetByIdAsync(userId, tag.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException("Tag not found");
            }

            // Validate level
            if (tag.Level < 1 || tag.Level > 3)
            {
                throw new ArgumentException("Level must be between 1 and 3");
            }

            // Validate parent if being changed
            if (tag.ParentId != existing.ParentId)
            {
                if (tag.ParentId.HasValue)
                {
                    // Check for circular reference
                    if (tag.ParentId.Value == tag.Id)
                    {
                        throw new ArgumentException("Tag cannot be its own parent");
                    }

                    var parent = await _context.Tags
                        .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == tag.ParentId.Value);

                    if (parent == null)
                    {
                        throw new ArgumentException("Parent tag does not exist or does not belong to user");
                    }

                    // Check if new parent is a descendant of this tag
                    if (await IsDescendantAsync(tag.Id, tag.ParentId.Value))
                    {
                        throw new ArgumentException("Cannot set a descendant as parent (would create circular reference)");
                    }

                    // Validate hierarchy levels
                    if (tag.Level != parent.Level + 1)
                    {
                        throw new ArgumentException($"Invalid hierarchy: Level {tag.Level} tag cannot have Level {parent.Level} parent");
                    }
                }
                else if (tag.Level != 1)
                {
                    throw new ArgumentException("Only Level 1 tags can have no parent");
                }
            }

            existing.Name = tag.Name;
            existing.Level = tag.Level;
            existing.ParentId = tag.ParentId;

            await _context.SaveChangesAsync();
            
            // Reload with relationships
            return (await GetByIdAsync(userId, existing.Id))!;
        }

        public async Task<bool> DeleteAsync(Guid userId, long tagId)
        {
            var tag = await GetByIdAsync(userId, tagId);
            if (tag == null)
            {
                return false;
            }

            // Check if tag has children
            var hasChildren = await _context.Tags
                .AnyAsync(t => t.ParentId == tagId);

            if (hasChildren)
            {
                throw new InvalidOperationException("Cannot delete tag with children. Delete or reassign children first.");
            }

            // Check if tag is used in budgets
            var usedInBudgets = await _context.Budgets
                .AnyAsync(b => b.TagId == tagId);

            if (usedInBudgets)
            {
                throw new InvalidOperationException("Cannot delete tag that is used in budgets.");
            }

            // Note: TransactionTags will be cascade deleted
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }

        // Helper method to check for circular references
        private async Task<bool> IsDescendantAsync(long ancestorId, long descendantId)
        {
            var current = await _context.Tags.FindAsync(descendantId);
            
            while (current != null && current.ParentId.HasValue)
            {
                if (current.ParentId.Value == ancestorId)
                {
                    return true;
                }
                current = await _context.Tags.FindAsync(current.ParentId.Value);
            }
            
            return false;
        }

        // Additional helper methods
        public async Task<List<Tag>> GetRootTagsAsync(Guid userId)
        {
            return await _context.Tags
                .Where(t => t.UserId == userId && t.ParentId == null)
                .Include(t => t.Children)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<Tag>> GetTagHierarchyAsync(Guid userId, long rootTagId)
        {
            var result = new List<Tag>();
            var root = await GetByIdAsync(userId, rootTagId);
            
            if (root != null)
            {
                result.Add(root);
                await LoadChildrenRecursiveAsync(userId, rootTagId, result);
            }
            
            return result;
        }

        private async Task LoadChildrenRecursiveAsync(Guid userId, long parentId, List<Tag> result)
        {
            var children = await GetChildrenAsync(userId, parentId);
            result.AddRange(children);
            
            foreach (var child in children)
            {
                await LoadChildrenRecursiveAsync(userId, child.Id, result);
            }
        }
    }
}