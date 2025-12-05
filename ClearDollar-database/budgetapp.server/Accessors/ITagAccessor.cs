using System.Collections.Generic;
using BudgetAppCSCE361.Data;

namespace BudgetApp.Server.Accessors
{
    public interface ITagAccessor
    {
        Task<List<Tag>> GetAllAsync(Guid userId);
        Task<Tag?> GetByIdAsync(Guid userId, long tagId);
        Task<List<Tag>> GetByLevelAsync(Guid userId, int level);
        Task<List<Tag>> GetChildrenAsync(Guid userId, long parentId);
        Task<Tag> AddAsync(Guid userId, Tag tag);
        Task<Tag> UpdateAsync(Guid userId, Tag tag);
        Task<bool> DeleteAsync(Guid userId, long tagId);
    }
}